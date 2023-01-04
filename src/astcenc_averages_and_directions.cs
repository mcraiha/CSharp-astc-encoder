
namespace ASTCEnc
{
	public static class AveragesAndDirections
	{
		public static void compute_partition_averages_rgb(PartitionInfo pi, ImageBlock blk, vfloat4[] averages)
		{
			uint partition_count = pi.partition_count;
			uint texel_count = blk.texel_count;
			//promise(texel_count > 0);

			// For 1 partition just use the precomputed mean
			if (partition_count == 1)
			{
				averages[0] = blk.data_mean.swz3(0, 1, 2);
			}
			// For 2 partitions scan results for partition 0, compute partition 1
			else if (partition_count == 2)
			{
				vfloatacc[] pp_avg_rgb = new vfloatacc[3];

				vint lane_id = vint.lane_id();
				for (uint i = 0; i < texel_count; i += Constants.ASTCENC_SIMD_WIDTH)
				{
					vint texel_partition = new vint(pi.partition_of_texel, i);

					vmask lane_mask = lane_id < new vint((int)texel_count);
					lane_id += new vint(Constants.ASTCENC_SIMD_WIDTH);

					vmask p0_mask = lane_mask & (texel_partition == new vint(0));

					vfloat data_r = vfloat.loada(blk.data_r, i);
					vfloatacc.haccumulate(pp_avg_rgb[0], data_r, p0_mask);

					vfloat data_g = vfloat.loada(blk.data_g, i);
					vfloatacc.haccumulate(pp_avg_rgb[1], data_g, p0_mask);

					vfloat data_b = vfloat.loada(blk.data_b, i);
					vfloatacc.haccumulate(pp_avg_rgb[2], data_b, p0_mask);
				}

				vfloat4 block_total = blk.data_mean.swz3(0, 1, 2) * (float)(blk.texel_count);

				vfloat4 p0_total = vfloat4.vfloat3(vfloatacc.hadd_s(pp_avg_rgb[0]),
										vfloatacc.hadd_s(pp_avg_rgb[1]),
										vfloatacc.hadd_s(pp_avg_rgb[2]));

				vfloat4 p1_total = block_total - p0_total;

				averages[0] = p0_total / (float)(pi.partition_texel_count[0]);
				averages[1] = p1_total / (float)(pi.partition_texel_count[1]);
			}
			// For 3 partitions scan results for partition 0/1, compute partition 2
			else if (partition_count == 3)
			{
				vfloatacc[,] pp_avg_rgb = new vfloatacc[2, 3];

				vint lane_id = vint.lane_id();
				for (uint i = 0; i < texel_count; i += Constants.ASTCENC_SIMD_WIDTH)
				{
					vint texel_partition = new vint(pi.partition_of_texel, i);

					vmask lane_mask = lane_id < new vint((int)texel_count);
					lane_id += new vint(Constants.ASTCENC_SIMD_WIDTH);

					vmask p0_mask = lane_mask & (texel_partition == new vint(0));
					vmask p1_mask = lane_mask & (texel_partition == new vint(1));

					vfloat data_r = vfloat.loada(blk.data_r, i);
					vfloatacc.haccumulate(pp_avg_rgb[0, 0], data_r, p0_mask);
					vfloatacc.haccumulate(pp_avg_rgb[1, 0], data_r, p1_mask);

					vfloat data_g = vfloat.loada(blk.data_g, i);
					vfloatacc.haccumulate(pp_avg_rgb[0, 1], data_g, p0_mask);
					vfloatacc.haccumulate(pp_avg_rgb[1, 1], data_g, p1_mask);

					vfloat data_b = vfloat.loada(blk.data_b, i);
					vfloatacc.haccumulate(pp_avg_rgb[0, 2], data_b, p0_mask);
					vfloatacc.haccumulate(pp_avg_rgb[1, 2], data_b, p1_mask);
				}

				vfloat4 block_total = blk.data_mean.swz3(0, 1, 2) * (float)(blk.texel_count);

				vfloat4 p0_total = vfloat4.vfloat3(vfloatacc.hadd_s(pp_avg_rgb[0, 0]),
										vfloatacc.hadd_s(pp_avg_rgb[0, 1]),
										vfloatacc.hadd_s(pp_avg_rgb[0, 2]));

				vfloat4 p1_total = vfloat4.vfloat3(vfloatacc.hadd_s(pp_avg_rgb[1, 0]),
										vfloatacc.hadd_s(pp_avg_rgb[1, 1]),
										vfloatacc.hadd_s(pp_avg_rgb[1, 2]));

				vfloat4 p2_total = block_total - p0_total - p1_total;

				averages[0] = p0_total / (float)(pi.partition_texel_count[0]);
				averages[1] = p1_total / (float)(pi.partition_texel_count[1]);
				averages[2] = p2_total / (float)(pi.partition_texel_count[2]);
			}
			else
			{
				// For 4 partitions scan results for partition 0/1/2, compute partition 3
				vfloatacc[,] pp_avg_rgb = new vfloat[3, 3];

				vint lane_id = vint.lane_id();
				for (uint i = 0; i < texel_count; i += Constants.ASTCENC_SIMD_WIDTH)
				{
					vint texel_partition = new vint(pi.partition_of_texel, i);

					vmask lane_mask = lane_id < new vint((int)texel_count);
					lane_id += new vint(Constants.ASTCENC_SIMD_WIDTH);

					vmask p0_mask = lane_mask & (texel_partition == new vint(0));
					vmask p1_mask = lane_mask & (texel_partition == new vint(1));
					vmask p2_mask = lane_mask & (texel_partition == new vint(2));

					vfloat data_r = vfloat.loada(blk.data_r, i);
					vfloatacc.haccumulate(pp_avg_rgb[0, 0], data_r, p0_mask);
					vfloatacc.haccumulate(pp_avg_rgb[1, 0], data_r, p1_mask);
					vfloatacc.haccumulate(pp_avg_rgb[2, 0], data_r, p2_mask);

					vfloat data_g = vfloat.loada(blk.data_g, i);
					vfloatacc.haccumulate(pp_avg_rgb[0, 1], data_g, p0_mask);
					vfloatacc.haccumulate(pp_avg_rgb[1, 1], data_g, p1_mask);
					vfloatacc.haccumulate(pp_avg_rgb[2, 1], data_g, p2_mask);

					vfloat data_b = vfloat.loada(blk.data_b, i);
					vfloatacc.haccumulate(pp_avg_rgb[0, 2], data_b, p0_mask);
					vfloatacc.haccumulate(pp_avg_rgb[1, 2], data_b, p1_mask);
					vfloatacc.haccumulate(pp_avg_rgb[2, 2], data_b, p2_mask);
				}

				vfloat4 block_total = blk.data_mean.swz3(0, 1, 2) * (float)(blk.texel_count);

				vfloat4 p0_total = vfloat4.vfloat3(vfloatacc.hadd_s(pp_avg_rgb[0, 0]),
										vfloatacc.hadd_s(pp_avg_rgb[0, 1]),
										vfloatacc.hadd_s(pp_avg_rgb[0, 2]));

				vfloat4 p1_total = vfloat4.vfloat3(vfloatacc.hadd_s(pp_avg_rgb[1, 0]),
										vfloatacc.hadd_s(pp_avg_rgb[1, 1]),
										vfloatacc.hadd_s(pp_avg_rgb[1, 2]));

				vfloat4 p2_total = vfloat4.vfloat3(vfloatacc.hadd_s(pp_avg_rgb[2, 0]),
										vfloatacc.hadd_s(pp_avg_rgb[2, 1]),
										vfloatacc.hadd_s(pp_avg_rgb[2, 2]));

				vfloat4 p3_total = block_total - p0_total - p1_total- p2_total;

				averages[0] = p0_total / (float)(pi.partition_texel_count[0]);
				averages[1] = p1_total / (float)(pi.partition_texel_count[1]);
				averages[2] = p2_total / (float)(pi.partition_texel_count[2]);
				averages[3] = p3_total / (float)(pi.partition_texel_count[3]);
			}
		}

		static void compute_partition_averages_rgba(PartitionInfo pi, ImageBlock blk, vfloat4[] averages) 
		{
			uint partition_count = pi.partition_count;
			uint texel_count = blk.texel_count;
			//promise(texel_count > 0);

			// For 1 partition just use the precomputed mean
			if (partition_count == 1)
			{
				averages[0] = blk.data_mean;
			}
			// For 2 partitions scan results for partition 0, compute partition 1
			else if (partition_count == 2)
			{
				vfloat4[] pp_avg_rgba = new vfloat4[4];

				vint lane_id = vint.lane_id();
				for (uint i = 0; i < texel_count; i += Constants.ASTCENC_SIMD_WIDTH)
				{
					vint texel_partition = new vint(pi.partition_of_texel, i);

					vmask lane_mask = lane_id < new vint((int)texel_count);
					lane_id += new vint(Constants.ASTCENC_SIMD_WIDTH);

					vmask p0_mask = lane_mask & (texel_partition == new vint(0));

					vfloat data_r = vfloat.loada(blk.data_r, i);
					vfloat4.haccumulate(pp_avg_rgba[0], data_r, p0_mask);

					vfloat data_g = vfloat.loada(blk.data_g, i);
					vfloat4.haccumulate(pp_avg_rgba[1], data_g, p0_mask);

					vfloat data_b = vfloat.loada(blk.data_b, i);
					vfloat4.haccumulate(pp_avg_rgba[2], data_b, p0_mask);

					vfloat data_a = vfloat.loada(blk.data_a, i);
					vfloat4.haccumulate(pp_avg_rgba[3], data_a, p0_mask);
				}

				vfloat4 block_total = blk.data_mean * (float)(blk.texel_count);

				vfloat4 p0_total = new vfloat4(vfloat4.hadd_s(pp_avg_rgba[0]),
										vfloat4.hadd_s(pp_avg_rgba[1]),
										vfloat4.hadd_s(pp_avg_rgba[2]),
										vfloat4.hadd_s(pp_avg_rgba[3]));

				vfloat4 p1_total = block_total - p0_total;

				averages[0] = p0_total / (float)(pi.partition_texel_count[0]);
				averages[1] = p1_total / (float)(pi.partition_texel_count[1]);
			}
			// For 3 partitions scan results for partition 0/1, compute partition 2
			else if (partition_count == 3)
			{
				vfloat4[,] pp_avg_rgba = new vfloat4[2, 4];

				vint lane_id = vint.lane_id();
				for (uint i = 0; i < texel_count; i += Constants.ASTCENC_SIMD_WIDTH)
				{
					vint texel_partition = new vint(pi.partition_of_texel, i);

					vmask lane_mask = lane_id < new vint((int)texel_count);
					lane_id += new vint(Constants.ASTCENC_SIMD_WIDTH);

					vmask p0_mask = lane_mask & (texel_partition == new vint(0));
					vmask p1_mask = lane_mask & (texel_partition == new vint(1));

					vfloat data_r = vfloat.loada(blk.data_r, i);
					vfloat4.haccumulate(pp_avg_rgba[0, 0], data_r, p0_mask);
					vfloat4.haccumulate(pp_avg_rgba[1, 0], data_r, p1_mask);

					vfloat data_g = vfloat.loada(blk.data_g, i);
					vfloat4.haccumulate(pp_avg_rgba[0, 1], data_g, p0_mask);
					vfloat4.haccumulate(pp_avg_rgba[1, 1], data_g, p1_mask);

					vfloat data_b = vfloat.loada(blk.data_b, i);
					vfloat4.haccumulate(pp_avg_rgba[0, 2], data_b, p0_mask);
					vfloat4.haccumulate(pp_avg_rgba[1, 2], data_b, p1_mask);

					vfloat data_a = vfloat.loada(blk.data_a, i);
					vfloat4.haccumulate(pp_avg_rgba[0, 3], data_a, p0_mask);
					vfloat4.haccumulate(pp_avg_rgba[1, 3], data_a, p1_mask);
				}

				vfloat4 block_total = blk.data_mean * (float)(blk.texel_count);

				vfloat4 p0_total = new vfloat4(vfloat4.hadd_s(pp_avg_rgba[0, 0]),
										vfloat4.hadd_s(pp_avg_rgba[0, 1]),
										vfloat4.hadd_s(pp_avg_rgba[0, 2]),
										vfloat4.hadd_s(pp_avg_rgba[0, 3]));

				vfloat4 p1_total = new vfloat4(vfloat4.hadd_s(pp_avg_rgba[1, 0]),
										vfloat4.hadd_s(pp_avg_rgba[1, 1]),
										vfloat4.hadd_s(pp_avg_rgba[1, 2]),
										vfloat4.hadd_s(pp_avg_rgba[1, 3]));

				vfloat4 p2_total = block_total - p0_total - p1_total;

				averages[0] = p0_total / (float)(pi.partition_texel_count[0]);
				averages[1] = p1_total / (float)(pi.partition_texel_count[1]);
				averages[2] = p2_total / (float)(pi.partition_texel_count[2]);
			}
			else
			{
				// For 4 partitions scan results for partition 0/1/2, compute partition 3
				vfloat4[,] pp_avg_rgba = new vfloat4[3, 4];

				vint lane_id = vint.lane_id();
				for (uint i = 0; i < texel_count; i += Constants.ASTCENC_SIMD_WIDTH)
				{
					vint texel_partition = new vint(pi.partition_of_texel, i);

					vmask lane_mask = lane_id < new vint((int)texel_count);
					lane_id += new vint(Constants.ASTCENC_SIMD_WIDTH);

					vmask p0_mask = lane_mask & (texel_partition == new vint(0));
					vmask p1_mask = lane_mask & (texel_partition == new vint(1));
					vmask p2_mask = lane_mask & (texel_partition == new vint(2));

					vfloat data_r = vfloat.loada(blk.data_r, i);
					vfloat4.haccumulate(pp_avg_rgba[0, 0], data_r, p0_mask);
					vfloat4.haccumulate(pp_avg_rgba[1, 0], data_r, p1_mask);
					vfloat4.haccumulate(pp_avg_rgba[2, 0], data_r, p2_mask);

					vfloat data_g = vfloat.loada(blk.data_g, i);
					vfloat4.haccumulate(pp_avg_rgba[0, 1], data_g, p0_mask);
					vfloat4.haccumulate(pp_avg_rgba[1, 1], data_g, p1_mask);
					vfloat4.haccumulate(pp_avg_rgba[2, 1], data_g, p2_mask);

					vfloat data_b = vfloat.loada(blk.data_b, i);
					vfloat4.haccumulate(pp_avg_rgba[0, 2], data_b, p0_mask);
					vfloat4.haccumulate(pp_avg_rgba[1, 2], data_b, p1_mask);
					vfloat4.haccumulate(pp_avg_rgba[2, 2], data_b, p2_mask);

					vfloat data_a = vfloat.loada(blk.data_a, i);
					vfloat4.haccumulate(pp_avg_rgba[0, 3], data_a, p0_mask);
					vfloat4.haccumulate(pp_avg_rgba[1, 3], data_a, p1_mask);
					vfloat4.haccumulate(pp_avg_rgba[2, 3], data_a, p2_mask);
				}

				vfloat4 block_total = blk.data_mean * (float)(blk.texel_count);

				vfloat4 p0_total = new vfloat4(vfloat4.hadd_s(pp_avg_rgba[0, 0]),
										vfloat4.hadd_s(pp_avg_rgba[0, 1]),
										vfloat4.hadd_s(pp_avg_rgba[0, 2]),
										vfloat4.hadd_s(pp_avg_rgba[0, 3]));

				vfloat4 p1_total = new vfloat4(vfloat4.hadd_s(pp_avg_rgba[1, 0]),
										vfloat4.hadd_s(pp_avg_rgba[1, 1]),
										vfloat4.hadd_s(pp_avg_rgba[1, 2]),
										vfloat4.hadd_s(pp_avg_rgba[1, 3]));

				vfloat4 p2_total = new vfloat4(vfloat4.hadd_s(pp_avg_rgba[2, 0]),
										vfloat4.hadd_s(pp_avg_rgba[2, 1]),
										vfloat4.hadd_s(pp_avg_rgba[2, 2]),
										vfloat4.hadd_s(pp_avg_rgba[2, 3]));

				vfloat4 p3_total = block_total - p0_total - p1_total- p2_total;

				averages[0] = p0_total / (float)(pi.partition_texel_count[0]);
				averages[1] = p1_total / (float)(pi.partition_texel_count[1]);
				averages[2] = p2_total / (float)(pi.partition_texel_count[2]);
				averages[3] = p3_total / (float)(pi.partition_texel_count[3]);
			}
		}

		void compute_avgs_and_dirs_4_comp(PartitionInfo pi, ImageBlock blk, PartitionMetrics[] pm) 
		{
			int partition_count = pi.partition_count;
			//promise(partition_count > 0);

			// Pre-compute partition_averages
			vfloat4[] partition_averages = new vfloat4[Constants.BLOCK_MAX_PARTITIONS];
			compute_partition_averages_rgba(pi, blk, partition_averages);

			for (int partition = 0; partition < partition_count; partition++)
			{
				const uint8_t *texel_indexes = pi.texels_of_partition[partition];
				uint texel_count = pi.partition_texel_count[partition];
				//promise(texel_count > 0);

				vfloat4 average = partition_averages[partition];
				pm[partition].avg = average;

				vfloat4 sum_xp = vfloat4.zero();
				vfloat4 sum_yp = vfloat4.zero();
				vfloat4 sum_zp = vfloat4.zero();
				vfloat4 sum_wp = vfloat4.zero();

				for (uint i = 0; i < texel_count; i++)
				{
					uint iwt = texel_indexes[i];
					vfloat4 texel_datum = blk.Texel(iwt);
					texel_datum = texel_datum - average;

					vfloat4 zero = vfloat4.zero();

					vmask4 tdm0 = texel_datum.swz4(0,0,0,0) > zero;
					sum_xp += vfloat4.select(zero, texel_datum, tdm0);

					vmask4 tdm1 = texel_datum.swz4(1,1,1,1) > zero;
					sum_yp += vfloat4.select(zero, texel_datum, tdm1);

					vmask4 tdm2 = texel_datum.swz4(2,2,2,2) > zero;
					sum_zp += vfloat4.select(zero, texel_datum, tdm2);

					vmask4 tdm3 = texel_datum.swz4(3,3,3,3) > zero;
					sum_wp += vfloat4.select(zero, texel_datum, tdm3);
				}

				vfloat4 prod_xp = vfloat4.dot(sum_xp, sum_xp);
				vfloat4 prod_yp = vfloat4.dot(sum_yp, sum_yp);
				vfloat4 prod_zp = vfloat4.dot(sum_zp, sum_zp);
				vfloat4 prod_wp = vfloat4.dot(sum_wp, sum_wp);

				vfloat4 best_vector = sum_xp;
				vfloat4 best_sum = prod_xp;

				vmask4 mask = prod_yp > best_sum;
				best_vector = vfloat4.select(best_vector, sum_yp, mask);
				best_sum = vfloat4.select(best_sum, prod_yp, mask);

				mask = prod_zp > best_sum;
				best_vector = vfloat4.select(best_vector, sum_zp, mask);
				best_sum = vfloat4.select(best_sum, prod_zp, mask);

				mask = prod_wp > best_sum;
				best_vector = vfloat4.select(best_vector, sum_wp, mask);

				pm[partition].dir = best_vector;
			}
		}

		void compute_avgs_and_dirs_3_comp(PartitionInfo pi, ImageBlock blk, uint omitted_component, PartitionMetrics[] pm) 
		{
			// Pre-compute partition_averages
			vfloat4[] partition_averages = new vfloat4[Constants.BLOCK_MAX_PARTITIONS];
			compute_partition_averages_rgba(pi, blk, partition_averages);

			float[] data_vr = blk.data_r;
			float[] data_vg = blk.data_g;
			float[] data_vb = blk.data_b;

			// TODO: Data-driven permute would be useful to avoid this ...
			if (omitted_component == 0)
			{
				partition_averages[0] = partition_averages[0].swz3(1, 2, 3);
				partition_averages[1] = partition_averages[1].swz3(1, 2, 3);
				partition_averages[2] = partition_averages[2].swz3(1, 2, 3);
				partition_averages[3] = partition_averages[3].swz3(1, 2, 3);

				data_vr = blk.data_g;
				data_vg = blk.data_b;
				data_vb = blk.data_a;
			}
			else if (omitted_component == 1)
			{
				partition_averages[0] = partition_averages[0].swz3(0, 2, 3);
				partition_averages[1] = partition_averages[1].swz3(0, 2, 3);
				partition_averages[2] = partition_averages[2].swz3(0, 2, 3);
				partition_averages[3] = partition_averages[3].swz3(0, 2, 3);

				data_vg = blk.data_b;
				data_vb = blk.data_a;
			}
			else if (omitted_component == 2)
			{
				partition_averages[0] = partition_averages[0].swz3(0, 1, 3);
				partition_averages[1] = partition_averages[1].swz3(0, 1, 3);
				partition_averages[2] = partition_averages[2].swz3(0, 1, 3);
				partition_averages[3] = partition_averages[3].swz3(0, 1, 3);

				data_vb = blk.data_a;
			}
			else
			{
				partition_averages[0] = partition_averages[0].swz3(0, 1, 2);
				partition_averages[1] = partition_averages[1].swz3(0, 1, 2);
				partition_averages[2] = partition_averages[2].swz3(0, 1, 2);
				partition_averages[3] = partition_averages[3].swz3(0, 1, 2);
			}

			uint partition_count = pi.partition_count;
			//promise(partition_count > 0);

			for (uint partition = 0; partition < partition_count; partition++)
			{
				const uint8_t *texel_indexes = pi.texels_of_partition[partition];
				uint texel_count = pi.partition_texel_count[partition];
				//promise(texel_count > 0);

				vfloat4 average = partition_averages[partition];
				pm[partition].avg = average;

				vfloat4 sum_xp = vfloat4.zero();
				vfloat4 sum_yp = vfloat4.zero();
				vfloat4 sum_zp = vfloat4.zero();

				for (uint i = 0; i < texel_count; i++)
				{
					uint iwt = texel_indexes[i];

					vfloat4 texel_datum = vfloat3(data_vr[iwt],
												data_vg[iwt],
												data_vb[iwt]);
					texel_datum = texel_datum - average;

					vfloat4 zero = vfloat4.zero();

					vmask4 tdm0 = texel_datum.swz4(0,0,0,0) > zero;
					sum_xp += vfloat4.select(zero, texel_datum, tdm0);

					vmask4 tdm1 = texel_datum.swz4(1,1,1,1) > zero;
					sum_yp += vfloat4.select(zero, texel_datum, tdm1);

					vmask4 tdm2 = texel_datum.swz4(2,2,2,2) > zero;
					sum_zp += vfloat4.select(zero, texel_datum, tdm2);
				}

				vfloat4 prod_xp = vfloat4.dot(sum_xp, sum_xp);
				vfloat4 prod_yp = vfloat4.dot(sum_yp, sum_yp);
				vfloat4 prod_zp = vfloat4.dot(sum_zp, sum_zp);

				vfloat4 best_vector = sum_xp;
				vfloat4 best_sum = prod_xp;

				vmask4 mask = prod_yp > best_sum;
				best_vector = vfloat4.select(best_vector, sum_yp, mask);
				best_sum = vfloat4.select(best_sum, prod_yp, mask);

				mask = prod_zp > best_sum;
				best_vector = vfloat4.select(best_vector, sum_zp, mask);

				pm[partition].dir = best_vector;
			}
		}

		void compute_avgs_and_dirs_3_comp_rgb(PartitionInfo pi, ImageBlock blk, PartitionMetrics[] pm) 
		{
			uint partition_count = pi.partition_count;
			//promise(partition_count > 0);

			// Pre-compute partition_averages
			vfloat4[] partition_averages = new vfloat4[Constants.BLOCK_MAX_PARTITIONS];
			compute_partition_averages_rgb(pi, blk, partition_averages);

			for (uint partition = 0; partition < partition_count; partition++)
			{
				const uint8_t *texel_indexes = pi.texels_of_partition[partition];
				uint texel_count = pi.partition_texel_count[partition];
				//promise(texel_count > 0);

				vfloat4 average = partition_averages[partition];
				pm[partition].avg = average;

				vfloat4 sum_xp = vfloat4.zero();
				vfloat4 sum_yp = vfloat4.zero();
				vfloat4 sum_zp = vfloat4.zero();

				for (uint i = 0; i < texel_count; i++)
				{
					uint iwt = texel_indexes[i];

					vfloat4 texel_datum = blk.Texel3(iwt);
					texel_datum = texel_datum - average;

					vfloat4 zero = vfloat4.zero();

					vmask4 tdm0 = texel_datum.swz4(0,0,0,0) > zero;
					sum_xp += vfloat4.select(zero, texel_datum, tdm0);

					vmask4 tdm1 = texel_datum.swz4(1,1,1,1) > zero;
					sum_yp += vfloat4.select(zero, texel_datum, tdm1);

					vmask4 tdm2 = texel_datum.swz4(2,2,2,2) > zero;
					sum_zp += vfloat4.select(zero, texel_datum, tdm2);
				}

				vfloat4 prod_xp = vfloat4.dot(sum_xp, sum_xp);
				vfloat4 prod_yp = vfloat4.dot(sum_yp, sum_yp);
				vfloat4 prod_zp = vfloat4.dot(sum_zp, sum_zp);

				vfloat4 best_vector = sum_xp;
				vfloat4 best_sum = prod_xp;

				vmask4 mask = prod_yp > best_sum;
				best_vector = vfloat4.select(best_vector, sum_yp, mask);
				best_sum = vfloat4.select(best_sum, prod_yp, mask);

				mask = prod_zp > best_sum;
				best_vector = vfloat4.select(best_vector, sum_zp, mask);

				pm[partition].dir = best_vector;
			}
		}

		void compute_avgs_and_dirs_2_comp(PartitionInfo pi, ImageBlock blk, uint component1, uint component2, PartitionMetrics[] pm) 
		{
			vfloat4 average;

			float[] data_vr = null;
			float[] data_vg = null;

			if (component1 == 0 && component2 == 1)
			{
				average = blk.data_mean.swz2(0, 1);

				data_vr = blk.data_r;
				data_vg = blk.data_g;
			}
			else if (component1 == 0 && component2 == 2)
			{
				average = blk.data_mean.swz2(0, 2);

				data_vr = blk.data_r;
				data_vg = blk.data_b;
			}
			else // (component1 == 1 && component2 == 2)
			{
				//assert(component1 == 1 && component2 == 2);

				average = blk.data_mean.swz2(1, 2);

				data_vr = blk.data_g;
				data_vg = blk.data_b;
			}

			uint partition_count = pi.partition_count;
			//promise(partition_count > 0);

			for (uint partition = 0; partition < partition_count; partition++)
			{
				const uint8_t *texel_indexes = pi.texels_of_partition[partition];
				uint texel_count = pi.partition_texel_count[partition];
				//promise(texel_count > 0);

				// Only compute a partition mean if more than one partition
				if (partition_count > 1)
				{
					average = vfloat4.zero();
					for (uint i = 0; i < texel_count; i++)
					{
						uint iwt = texel_indexes[i];
						average += vfloat2(data_vr[iwt], data_vg[iwt]);
					}

					average = average / (float)(texel_count);
				}

				pm[partition].avg = average;

				vfloat4 sum_xp = vfloat4.zero();
				vfloat4 sum_yp = vfloat4.zero();

				for (uint i = 0; i < texel_count; i++)
				{
					uint iwt = texel_indexes[i];
					vfloat4 texel_datum = vfloat2(data_vr[iwt], data_vg[iwt]);
					texel_datum = texel_datum - average;

					vfloat4 zero = vfloat4.zero();

					vmask4 tdm0 = texel_datum.swz4(0,0,0,0) > zero;
					sum_xp += vfloat4.select(zero, texel_datum, tdm0);

					vmask4 tdm1 = texel_datum.swz4(1,1,1,1) > zero;
					sum_yp += vfloat4.select(zero, texel_datum, tdm1);
				}

				vfloat4 prod_xp = vfloat4.dot(sum_xp, sum_xp);
				vfloat4 prod_yp = vfloat4.dot(sum_yp, sum_yp);

				vfloat4 best_vector = sum_xp;
				vfloat4 best_sum = prod_xp;

				vmask4 mask = prod_yp > best_sum;
				best_vector = vfloat4.select(best_vector, sum_yp, mask);

				pm[partition].dir = best_vector;
			}
		}

		/* See header for documentation. */
		void compute_error_squared_rgba(PartitionInfo pi, ImageBlock blk, ProcessedLine4[] uncor_plines, ProcessedLine4[] samec_plines, float[] uncor_lengths, float[] samec_lengths, out float uncor_error, out float samec_error) 
		{
			uint partition_count = pi.partition_count;
			//promise(partition_count > 0);

			vfloatacc uncor_errorsumv = vfloatacc.zero();
			vfloatacc samec_errorsumv = vfloatacc.zero();

			for (uint partition = 0; partition < partition_count; partition++)
			{
				byte[] texel_indexes = pi.texels_of_partition[partition];

				float uncor_loparam = 1e10f;
				float uncor_hiparam = -1e10f;

				float samec_loparam = 1e10f;
				float samec_hiparam = -1e10f;

				ProcessedLine4 l_uncor = uncor_plines[partition];
				ProcessedLine4 l_samec = samec_plines[partition];

				uint texel_count = pi.partition_texel_count[partition];
				//promise(texel_count > 0);

				// Vectorize some useful scalar inputs
				vfloat l_uncor_bs0 = new vfloat(l_uncor.bs.lane(0));
				vfloat l_uncor_bs1 = new vfloat(l_uncor.bs.lane(1));
				vfloat l_uncor_bs2 = new vfloat(l_uncor.bs.lane(2));
				vfloat l_uncor_bs3 = new vfloat(l_uncor.bs.lane<3>());

				vfloat l_uncor_amod0 = new vfloat(l_uncor.amod.lane(0));
				vfloat l_uncor_amod1 = new vfloat(l_uncor.amod.lane(1));
				vfloat l_uncor_amod2 = new vfloat(l_uncor.amod.lane(2));
				vfloat l_uncor_amod3 = new vfloat(l_uncor.amod.lane<3>());

				vfloat l_samec_bs0 = new vfloat(l_samec.bs.lane(0));
				vfloat l_samec_bs1 = new vfloat(l_samec.bs.lane(1));
				vfloat l_samec_bs2 = new vfloat(l_samec.bs.lane(2));
				vfloat l_samec_bs3 = new vfloat(l_samec.bs.lane<3>());

				//assert(all(l_samec.amod == vfloat4(0.0f)));

				vfloat uncor_loparamv = new vfloat(1e10f);
				vfloat uncor_hiparamv = new vfloat(-1e10f);

				vfloat samec_loparamv = new vfloat(1e10f);
				vfloat samec_hiparamv = new vfloat(-1e10f);

				vfloat ew_r = new vfloat(blk.channel_weight.lane(0));
				vfloat ew_g = new vfloat(blk.channel_weight.lane(1));
				vfloat ew_b = new vfloat(blk.channel_weight.lane(2));
				vfloat ew_a = new vfloat(blk.channel_weight.lane<3>());

				// This implementation over-shoots, but this is safe as we initialize the texel_indexes
				// array to extend the last value. This means min/max are not impacted, but we need to mask
				// out the dummy values when we compute the line weighting.
				vint lane_ids = vint.lane_id();
				for (uint i = 0; i < texel_count; i += Constants.ASTCENC_SIMD_WIDTH)
				{
					vmask mask = lane_ids < new vint((int)texel_count);
					vint texel_idxs = new vint(texel_indexes, i);

					vfloat data_r = gatherf(blk.data_r, texel_idxs);
					vfloat data_g = gatherf(blk.data_g, texel_idxs);
					vfloat data_b = gatherf(blk.data_b, texel_idxs);
					vfloat data_a = gatherf(blk.data_a, texel_idxs);

					vfloat uncor_param = (data_r * l_uncor_bs0)
									+ (data_g * l_uncor_bs1)
									+ (data_b * l_uncor_bs2)
									+ (data_a * l_uncor_bs3);

					uncor_loparamv = min(uncor_param, uncor_loparamv);
					uncor_hiparamv = max(uncor_param, uncor_hiparamv);

					vfloat uncor_dist0 = (l_uncor_amod0 - data_r)
									+ (uncor_param * l_uncor_bs0);
					vfloat uncor_dist1 = (l_uncor_amod1 - data_g)
									+ (uncor_param * l_uncor_bs1);
					vfloat uncor_dist2 = (l_uncor_amod2 - data_b)
									+ (uncor_param * l_uncor_bs2);
					vfloat uncor_dist3 = (l_uncor_amod3 - data_a)
									+ (uncor_param * l_uncor_bs3);

					vfloat uncor_err = (ew_r * uncor_dist0 * uncor_dist0)
									+ (ew_g * uncor_dist1 * uncor_dist1)
									+ (ew_b * uncor_dist2 * uncor_dist2)
									+ (ew_a * uncor_dist3 * uncor_dist3);

					haccumulate(uncor_errorsumv, uncor_err, mask);

					// Process samechroma data
					vfloat samec_param = (data_r * l_samec_bs0)
									+ (data_g * l_samec_bs1)
									+ (data_b * l_samec_bs2)
									+ (data_a * l_samec_bs3);

					samec_loparamv = min(samec_param, samec_loparamv);
					samec_hiparamv = max(samec_param, samec_hiparamv);

					vfloat samec_dist0 = samec_param * l_samec_bs0 - data_r;
					vfloat samec_dist1 = samec_param * l_samec_bs1 - data_g;
					vfloat samec_dist2 = samec_param * l_samec_bs2 - data_b;
					vfloat samec_dist3 = samec_param * l_samec_bs3 - data_a;

					vfloat samec_err = (ew_r * samec_dist0 * samec_dist0)
									+ (ew_g * samec_dist1 * samec_dist1)
									+ (ew_b * samec_dist2 * samec_dist2)
									+ (ew_a * samec_dist3 * samec_dist3);

					vfloatacc.haccumulate(samec_errorsumv, samec_err, mask);

					lane_ids += new vint(Constants.ASTCENC_SIMD_WIDTH);
				}

				uncor_loparam = hmin_s(uncor_loparamv);
				uncor_hiparam = hmax_s(uncor_hiparamv);

				samec_loparam = hmin_s(samec_loparamv);
				samec_hiparam = hmax_s(samec_hiparamv);

				float uncor_linelen = uncor_hiparam - uncor_loparam;
				float samec_linelen = samec_hiparam - samec_loparam;

				// Turn very small numbers and NaNs into a small number
				uncor_lengths[partition] = astc::max(uncor_linelen, 1e-7f);
				samec_lengths[partition] = astc::max(samec_linelen, 1e-7f);
			}

			uncor_error = hadd_s(uncor_errorsumv);
			samec_error = hadd_s(samec_errorsumv);
		}

		/* See header for documentation. */
		void compute_error_squared_rgb(PartitionInfo pi, ImageBlock blk, PartitionLines3[] plines, out float uncor_error, out float samec_error) 
		{
			uint partition_count = pi.partition_count;
			//promise(partition_count > 0);

			vfloatacc uncor_errorsumv = vfloatacc.zero();
			vfloatacc samec_errorsumv = vfloatacc.zero();

			for (uint partition = 0; partition < partition_count; partition++)
			{
				PartitionLines3 pl = plines[partition];
				byte[] texel_indexes = pi.texels_of_partition[partition];
				uint texel_count = pi.partition_texel_count[partition];
				//promise(texel_count > 0);

				float uncor_loparam = 1e10f;
				float uncor_hiparam = -1e10f;

				float samec_loparam = 1e10f;
				float samec_hiparam = -1e10f;

				ProcessedLine3 l_uncor = pl.uncor_pline;
				ProcessedLine3 l_samec = pl.samec_pline;

				// This implementation is an example vectorization of this function.
				// It works for - the codec is a 2-4% faster than not vectorizing - but
				// the benefit is limited by the use of gathers and register pressure

				// Vectorize some useful scalar inputs
				vfloat l_uncor_bs0 = new vfloat(l_uncor.bs.lane(0));
				vfloat l_uncor_bs1 = new vfloat(l_uncor.bs.lane(1));
				vfloat l_uncor_bs2 = new vfloat(l_uncor.bs.lane(2));

				vfloat l_uncor_amod0 = new vfloat(l_uncor.amod.lane(0));
				vfloat l_uncor_amod1 = new vfloat(l_uncor.amod.lane(1));
				vfloat l_uncor_amod2 = new vfloat(l_uncor.amod.lane(2));

				vfloat l_samec_bs0 = new vfloat(l_samec.bs.lane(0));
				vfloat l_samec_bs1 = new vfloat(l_samec.bs.lane(1));
				vfloat l_samec_bs2 = new vfloat(l_samec.bs.lane(2));

				//assert(all(l_samec.amod == vfloat4(0.0f)));

				vfloat uncor_loparamv = new vfloat(1e10f);
				vfloat uncor_hiparamv = new vfloat(-1e10f);

				vfloat samec_loparamv = new vfloat(1e10f);
				vfloat samec_hiparamv = new vfloat(-1e10f);

				vfloat ew_r = new vfloat(blk.channel_weight.lane(0));
				vfloat ew_g = new vfloat(blk.channel_weight.lane(1));
				vfloat ew_b = new vfloat(blk.channel_weight.lane(2));

				// This implementation over-shoots, but this is safe as we initialize the weights array
				// to extend the last value. This means min/max are not impacted, but we need to mask
				// out the dummy values when we compute the line weighting.
				vint lane_ids = vint.lane_id();
				for (uint i = 0; i < texel_count; i += Constants.ASTCENC_SIMD_WIDTH)
				{
					vmask mask = lane_ids < vint(texel_count);
					vint texel_idxs = new vint(texel_indexes, i);

					vfloat data_r = gatherf(blk.data_r, texel_idxs);
					vfloat data_g = gatherf(blk.data_g, texel_idxs);
					vfloat data_b = gatherf(blk.data_b, texel_idxs);

					vfloat uncor_param = (data_r * l_uncor_bs0)
									+ (data_g * l_uncor_bs1)
									+ (data_b * l_uncor_bs2);

					uncor_loparamv = min(uncor_param, uncor_loparamv);
					uncor_hiparamv = max(uncor_param, uncor_hiparamv);

					vfloat uncor_dist0 = (l_uncor_amod0 - data_r)
									+ (uncor_param * l_uncor_bs0);
					vfloat uncor_dist1 = (l_uncor_amod1 - data_g)
									+ (uncor_param * l_uncor_bs1);
					vfloat uncor_dist2 = (l_uncor_amod2 - data_b)
									+ (uncor_param * l_uncor_bs2);

					vfloat uncor_err = (ew_r * uncor_dist0 * uncor_dist0)
									+ (ew_g * uncor_dist1 * uncor_dist1)
									+ (ew_b * uncor_dist2 * uncor_dist2);

					haccumulate(uncor_errorsumv, uncor_err, mask);

					// Process samechroma data
					vfloat samec_param = (data_r * l_samec_bs0)
									+ (data_g * l_samec_bs1)
									+ (data_b * l_samec_bs2);

					samec_loparamv = min(samec_param, samec_loparamv);
					samec_hiparamv = max(samec_param, samec_hiparamv);

					vfloat samec_dist0 = samec_param * l_samec_bs0 - data_r;
					vfloat samec_dist1 = samec_param * l_samec_bs1 - data_g;
					vfloat samec_dist2 = samec_param * l_samec_bs2 - data_b;

					vfloat samec_err = (ew_r * samec_dist0 * samec_dist0)
									+ (ew_g * samec_dist1 * samec_dist1)
									+ (ew_b * samec_dist2 * samec_dist2);

					haccumulate(samec_errorsumv, samec_err, mask);

					lane_ids += new vint(Constants.ASTCENC_SIMD_WIDTH);
				}

				uncor_loparam = hmin_s(uncor_loparamv);
				uncor_hiparam = hmax_s(uncor_hiparamv);

				samec_loparam = hmin_s(samec_loparamv);
				samec_hiparam = hmax_s(samec_hiparamv);

				float uncor_linelen = uncor_hiparam - uncor_loparam;
				float samec_linelen = samec_hiparam - samec_loparam;

				// Turn very small numbers and NaNs into a small number
				pl.uncor_line_len = astc::max(uncor_linelen, 1e-7f);
				pl.samec_line_len = astc::max(samec_linelen, 1e-7f);
			}

			uncor_error = hadd_s(uncor_errorsumv);
			samec_error = hadd_s(samec_errorsumv);
		}
	}
}