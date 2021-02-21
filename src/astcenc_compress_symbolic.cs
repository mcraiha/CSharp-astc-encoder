using System;
using System.Diagnostics;

namespace ASTCEnc
{
	public static class CompressSymbolic
	{
		static int realign_weights(ASTCEncProfile decode_mode, BlockSizeDescriptor bsd, ImageBlock blk, ErrorWeightBlock ewb, SymbolicCompressedBlock scb, byte[] plane1_weight_set8, byte[] plane2_weight_set8) 
		{
			// Get the partition descriptor
			int partition_count = scb.partition_count;
			PartitionInfo  pt = get_partition_table(bsd, partition_count);
			pt += scb.partition_index;

			// Get the quantization table
			int packed_index = bsd.block_mode_packed_index[scb.block_mode];
			Debug.Assert(packed_index >= 0 && packed_index < bsd.block_mode_count);
			BlockMode bm = bsd.block_modes[packed_index];
			int weight_quantization_level = bm.quantization_mode;
			QuantizationAndTransferTable qat = &(quant_and_xfer_tables[weight_quantization_level]);

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
			vint4[] endpnt0 = new vint4[4];
			vint4[] endpnt1 = new vint4[4];
			vfloat4[] endpnt0f = new vfloat4[4];
			vfloat4[] offset = new vfloat4[4];

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
			byte[] weight_set8 = plane1_weight_set8;
			int adjustments = 0;

			// For each plane and partition ...
			for (int pl_idx = 0; pl_idx <= max_plane; pl_idx++)
			{
				for (int pa_idx = 0; pa_idx < partition_count; pa_idx++)
				{
					// Compute the endpoint delta for all channels in current plane
					vint4 epd = endpnt1[pa_idx] - endpnt0[pa_idx];

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
						int texel = it.weight_texel[we_idx, te_idx];
						const uint8_t *texel_weights = it.texel_weights_texel[we_idx, te_idx];
						const float *texel_weights_float = it.texel_weights_float_texel[we_idx, te_idx];
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

						vfloat4 color_offset = offset[partition];
						vfloat4 color_base   = endpnt0f[partition];

						vfloat4 color = color_base + color_offset * plane_weight;

						vfloat4 origcolor    = new vfloat4(blk.data_r[texel], blk.data_g[texel],
													blk.data_b[texel], blk.data_a[texel]);
						vfloat4 error_weight = new vfloat4(ewb.texel_weight_r[texel], ewb.texel_weight_g[texel],
													ewb.texel_weight_b[texel], ewb.texel_weight_a[texel]);

						vfloat4 colordiff       = color - origcolor;
						vfloat4 color_up_diff   = colordiff + color_offset * plane_up_weight;
						vfloat4 color_down_diff = colordiff + color_offset * plane_down_weight;
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
					best_errorval_in_mode = Math.Min(errorval, best_errorval_in_mode);

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

		static float compress_symbolic_block_fixed_partition_2_planes(ASTCEncProfile decode_mode, bool only_always, int tune_candidate_limit, float tune_errorval_threshold, int max_refinement_iters, BlockSizeDescriptor bsd, int partition_count, int partition_index, int separate_component, ImageBlock blk, ErrorWeightBlock ewb, SymbolicCompressedBlock scb, CompressFixedPartitionBuffers tmpbuf) 
		{
			int[] free_bits_for_partition_count = new int[5] {
				0, 113 - 4, 109 - 4 - PARTITION_BITS, 106 - 4 - PARTITION_BITS, 103 - 4 - PARTITION_BITS
			};

			PartitionInfo pi = get_partition_table(bsd, partition_count);
			pi += partition_index;

			// first, compute ideal weights and endpoint colors
			EndpointsAndWeights ei1 = tmpbuf.ei1;
			EndpointsAndWeights ei2 = tmpbuf.ei2;
			EndpointsAndWeights[] eix1 = tmpbuf.eix1;
			EndpointsAndWeights[] eix2 = tmpbuf.eix2;
			compute_endpoints_and_ideal_weights_2_planes(bsd, pi, blk, ewb, separate_component, ei1, ei2);

			// next, compute ideal weights and endpoint colors for every decimation.
			DecimationTable[] ixtab2 = bsd.decimation_tables;

			float[] decimated_quantized_weights = tmpbuf.decimated_quantized_weights;
			float[] decimated_weights = tmpbuf.decimated_weights;
			float[] flt_quantized_decimated_quantized_weights = tmpbuf.flt_quantized_decimated_quantized_weights;
			byte[] u8_quantized_decimated_quantized_weights = tmpbuf.u8_quantized_decimated_quantized_weights;

			// for each decimation mode, compute an ideal set of weights
			for (int i = 0; i < bsd.decimation_mode_count; i++)
			{
				DecimationMode dm = bsd.decimation_modes[i];
				if (dm.maxprec_2planes < 0 || (only_always && !dm.percentile_always) || !dm.percentile_hit)
				{
					continue;
				}

				eix1[i] = ei1;
				eix2[i] = ei2;

				compute_ideal_weights_for_decimation_table(
					eix1[i],
					*(ixtab2[i]),
					decimated_quantized_weights + (2 * i) * Constants.MAX_WEIGHTS_PER_BLOCK,
					decimated_weights + (2 * i) * Constants.MAX_WEIGHTS_PER_BLOCK);

				compute_ideal_weights_for_decimation_table(
					eix2[i],
					*(ixtab2[i]),
					decimated_quantized_weights + (2 * i + 1) * Constants.MAX_WEIGHTS_PER_BLOCK,
					decimated_weights + (2 * i + 1) * Constants.MAX_WEIGHTS_PER_BLOCK);
			}

			// compute maximum colors for the endpoints and ideal weights.
			// for each endpoint-and-ideal-weight pair, compute the smallest weight value
			// that will result in a color value greater than 1.

			vfloat4 min_ep1 = new vfloat4(10.0f);
			vfloat4 min_ep2 = new vfloat4(10.0f);
			for (int i = 0; i < partition_count; i++)
			{
				vfloat4 ep1 = (new vfloat4(1.0f) - ei1.ep.endpt0[i]) / (ei1.ep.endpt1[i] - ei1.ep.endpt0[i]);
				vmask4 use_ep1 = (ep1 > new vfloat4(0.5f)) & (ep1 < min_ep1);
				min_ep1 = select(min_ep1, ep1, use_ep1);

				vfloat4 ep2 = (new vfloat4(1.0f) - ei2.ep.endpt0[i]) / (ei2.ep.endpt1[i] - ei2.ep.endpt0[i]);
				vmask4 use_ep2 = (ep2 > new vfloat4(0.5f)) & (ep2 < min_ep2);
				min_ep2 = select(min_ep2, ep2, use_ep2);
			}

			vfloat4 err_max = new vfloat4(1e30f);
			vmask4 err_mask = vint4::lane_id() == vint4(separate_component);

			// Set the separate component to max error in ep1
			min_ep1 = select(min_ep1, err_max, err_mask);

			float min_wt_cutoff1 = hmin_s(min_ep1);

			// Set the minwt2 to the separate component min in ep2
			float min_wt_cutoff2 = hmin_s(select(err_max, min_ep2, err_mask));

			float[] weight_low_value1 = new float[Constants.MAX_WEIGHT_MODES];
			float[] weight_high_value1 = new float[Constants.MAX_WEIGHT_MODES];
			float[] weight_low_value2 = new float[Constants.MAX_WEIGHT_MODES];
			float[] weight_high_value2 = new float[Constants.MAX_WEIGHT_MODES];

			compute_angular_endpoints_2planes(
				only_always, bsd,
				decimated_quantized_weights, decimated_weights,
				weight_low_value1, weight_high_value1,
				weight_low_value2, weight_high_value2);

			// for each mode (which specifies a decimation and a quantization):
			// * generate an optimized set of quantized weights.
			// * compute quantization errors for each mode
			// * compute number of bits needed for the quantized weights.

			int[] qwt_bitcounts = new int[Constants.MAX_WEIGHT_MODES];
			float[] qwt_errors = new float[Constants.MAX_WEIGHT_MODES];
			for (int i = 0; i < bsd.block_mode_count; ++i)
			{
				BlockMode bm = bsd.block_modes[i];
				if ((!bm.is_dual_plane) || (only_always && !bm.percentile_always) || !bm.percentile_hit)
				{
					qwt_errors[i] = 1e38f;
					continue;
				}

				int decimation_mode = bm.decimation_mode;

				if (weight_high_value1[i] > 1.02f * min_wt_cutoff1)
				{
					weight_high_value1[i] = 1.0f;
				}

				if (weight_high_value2[i] > 1.02f * min_wt_cutoff2)
				{
					weight_high_value2[i] = 1.0f;
				}

				// compute weight bitcount for the mode
				int bits_used_by_weights = get_ise_sequence_bitcount(
					2 * ixtab2[decimation_mode]->weight_count,
					(QuantMethod)bm.quant_mode);
				int bitcount = free_bits_for_partition_count[partition_count] - bits_used_by_weights;
				if (bitcount <= 0 || bits_used_by_weights < 24 || bits_used_by_weights > 96)
				{
					qwt_errors[i] = 1e38f;
					continue;
				}
				qwt_bitcounts[i] = bitcount;

				// then, generate the optimized set of weights for the mode.
				compute_quantized_weights_for_decimation_table(
					ixtab2[decimation_mode],
					weight_low_value1[i],
					weight_high_value1[i],
					decimated_quantized_weights + Constants.MAX_WEIGHTS_PER_BLOCK * (2 * decimation_mode),
					flt_quantized_decimated_quantized_weights + Constants.MAX_WEIGHTS_PER_BLOCK * (2 * i),
					u8_quantized_decimated_quantized_weights + Constants.MAX_WEIGHTS_PER_BLOCK * (2 * i), bm.quant_mode);

				compute_quantized_weights_for_decimation_table(
					ixtab2[decimation_mode],
					weight_low_value2[i],
					weight_high_value2[i],
					decimated_quantized_weights + Constants.MAX_WEIGHTS_PER_BLOCK * (2 * decimation_mode + 1),
					flt_quantized_decimated_quantized_weights + Constants.MAX_WEIGHTS_PER_BLOCK * (2 * i + 1),
					u8_quantized_decimated_quantized_weights + Constants.MAX_WEIGHTS_PER_BLOCK * (2 * i + 1), bm.quant_mode);


				// then, compute quantization errors for the block mode.
				qwt_errors[i] =	compute_error_of_weight_set(
									&(eix1[decimation_mode]),
									ixtab2[decimation_mode],
									flt_quantized_decimated_quantized_weights + Constants.MAX_WEIGHTS_PER_BLOCK * (2 * i))

							+ compute_error_of_weight_set(
									&(eix2[decimation_mode]),
									ixtab2[decimation_mode],
									flt_quantized_decimated_quantized_weights + Constants.MAX_WEIGHTS_PER_BLOCK * (2 * i + 1));
			}

			// decide the optimal combination of color endpoint encodings and weight encodings.
			int[,] partition_format_specifiers = new int[Constants.TUNE_MAX_TRIAL_CANDIDATES, 4];
			int[] quantized_weight = new int[Constants.TUNE_MAX_TRIAL_CANDIDATES];
			int[] color_quant_level = new int[Constants.TUNE_MAX_TRIAL_CANDIDATES];
			int[] color_quant_level_mod = new int[Constants.TUNE_MAX_TRIAL_CANDIDATES];

			Endpoints epm;
			merge_endpoints(&(ei1.ep), &(ei2.ep), separate_component, &epm);

			determine_optimal_set_of_endpoint_formats_to_use(
				bsd, pi, blk, ewb, &epm, separate_component, qwt_bitcounts, qwt_errors,
				tune_candidate_limit, partition_format_specifiers, quantized_weight,
				color_quant_level, color_quant_level_mod);

			// then iterate over the tune_candidate_limit believed-to-be-best modes to
			// find out which one is actually best.
			float best_errorval_in_mode = 1e30f;
			float best_errorval_in_scb = scb.errorval;

			for (int i = 0; i < tune_candidate_limit; i++)
			{
				TRACE_NODE(node0, "candidate");

				int qw_packed_index = quantized_weight[i];
				if (qw_packed_index < 0)
				{
					trace_add_data("failed", "error_block");
					continue;
				}

				uint8_t *u8_weight1_src;
				uint8_t *u8_weight2_src;
				int weights_to_copy;

				Debug.Assert(qw_packed_index >= 0 && qw_packed_index < bsd.block_mode_count);
				const block_mode& qw_bm = bsd->block_modes[qw_packed_index];

				int decimation_mode = qw_bm.decimation_mode;
				int weight_quant_mode = qw_bm.quant_mode;
				DecimationTable it = ixtab2[decimation_mode];

				u8_weight1_src = u8_quantized_decimated_quantized_weights + Constants.MAX_WEIGHTS_PER_BLOCK * (2 * qw_packed_index);
				u8_weight2_src = u8_quantized_decimated_quantized_weights + Constants.MAX_WEIGHTS_PER_BLOCK * (2 * qw_packed_index + 1);
				weights_to_copy = it.weight_count;

				trace_add_data("weight_x", it->weight_x);
				trace_add_data("weight_y", it->weight_y);
				trace_add_data("weight_z", it->weight_z);
				trace_add_data("weight_quant", weight_quant_mode);

				// recompute the ideal color endpoints before storing them.
				merge_endpoints(&(eix1[decimation_mode].ep), &(eix2[decimation_mode].ep), separate_component, &epm);

				vfloat4[] rgbs_colors = new vfloat4[4];
				vfloat4[] rgbo_colors = new vfloat4[4];

				// TODO: Ping-pong between two buffers and make this zero copy
				SymbolicCompressedBlock workscb;
				for (int l = 0; l < max_refinement_iters; l++)
				{
					recompute_ideal_colors_2planes(
						weight_quant_mode, &epm, rgbs_colors, rgbo_colors,
						u8_weight1_src, u8_weight2_src, separate_component, pi, it, blk, ewb);

					// store the colors for the block
					for (int j = 0; j < partition_count; j++)
					{
						workscb.color_formats[j] = pack_color_endpoints(
													epm.endpt0[j],
													epm.endpt1[j],
													rgbs_colors[j], rgbo_colors[j],
													partition_format_specifiers[i][j],
													workscb.color_values[j],
													color_quant_level[i]);
					}

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
								epm.endpt0[j],
								epm.endpt1[j],
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
					workscb.plane2_color_component = separate_component;
					workscb.error_block = 0;

					if (workscb.color_quant_level < 4)
					{
						workscb.error_block = 1;	// should never happen, but cannot prove it impossible
					}

					// Pre-realign test
					if (l == 0)
					{
						for (int j = 0; j < weights_to_copy; j++)
						{
							workscb.weights[j] = u8_weight1_src[j];
							workscb.weights[j + Constants.PLANE2_WEIGHTS_OFFSET] = u8_weight2_src[j];
						}

						float errorval = compute_symbolic_block_difference(decode_mode, bsd, &workscb, blk, ewb);
						trace_add_data("error_prerealign", errorval);
						best_errorval_in_mode = Math.Min(errorval, best_errorval_in_mode);

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
						u8_weight1_src, u8_weight2_src);

					// Post-realign test
					for (int j = 0; j < weights_to_copy; j++)
					{
						workscb.weights[j] = u8_weight1_src[j];
						workscb.weights[j + Constants.PLANE2_WEIGHTS_OFFSET] = u8_weight2_src[j];
					}

					float errorval = compute_symbolic_block_difference(decode_mode, bsd, &workscb, blk, ewb);
					trace_add_data("error_postrealign", errorval);
					best_errorval_in_mode = Math.Min(errorval, best_errorval_in_mode);

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

		private static void expand_deblock_weights(astcenc_context ctx) 
		{
			uint xdim = ctx.config.block_x;
			uint ydim = ctx.config.block_y;
			uint zdim = ctx.config.block_z;

			float centerpos_x = static_cast<float>(xdim - 1) * 0.5f;
			float centerpos_y = static_cast<float>(ydim - 1) * 0.5f;
			float centerpos_z = static_cast<float>(zdim - 1) * 0.5f;
			float[] bef = ctx.deblock_weights;

			for (uint z = 0; z < zdim; z++)
			{
				for (uint y = 0; y < ydim; y++)
				{
					for (uint x = 0; x < xdim; x++)
					{
						float xdif = (static_cast<float>(x) - centerpos_x) / static_cast<float>(xdim);
						float ydif = (static_cast<float>(y) - centerpos_y) / static_cast<float>(ydim);
						float zdif = (static_cast<float>(z) - centerpos_z) / static_cast<float>(zdim);

						float wdif = 0.36f;
						float dist = astc::sqrt(xdif * xdif + ydif * ydif + zdif * zdif + wdif * wdif);
						*bef = powf(dist, ctx.config.b_deblock_weight);
						bef++;
					}
				}
			}
		}

		// Function to set error weights for each color component for each texel in a block.
		// Returns the sum of all the error values set.
		static float prepare_error_weight_block(astcenc_context ctx, ASTCEncImage input_image, BlockSizeDescriptor bsd, ImageBlock blk, ErrorWeightBlock ewb) 
		{
			int idx = 0;
			int any_mean_stdev_weight =
				ctx.config.v_rgb_mean != 0.0f || ctx.config.v_rgb_stdev != 0.0f || 
				ctx.config.v_a_mean != 0.0f || ctx.config.v_a_stdev != 0.0f;

			vfloat4[] derv = new vfloat4[Constants.MAX_TEXELS_PER_BLOCK];
			imageblock_initialize_deriv(blk, bsd.texel_count, derv);
			vfloat4 color_weights = new vfloat4(ctx.config.cw_r_weight,
								ctx.config.cw_g_weight,
								ctx.config.cw_b_weight,
								ctx.config.cw_a_weight);

			for (int z = 0; z < bsd.zdim; z++)
			{
				for (int y = 0; y < bsd.ydim; y++)
				{
					for (int x = 0; x < bsd.xdim; x++)
					{
						uint xpos = x + blk.xpos;
						uint ypos = y + blk.ypos;
						uint zpos = z + blk.zpos;

						if (xpos >= input_image.dim_x || ypos >= input_image.dim_y || zpos >= input_image.dim_z)
						{
							ewb.error_weights[idx] = new vfloat4(1e-11f);
						}
						else
						{
							vfloat4 error_weight = new vfloat4(ctx.config.v_rgb_base,
												ctx.config.v_rgb_base,
												ctx.config.v_rgb_base,
												ctx.config.v_a_base);

							int ydt = input_image.dim_x;
							int zdt = input_image.dim_x * input_image.dim_y;

							if (any_mean_stdev_weight)
							{
								vfloat4 avg = ctx.input_averages[zpos * zdt + ypos * ydt + xpos];
								avg = max(avg, 6e-5f);
								avg = avg * avg;

								vfloat4 variance = ctx.input_variances[zpos * zdt + ypos * ydt + xpos];
								variance = variance * variance;

								float favg = hadd_rgb_s(avg) * (1.0f / 3.0f);
								float fvar = hadd_rgb_s(variance) * (1.0f / 3.0f);

								float mixing = ctx.config.v_rgba_mean_stdev_mix;
								avg.set_lane<0>(favg * mixing + avg.lane<0>() * (1.0f - mixing));
								avg.set_lane<1>(favg * mixing + avg.lane<1>() * (1.0f - mixing));
								avg.set_lane<2>(favg * mixing + avg.lane<2>() * (1.0f - mixing));

								variance.set_lane<0>(fvar * mixing + variance.lane<0>() * (1.0f - mixing));
								variance.set_lane<1>(fvar * mixing + variance.lane<1>() * (1.0f - mixing));
								variance.set_lane<2>(fvar * mixing + variance.lane<2>() * (1.0f - mixing));

								// TODO: Vectorize this ...
								vfloat4 stdev = new vfloat4(astc::sqrt(Math.Max(variance.lane<0>(), 0.0f)),
														astc::sqrt(Math.Max(variance.lane<1>(), 0.0f)),
														astc::sqrt(Math.Max(variance.lane<2>(), 0.0f)),
														astc::sqrt(Math.Max(variance.lane<3>(), 0.0f)));

								vfloat4 scalea = new vfloat4(ctx.config.v_rgb_mean, ctx.config.v_rgb_mean, ctx.config.v_rgb_mean, ctx.config.v_a_mean);
								avg = avg * scalea;

								vfloat4 scales = new vfloat4(ctx.config.v_rgb_stdev, ctx.config.v_rgb_stdev, ctx.config.v_rgb_stdev, ctx.config.v_a_stdev);
								stdev = stdev * scales;

								error_weight = error_weight + avg + stdev;
								error_weight = 1.0f / error_weight;
							}

							if (ctx.config.flags & ASTCENC_FLG_MAP_NORMAL)
							{
								// Convert from 0 to 1 to -1 to +1 range.
								float xN = ((blk.data_r[idx] * (1.0f / 65535.0f)) - 0.5f) * 2.0f;
								float yN = ((blk.data_a[idx] * (1.0f / 65535.0f)) - 0.5f) * 2.0f;

								float denom = 1.0f - xN * xN - yN * yN;
								denom = Math.Max(denom, 0.1f);
								denom = 1.0f / denom;
								error_weight.set_lane<0>(error_weight.lane<0>() * (1.0f + xN * xN * denom));
								error_weight.set_lane<3>(error_weight.lane<3>() * (1.0f + yN * yN * denom));
							}

							if (ctx.config.flags & ASTCENC_FLG_USE_ALPHA_WEIGHT)
							{
								float alpha_scale;
								if (ctx.config.a_scale_radius != 0)
								{
									alpha_scale = ctx.input_alpha_averages[zpos * zdt + ypos * ydt + xpos];
								}
								else
								{
									alpha_scale = blk.data_a[idx] * (1.0f / 65535.0f);
								}

								alpha_scale = Math.Max(alpha_scale, 0.0001f);

								alpha_scale *= alpha_scale;
								error_weight.set_lane<0>(error_weight.lane<0>() * alpha_scale);
								error_weight.set_lane<1>(error_weight.lane<1>() * alpha_scale);
								error_weight.set_lane<2>(error_weight.lane<2>() * alpha_scale);
							}

							error_weight = error_weight * color_weights;
							error_weight = error_weight * ctx.deblock_weights[idx];

							// when we loaded the block to begin with, we applied a transfer function
							// and computed the derivative of the transfer function. However, the
							// error-weight computation so far is based on the original color values,
							// not the transfer-function values. As such, we must multiply the
							// error weights by the derivative of the inverse of the transfer function,
							// which is equivalent to dividing by the derivative of the transfer
							// function.

							error_weight = error_weight / (derv[idx] * derv[idx] * 1e-10f);
							ewb.error_weights[idx] = error_weight;
						}
						idx++;
					}
				}
			}

			vfloat4 error_weight_sum = vfloat4::zero();
			int texels_per_block = bsd.texel_count;
			for (int i = 0; i < texels_per_block; i++)
			{
				error_weight_sum = error_weight_sum + ewb.error_weights[i];

				float wr = ewb.error_weights[i].lane<0>();
				float wg = ewb.error_weights[i].lane<1>();
				float wb = ewb.error_weights[i].lane<2>();
				float wa = ewb.error_weights[i].lane<3>();

				ewb.texel_weight_r[i] = wr;
				ewb.texel_weight_g[i] = wg;
				ewb.texel_weight_b[i] = wb;
				ewb.texel_weight_a[i] = wa;

				ewb.texel_weight_rg[i] = (wr + wg) * 0.5f;
				ewb.texel_weight_rb[i] = (wr + wb) * 0.5f;
				ewb.texel_weight_gb[i] = (wg + wb) * 0.5f;
				ewb.texel_weight_ra[i] = (wr + wa) * 0.5f;

				ewb.texel_weight_gba[i] = (wg + wb + wa) * 0.333333f;
				ewb.texel_weight_rba[i] = (wr + wb + wa) * 0.333333f;
				ewb.texel_weight_rga[i] = (wr + wg + wa) * 0.333333f;
				ewb.texel_weight_rgb[i] = (wr + wg + wb) * 0.333333f;

				ewb.texel_weight[i] = (wr + wg + wb + wa) * 0.25f;
			}

			return hadd_s(error_weight_sum);
		}

		static float prepare_block_statistics(int texels_per_block, ImageBlock blk, ErrorWeightBlock ewb) 
		{
			// compute covariance matrix, as a collection of 10 scalars
			// (that form the upper-triangular row of the matrix; the matrix is
			// symmetric, so this is all we need)
			float rss = 0.0f;
			float gss = 0.0f;
			float bss = 0.0f;
			float ass = 0.0f;
			float rr_var = 0.0f;
			float gg_var = 0.0f;
			float bb_var = 0.0f;
			float aa_var = 0.0f;
			float rg_cov = 0.0f;
			float rb_cov = 0.0f;
			float ra_cov = 0.0f;
			float gb_cov = 0.0f;
			float ga_cov = 0.0f;
			float ba_cov = 0.0f;

			float weight_sum = 0.0f;

			for (int i = 0; i < texels_per_block; i++)
			{
				float weight = ewb.texel_weight[i];
				Debug.Assert(weight >= 0.0f);
				weight_sum += weight;

				float r = blk.data_r[i];
				float g = blk.data_g[i];
				float b = blk.data_b[i];
				float a = blk.data_a[i];

				float rw = r * weight;
				rss += rw;
				rr_var += r * rw;
				rg_cov += g * rw;
				rb_cov += b * rw;
				ra_cov += a * rw;

				float gw = g * weight;
				gss += gw;
				gg_var += g * gw;
				gb_cov += b * gw;
				ga_cov += a * gw;

				float bw = b * weight;
				bss += bw;
				bb_var += b * bw;
				ba_cov += a * bw;

				float aw = a * weight;
				ass += aw;
				aa_var += a * aw;
			}

			float rpt = 1.0f / Math.Max(weight_sum, 1e-7f);

			rr_var -= rss * (rss * rpt);
			rg_cov -= gss * (rss * rpt);
			rb_cov -= bss * (rss * rpt);
			ra_cov -= ass * (rss * rpt);

			gg_var -= gss * (gss * rpt);
			gb_cov -= bss * (gss * rpt);
			ga_cov -= ass * (gss * rpt);

			bb_var -= bss * (bss * rpt);
			ba_cov -= ass * (bss * rpt);

			aa_var -= ass * (ass * rpt);

			rg_cov *= astc::rsqrt(Math.Max(rr_var * gg_var, 1e-30f));
			rb_cov *= astc::rsqrt(Math.Max(rr_var * bb_var, 1e-30f));
			ra_cov *= astc::rsqrt(Math.Max(rr_var * aa_var, 1e-30f));
			gb_cov *= astc::rsqrt(Math.Max(gg_var * bb_var, 1e-30f));
			ga_cov *= astc::rsqrt(Math.Max(gg_var * aa_var, 1e-30f));
			ba_cov *= astc::rsqrt(Math.Max(bb_var * aa_var, 1e-30f));

			if (astc::isnan(rg_cov)) rg_cov = 1.0f;
			if (astc::isnan(rb_cov)) rb_cov = 1.0f;
			if (astc::isnan(ra_cov)) ra_cov = 1.0f;
			if (astc::isnan(gb_cov)) gb_cov = 1.0f;
			if (astc::isnan(ga_cov)) ga_cov = 1.0f;
			if (astc::isnan(ba_cov)) ba_cov = 1.0f;

			float lowest_correlation = Math.Min(Math.Abs(rg_cov), Math.Abs(rb_cov));
			lowest_correlation       = Math.Min(lowest_correlation, Math.Abs(ra_cov));
			lowest_correlation       = Math.Min(lowest_correlation, Math.Abs(gb_cov));
			lowest_correlation       = Math.Min(lowest_correlation, Math.Abs(ga_cov));
			lowest_correlation       = Math.Min(lowest_correlation, Math.Abs(ba_cov));

			return lowest_correlation;
		}

		static public void compress_block(astcenc_context ctx, ASTCEncImage input_image, ImageBlock blk, SymbolicCompressedBlock scb, PhysicalCompressedBlock pcb, compress_symbolic_block_buffers tmpbuf)
		{
			ASTCEncProfile decode_mode = ctx.config.profile;
			ErrorWeightBlock ewb = tmpbuf.ewb;
			BlockSizeDescriptor bsd = ctx.bsd;
			float lowest_correl;

			// Set stricter block targets for luminance data as we have more bits to
			// play with - fewer endpoints and never need a second weight plane
			bool block_is_l = imageblock_is_lum(blk);
			float block_is_l_scale = block_is_l ? 1.0f / 1.5f : 1.0f;

			// Set slightly stricter block targets for lumalpha data as we have more
			// bits to play with - fewer endpoints but may use a second weight plane
			bool block_is_la = imageblock_is_lumalp(blk);
			float block_is_la_scale = block_is_la ? 1.0f / 1.05f : 1.0f;

			if (all(blk.data_min == blk.data_max))
			{
				// detected a constant-color block. Encode as FP16 if using HDR
				scb.error_block = 0;
				scb.partition_count = 0;

				if ((decode_mode == ASTCENC_PRF_HDR) ||
					(decode_mode == ASTCENC_PRF_HDR_RGB_LDR_A))
				{
					scb.block_mode = -1;
					vint4 color_f16 = float_to_float16(blk->origin_texel);
					store(color_f16, scb.constant_color);
				}
				else
				{
					// Encode as UNORM16 if NOT using HDR.
					scb.block_mode = -2;
					vfloat4 color_f32 = clamp(0.0f, 1.0f, blk.origin_texel) * 65535.0f;
					vint4 color_u16 = float_to_int_rtn(color_f32);
					store(color_u16, scb.constant_color);
				}

				symbolic_to_physical(bsd, scb, pcb);
				return;
			}

			float error_weight_sum = prepare_error_weight_block(ctx, input_image, bsd, blk, ewb);
			float error_threshold = ctx.config.tune_db_limit
								* error_weight_sum
								* block_is_l_scale
								* block_is_la_scale;

			// Set SCB and mode errors to a very high error value
			scb.errorval = 1e30f;
			scb.error_block = 1;

			float[] best_errorvals_in_modes = new float[13];
			for (int i = 0; i < 13; i++)
			{
				best_errorvals_in_modes[i] = 1e30f;
			}

			int uses_alpha = imageblock_uses_alpha(blk);

			// Trial using 1 plane of weights and 1 partition.

			// Most of the time we test it twice, first with a mode cutoff of 0 and
			// then with the specified mode cutoff. This causes an early-out that
			// speeds up encoding of easy blocks. However, this optimization is
			// disabled for 4x4 and 5x4 blocks where it nearly always slows down the
			// compression and slightly reduces image quality.

			float[] errorval_mult = new float[2] {
				1.0f / ctx.config.tune_mode0_mse_overshoot,
				1.0f
			};

			float errorval_overshoot = 1.0f / ctx.config.tune_refinement_mse_overshoot;

			int start_trial = bsd.texel_count < (int)TUNE_MAX_TEXELS_MODE0_FASTPATH ? 1 : 0;
			for (int i = start_trial; i < 2; i++)
			{
				float errorval = compress_symbolic_block_fixed_partition_1_plane(
					decode_mode, i == 0,
					ctx.config.tune_candidate_limit,
					error_threshold * errorval_mult[i] * errorval_overshoot,
					ctx.config.tune_refinement_limit,
					bsd, 1, 0, blk, ewb, scb, &tmpbuf.planes);

				// Mode 0
				best_errorvals_in_modes[0] = errorval;
				if (errorval < (error_threshold * errorval_mult[i]))
				{
					goto END_OF_TESTS;
				}
			}

			lowest_correl = prepare_block_statistics(bsd.texel_count, blk, ewb);

			// next, test the four possible 1-partition, 2-planes modes
			for (int i = 0; i < 4; i++)
			{
				if (lowest_correl > ctx.config.tune_two_plane_early_out_limit)
				{
					continue;
				}

				if (blk->grayscale && i != 3)
				{
					continue;
				}

				if (!uses_alpha && i == 3)
				{
					continue;
				}

				float errorval = compress_symbolic_block_fixed_partition_2_planes(
					decode_mode, false,
					ctx.config.tune_candidate_limit,
					error_threshold * errorval_overshoot,
					ctx.config.tune_refinement_limit,
					bsd, 1,	// partition count
					0,	// partition index
					i,	// the color component to test a separate plane of weights for.
					blk, ewb, scb, &tmpbuf.planes);

				// Modes 7, 10 (13 is unreachable)
				if (errorval < error_threshold)
				{
					trace_add_data("exit", "quality hit");
					goto END_OF_TESTS;
				}
			}

			// find best blocks for 2, 3 and 4 partitions
			for (int partition_count = 2; partition_count <= 4; partition_count++)
			{
				int[] partition_indices_1plane = new int[2];
				int partition_index_2planes;

				find_best_partitionings(bsd, blk, ewb, partition_count,
										ctx.config.tune_partition_limit,
										&(partition_indices_1plane[0]),
										&(partition_indices_1plane[1]),
										&partition_index_2planes);

				for (int i = 0; i < 2; i++)
				{
					float errorval = compress_symbolic_block_fixed_partition_1_plane(
						decode_mode, false,
						ctx.config.tune_candidate_limit,
						error_threshold * errorval_overshoot,
						ctx.config.tune_refinement_limit,
						bsd, partition_count, partition_indices_1plane[i],
						blk, ewb, scb, &tmpbuf->planes);

					// Modes 5, 6, 8, 9, 11, 12
					best_errorvals_in_modes[3 * (partition_count - 2) + 5 + i] = errorval;
					if (errorval < error_threshold)
					{
						goto END_OF_TESTS;
					}
				}

				if (partition_count == 2 && Math.Min(best_errorvals_in_modes[5], best_errorvals_in_modes[6]) > (best_errorvals_in_modes[0] * ctx.config.tune_partition_early_out_limit))
				{
					goto END_OF_TESTS;
				}

				// Skip testing dual weight planes for:
				// * 4 partitions (can't be encoded by the format)
				if (partition_count == 4)
				{
					continue;
				}

				// * Luminance only blocks (never need for a second plane)
				if (blk.grayscale && !uses_alpha)
				{
					trace_add_data("skip", "grayscale no alpha block ");
					continue;
				}

				// * Blocks with higher component correlation than the tuning cutoff
				if (lowest_correl > ctx.config.tune_two_plane_early_out_limit)
				{
					trace_add_data("skip", "tune_two_plane_early_out_limit");
					continue;
				}

				float errorval = compress_symbolic_block_fixed_partition_2_planes(
					decode_mode,
					false,
					ctx.config.tune_candidate_limit,
					error_threshold * errorval_overshoot,
					ctx.config.tune_refinement_limit,
					bsd,
					partition_count,
					partition_index_2planes & (PARTITION_COUNT - 1),
					partition_index_2planes >> PARTITION_BITS,
					blk, ewb, scb, &tmpbuf->planes);

				// Modes 7, 10 (13 is unreachable)
				if (errorval < error_threshold)
				{
					goto END_OF_TESTS;
				}
			}

		END_OF_TESTS:
			// Compress to a physical block
			symbolic_to_physical(bsd, scb, pcb);
		}
	}
}