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

		/*
			function for compressing a block symbolically, given that we have already decided on a partition
		*/
		static float compress_symbolic_block_fixed_partition_1_plane(ASTCEncProfile decode_mode, bool only_always, int tune_candidate_limit, float tune_errorval_threshold, int max_refinement_iters, BlockSizeDescriptor bsd, int partition_count, int partition_index, ImageBlock blk, ErrorWeightBlock ewb,
			SymbolicCompressedBlock scb, CompressFixedPartitionBuffers tmpbuf) 
		{
			int[] free_bits_for_partition_count = new int[5] {
				0, 115 - 4, 111 - 4 - PARTITION_BITS, 108 - 4 - PARTITION_BITS, 105 - 4 - PARTITION_BITS
			};

			PartitionInfo pi = get_partition_table(bsd, partition_count);
			pi += partition_index;

			// first, compute ideal weights and endpoint colors, under the assumption that
			// there is no quantization or decimation going on.
			EndpointsAndWeights ei = tmpbuf.ei1;
			EndpointsAndWeights[] eix = tmpbuf.eix1;
			compute_endpoints_and_ideal_weights_1_plane(bsd, pi, blk, ewb, ei);

			// next, compute ideal weights and endpoint colors for every decimation.
			DecimationTable[] ixtab2 = bsd.decimation_tables;

			float[] decimated_quantized_weights = tmpbuf.decimated_quantized_weights;
			float[] decimated_weights = tmpbuf.decimated_weights;
			float[] flt_quantized_decimated_quantized_weights = tmpbuf.flt_quantized_decimated_quantized_weights;
			byte[] u8_quantized_decimated_quantized_weights = tmpbuf.u8_quantized_decimated_quantized_weights;

			// for each decimation mode, compute an ideal set of weights
			// (that is, weights computed with the assumption that they are not quantized)
			for (int i = 0; i < bsd.decimation_mode_count; i++)
			{
				DecimationMode dm = bsd.decimation_modes[i];
				if (dm.maxprec_1plane < 0 || (only_always && !dm.percentile_always) || !dm.percentile_hit)
				{
					continue;
				}

				eix[i] = *ei;

				compute_ideal_weights_for_decimation_table(
					eix[i],
					*(ixtab2[i]),
					decimated_quantized_weights + i * Constants.MAX_WEIGHTS_PER_BLOCK,
					decimated_weights + i * Constants.MAX_WEIGHTS_PER_BLOCK);
			}

			// compute maximum colors for the endpoints and ideal weights.
			// for each endpoint-and-ideal-weight pair, compute the smallest weight value
			// that will result in a color value greater than 1.
			vfloat4 min_ep(10.0f);
			for (int i = 0; i < partition_count; i++)
			{
				vfloat4 ep = (vfloat4(1.0f) - ei.ep.endpt0[i]) / (ei.ep.endpt1[i] - ei.ep.endpt0[i]);

				vmask4 use_ep = (ep > vfloat4(0.5f)) & (ep < min_ep);
				min_ep = select(min_ep, ep, use_ep);
			}

			float min_wt_cutoff = hmin_s(min_ep);

			// for each mode, use the angular method to compute a shift.
			float[] weight_low_value = new float[Constants.MAX_WEIGHT_MODES];
			float[] weight_high_value = new float[Constants.MAX_WEIGHT_MODES];

			compute_angular_endpoints_1plane(
				only_always, bsd,
				decimated_quantized_weights, decimated_weights,
				weight_low_value, weight_high_value);

			// for each mode (which specifies a decimation and a quantization):
			// * compute number of bits needed for the quantized weights.
			// * generate an optimized set of quantized weights.
			// * compute quantization errors for the mode.
			int[] qwt_bitcounts = new int[Constants.MAX_WEIGHT_MODES];
			float[] qwt_errors = new float[Constants.MAX_WEIGHT_MODES];

			for (int i = 0; i < bsd.block_mode_count; ++i)
			{
				BlockMode bm = bsd.block_modes[i];
				if (bm.is_dual_plane || (only_always && !bm.percentile_always) || !bm.percentile_hit)
				{
					qwt_errors[i] = 1e38f;
					continue;
				}

				if (weight_high_value[i] > 1.02f * min_wt_cutoff)
				{
					weight_high_value[i] = 1.0f;
				}

				int decimation_mode = bm.decimation_mode;

				// compute weight bitcount for the mode
				int bits_used_by_weights = get_ise_sequence_bitcount(
					ixtab2[decimation_mode].weight_count,
					(QuantMethod)bm.quant_mode);
				int bitcount = free_bits_for_partition_count[partition_count] - bits_used_by_weights;
				if (bitcount <= 0 || bits_used_by_weights < 24 || bits_used_by_weights > 96)
				{
					qwt_errors[i] = 1e38f;
					continue;
				}
				qwt_bitcounts[i] = bitcount;

				// then, generate the optimized set of weights for the weight mode.
				compute_quantized_weights_for_decimation_table(
					ixtab2[decimation_mode],
					weight_low_value[i], weight_high_value[i],
					decimated_quantized_weights + Constants.MAX_WEIGHTS_PER_BLOCK * decimation_mode,
					flt_quantized_decimated_quantized_weights + Constants.MAX_WEIGHTS_PER_BLOCK * i,
					u8_quantized_decimated_quantized_weights + Constants.MAX_WEIGHTS_PER_BLOCK * i,
					bm.quant_mode);

				// then, compute weight-errors for the weight mode.
				qwt_errors[i] = compute_error_of_weight_set(
									&(eix[decimation_mode]),
									ixtab2[decimation_mode],
									flt_quantized_decimated_quantized_weights + Constants.MAX_WEIGHTS_PER_BLOCK * i);
			}

			// for each weighting mode, determine the optimal combination of color endpoint encodings
			// and weight encodings; return results for the 4 best-looking modes.

			int[,] partition_format_specifiers = new int[Constants.TUNE_MAX_TRIAL_CANDIDATES, 4];
			int[] quantized_weight = new int[Constants.TUNE_MAX_TRIAL_CANDIDATES];
			int[] color_quant_level = new int[Constants.TUNE_MAX_TRIAL_CANDIDATES];
			int[] color_quant_level_mod = new int[Constants.TUNE_MAX_TRIAL_CANDIDATES];

			determine_optimal_set_of_endpoint_formats_to_use(
				bsd, pi, blk, ewb, &(ei.ep), -1, qwt_bitcounts, qwt_errors,
				tune_candidate_limit, partition_format_specifiers, quantized_weight,
				color_quant_level, color_quant_level_mod);

			// then iterate over the tune_candidate_limit believed-to-be-best modes to
			// find out which one is actually best.
			float best_errorval_in_mode = 1e30f;
			float best_errorval_in_scb = scb.errorval;

			for (int i = 0; i < tune_candidate_limit; i++)
			{
				TRACE_NODE(node0, "candidate");

				uint8_t *u8_weight_src;
				int weights_to_copy;

				int qw_packed_index = quantized_weight[i];
				if (qw_packed_index < 0)
				{
					trace_add_data("failed", "error_block");
					continue;
				}

				Debug.Assert(qw_packed_index >= 0 && qw_packed_index < bsd.block_mode_count);
				BlockMode qw_bm = bsd.block_modes[qw_packed_index];

				int decimation_mode = qw_bm.decimation_mode;
				int weight_quant_mode = qw_bm.quant_mode;
				DecimationTable it = ixtab2[decimation_mode];
				u8_weight_src = u8_quantized_decimated_quantized_weights + Constants.MAX_WEIGHTS_PER_BLOCK * qw_packed_index;
				weights_to_copy = it.weight_count;

				trace_add_data("weight_x", it->weight_x);
				trace_add_data("weight_y", it->weight_y);
				trace_add_data("weight_z", it->weight_z);
				trace_add_data("weight_quant", weight_quant_mode);

				// recompute the ideal color endpoints before storing them.
				vfloat4[] rgbs_colors = new vfloat4[4];
				vfloat4[] rgbo_colors = new vfloat4[4];

				// TODO: Can we ping-pong between two buffers and make this zero copy?
				SymbolicCompressedBlock workscb;
				for (int l = 0; l < max_refinement_iters; l++)
				{
					recompute_ideal_colors_1plane(
						weight_quant_mode, &(eix[decimation_mode].ep),
						rgbs_colors, rgbo_colors, u8_weight_src, pi, it, blk, ewb);

					// quantize the chosen color

					// store the colors for the block
					for (int j = 0; j < partition_count; j++)
					{
						workscb.color_formats[j] = pack_color_endpoints(
							eix[decimation_mode].ep.endpt0[j],
							eix[decimation_mode].ep.endpt1[j],
							rgbs_colors[j],
							rgbo_colors[j],
							partition_format_specifiers[i, j],
							workscb.color_values[j],
							color_quant_level[i]);
					}

					// if all the color endpoint modes are the same, we get a few more
					// bits to store colors; let's see if we can take advantage of this:
					// requantize all the colors and see if the endpoint modes remain the same;
					// if they do, then exploit it.
					workscb.color_formats_matched = 0;

					if ((partition_count >= 2 && workscb.color_formats[0] == workscb.color_formats[1]
						&& color_quant_level[i] != color_quant_level_mod[i])
						&& (partition_count == 2 || (workscb.color_formats[0] == workscb.color_formats[2]
						&& (partition_count == 3 || (workscb.color_formats[0] == workscb.color_formats[3])))))
					{
						int[,] colorvals = new int[4, 12];
						int[] color_formats_mod = new int[4] { 0, 0, 0, 0 };
						for (int j = 0; j < partition_count; j++)
						{
							color_formats_mod[j] = pack_color_endpoints(
								eix[decimation_mode].ep.endpt0[j],
								eix[decimation_mode].ep.endpt1[j],
								rgbs_colors[j],
								rgbo_colors[j],
								partition_format_specifiers[i, j],
								colorvals[j],
								color_quant_level_mod[i]);
						}

						if (color_formats_mod[0] == color_formats_mod[1]
							&& (partition_count == 2 || (color_formats_mod[0] == color_formats_mod[2]
							&& (partition_count == 3 || (color_formats_mod[0] == color_formats_mod[3])))))
						{
							workscb.color_formats_matched = 1;
							for (int j = 0; j < 4; j++)
							{
								for (int k = 0; k < 12; k++)
								{
									workscb.color_values[j, k] = colorvals[j, k];
								}
							}

							for (int j = 0; j < 4; j++)
							{
								workscb.color_formats[j] = color_formats_mod[j];
							}
						}
					}

					// store header fields
					workscb.partition_count = partition_count;
					workscb.partition_index = partition_index;
					workscb.color_quant_level = workscb.color_formats_matched ? color_quant_level_mod[i] : color_quant_level[i];
					workscb.block_mode = qw_bm.mode_index;
					workscb.error_block = 0;

					if (workscb.color_quant_level < 4)
					{
						workscb.error_block = 1; // should never happen, but cannot prove it impossible.
					}

					// Pre-realign test
					if (l == 0)
					{
						for (int j = 0; j < weights_to_copy; j++)
						{
							workscb.weights[j] = u8_weight_src[j];
						}

						float errorval = compute_symbolic_block_difference(decode_mode, bsd, &workscb, blk, ewb);
						trace_add_data("error_prerealign", errorval);
						best_errorval_in_mode = astc::min(errorval, best_errorval_in_mode);

						// Average refinement improvement is 3.5% per iteration
						// (allow 5%), but the first iteration can help more so we give
						// it a extra 10% leeway. Use this knowledge to drive a
						// heuristic to skip blocks that are unlikely to catch up with
						// the best block we have already.
						int iters_remaining = max_refinement_iters - l;
						float threshold = (0.05f * static_cast<float>(iters_remaining)) + 1.1f;
						if (errorval > (threshold * best_errorval_in_scb))
						{
							break;
						}

						if (errorval < best_errorval_in_scb)
						{
							best_errorval_in_scb = errorval;
							workscb.errorval = errorval;
							scb = workscb;

							if (errorval < tune_errorval_threshold)
							{
								return errorval;
							}
						}
					}

					// perform a final pass over the weights to try to improve them.
					int adjustments = realign_weights(
						decode_mode, bsd, blk, ewb, &workscb,
						u8_weight_src, nullptr);

					// Post-realign test
					for (int j = 0; j < weights_to_copy; j++)
					{
						workscb.weights[j] = u8_weight_src[j];
					}

					float errorval = compute_symbolic_block_difference(decode_mode, bsd, &workscb, blk, ewb);
					trace_add_data("error_postrealign", errorval);
					best_errorval_in_mode = astc::min(errorval, best_errorval_in_mode);

					// Average refinement improvement is 3.5% per iteration, so skip
					// blocks that are unlikely to catch up with the best block we
					// have already. Assume a 5% per step to give benefit of the doubt
					int iters_remaining = max_refinement_iters - 1 - l;
					float threshold = (0.05f * static_cast<float>(iters_remaining)) + 1.0f;
					if (errorval > (threshold * best_errorval_in_scb))
					{
						break;
					}

					if (errorval < best_errorval_in_scb)
					{
						best_errorval_in_scb = errorval;
						workscb.errorval = errorval;
						scb = workscb;

						if (errorval < tune_errorval_threshold)
						{
							return errorval;
						}
					}

					if (adjustments == 0)
					{
						break;
					}
				}
			}

			return best_errorval_in_mode;
		}
	}
}