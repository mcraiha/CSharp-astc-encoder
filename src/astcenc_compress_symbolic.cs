using System.Diagnostics;

namespace ASTCEnc
{
	public static class CompressSymbolic
	{
		static int realign_weights(ASTCEncProfile decode_mode, BlockSizeDescriptor bsd, ImageBlock blk, ErrorWeightBlock ewb, SymbolicCompressedBlock scb, byte[] plane1_weight_set8, byte[] plane2_weight_set8) 
		{
			// Get the partition descriptor
			int partition_count = scb.partition_count;
			const partition_info *pt = get_partition_table(bsd, partition_count);
			pt += scb.partition_index;

			// Get the quantization table
			const int packed_index = bsd.block_mode_packed_index[scb.block_mode];
			Debug.Assert(packed_index >= 0 && packed_index < bsd.block_mode_count);
			BlockMode bm = bsd.block_modes[packed_index];
			int weight_quantization_level = bm.quantization_mode;
			const quantization_and_transfer_table *qat = &(quant_and_xfer_tables[weight_quantization_level]);

			// Get the decimation table
			DecimationTable[] ixtab2 = bsd.decimation_tables;
			DecimationTable it = ixtab2[bm.decimation_mode];
			int weight_count = it.weight_count;

			int max_plane = bm.is_dual_plane;
			int plane2_component = max_plane ? scb.plane2_color_component : 0;
			int plane_mask = max_plane ? 1 << plane2_component : 0;

			// Decode the color endpoints
			int rgb_hdr;
			int alpha_hdr;
			int nan_endpoint;
			int4 endpnt0[4];
			int4 endpnt1[4];
			float4 endpnt0f[4];
			float4 offset[4];

			promise(partition_count > 0);
			promise(weight_count > 0);
			promise(max_plane >= 0);

			for (int pa_idx = 0; pa_idx < partition_count; pa_idx++)
			{
				unpack_color_endpoints(decode_mode,
									scb.color_formats[pa_idx],
									scb.color_quantization_level,
									scb.color_values[pa_idx],
									&rgb_hdr, &alpha_hdr, &nan_endpoint,
									// TODO: Fix these casts ...
									reinterpret_cast<uint4*>(&endpnt0[pa_idx]),
									reinterpret_cast<uint4*>(&endpnt1[pa_idx]));
			}

			byte[] uq_pl_weights = new byte[Constants.MAX_WEIGHTS_PER_BLOCK];
			uint8_t* weight_set8 = plane1_weight_set8;
			int adjustments = 0;

			// For each plane and partition ...
			for (int pl_idx = 0; pl_idx <= max_plane; pl_idx++)
			{
				for (int pa_idx = 0; pa_idx < partition_count; pa_idx++)
				{
					// Compute the endpoint delta for all channels in current plane
					int4 epd = endpnt1[pa_idx] - endpnt0[pa_idx];

					if ((plane_mask & 1) == 1) epd.r = 0;
					if ((plane_mask & 2) == 1) epd.g = 0;
					if ((plane_mask & 4) == 1) epd.b = 0;
					if ((plane_mask & 8) == 1) epd.a = 0;

					endpnt0f[pa_idx] = new Float4((float)endpnt0[pa_idx].r, (float)endpnt0[pa_idx].g,
											(float)endpnt0[pa_idx].b, (float)endpnt0[pa_idx].a);
					offset[pa_idx] = new Float4((float)epd.r, (float)epd.g, (float)epd.b, (float)epd.a);
					offset[pa_idx] = offset[pa_idx] * (1.0f / 64.0f);
				}

				// Create an unquantized weight grid for this decimation level
				for (int we_idx = 0; we_idx < weight_count; we_idx++)
				{
					uq_pl_weights[we_idx] = qat.unquantized_value[weight_set8[we_idx]];
				}

				// For each weight compute previous, current, and next errors
				for (int we_idx = 0; we_idx < weight_count; we_idx++)
				{
					int uqw = uq_pl_weights[we_idx];

					uint prev_and_next = qat.prev_next_values[uqw];
					int prev_wt_uq = prev_and_next & 0xFF;
					int next_wt_uq = (prev_and_next >> 8) & 0xFF;

					int uqw_next_dif = next_wt_uq - uqw;
					int uqw_prev_dif = prev_wt_uq - uqw;

					float current_error = 0.0f;
					float up_error = 0.0f;
					float down_error = 0.0f;

					// Interpolate the colors to create the diffs
					int texels_to_evaluate = it.weight_texel_count[we_idx];
					promise(texels_to_evaluate > 0);
					for (int te_idx = 0; te_idx < texels_to_evaluate; te_idx++)
					{
						int texel = it.weight_texel[we_idx][te_idx];
						const uint8_t *texel_weights = it.texel_weights_texel[we_idx][te_idx];
						const float *texel_weights_float = it.texel_weights_float_texel[we_idx][te_idx];
						float twf0 = texel_weights_float[0];
						float weight_base =
							((static_cast<float>(uqw) * twf0
							+ static_cast<float>(uq_pl_weights[texel_weights[1]])  * texel_weights_float[1])
							+ (static_cast<float>(uq_pl_weights[texel_weights[2]]) * texel_weights_float[2]
							+ static_cast<float>(uq_pl_weights[texel_weights[3]]) * texel_weights_float[3]));

						int partition = pt.partition_of_texel[texel];

						weight_base = weight_base + 0.5f;
						float plane_weight = astc::flt_rd(weight_base);
						float plane_up_weight = astc::flt_rd(weight_base + static_cast<float>(uqw_next_dif) * twf0) - plane_weight;
						float plane_down_weight = astc::flt_rd(weight_base + static_cast<float>(uqw_prev_dif) * twf0) - plane_weight;

						float4 color_offset = offset[partition];
						float4 color_base   = endpnt0f[partition];

						float4 color = color_base + color_offset * plane_weight;

						float4 origcolor    = float4(blk->data_r[texel], blk->data_g[texel],
													blk->data_b[texel], blk->data_a[texel]);
						float4 error_weight = float4(ewb->texel_weight_r[texel], ewb->texel_weight_g[texel],
													ewb->texel_weight_b[texel], ewb->texel_weight_a[texel]);

						float4 colordiff       = color - origcolor;
						float4 color_up_diff   = colordiff + color_offset * plane_up_weight;
						float4 color_down_diff = colordiff + color_offset * plane_down_weight;
						current_error += dot(colordiff       * colordiff,       error_weight);
						up_error      += dot(color_up_diff   * color_up_diff,   error_weight);
						down_error    += dot(color_down_diff * color_down_diff, error_weight);
					}

					// Check if the prev or next error is better, and if so use it
					if ((up_error < current_error) && (up_error < down_error))
					{
						uq_pl_weights[we_idx] = next_wt_uq;
						weight_set8[we_idx] = (byte)((prev_and_next >> 24) & 0xFF);
						adjustments++;
					}
					else if (down_error < current_error)
					{
						uq_pl_weights[we_idx] = prev_wt_uq;
						weight_set8[we_idx] = (byte)((prev_and_next >> 16) & 0xFF);
						adjustments++;
					}
				}

				// Prepare iteration for plane 2
				weight_set8 = plane2_weight_set8;
				plane_mask ^= 0xF;
			}

			return adjustments;
		}
	}
}