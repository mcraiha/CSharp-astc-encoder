#if !ASTCENC_DECOMPRESS_ONLY

/**
 * @brief Functions to calculate variance per component in a NxN footprint.
 *
 * We need N to be parametric, so the routine below uses summed area tables in order to execute in
 * O(1) time independent of how big N is.
 *
 * The addition uses a Brent-Kung-based parallel prefix adder. This uses the prefix tree to first
 * perform a binary reduction, and then distributes the results. This method means that there is no
 * serial dependency between a given element and the next one, and also significantly improves
 * numerical stability allowing us to use floats rather than doubles.
 */

using System;
using System.Diagnostics;

namespace ASTCEnc
{
	public static class ComputeVariance
	{
		/**
		* @brief Generate a prefix-sum array using the Brent-Kung algorithm.
		*
		* This will take an input array of the form:
		*     v0, v1, v2, ...
		* ... and modify in-place to turn it into a prefix-sum array of the form:
		*     v0, v0+v1, v0+v1+v2, ...
		*
		* @param d      The array to prefix-sum.
		* @param items  The number of items in the array.
		* @param stride The item spacing in the array; i.e. dense arrays should use 1.
		*/
		static void brent_kung_prefix_sum(vfloat4[] d, uint items, int stride) 
		{
			if (items < 2)
				return;

			uint lc_stride = 2;
			uint log2_stride = 1;

			// The reduction-tree loop
			do {
				uint step = lc_stride >> 1;
				uint start = lc_stride - 1;
				uint iters = items >> (int)log2_stride;

				//vfloat4 *da = d + (start * stride);
				var da = new Span<vfloat4>(d).Slice((int)(start * stride));
				ptrdiff_t ofs = -(ptrdiff_t)(step * stride);
				uint ofs_stride = stride << (int)log2_stride;

				while (iters)
				{
					*da = *da + da[ofs];
					da += ofs_stride;
					iters--;
				}

				log2_stride += 1;
				lc_stride <<= 1;
			} while (lc_stride <= items);

			// The expansion-tree loop
			do {
				log2_stride -= 1;
				lc_stride >>= 1;

				uint step = lc_stride >> 1;
				uint start = step + lc_stride - 1;
				uint iters = (items - step) >> log2_stride;

				vfloat4 *da = d + (start * stride);
				ptrdiff_t ofs = -(ptrdiff_t)(step * stride);
				uint ofs_stride = stride << log2_stride;

				while (iters)
				{
					*da = *da + da[ofs];
					da += ofs_stride;
					iters--;
				}
			} while (lc_stride > 2);
		}

		private static void SetVARBUF(vfloat4[] varbuf, int z, int y, int x, int zst, int yst, vfloat4 d)
		{
			varbuf[z * zst + y * yst + x] = d;
		}

		// Macros to act as accessor functions for the work-memory
			//#define VARBUF1(z, y, x) varbuf1[z * zst + y * yst + x]
			//#define VARBUF2(z, y, x) varbuf2[z * zst + y * yst + x]

		/* See header for documentation. */
		static void compute_pixel_region_variance(
			astcenc_contexti ctx,
			pixel_region_args arg
		) {
			// Unpack the memory structure into local variables
			ASTCEncImage img = arg.img;
			ASTCEncSwizzle swz = arg.swz;
			bool have_z = arg.have_z;

			int size_x = (int)arg.size_x;
			int size_y = (int)arg.size_y;
			int size_z = (int)arg.size_z;

			int offset_x = (int)arg.offset_x;
			int offset_y = (int)arg.offset_y;
			int offset_z = (int)arg.offset_z;

			int alpha_kernel_radius = (int)arg.alpha_kernel_radius;

			float[] input_alpha_averages = ctx.input_alpha_averages;
			vfloat4[] work_memory = arg.work_memory;

			// Compute memory sizes and dimensions that we need
			int kernel_radius = alpha_kernel_radius;
			int kerneldim = 2 * kernel_radius + 1;
			int kernel_radius_xy = kernel_radius;
			int kernel_radius_z = have_z ? kernel_radius : 0;

			int padsize_x = size_x + kerneldim;
			int padsize_y = size_y + kerneldim;
			int padsize_z = size_z + (have_z ? kerneldim : 0);
			int sizeprod = padsize_x * padsize_y * padsize_z;

			int zd_start = have_z ? 1 : 0;

			vfloat4[] varbuf1 = work_memory;
			vfloat4[] varbuf2 = work_memory + sizeprod;

			// Scaling factors to apply to Y and Z for accesses into the work buffers
			int yst = padsize_x;
			int zst = padsize_x * padsize_y;

			// Scaling factors to apply to Y and Z for accesses into result buffers
			int ydt = (int)img.dim_x;
			int zdt = (int)(img.dim_x * img.dim_y);

			

			// Load N and N^2 values into the work buffers
			if (img.data_type == ASTCEncType.ASTCENC_TYPE_U8)
			{
				// Swizzle data structure 4 = ZERO, 5 = ONE
				byte[] data = new byte[6];
				data[(int)ASTCEncSwizzleChannel.ASTCENC_SWZ_0] = 0;
				data[(int)ASTCEncSwizzleChannel.ASTCENC_SWZ_1] = 255;

				for (int z = zd_start; z < padsize_z; z++)
				{
					int z_src = (z - zd_start) + offset_z - kernel_radius_z;
					z_src = ASTCMath.clamp(z_src, 0, (int)(img.dim_z - 1));
					byte[] data8 = static_cast<byte[]>(img.data[z_src]);
					
					for (int y = 1; y < padsize_y; y++)
					{
						int y_src = (y - 1) + offset_y - kernel_radius_xy;
						y_src = ASTCMath.clamp(y_src, 0, (int)(img.dim_y - 1));

						for (int x = 1; x < padsize_x; x++)
						{
							int x_src = (x - 1) + offset_x - kernel_radius_xy;
							x_src = ASTCMath.clamp(x_src, 0, (int)(img.dim_x - 1));

							data[0] = data8[(4 * img.dim_x * y_src) + (4 * x_src    )];
							data[1] = data8[(4 * img.dim_x * y_src) + (4 * x_src + 1)];
							data[2] = data8[(4 * img.dim_x * y_src) + (4 * x_src + 2)];
							data[3] = data8[(4 * img.dim_x * y_src) + (4 * x_src + 3)];

							byte r = data[(int)swz.r];
							byte g = data[(int)swz.g];
							byte b = data[(int)swz.b];
							byte a = data[(int)swz.a];

							vfloat4 d = new vfloat4 (r * (1.0f / 255.0f),
												g * (1.0f / 255.0f),
												b * (1.0f / 255.0f),
												a * (1.0f / 255.0f));

							//VARBUF1(z, y, x) = d;
							//VARBUF2(z, y, x) = d * d;
							SetVARBUF(varbuf1, z, y, x, zst, yst, d);
							SetVARBUF(varbuf2, z, y, x, zst, yst, d * d);						
						}
					}
				}
			}
			else if (img.data_type == ASTCEncType.ASTCENC_TYPE_F16)
			{
				// Swizzle data structure 4 = ZERO, 5 = ONE (in FP16)
				ushort[] data = new ushort[6];
				data[(int)ASTCEncSwizzleChannel.ASTCENC_SWZ_0] = 0;
				data[(int)ASTCEncSwizzleChannel.ASTCENC_SWZ_1] = 0x3C00;

				for (int z = zd_start; z < padsize_z; z++)
				{
					int z_src = (z - zd_start) + offset_z - kernel_radius_z;
					z_src = ASTCMath.clamp(z_src, 0, (int)(img.dim_z - 1));
					ushort[] data16 = static_cast<ushort[]>(img.data[z_src]);

					for (int y = 1; y < padsize_y; y++)
					{
						int y_src = (y - 1) + offset_y - kernel_radius_xy;
						y_src = ASTCMath.clamp(y_src, 0, (int)(img.dim_y - 1));

						for (int x = 1; x < padsize_x; x++)
						{
							int x_src = (x - 1) + offset_x - kernel_radius_xy;
							x_src = ASTCMath.clamp(x_src, 0, (int)(img.dim_x - 1));

							data[0] = data16[(4 * img.dim_x * y_src) + (4 * x_src    )];
							data[1] = data16[(4 * img.dim_x * y_src) + (4 * x_src + 1)];
							data[2] = data16[(4 * img.dim_x * y_src) + (4 * x_src + 2)];
							data[3] = data16[(4 * img.dim_x * y_src) + (4 * x_src + 3)];

							vint4 di = new vint4(data[(int)swz.r], data[(int)swz.g], data[(int)swz.b], data[(int)swz.a]);
							vfloat4 d = float16_to_float(di);

							VARBUF1(z, y, x) = d;
							VARBUF2(z, y, x) = d * d;
						}
					}
				}
			}
			else // if (img.data_type == ASTCENC_TYPE_F32)
			{
				Debug.Assert(img.data_type == ASTCEncType.ASTCENC_TYPE_F32);

				// Swizzle data structure 4 = ZERO, 5 = ONE (in FP16)
				float[] data = new float[6];
				data[(int)ASTCEncSwizzleChannel.ASTCENC_SWZ_0] = 0.0f;
				data[(int)ASTCEncSwizzleChannel.ASTCENC_SWZ_1] = 1.0f;

				for (int z = zd_start; z < padsize_z; z++)
				{
					int z_src = (z - zd_start) + offset_z - kernel_radius_z;
					z_src = ASTCMath.clamp(z_src, 0, (int)(img.dim_z - 1));
					float[] data32 = static_cast<float*>(img.data[z_src]);

					for (int y = 1; y < padsize_y; y++)
					{
						int y_src = (y - 1) + offset_y - kernel_radius_xy;
						y_src = ASTCMath.clamp(y_src, 0, (int)(img.dim_y - 1));

						for (int x = 1; x < padsize_x; x++)
						{
							int x_src = (x - 1) + offset_x - kernel_radius_xy;
							x_src = ASTCMath.clamp(x_src, 0, (int)(img.dim_x - 1));

							data[0] = data32[(4 * img.dim_x * y_src) + (4 * x_src    )];
							data[1] = data32[(4 * img.dim_x * y_src) + (4 * x_src + 1)];
							data[2] = data32[(4 * img.dim_x * y_src) + (4 * x_src + 2)];
							data[3] = data32[(4 * img.dim_x * y_src) + (4 * x_src + 3)];

							float r = data[(int)swz.r];
							float g = data[(int)swz.g];
							float b = data[(int)swz.b];
							float a = data[(int)swz.a];

							vfloat4 d = new vfloat4(r, g, b, a);

							VARBUF1(z, y, x) = d;
							VARBUF2(z, y, x) = d * d;
						}
					}
				}
			}

			// Pad with an extra layer of 0s; this forms the edge of the SAT tables
			vfloat4 vbz = vfloat4.zero();
			for (int z = 0; z < padsize_z; z++)
			{
				for (int y = 0; y < padsize_y; y++)
				{
					VARBUF1(z, y, 0) = vbz;
					VARBUF2(z, y, 0) = vbz;
				}

				for (int x = 0; x < padsize_x; x++)
				{
					VARBUF1(z, 0, x) = vbz;
					VARBUF2(z, 0, x) = vbz;
				}
			}

			if (have_z)
			{
				for (int y = 0; y < padsize_y; y++)
				{
					for (int x = 0; x < padsize_x; x++)
					{
						VARBUF1(0, y, x) = vbz;
						VARBUF2(0, y, x) = vbz;
					}
				}
			}

			// Generate summed-area tables for N and N^2; this is done in-place, using
			// a Brent-Kung parallel-prefix based algorithm to minimize precision loss
			for (int z = zd_start; z < padsize_z; z++)
			{
				for (int y = 1; y < padsize_y; y++)
				{
					brent_kung_prefix_sum(&(VARBUF1(z, y, 1)), padsize_x - 1, 1);
					brent_kung_prefix_sum(&(VARBUF2(z, y, 1)), padsize_x - 1, 1);
				}
			}

			for (int z = zd_start; z < padsize_z; z++)
			{
				for (int x = 1; x < padsize_x; x++)
				{
					brent_kung_prefix_sum(&(VARBUF1(z, 1, x)), padsize_y - 1, yst);
					brent_kung_prefix_sum(&(VARBUF2(z, 1, x)), padsize_y - 1, yst);
				}
			}

			if (have_z)
			{
				for (int y = 1; y < padsize_y; y++)
				{
					for (int x = 1; x < padsize_x; x++)
					{
						brent_kung_prefix_sum(&(VARBUF1(1, y, x)), padsize_z - 1, zst);
						brent_kung_prefix_sum(&(VARBUF2(1, y, x)), padsize_z - 1, zst);
					}
				}
			}

			// Compute a few constants used in the variance-calculation.
			float alpha_kdim = (float)(2 * alpha_kernel_radius + 1);
			float alpha_rsamples;

			if (have_z)
			{
				alpha_rsamples = 1.0f / (alpha_kdim * alpha_kdim * alpha_kdim);
			}
			else
			{
				alpha_rsamples = 1.0f / (alpha_kdim * alpha_kdim);
			}

			// Use the summed-area tables to compute variance for each neighborhood
			if (have_z)
			{
				for (int z = 0; z < size_z; z++)
				{
					int z_src = z + kernel_radius_z;
					int z_dst = z + offset_z;
					int z_low  = z_src - alpha_kernel_radius;
					int z_high = z_src + alpha_kernel_radius + 1;

					for (int y = 0; y < size_y; y++)
					{
						int y_src = y + kernel_radius_xy;
						int y_dst = y + offset_y;
						int y_low  = y_src - alpha_kernel_radius;
						int y_high = y_src + alpha_kernel_radius + 1;

						for (int x = 0; x < size_x; x++)
						{
							int x_src = x + kernel_radius_xy;
							int x_dst = x + offset_x;
							int x_low  = x_src - alpha_kernel_radius;
							int x_high = x_src + alpha_kernel_radius + 1;

							// Summed-area table lookups for alpha average
							float vasum = (  VARBUF1(z_high, y_low,  x_low).lane<3>()
										- VARBUF1(z_high, y_low,  x_high).lane<3>()
										- VARBUF1(z_high, y_high, x_low).lane<3>()
										+ VARBUF1(z_high, y_high, x_high).lane<3>()) -
										(  VARBUF1(z_low,  y_low,  x_low).lane<3>()
										- VARBUF1(z_low,  y_low,  x_high).lane<3>()
										- VARBUF1(z_low,  y_high, x_low).lane<3>()
										+ VARBUF1(z_low,  y_high, x_high).lane<3>());

							int out_index = z_dst * zdt + y_dst * ydt + x_dst;
							input_alpha_averages[out_index] = (vasum * alpha_rsamples);
						}
					}
				}
			}
			else
			{
				for (int y = 0; y < size_y; y++)
				{
					int y_src = y + kernel_radius_xy;
					int y_dst = y + offset_y;
					int y_low  = y_src - alpha_kernel_radius;
					int y_high = y_src + alpha_kernel_radius + 1;

					for (int x = 0; x < size_x; x++)
					{
						int x_src = x + kernel_radius_xy;
						int x_dst = x + offset_x;
						int x_low  = x_src - alpha_kernel_radius;
						int x_high = x_src + alpha_kernel_radius + 1;

						// Summed-area table lookups for alpha average
						float vasum = VARBUF1(0, y_low,  x_low).lane<3>()
									- VARBUF1(0, y_low,  x_high).lane<3>()
									- VARBUF1(0, y_high, x_low).lane<3>()
									+ VARBUF1(0, y_high, x_high).lane<3>();

						int out_index = y_dst * ydt + x_dst;
						input_alpha_averages[out_index] = (vasum * alpha_rsamples);
					}
				}
			}
		}

		/* See header for documentation. */
		static uint init_compute_averages(ASTCEncImage img, uint alpha_kernel_radius, ASTCEncSwizzle swz, avg_args ag) 
		{
			uint size_x = img.dim_x;
			uint size_y = img.dim_y;
			uint size_z = img.dim_z;

			// Compute maximum block size and from that the working memory buffer size
			uint kernel_radius = alpha_kernel_radius;
			uint kerneldim = 2 * kernel_radius + 1;

			bool have_z = (size_z > 1);
			uint max_blk_size_xy = have_z ? 16 : 32;
			uint max_blk_size_z = ASTCMath.min(size_z, have_z ? 16u : 1u);

			uint max_padsize_xy = max_blk_size_xy + kerneldim;
			uint max_padsize_z = max_blk_size_z + (have_z ? kerneldim : 0);

			// Perform block-wise averages calculations across the image
			// Initialize fields which are not populated until later
			ag.arg.size_x = 0;
			ag.arg.size_y = 0;
			ag.arg.size_z = 0;
			ag.arg.offset_x = 0;
			ag.arg.offset_y = 0;
			ag.arg.offset_z = 0;
			ag.arg.work_memory = null;

			ag.arg.img = img;
			ag.arg.swz = swz;
			ag.arg.have_z = have_z;
			ag.arg.alpha_kernel_radius = alpha_kernel_radius;

			ag.img_size_x = size_x;
			ag.img_size_y = size_y;
			ag.img_size_z = size_z;
			ag.blk_size_xy = max_blk_size_xy;
			ag.blk_size_z = max_blk_size_z;
			ag.work_memory_size = 2 * max_padsize_xy * max_padsize_xy * max_padsize_z;

			// The parallel task count
			uint z_tasks = (size_z + max_blk_size_z - 1) / max_blk_size_z;
			uint y_tasks = (size_y + max_blk_size_xy - 1) / max_blk_size_xy;
			return z_tasks * y_tasks;
		}
	}
}

#endif // !ASTCENC_DECOMPRESS_ONLY