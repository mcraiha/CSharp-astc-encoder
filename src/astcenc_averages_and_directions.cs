
namespace ASTCEnc
{
	public static class AveragesAndDirections
	{
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

		public static void compute_averages_and_directions_3_components(PartitionInfo pt, ImageBlock blk, ErrorWeightBlock ewb, Float3[] color_scalefactors, int omitted_component, Float3[] averages, Float3[] directions) 
		{
			const float[] texel_weights;
			const float[] data_vr;
			const float[] data_vg;
			const float[] data_vb;

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

				float prod_xp = dot(sum_xp, sum_xp);
				float prod_yp = dot(sum_yp, sum_yp);

				Float2 best_vector = sum_xp;
				float best_sum = prod_xp;

				if (prod_yp > best_sum)
				{
					best_vector = sum_yp;
				}

				directions[partition] = best_vector;
			}
		}
	}
}