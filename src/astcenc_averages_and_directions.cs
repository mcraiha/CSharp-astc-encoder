
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

		void compute_avgs_and_dirs_4_comp(PartitionInfo pi, ImageBlock blk, partition_metrics pm[BLOCK_MAX_PARTITIONS]) 
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
					vfloat4 texel_datum = blk.texel(iwt);
					texel_datum = texel_datum - average;

					vfloat4 zero = vfloat4.zero();

					vmask4 tdm0 = texel_datum.swz<0,0,0,0>() > zero;
					sum_xp += select(zero, texel_datum, tdm0);

					vmask4 tdm1 = texel_datum.swz<1,1,1,1>() > zero;
					sum_yp += select(zero, texel_datum, tdm1);

					vmask4 tdm2 = texel_datum.swz<2,2,2,2>() > zero;
					sum_zp += select(zero, texel_datum, tdm2);

					vmask4 tdm3 = texel_datum.swz<3,3,3,3>() > zero;
					sum_wp += select(zero, texel_datum, tdm3);
				}

				vfloat4 prod_xp = vfloat4.dot(sum_xp, sum_xp);
				vfloat4 prod_yp = vfloat4.dot(sum_yp, sum_yp);
				vfloat4 prod_zp = vfloat4.dot(sum_zp, sum_zp);
				vfloat4 prod_wp = vfloat4.dot(sum_wp, sum_wp);

				vfloat4 best_vector = sum_xp;
				vfloat4 best_sum = prod_xp;

				vmask4 mask = prod_yp > best_sum;
				best_vector = select(best_vector, sum_yp, mask);
				best_sum = select(best_sum, prod_yp, mask);

				mask = prod_zp > best_sum;
				best_vector = select(best_vector, sum_zp, mask);
				best_sum = select(best_sum, prod_zp, mask);

				mask = prod_wp > best_sum;
				best_vector = select(best_vector, sum_wp, mask);

				pm[partition].dir = best_vector;
			}
		}
	}
}