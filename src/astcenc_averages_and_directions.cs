
namespace ASTCEnc
{
	public static class AveragesAndDirections
	{
		public static void compute_partition_averages_rgb<Vint>(PartitionInfo pi, ImageBlock blk, vfloat4[] averages)
		{
			uint partition_count = pi.partition_count;
			uint texel_count = blk.texel_count;
			//promise(texel_count > 0);

			// For 1 partition just use the precomputed mean
			if (partition_count == 1)
			{
				averages[0] = blk.data_mean.swz<0, 1, 2>();
			}
			// For 2 partitions scan results for partition 0, compute partition 1
			else if (partition_count == 2)
			{
				vfloatacc[] pp_avg_rgb = new vfloatacc[3];

				vint lane_id = vint.lane_id();
				for (uint i = 0; i < texel_count; i += Constants.ASTCENC_SIMD_WIDTH)
				{
					vint texel_partition(pi.partition_of_texel + i);

					vmask lane_mask = lane_id < vint(texel_count);
					lane_id += vint(ASTCENC_SIMD_WIDTH);

					vmask p0_mask = lane_mask & (texel_partition == new vint(0));

					vfloat data_r = loada(blk.data_r + i);
					vfloatacc.haccumulate(pp_avg_rgb[0], data_r, p0_mask);

					vfloat data_g = loada(blk.data_g + i);
					vfloatacc.haccumulate(pp_avg_rgb[1], data_g, p0_mask);

					vfloat data_b = loada(blk.data_b + i);
					vfloatacc.haccumulate(pp_avg_rgb[2], data_b, p0_mask);
				}

				vfloat4 block_total = blk.data_mean.swz<0, 1, 2>() * (float)(blk.texel_count);

				vfloat4 p0_total = new vfloat3(vfloatacc.hadd_s(pp_avg_rgb[0]),
										vfloatacc.hadd_s(pp_avg_rgb[1]),
										vfloatacc.hadd_s(pp_avg_rgb[2]));

				vfloat4 p1_total = block_total - p0_total;

				averages[0] = p0_total / (float)(pi.partition_texel_count[0]);
				averages[1] = p1_total / (float)(pi.partition_texel_count[1]);
			}
			// For 3 partitions scan results for partition 0/1, compute partition 2
			else if (partition_count == 3)
			{
				vfloatacc pp_avg_rgb[2][3] {};

				vint lane_id = vint.lane_id();
				for (uint i = 0; i < texel_count; i += Constants.ASTCENC_SIMD_WIDTH)
				{
					vint texel_partition = new vint(pi.partition_of_texel + i);

					vmask lane_mask = lane_id < vint(texel_count);
					lane_id += vint(ASTCENC_SIMD_WIDTH);

					vmask p0_mask = lane_mask & (texel_partition == new vint(0));
					vmask p1_mask = lane_mask & (texel_partition == new vint(1));

					vfloat data_r = loada(blk.data_r + i);
					vfloatacc.haccumulate(pp_avg_rgb[0][0], data_r, p0_mask);
					vfloatacc.haccumulate(pp_avg_rgb[1][0], data_r, p1_mask);

					vfloat data_g = loada(blk.data_g + i);
					vfloatacc.haccumulate(pp_avg_rgb[0][1], data_g, p0_mask);
					vfloatacc.haccumulate(pp_avg_rgb[1][1], data_g, p1_mask);

					vfloat data_b = loada(blk.data_b + i);
					vfloatacc.haccumulate(pp_avg_rgb[0][2], data_b, p0_mask);
					vfloatacc.haccumulate(pp_avg_rgb[1][2], data_b, p1_mask);
				}

				vfloat4 block_total = blk.data_mean.swz<0, 1, 2>() * static_cast<float>(blk.texel_count);

				vfloat4 p0_total = vfloat3(hadd_s(pp_avg_rgb[0][0]),
										hadd_s(pp_avg_rgb[0][1]),
										hadd_s(pp_avg_rgb[0][2]));

				vfloat4 p1_total = vfloat3(hadd_s(pp_avg_rgb[1][0]),
										hadd_s(pp_avg_rgb[1][1]),
										hadd_s(pp_avg_rgb[1][2]));

				vfloat4 p2_total = block_total - p0_total - p1_total;

				averages[0] = p0_total / static_cast<float>(pi.partition_texel_count[0]);
				averages[1] = p1_total / static_cast<float>(pi.partition_texel_count[1]);
				averages[2] = p2_total / static_cast<float>(pi.partition_texel_count[2]);
			}
			else
			{
				// For 4 partitions scan results for partition 0/1/2, compute partition 3
				vfloatacc[,] pp_avg_rgb = new vfloat[3, 3];

				vint lane_id = vint.lane_id();
				for (uint i = 0; i < texel_count; i += Constants.ASTCENC_SIMD_WIDTH)
				{
					vint texel_partition(pi.partition_of_texel + i);

					vmask lane_mask = lane_id < vint(texel_count);
					lane_id += new vint(Constants.ASTCENC_SIMD_WIDTH);

					vmask p0_mask = lane_mask & (texel_partition == new vint(0));
					vmask p1_mask = lane_mask & (texel_partition == new vint(1));
					vmask p2_mask = lane_mask & (texel_partition == new vint(2));

					vfloat data_r = loada(blk.data_r + i);
					vfloatacc.haccumulate(pp_avg_rgb[0, 0], data_r, p0_mask);
					vfloatacc.haccumulate(pp_avg_rgb[1, 0], data_r, p1_mask);
					vfloatacc.haccumulate(pp_avg_rgb[2, 0], data_r, p2_mask);

					vfloat data_g = loada(blk.data_g + i);
					vfloatacc.haccumulate(pp_avg_rgb[0, 1], data_g, p0_mask);
					vfloatacc.haccumulate(pp_avg_rgb[1, 1], data_g, p1_mask);
					vfloatacc.haccumulate(pp_avg_rgb[2, 1], data_g, p2_mask);

					vfloat data_b = loada(blk.data_b + i);
					vfloatacc.haccumulate(pp_avg_rgb[0, 2], data_b, p0_mask);
					vfloatacc.haccumulate(pp_avg_rgb[1, 2], data_b, p1_mask);
					vfloatacc.haccumulate(pp_avg_rgb[2, 2], data_b, p2_mask);
				}

				vfloat4 block_total = blk.data_mean.swz<0, 1, 2>() * (float)(blk.texel_count);

				vfloat4 p0_total = new vfloat3(hadd_s(pp_avg_rgb[0, 0]),
										hadd_s(pp_avg_rgb[0, 1]),
										hadd_s(pp_avg_rgb[0, 2]));

				vfloat4 p1_total = new vfloat3(hadd_s(pp_avg_rgb[1, 0]),
										hadd_s(pp_avg_rgb[1, 1]),
										hadd_s(pp_avg_rgb[1, 2]));

				vfloat4 p2_total = new vfloat3(hadd_s(pp_avg_rgb[2, 0]),
										hadd_s(pp_avg_rgb[2, 1]),
										hadd_s(pp_avg_rgb[2, 2]));

				vfloat4 p3_total = block_total - p0_total - p1_total- p2_total;

				averages[0] = p0_total / static_cast<float>(pi.partition_texel_count[0]);
				averages[1] = p1_total / static_cast<float>(pi.partition_texel_count[1]);
				averages[2] = p2_total / static_cast<float>(pi.partition_texel_count[2]);
				averages[3] = p3_total / static_cast<float>(pi.partition_texel_count[3]);
			}
		}

		public static void compute_averages_and_directions_rgb(PartitionInfo pt, ImageBlock blk, ErrorWeightBlock ewb, Float4[] color_scalefactors, Float3[] averages, Float3[] directions_rgb) 
		{
			float[] texel_weights = ewb.texel_weight_rgb;

			int partition_count = pt.partition_count;
			promise(partition_count > 0);

			for (int partition = 0; partition < partition_count; partition++)
			{
				byte[] weights = pt.texels_of_partition[partition];

				Float3 base_sum = new Float3(0.0f, 0.0f, 0.0f);
				float partition_weight = 0.0f;

				int texel_count = pt.texels_per_partition[partition];
				promise(texel_count > 0);

				for (int i = 0; i < texel_count; i++)
				{
					int iwt = weights[i];
					float weight = texel_weights[iwt];
					Float3 texel_datum = new Float3(blk->data_r[iwt],
												blk->data_g[iwt],
												blk->data_b[iwt]) * weight;
					partition_weight += weight;

					base_sum = base_sum + texel_datum;
				}

				Float4 csf = color_scalefactors[partition];
				Float3 average = base_sum * (1.0f / astc::max(partition_weight, 1e-7f));
				averages[partition] = average * new Float3(csf.r, csf.g, csf.b);

				Float3 sum_xp = new float3(0.0f);
				Float3 sum_yp = new float3(0.0f);
				Float3 sum_zp = new float3(0.0f);

				for (int i = 0; i < texel_count; i++)
				{
					int iwt = weights[i];
					float weight = texel_weights[iwt];
					Float3 texel_datum = new Float3(blk->data_r[iwt],
												blk->data_g[iwt],
												blk->data_b[iwt]);
					texel_datum = (texel_datum - average) * weight;

					if (texel_datum.r > 0.0f)
					{
						sum_xp = sum_xp + texel_datum;
					}

					if (texel_datum.g > 0.0f)
					{
						sum_yp = sum_yp + texel_datum;
					}

					if (texel_datum.b > 0.0f)
					{
						sum_zp = sum_zp + texel_datum;
					}
				}

				float prod_xp = dot(sum_xp, sum_xp);
				float prod_yp = dot(sum_yp, sum_yp);
				float prod_zp = dot(sum_zp, sum_zp);

				Float3 best_vector = sum_xp;
				float best_sum = prod_xp;

				if (prod_yp > best_sum)
				{
					best_vector = sum_yp;
					best_sum = prod_yp;
				}

				if (prod_zp > best_sum)
				{
					best_vector = sum_zp;
				}

				directions_rgb[partition] = best_vector;
			}
		}

		// For a full block, functions to compute averages and dominant directions. The
		// averages and directions are computed separately for each partition.
		// We have separate versions for blocks with and without alpha, since the
		// processing for blocks with alpha is significantly more expensive. The
		// direction vectors it produces are NOT normalized.
		public static void compute_averages_and_directions_rgba(PartitionInfo pt, ImageBlock blk, ErrorWeightBlock ewb, Float4[] color_scalefactors, Float4[] averages, Float4[] directions_rgba) 
		{
			int partition_count = pt.partition_count;
			promise(partition_count > 0);

			for (int partition = 0; partition < partition_count; partition++)
			{
				byte[] weights = pt.texels_of_partition[partition];

				Float4 base_sum = new Float4(0.0f);
				float partition_weight = 0.0f;

				int texel_count = pt.texels_per_partition[partition];
				promise(texel_count > 0);

				for (int i = 0; i < texel_count; i++)
				{
					int iwt = weights[i];
					float weight = ewb.texel_weight[iwt];
					Float4 texel_datum = new Float4(blk.data_r[iwt],
												blk.data_g[iwt],
												blk.data_b[iwt],
												blk.data_a[iwt]) * weight;
					partition_weight += weight;

					base_sum = base_sum + texel_datum;
				}

				Float4 average = base_sum * (1.0f / astc::max(partition_weight, 1e-7f));
				averages[partition] = average * color_scalefactors[partition];

				Float4 sum_xp = new Float4(0.0f);
				Float4 sum_yp = new Float4(0.0f);
				Float4 sum_zp = new Float4(0.0f);
				Float4 sum_wp = new Float4(0.0f);

				for (int i = 0; i < texel_count; i++)
				{
					int iwt = weights[i];
					float weight = ewb.texel_weight[iwt];
					Float4 texel_datum = Float4(blk.data_r[iwt],
												blk.data_g[iwt],
												blk.data_b[iwt],
												blk.data_a[iwt]);
					texel_datum = (texel_datum - average) * weight;

					if (texel_datum.r > 0.0f)
					{
						sum_xp = sum_xp + texel_datum;
					}

					if (texel_datum.g > 0.0f)
					{
						sum_yp = sum_yp + texel_datum;
					}

					if (texel_datum.b > 0.0f)
					{
						sum_zp = sum_zp + texel_datum;
					}

					if (texel_datum.a > 0.0f)
					{
						sum_wp = sum_wp + texel_datum;
					}
				}

				float prod_xp = dot(sum_xp, sum_xp);
				float prod_yp = dot(sum_yp, sum_yp);
				float prod_zp = dot(sum_zp, sum_zp);
				float prod_wp = dot(sum_wp, sum_wp);

				Float4 best_vector = sum_xp;
				float best_sum = prod_xp;

				if (prod_yp > best_sum)
				{
					best_vector = sum_yp;
					best_sum = prod_yp;
				}

				if (prod_zp > best_sum)
				{
					best_vector = sum_zp;
					best_sum = prod_zp;
				}

				if (prod_wp > best_sum)
				{
					best_vector = sum_wp;
				}

				directions_rgba[partition] = best_vector;
			}
		}

		

		public static void compute_averages_and_directions_3_components(PartitionInfo pt, ImageBlock blk, ErrorWeightBlock ewb, Float3[] color_scalefactors, int omitted_component, Float3[] averages, Float3[] directions) 
		{
			float[] texel_weights;
			float[] data_vr;
			float[] data_vg;
			float[] data_vb;

			if (omitted_component == 0)
			{
				texel_weights = ewb.texel_weight_gba;
				data_vr = blk.data_g;
				data_vg = blk.data_b;
				data_vb = blk.data_a;
			}
			else if (omitted_component == 1)
			{
				texel_weights = ewb.texel_weight_rba;
				data_vr = blk.data_r;
				data_vg = blk.data_b;
				data_vb = blk.data_a;
			}
			else if (omitted_component == 2)
			{
				texel_weights = ewb.texel_weight_rga;
				data_vr = blk.data_r;
				data_vg = blk.data_g;
				data_vb = blk.data_a;
			}
			else
			{
				assert(omitted_component == 3);
				texel_weights = ewb.texel_weight_rgb;
				data_vr = blk.data_r;
				data_vg = blk.data_g;
				data_vb = blk.data_b;
			}

			int partition_count = pt.partition_count;
			promise(partition_count > 0);

			for (int partition = 0; partition < partition_count; partition++)
			{
				byte[] weights = pt.texels_of_partition[partition];

				Float3 base_sum = new Float3(0.0f);
				float partition_weight = 0.0f;

				int texel_count = pt.texels_per_partition[partition];
				promise(texel_count > 0);

				for (int i = 0; i < texel_count; i++)
				{
					int iwt = weights[i];
					float weight = texel_weights[iwt];
					Float3 texel_datum = new Float3(data_vr[iwt],
												data_vg[iwt],
												data_vb[iwt]) * weight;
					partition_weight += weight;

					base_sum = base_sum + texel_datum;
				}

				Float3 csf = color_scalefactors[partition];

				Float3 average = base_sum * (1.0f / astc::max(partition_weight, 1e-7f));
				averages[partition] = average * new Float3(csf.r, csf.g, csf.b);

				Float3 sum_xp = new Float3(0.0f);
				Float3 sum_yp = new Float3(0.0f);
				Float3 sum_zp = new Float3(0.0f);

				for (int i = 0; i < texel_count; i++)
				{
					int iwt = weights[i];
					float weight = texel_weights[iwt];
					Float3 texel_datum = new Float3(data_vr[iwt],
												data_vg[iwt],
												data_vb[iwt]);
					texel_datum = (texel_datum - average) * weight;

					if (texel_datum.r > 0.0f)
					{
						sum_xp = sum_xp + texel_datum;
					}

					if (texel_datum.g > 0.0f)
					{
						sum_yp = sum_yp + texel_datum;
					}

					if (texel_datum.b > 0.0f)
					{
						sum_zp = sum_zp + texel_datum;
					}
				}

				float prod_xp = dot(sum_xp, sum_xp);
				float prod_yp = dot(sum_yp, sum_yp);
				float prod_zp = dot(sum_zp, sum_zp);

				Float3 best_vector = sum_xp;
				float best_sum = prod_xp;

				if (prod_yp > best_sum)
				{
					best_vector = sum_yp;
					best_sum = prod_yp;
				}

				if (prod_zp > best_sum)
				{
					best_vector = sum_zp;
				}

				if (dot(best_vector, best_vector) < 1e-18f)
				{
					best_vector = new float3(1.0f, 1.0f, 1.0f);
				}

				directions[partition] = best_vector;
			}
		}

		public static void compute_averages_and_directions_2_components(PartitionInfo pt, Imageblock blk, ErrorWeightBlock ewb, Float2[] color_scalefactors, int component1, int component2, Float2[] averages, Float2[] directions) 
		{
			float[] texel_weights;
			float[] data_vr = nullptr;
			float[] data_vg = nullptr;

			if (component1 == 0 && component2 == 1)
			{
				texel_weights = ewb.texel_weight_rg;
				data_vr = blk.data_r;
				data_vg = blk.data_g;
			}
			else if (component1 == 0 && component2 == 2)
			{
				texel_weights = ewb.texel_weight_rb;
				data_vr = blk.data_r;
				data_vg = blk.data_b;
			}
			else // (component1 == 1 && component2 == 2)
			{
				assert(component1 == 1 && component2 == 2);
				texel_weights = ewb.texel_weight_gb;
				data_vr = blk.data_g;
				data_vg = blk.data_b;
			}

			int partition_count = pt.partition_count;
			promise(partition_count > 0);

			for (int partition = 0; partition < partition_count; partition++)
			{
				byte[] weights = pt.texels_of_partition[partition];

				Float2 base_sum = new Float2(0.0f);
				float partition_weight = 0.0f;

				int texel_count = pt.texels_per_partition[partition];
				promise(texel_count > 0);

				for (int i = 0; i < texel_count; i++)
				{
					int iwt = weights[i];
					float weight = texel_weights[iwt];
					Float2 texel_datum = new Float2(data_vr[iwt], data_vg[iwt]) * weight;
					partition_weight += weight;

					base_sum = base_sum + texel_datum;
				}

				Float2 csf = color_scalefactors[partition];

				Float2 average = base_sum * (1.0f / astc::max(partition_weight, 1e-7f));
				averages[partition] = average * new Float2(csf.r, csf.g);

				Float2 sum_xp = new Float2(0.0f);
				Float2 sum_yp = new Float2(0.0f);

				for (int i = 0; i < texel_count; i++)
				{
					int iwt = weights[i];
					float weight = texel_weights[iwt];
					Float2 texel_datum = new Float2(data_vr[iwt], data_vg[iwt]);
					texel_datum = (texel_datum - average) * weight;

					if (texel_datum.r > 0.0f)
					{
						sum_xp = sum_xp + texel_datum;
					}

					if (texel_datum.g > 0.0f)
					{
						sum_yp = sum_yp + texel_datum;
					}
				}

				float prod_xp = Float2.dot(sum_xp, sum_xp);
				float prod_yp = Float2.dot(sum_yp, sum_yp);

				Float2 best_vector = sum_xp;
				float best_sum = prod_xp;

				if (prod_yp > best_sum)
				{
					best_vector = sum_yp;
				}

				directions[partition] = best_vector;
			}
		}

		public static void compute_error_squared_rgba(PartitionInfo pt, ImageBlock blk, ErrorWeightBlock ewb, ProcessedLine4[] uncor_plines, ProcessedLine4[] samec_plines, float[] uncor_lengths, float[] samec_lengths, float[] uncor_errors, float[] samec_errors) 
		{
			float uncor_errorsum = 0.0f;
			float samec_errorsum = 0.0f;

			int partition_count = pt.partition_count;
			promise(partition_count > 0);

			for (int partition = 0; partition < partition_count; partition++)
			{
				// TODO: sort partitions by number of texels. For warp-architectures,
				// this can reduce the running time by about 25-50%.
				byte[] weights = pt.texels_of_partition[partition];

				float uncor_loparam = 1e10f;
				float uncor_hiparam = -1e10f;

				float samec_loparam = 1e10f;
				float samec_hiparam = -1e10f;

				ProcessedLine4 l_uncor = uncor_plines[partition];
				ProcessedLine4 l_samec = samec_plines[partition];

				// TODO: split up this loop due to too many temporaries; in particular,
				// the six line functions will consume 18 vector registers
				int texel_count = pt.texels_per_partition[partition];
				promise(texel_count > 0);

				int i = 0;

				// Loop tail
				for (/* */; i < texel_count; i++)
				{
					int iwt = weights[i];

					Float4 dat = new Float4(blk.data_r[iwt],
										blk.data_g[iwt],
										blk.data_b[iwt],
										blk.data_a[iwt]);

					Float4 ews = ewb.error_weights[iwt];

					float uncor_param = Float4.dot(dat, l_uncor.bs);
					uncor_loparam = astc::min(uncor_param, uncor_loparam);
					uncor_hiparam = astc::max(uncor_param, uncor_hiparam);

					float samec_param = Float4.dot(dat, l_samec.bs);
					samec_loparam = astc::min(samec_param, samec_loparam);
					samec_hiparam = astc::max(samec_param, samec_hiparam);

					Float4 uncor_dist  = (l_uncor.amod - dat)
									+ (uncor_param * l_uncor.bis);
					uncor_errorsum += Float4.dot(ews, uncor_dist * uncor_dist);

					Float4 samec_dist = (l_samec.amod - dat)
									+ (samec_param * l_samec.bis);
					samec_errorsum += Float4.dot(ews, samec_dist * samec_dist);
				}

				float uncor_linelen = uncor_hiparam - uncor_loparam;
				float samec_linelen = samec_hiparam - samec_loparam;

				// Turn very small numbers and NaNs into a small number
				uncor_linelen = astc::max(uncor_linelen, 1e-7f);
				samec_linelen = astc::max(samec_linelen, 1e-7f);

				uncor_lengths[partition] = uncor_linelen;
				samec_lengths[partition] = samec_linelen;
			}

			uncor_errors = uncor_errorsum;
			samec_errors = samec_errorsum;
		}

		public static void compute_error_squared_rgb(PartitionInfo pt, ImageBlock blk, ErrorWeightBlock ewb, ProcessedLine3[] uncor_plines, ProcessedLine3[] samec_plines, float[] uncor_lengths, float[] samec_lengths, float[] uncor_errors, float[] samec_errors) 
		{
			float uncor_errorsum = 0.0f;
			float samec_errorsum = 0.0f;

			int partition_count = pt.partition_count;
			promise(partition_count > 0);

			for (int partition = 0; partition < partition_count; partition++)
			{
				byte[] weights = pt.texels_of_partition[partition];

				float uncor_loparam = 1e10f;
				float uncor_hiparam = -1e10f;

				float samec_loparam = 1e10f;
				float samec_hiparam = -1e10f;

				ProcessedLine3 l_uncor = uncor_plines[partition];
				ProcessedLine3 l_samec = samec_plines[partition];

				// TODO: split up this loop due to too many temporaries; in
				// particular, the six line functions will consume 18 vector registers
				int texel_count = pt.texels_per_partition[partition];
				promise(texel_count > 0);

				int i = 0;

				// Loop tail
				for (/* */; i < texel_count; i++)
				{
					int iwt = weights[i];

					Float3 dat = new Float3(blk.data_r[iwt],
										blk.data_g[iwt],
										blk.data_b[iwt]);

					Float3 ews = new Float3(ewb.error_weights[iwt].r,
										ewb.error_weights[iwt].g,
										ewb.error_weights[iwt].b);

					float uncor_param = Float3.dot(dat, l_uncor.bs);
					uncor_loparam  = astc::min(uncor_param, uncor_loparam);
					uncor_hiparam = astc::max(uncor_param, uncor_hiparam);

					float samec_param = Float3.dot(dat, l_samec.bs);
					samec_loparam  = astc::min(samec_param, samec_loparam);
					samec_hiparam = astc::max(samec_param, samec_hiparam);

					Float3 uncor_dist  = (l_uncor.amod - dat)
									+ (uncor_param * l_uncor.bis);
					uncor_errorsum += Float3.dot(ews, uncor_dist * uncor_dist);

					Float3 samec_dist = (l_samec.amod - dat)
									+ (samec_param * l_samec.bis);
					samec_errorsum += Float3.dot(ews, samec_dist * samec_dist);
				}

				float uncor_linelen = uncor_hiparam - uncor_loparam;
				float samec_linelen = samec_hiparam - samec_loparam;

				// Turn very small numbers and NaNs into a small number
				uncor_linelen = astc::max(uncor_linelen, 1e-7f);
				samec_linelen = astc::max(samec_linelen, 1e-7f);

				uncor_lengths[partition] = uncor_linelen;
				samec_lengths[partition] = samec_linelen;
			}

			uncor_errors = uncor_errorsum;
			samec_errors = samec_errorsum;
		}
	}
}