using System;
using System.Diagnostics;

namespace ASTCEnc
{
	public static class DecompressSymbolic
	{
		
		/**
		* @brief Compute the integer linear interpolation of two color endpoints.
		*
		* @param decode_mode   The ASTC profile (linear or sRGB)
		* @param color0        The endpoint0 color.
		* @param color1        The endpoint1 color.
		* @param weights        The interpolation weight (between 0 and 64).
		*
		* @return The interpolated color.
		*/
		public static vint4 lerp_color_int(
			ASTCEncProfile decode_mode,
			vint4 color0,
			vint4 color1,
			vint4 weights
		) {
			vint4 weight1 = weights;
			vint4 weight0 = new vint4(64) - weight1;

			if (decode_mode == ASTCEncProfile.ASTCENC_PRF_LDR_SRGB)
			{
				color0 = asr<8>(color0);
				color1 = asr<8>(color1);
			}

			vint4 color = (color0 * weight0) + (color1 * weight1) + new vint4(32);
			color = asr<6>(color);

			if (decode_mode == ASTCEncProfile.ASTCENC_PRF_LDR_SRGB)
			{
				color = color * new vint4(257);
			}

			return color;
		}


		/**
		* @brief Convert integer color value into a float value for the decoder.
		*
		* @param data       The integer color value post-interpolation.
		* @param lns_mask   If set treat lane as HDR (LNS) else LDR (unorm16).
		*
		* @return The float color value.
		*/
		public static vfloat4 decode_texel(
			vint4 data,
			vmask4 lns_mask
		) {
			vint4 color_lns = vint4.zero();
			vint4 color_unorm = vint4.zero();

			if (any(lns_mask))
			{
				color_lns = lns_to_sf16(data);
			}

			if (!all(lns_mask))
			{
				color_unorm = unorm16_to_sf16(data);
			}

			// Pick components and then convert to FP16
			vint4 datai = vint4.select(color_unorm, color_lns, lns_mask);
			return float16_to_float(datai);
		}

		/* See header for documentation. */
		public void unpack_weights(
			BlockSizeDescriptor bsd,
			SymbolicCompressedBlock scb,
			DecimationInfo di,
			bool is_dual_plane,
			int[] weights_plane1,
			int[] weights_plane2
		) {
			// Safe to overshoot as all arrays are allocated to full size
			if (!is_dual_plane)
			{
				// Build full 64-entry weight lookup table
				vint4 tab0(reinterpret_cast<const int*>(scb.weights +  0));
				vint4 tab1(reinterpret_cast<const int*>(scb.weights + 16));
				vint4 tab2(reinterpret_cast<const int*>(scb.weights + 32));
				vint4 tab3(reinterpret_cast<const int*>(scb.weights + 48));

				vint tab0p, tab1p, tab2p, tab3p;
				vtable_prepare(tab0, tab1, tab2, tab3, tab0p, tab1p, tab2p, tab3p);

				for (uint i = 0; i < bsd.texel_count; i += Constants.ASTCENC_SIMD_WIDTH)
				{
					vint summed_value = new vint(8);
					vint weight_count = new vint(di.texel_weight_count, i);
					int max_weight_count = hmax(weight_count).lane(0);

					//promise(max_weight_count > 0);
					for (int j = 0; j < max_weight_count; j++)
					{
						vint texel_weights = new vint(di.texel_weights_4t[j], i);
						vint texel_weights_int = new vint(di.texel_weights_int_4t[j], i);

						summed_value += vtable_8bt_32bi(tab0p, tab1p, tab2p, tab3p, texel_weights) * texel_weights_int;
					}

					store(lsr<4>(summed_value), weights_plane1 + i);
				}
			}
			else
			{
				// Build a 32-entry weight lookup table per plane
				// Plane 1
				vint4 tab0_plane1(reinterpret_cast<const int*>(scb.weights +  0));
				vint4 tab1_plane1(reinterpret_cast<const int*>(scb.weights + 16));
				vint tab0_plane1p, tab1_plane1p;
				vtable_prepare(tab0_plane1, tab1_plane1, tab0_plane1p, tab1_plane1p);

				// Plane 2
				vint4 tab0_plane2(reinterpret_cast<const int*>(scb.weights + 32));
				vint4 tab1_plane2(reinterpret_cast<const int*>(scb.weights + 48));
				vint tab0_plane2p, tab1_plane2p;
				vtable_prepare(tab0_plane2, tab1_plane2, tab0_plane2p, tab1_plane2p);

				for (uint i = 0; i < bsd.texel_count; i += Constants.ASTCENC_SIMD_WIDTH)
				{
					vint sum_plane1 = new vint(8);
					vint sum_plane2 = new vint(8);

					vint weight_count = new vint(di.texel_weight_count, i);
					int max_weight_count = hmax(weight_count).lane(0);

					//promise(max_weight_count > 0);
					for (int j = 0; j < max_weight_count; j++)
					{
						vint texel_weights = new vint(di.texel_weights_4t[j], i);
						vint texel_weights_int = new vint(di.texel_weights_int_4t[j], i);

						sum_plane1 += vtable_8bt_32bi(tab0_plane1p, tab1_plane1p, texel_weights) * texel_weights_int;
						sum_plane2 += vtable_8bt_32bi(tab0_plane2p, tab1_plane2p, texel_weights) * texel_weights_int;
					}

					store(lsr<4>(sum_plane1), weights_plane1 + i);
					store(lsr<4>(sum_plane2), weights_plane2 + i);
				}
			}
		}

		/**
		* @brief Return an FP32 NaN value for use in error colors.
		*
		* This NaN encoding will turn into 0xFFFF when converted to an FP16 NaN.
		*
		* @return The float color value.
		*/
		static float error_color_nan()
		{
			if32 v;
			v.u = 0xFFFFE000U;
			return v.f;
		}

		/* See header for documentation. */
		public static void decompress_symbolic_block(
			ASTCEncProfile decode_mode,
			BlockSizeDescriptor bsd,
			int xpos,
			int ypos,
			int zpos,
			SymbolicCompressedBlock scb,
			ImageBlock blk
		) {
			blk.xpos = xpos;
			blk.ypos = ypos;
			blk.zpos = zpos;

			blk.data_min = vfloat4.zero();
			blk.data_mean = vfloat4.zero();
			blk.data_max = vfloat4.zero();
			blk.grayscale = false;

			// If we detected an error-block, blow up immediately.
			if (scb.block_type == SYM_BTYPE.SYM_BTYPE_ERROR)
			{
				for (uint i = 0; i < bsd.texel_count; i++)
				{
					blk.data_r[i] = error_color_nan();
					blk.data_g[i] = error_color_nan();
					blk.data_b[i] = error_color_nan();
					blk.data_a[i] = error_color_nan();
					blk.rgb_lns[i] = 0;
					blk.alpha_lns[i] = 0;
				}

				return;
			}

			if ((scb.block_type == SYM_BTYPE.SYM_BTYPE_CONST_F16) ||
				(scb.block_type == SYM_BTYPE.SYM_BTYPE_CONST_U16))
			{
				vfloat4 color;
				byte use_lns = 0;

				// UNORM16 constant color block
				if (scb.block_type == SYM_BTYPE.SYM_BTYPE_CONST_U16)
				{
					vint4 colori = new (scb.constant_color);

					// For sRGB decoding a real decoder would just use the top 8 bits for color conversion.
					// We don't color convert, so rescale the top 8 bits into the full 16 bit dynamic range.
					if (decode_mode == ASTCEncProfile.ASTCENC_PRF_LDR_SRGB)
					{
						colori = asr<8>(colori) * 257;
					}

					vint4 colorf16 = unorm16_to_sf16(colori);
					color = float16_to_float(colorf16);
				}
				// FLOAT16 constant color block
				else
				{
					switch (decode_mode)
					{
					case ASTCEncProfile.ASTCENC_PRF_LDR_SRGB:
					case ASTCEncProfile.ASTCENC_PRF_LDR:
						color = new vfloat4(error_color_nan());
						break;
					case ASTCEncProfile.ASTCENC_PRF_HDR_RGB_LDR_A:
					case ASTCEncProfile.ASTCENC_PRF_HDR:
						// Constant-color block; unpack from FP16 to FP32.
						color = float16_to_float(vint4(scb.constant_color));
						use_lns = 1;
						break;
					}
				}

				for (uint i = 0; i < bsd.texel_count; i++)
				{
					blk.data_r[i] = color.lane(0);
					blk.data_g[i] = color.lane(1);
					blk.data_b[i] = color.lane(2);
					blk.data_a[i] = color.lane(3);
					blk.rgb_lns[i] = use_lns;
					blk.alpha_lns[i] = use_lns;
				}

				return;
			}

			// Get the appropriate partition-table entry
			int partition_count = scb.partition_count;
			PartitionInfo pi = bsd.get_partition_info((uint)partition_count, scb.partition_index);

			// Get the appropriate block descriptors
			BlockMode bm = bsd.get_block_mode(scb.block_mode);
			DecimationInfo di = bsd.get_decimation_info(bm.decimation_mode);

			bool is_dual_plane = bm.is_dual_plane;

			// Unquantize and undecimate the weights
			int[] plane1_weights = new int[Constants.BLOCK_MAX_TEXELS];
			int[] plane2_weights = new int[Constants.BLOCK_MAX_TEXELS];
			unpack_weights(bsd, scb, di, is_dual_plane, plane1_weights, plane2_weights);

			// Now that we have endpoint colors and weights, we can unpack texel colors
			int plane2_component = is_dual_plane ? scb.plane2_component : -1;
			vmask4 plane2_mask = vint4.lane_id() == new vint4(plane2_component);

			for (int i = 0; i < partition_count; i++)
			{
				// Decode the color endpoints for this partition
				vint4 ep0;
				vint4 ep1;
				bool rgb_lns;
				bool a_lns;

				unpack_color_endpoints(decode_mode,
									scb.color_formats[i],
									scb.get_color_quant_mode(),
									scb.color_values[i],
									rgb_lns, a_lns,
									ep0, ep1);

				vmask4 lns_mask = new vmask4(rgb_lns, rgb_lns, rgb_lns, a_lns);

				int texel_count = pi.partition_texel_count[i];
				for (int j = 0; j < texel_count; j++)
				{
					int tix = pi.texels_of_partition[i][j];
					vint4 weight = vint4.select(new vint4(plane1_weights[tix]), new vint4(plane2_weights[tix]), plane2_mask);
					vint4 color = lerp_color_int(decode_mode, ep0, ep1, weight);
					vfloat4 colorf = decode_texel(color, lns_mask);

					blk.data_r[tix] = colorf.lane(0);
					blk.data_g[tix] = colorf.lane(1);
					blk.data_b[tix] = colorf.lane(2);
					blk.data_a[tix] = colorf.lane(3);
				}
			}
		}

		#if !ASTCENC_DECOMPRESS_ONLY

		/* See header for documentation. */
		float compute_symbolic_block_difference_2plane(
			ASTCEncConfig config, 
			BlockSizeDescriptor bsd,
			SymbolicCompressedBlock scb,
			ImageBlock blk
		) {
			// If we detected an error-block, blow up immediately.
			if (scb.block_type == SYM_BTYPE.SYM_BTYPE_ERROR)
			{
				return ERROR_CALC_DEFAULT;
			}

			Debug.Assert(scb.block_mode >= 0);
			Debug.Assert(scb.partition_count == 1);
			Debug.Assert(bsd.get_block_mode(scb.block_mode).is_dual_plane);

			// Get the appropriate block descriptor
			BlockMode bm = bsd.get_block_mode(scb.block_mode);
			DecimationInfo di = bsd.get_decimation_info(bm.decimation_mode);

			// Unquantize and undecimate the weights
			int[] plane1_weights = new int[Constants.BLOCK_MAX_TEXELS];
			int[] plane2_weights = new int[Constants.BLOCK_MAX_TEXELS];
			unpack_weights(bsd, scb, di, true, plane1_weights, plane2_weights);

			vmask4 plane2_mask = vint4.lane_id() == new vint4(scb.plane2_component);

			vfloat4 summa = vfloat4.zero();

			// Decode the color endpoints for this partition
			vint4 ep0;
			vint4 ep1;
			bool rgb_lns;
			bool a_lns;

			unpack_color_endpoints(config.profile,
								scb.color_formats[0],
								scb.get_color_quant_mode(),
								scb.color_values[0],
								rgb_lns, a_lns,
								ep0, ep1);

			// Unpack and compute error for each texel in the partition
			uint texel_count = bsd.texel_count;
			for (uint i = 0; i < texel_count; i++)
			{
				vint4 weight = vint4.select(new vint4(plane1_weights[i]), new vint4(plane2_weights[i]), plane2_mask);
				vint4 colori = lerp_color_int(config.profile, ep0, ep1, weight);

				vfloat4 color = int_to_float(colori);
				vfloat4 oldColor = blk.texel(i);

				// Compare error using a perceptual decode metric for RGBM textures
				if (config.flags & ASTCENC_FLG_MAP_RGBM)
				{
					// Fail encodings that result in zero weight M pixels. Note that this can cause
					// "interesting" artifacts if we reject all useful encodings - we typically get max
					// brightness encodings instead which look just as bad. We recommend users apply a
					// bias to their stored M value, limiting the lower value to 16 or 32 to avoid
					// getting small M values post-quantization, but we can't prove it would never
					// happen, especially at low bit rates ...
					if (color.lane(3) == 0.0f)
					{
						return -ERROR_CALC_DEFAULT;
					}

					// Compute error based on decoded RGBM color
					color = new vfloat4(
						color.lane(0) * color.lane(3) * config.rgbm_m_scale,
						color.lane(1) * color.lane(3) * config.rgbm_m_scale,
						color.lane(2) * color.lane(3) * config.rgbm_m_scale,
						1.0f
					);

					oldColor = new vfloat4(
						oldColor.lane(0) * oldColor.lane(3) * config.rgbm_m_scale,
						oldColor.lane(1) * oldColor.lane(3) * config.rgbm_m_scale,
						oldColor.lane(2) * oldColor.lane(3) * config.rgbm_m_scale,
						1.0f
					);
				}

				vfloat4 error = oldColor - color;
				error = min(abs(error), 1e15f);
				error = error * error;

				summa += min(dot(error, blk.channel_weight), ERROR_CALC_DEFAULT);
			}

			return summa.lane(0);
		}

		/* See header for documentation. */
		float compute_symbolic_block_difference_1plane(
			ASTCEncConfig config,
			BlockSizeDescriptor bsd,
			SymbolicCompressedBlock scb,
			ImageBlock blk
		) {
			Debug.Assert(bsd.get_block_mode(scb.block_mode).is_dual_plane == false);

			// If we detected an error-block, blow up immediately.
			if (scb.block_type == SYM_BTYPE.SYM_BTYPE_ERROR)
			{
				return ERROR_CALC_DEFAULT;
			}

			Debug.Assert(scb.block_mode >= 0);

			// Get the appropriate partition-table entry
			uint partition_count = scb.partition_count;
			PartitionInfo pi = bsd.get_partition_info(partition_count, scb.partition_index);

			// Get the appropriate block descriptor
			BlockMode bm = bsd.get_block_mode(scb.block_mode);
			DecimationInfo di = bsd.get_decimation_info(bm.decimation_mode);

			// Unquantize and undecimate the weights
			int[] plane1_weights = new int[Constants.BLOCK_MAX_TEXELS];
			unpack_weights(bsd, scb, di, false, plane1_weights, null);

			vfloat4 summa = vfloat4.zero();
			for (uint i = 0; i < partition_count; i++)
			{
				// Decode the color endpoints for this partition
				vint4 ep0;
				vint4 ep1;
				bool rgb_lns;
				bool a_lns;

				unpack_color_endpoints(config.profile,
									scb.color_formats[i],
									scb.get_color_quant_mode(),
									scb.color_values[i],
									rgb_lns, a_lns,
									ep0, ep1);

				// Unpack and compute error for each texel in the partition
				uint texel_count = pi.partition_texel_count[i];
				for (uint j = 0; j < texel_count; j++)
				{
					uint tix = pi.texels_of_partition[i][j];
					vint4 colori = lerp_color_int(config.profile, ep0, ep1,
												new vint4(plane1_weights[tix]));

					vfloat4 color = int_to_float(colori);
					vfloat4 oldColor = blk.texel(tix);

					// Compare error using a perceptual decode metric for RGBM textures
					if (config.flags & ASTCENC_FLG_MAP_RGBM)
					{
						// Fail encodings that result in zero weight M pixels. Note that this can cause
						// "interesting" artifacts if we reject all useful encodings - we typically get max
						// brightness encodings instead which look just as bad. We recommend users apply a
						// bias to their stored M value, limiting the lower value to 16 or 32 to avoid
						// getting small M values post-quantization, but we can't prove it would never
						// happen, especially at low bit rates ...
						if (color.lane(3) == 0.0f)
						{
							return -ERROR_CALC_DEFAULT;
						}

						// Compute error based on decoded RGBM color
						color = new vfloat4(
							color.lane(0) * color.lane(3) * config.rgbm_m_scale,
							color.lane(1) * color.lane(3) * config.rgbm_m_scale,
							color.lane(2) * color.lane(3) * config.rgbm_m_scale,
							1.0f
						);

						oldColor = new vfloat4(
							oldColor.lane(0) * oldColor.lane(3) * config.rgbm_m_scale,
							oldColor.lane(1) * oldColor.lane(3) * config.rgbm_m_scale,
							oldColor.lane(2) * oldColor.lane(3) * config.rgbm_m_scale,
							1.0f
						);
					}

					vfloat4 error = oldColor - color;
					error = min(abs(error), 1e15f);
					error = error * error;

					summa += min(dot(error, blk.channel_weight), ERROR_CALC_DEFAULT);
				}
			}

			return summa.lane(0);
		}

		/* See header for documentation. */
		float compute_symbolic_block_difference_1plane_1partition(
			ASTCEncConfig config,
			BlockSizeDescriptor bsd,
			SymbolicCompressedBlock scb,
			ImageBlock blk
		) {
			// If we detected an error-block, blow up immediately.
			if (scb.block_type == SYM_BTYPE.SYM_BTYPE_ERROR)
			{
				return ERROR_CALC_DEFAULT;
			}

			Debug.Assert(scb.block_mode >= 0);
			Debug.Assert(bsd.get_partition_info(scb.partition_count, scb.partition_index).partition_count == 1);

			// Get the appropriate block descriptor
			BlockMode bm = bsd.get_block_mode(scb.block_mode);
			DecimationInfo di = bsd.get_decimation_info(bm.decimation_mode);

			// Unquantize and undecimate the weights
			int[] plane1_weights = new int[Constants.BLOCK_MAX_TEXELS];
			unpack_weights(bsd, scb, di, false, plane1_weights, null);

			// Decode the color endpoints for this partition
			vint4 ep0;
			vint4 ep1;
			bool rgb_lns;
			bool a_lns;

			unpack_color_endpoints(config.profile,
								scb.color_formats[0],
								scb.get_color_quant_mode(),
								scb.color_values[0],
								rgb_lns, a_lns,
								ep0, ep1);


			// Pre-shift sRGB so things round correctly
			if (config.profile == ASTCEncProfile.ASTCENC_PRF_LDR_SRGB)
			{
				ep0 = asr<8>(ep0);
				ep1 = asr<8>(ep1);
			}

			// Unpack and compute error for each texel in the partition
			vfloatacc summav = vfloatacc.zero();

			vint lane_id = vint.lane_id();
			vint srgb_scale = new vint(config.profile == ASTCEncProfile.ASTCENC_PRF_LDR_SRGB ? 257 : 1);

			uint texel_count = bsd.texel_count;
			for (uint i = 0; i < texel_count; i += Constants.ASTCENC_SIMD_WIDTH)
			{
				// Compute EP1 contribution
				vint weight1 = vint::loada(plane1_weights + i);
				vint ep1_r = new vint(ep1.lane(0)) * weight1;
				vint ep1_g = new vint(ep1.lane(1)) * weight1;
				vint ep1_b = new vint(ep1.lane(2)) * weight1;
				vint ep1_a = new vint(ep1.lane(3)) * weight1;

				// Compute EP0 contribution
				vint weight0 = new vint(64) - weight1;
				vint ep0_r = new vint(ep0.lane(0)) * weight0;
				vint ep0_g = new vint(ep0.lane(1)) * weight0;
				vint ep0_b = new vint(ep0.lane(2)) * weight0;
				vint ep0_a = new vint(ep0.lane(3)) * weight0;

				// Shift so things round correctly
				vint colori_r = asr<6>(ep0_r + ep1_r + vint(32)) * srgb_scale;
				vint colori_g = asr<6>(ep0_g + ep1_g + vint(32)) * srgb_scale;
				vint colori_b = asr<6>(ep0_b + ep1_b + vint(32)) * srgb_scale;
				vint colori_a = asr<6>(ep0_a + ep1_a + vint(32)) * srgb_scale;

				// Compute color diff
				vfloat color_r = int_to_float(colori_r);
				vfloat color_g = int_to_float(colori_g);
				vfloat color_b = int_to_float(colori_b);
				vfloat color_a = int_to_float(colori_a);

				vfloat color_orig_r = loada(blk.data_r + i);
				vfloat color_orig_g = loada(blk.data_g + i);
				vfloat color_orig_b = loada(blk.data_b + i);
				vfloat color_orig_a = loada(blk.data_a + i);

				vfloat color_error_r = min(abs(color_orig_r - color_r), new vfloat(1e15f));
				vfloat color_error_g = min(abs(color_orig_g - color_g), new vfloat(1e15f));
				vfloat color_error_b = min(abs(color_orig_b - color_b), new vfloat(1e15f));
				vfloat color_error_a = min(abs(color_orig_a - color_a), new vfloat(1e15f));

				// Compute squared error metric
				color_error_r = color_error_r * color_error_r;
				color_error_g = color_error_g * color_error_g;
				color_error_b = color_error_b * color_error_b;
				color_error_a = color_error_a * color_error_a;

				vfloat metric = color_error_r * blk.channel_weight.lane(0)
							+ color_error_g * blk.channel_weight.lane(1)
							+ color_error_b * blk.channel_weight.lane(2)
							+ color_error_a * blk.channel_weight.lane(3);

				// Mask off bad lanes
				vmask mask = lane_id < new vint(texel_count);
				lane_id += new vint(Constants.ASTCENC_SIMD_WIDTH);
				haccumulate(summav, metric, mask);
			}

			return hadd_s(summav);
		}
	}
}