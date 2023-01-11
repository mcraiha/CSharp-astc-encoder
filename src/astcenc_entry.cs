using System.Diagnostics;

namespace ASTCEnc
{
	/**
	* @brief Record of the quality tuning parameter values.
	*
	* See the @c astcenc_config structure for detailed parameter documentation.
	*
	* Note that the mse_overshoot entries are scaling factors relative to the
	* base MSE to hit db_limit. A 20% overshoot is harder to hit for a higher
	* base db_limit, so we may actually use lower ratios for the more through
	* search presets because the underlying db_limit is so much higher.
	*/
	public struct AstcencPresetConfig 
	{
		public float quality;
		public uint tune_partition_count_limit;
		public uint tune_2partition_index_limit;
		public uint tune_3partition_index_limit;
		public uint tune_4partition_index_limit;
		public uint tune_block_mode_limit;
		public uint tune_refinement_limit;
		public uint tune_candidate_limit;
		public uint tune_2partitioning_candidate_limit;
		public uint tune_3partitioning_candidate_limit;
		public uint tune_4partitioning_candidate_limit;
		public float tune_db_limit_a_base;
		public float tune_db_limit_b_base;
		public float tune_mode0_mse_overshoot;
		public float tune_refinement_mse_overshoot;
		public float tune_2_partition_early_out_limit_factor;
		public float tune_3_partition_early_out_limit_factor;
		public float tune_2_plane_early_out_limit_correlation;

		public AstcencPresetConfig(float quality, uint tune_partition_count_limit, uint tune_2partition_index_limit, uint tune_3partition_index_limit, uint tune_4partition_index_limit, uint tune_block_mode_limit, uint tune_refinement_limit, uint tune_candidate_limit, uint tune_2partitioning_candidate_limit, uint tune_3partitioning_candidate_limit, uint tune_4partitioning_candidate_limit, float tune_db_limit_a_base, float tune_db_limit_b_base, float tune_mode0_mse_overshoot, float tune_refinement_mse_overshoot, float tune_2_partition_early_out_limit_factor, float tune_3_partition_early_out_limit_factor, float tune_2_plane_early_out_limit_correlation)
		{
			this.quality = quality;
			this.tune_partition_count_limit = tune_partition_count_limit;
			this.tune_2partition_index_limit = tune_2partition_index_limit;
			this.tune_3partition_index_limit = tune_3partition_index_limit;
			this.tune_4partition_index_limit = tune_4partition_index_limit;
			this.tune_block_mode_limit = tune_block_mode_limit;
			this.tune_refinement_limit = tune_refinement_limit;
			this.tune_candidate_limit = tune_candidate_limit;
			this.tune_2partitioning_candidate_limit = tune_2partitioning_candidate_limit;
			this.tune_3partitioning_candidate_limit = tune_3partitioning_candidate_limit;
			this.tune_4partitioning_candidate_limit = tune_4partitioning_candidate_limit;
			this.tune_db_limit_a_base = tune_db_limit_a_base;
			this.tune_db_limit_b_base = tune_db_limit_b_base;
			this.tune_mode0_mse_overshoot = tune_mode0_mse_overshoot;
			this.tune_refinement_mse_overshoot = tune_refinement_mse_overshoot;
			this.tune_2_partition_early_out_limit_factor = tune_2_partition_early_out_limit_factor;
			this.tune_3_partition_early_out_limit_factor = tune_3_partition_early_out_limit_factor;
			this.tune_2_plane_early_out_limit_correlation = tune_2_plane_early_out_limit_correlation;
		}
	}

	public static class Entry
	{
		public static readonly AstcencPresetConfig[] preset_configs_high = new AstcencPresetConfig[] 
		{
			new AstcencPresetConfig(ASTCEnc.ASTCENC_PRE_FASTEST, 2, 10, 6, 4, 43, 2, 2, 2, 2, 2, 85.2f, 63.2f, 3.5f, 3.5f, 1.0f, 1.0f, 0.85f),
			new AstcencPresetConfig(ASTCEnc.ASTCENC_PRE_FAST, 3, 18, 10, 8, 55, 3, 3, 2, 2, 2, 85.2f, 63.2f, 3.5f, 3.5f, 1.0f, 1.0f, 0.90f),
			new AstcencPresetConfig(ASTCEnc.ASTCENC_PRE_MEDIUM, 4, 34, 28, 16, 77, 3, 3, 2, 2, 2, 95.0f, 70.0f, 2.5f, 2.5f, 1.1f, 1.05f, 0.95f),
			new AstcencPresetConfig(ASTCEnc.ASTCENC_PRE_THOROUGH, 4, 82, 60, 30, 94, 4, 4, 3, 2, 2, 105.0f, 77.0f, 10.0f, 10.0f, 1.35f, 1.15f, 0.97f),
			new AstcencPresetConfig(ASTCEnc.ASTCENC_PRE_VERYTHOROUGH, 4, 256, 128, 64, 98, 4, 6, 20, 14, 8, 200.0f, 200.0f, 10.0f, 10.0f, 1.6f, 1.4f, 0.98f),
			new AstcencPresetConfig(ASTCEnc.ASTCENC_PRE_EXHAUSTIVE, 4, 512, 512, 512, 100, 4, 8, 32, 32, 32, 200.0f, 200.0f, 10.0f, 10.0f, 2.0f, 2.0f, 0.99f),
		};
		
		public static readonly AstcencPresetConfig[] preset_configs_mid = new AstcencPresetConfig[] 
		{
			new AstcencPresetConfig(ASTCEnc.ASTCENC_PRE_FASTEST, 2, 10, 6, 4, 43, 2, 2, 2, 2, 2, 85.2f, 63.2f, 3.5f, 3.5f, 1.0f, 1.0f, 0.80f),
			new AstcencPresetConfig(ASTCEnc.ASTCENC_PRE_FAST, 3, 18, 12, 10, 55, 3, 3, 2, 2, 2, 85.2f, 63.2f, 3.5f, 3.5f, 1.0f, 1.0f, 0.85f),
			new AstcencPresetConfig(ASTCEnc.ASTCENC_PRE_MEDIUM, 4, 34, 28, 16, 77, 3, 3, 2, 2, 2, 95.0f, 70.0f, 3.0f, 3.0f, 1.1f, 1.05f, 0.90f),
			new AstcencPresetConfig(ASTCEnc.ASTCENC_PRE_THOROUGH, 4, 82, 60, 30, 94, 4, 4, 3, 2, 2, 105.0f, 77.0f, 10.0f, 10.0f, 1.4f, 1.2f, 0.95f),
			new AstcencPresetConfig(ASTCEnc.ASTCENC_PRE_VERYTHOROUGH, 4, 256, 128, 64, 98, 4, 6, 12, 8, 3, 200.0f, 200.0f, 10.0f, 10.0f, 1.6f, 1.4f, 0.98f),
			new AstcencPresetConfig(ASTCEnc.ASTCENC_PRE_EXHAUSTIVE, 4, 256, 256, 256, 100, 4, 8, 32, 32, 32, 200.0f, 200.0f, 10.0f, 10.0f, 2.0f, 2.0f, 0.99f),
		};

		public static readonly AstcencPresetConfig[] preset_configs_low = new AstcencPresetConfig[] 
		{
			new AstcencPresetConfig(ASTCEnc.ASTCENC_PRE_FASTEST, 2, 10, 6, 4, 40, 2, 2, 2, 2, 2, 85.0f, 63.0f, 3.5f, 3.5f, 1.0f, 1.0f, 0.80f),
			new AstcencPresetConfig(ASTCEnc.ASTCENC_PRE_FAST, 2, 18, 12, 10, 55, 3, 3, 2, 2, 2, 85.0f, 63.0f, 3.5f, 3.5f, 1.0f, 1.0f, 0.85f),
			new AstcencPresetConfig(ASTCEnc.ASTCENC_PRE_MEDIUM, 3, 34, 28, 16, 77, 3, 3, 2, 2, 2, 95.0f, 70.0f, 3.5f, 3.5f, 1.1f, 1.05f, 0.90f),
			new AstcencPresetConfig(ASTCEnc.ASTCENC_PRE_THOROUGH, 4, 82, 60, 30, 93, 4, 4, 3, 2, 2, 105.0f, 77.0f, 10.0f, 10.0f, 1.3f, 1.2f, 0.97f),
			new AstcencPresetConfig(ASTCEnc.ASTCENC_PRE_VERYTHOROUGH, 4, 256, 128, 64, 98, 4, 6, 9, 5, 2, 200.0f, 200.0f, 10.0f, 10.0f, 1.6f, 1.4f, 0.98f),
			new AstcencPresetConfig(ASTCEnc.ASTCENC_PRE_EXHAUSTIVE, 4, 256, 256, 256, 100, 4, 8, 32, 32, 32, 200.0f, 200.0f, 10.0f, 10.0f, 2.0f, 2.0f, 0.99f),
		};

		public static ASTCEncError validate_cpu_float()
		{
			float p;
			float xprec_testval = 2.51f;
			p = xprec_testval + 12582912.0f;
			float q = p - 12582912.0f;

			if (q != 3.0f)
			{
				return ASTCEncError.ASTCENC_ERR_BAD_CPU_FLOAT;
			}

			return ASTCEncError.ASTCENC_SUCCESS;
		}

		public static ASTCEncError validate_cpu_isa()
		{
			/*
			#if ASTCENC_SSE >= 41
				if (!cpu_supports_sse41())
				{
					return ASTCENC_ERR_BAD_CPU_ISA;
				}
			#endif

			#if ASTCENC_POPCNT >= 1
				if (!cpu_supports_popcnt())
				{
					return ASTCENC_ERR_BAD_CPU_ISA;
				}
			#endif

			#if ASTCENC_F16C >= 1
				if (!cpu_supports_f16c())
				{
					return ASTCENC_ERR_BAD_CPU_ISA;
				}
			#endif

			#if ASTCENC_AVX >= 2
				if (!cpu_supports_avx2())
				{
					return ASTCENC_ERR_BAD_CPU_ISA;
				}
			#endif
			*/
			return ASTCEncError.ASTCENC_SUCCESS;
		}

		public static ASTCEncError validate_profile(ASTCEncProfile profile) 
		{
			// Values in this enum are from an external user, so not guaranteed to be
			// bounded to the enum values
			switch (profile)
			{
				case ASTCEncProfile.ASTCENC_PRF_LDR_SRGB:
				case ASTCEncProfile.ASTCENC_PRF_LDR:
				case ASTCEncProfile.ASTCENC_PRF_HDR_RGB_LDR_A:
				case ASTCEncProfile.ASTCENC_PRF_HDR:
					return ASTCEncError.ASTCENC_SUCCESS;
				default:
					return ASTCEncError.ASTCENC_ERR_BAD_PROFILE;
			}
		}

		static ASTCEncError validate_block_size(uint block_x, uint block_y, uint block_z) 
		{
			// Test if this is a legal block size at all
			bool is_legal = (((block_z <= 1) && is_legal_2d_block_size(block_x, block_y)) ||
							((block_z >= 2) && is_legal_3d_block_size(block_x, block_y, block_z)));
			if (!is_legal)
			{
				return ASTCEncError.ASTCENC_ERR_BAD_BLOCK_SIZE;
			}

			// Test if this build has sufficient capacity for this block size
			bool have_capacity = (block_x * block_y * block_z) <= Constants.BLOCK_MAX_TEXELS;
			if (!have_capacity)
			{
				return ASTCEncError.ASTCENC_ERR_NOT_IMPLEMENTED;
			}

			return ASTCEncError.ASTCENC_SUCCESS;
		}

		public static ASTCEncError validate_flags(uint flags)
		{
			// Flags field must not contain any unknown flag bits
			uint exMask = ~ASTCEnc.ASTCENC_ALL_FLAGS;
			if (ASTCMath.popcount(flags & exMask) != 0)
			{
				return ASTCEncError.ASTCENC_ERR_BAD_FLAGS;
			}

			// Flags field must only contain at most a single map type
			exMask = ASTCEnc.ASTCENC_FLG_MAP_MASK
				| ASTCEnc.ASTCENC_FLG_MAP_NORMAL
				| ASTCEnc.ASTCENC_FLG_MAP_RGBM;
			if (ASTCMath.popcount(flags & exMask) > 1)
			{
				return ASTCEncError.ASTCENC_ERR_BAD_FLAGS;
			}

			return ASTCEncError.ASTCENC_SUCCESS;
		}

		public static ASTCEncError validate_compression_swz(ASTCEncSwizzleChannel swizzle) 
		{
			// Not all enum values are handled; SWZ_Z is invalid for compression
			switch (swizzle)
			{
				case ASTCEncSwizzleChannel.ASTCENC_SWZ_R:
				case ASTCEncSwizzleChannel.ASTCENC_SWZ_G:
				case ASTCEncSwizzleChannel.ASTCENC_SWZ_B:
				case ASTCEncSwizzleChannel.ASTCENC_SWZ_A:
				case ASTCEncSwizzleChannel.ASTCENC_SWZ_0:
				case ASTCEncSwizzleChannel.ASTCENC_SWZ_1:
					return ASTCEncError.ASTCENC_SUCCESS;
				default:
					return ASTCEncError.ASTCENC_ERR_BAD_SWIZZLE;
			}
		}

		public static ASTCEncError validate_compression_swizzle(ASTCEncSwizzle swizzle) 
		{
			if (validate_compression_swz(swizzle.r) == ASTCEncError.ASTCENC_ERR_BAD_SWIZZLE ||
				validate_compression_swz(swizzle.g) == ASTCEncError.ASTCENC_ERR_BAD_SWIZZLE ||
				validate_compression_swz(swizzle.b) == ASTCEncError.ASTCENC_ERR_BAD_SWIZZLE ||
				validate_compression_swz(swizzle.a) == ASTCEncError.ASTCENC_ERR_BAD_SWIZZLE)
			{
				return ASTCEncError.ASTCENC_ERR_BAD_SWIZZLE;
			}

			return ASTCEncError.ASTCENC_SUCCESS;
		}

		static ASTCEncError validate_decompression_swz(ASTCEncSwizzleChannel swizzle) 
		{
			// Values in this enum are from an external user, so not guaranteed to be
			// bounded to the enum values
			switch (swizzle)
			{
				case ASTCEncSwizzleChannel.ASTCENC_SWZ_R:
				case ASTCEncSwizzleChannel.ASTCENC_SWZ_G:
				case ASTCEncSwizzleChannel.ASTCENC_SWZ_B:
				case ASTCEncSwizzleChannel.ASTCENC_SWZ_A:
				case ASTCEncSwizzleChannel.ASTCENC_SWZ_0:
				case ASTCEncSwizzleChannel.ASTCENC_SWZ_1:
				case ASTCEncSwizzleChannel.ASTCENC_SWZ_Z:
					return ASTCEncError.ASTCENC_SUCCESS;
				default:
					return ASTCEncError.ASTCENC_ERR_BAD_SWIZZLE;
			}
		}

		public static ASTCEncError validate_decompression_swizzle(ASTCEncSwizzle swizzle) 
		{
			if (validate_decompression_swz(swizzle.r) == ASTCEncError.ASTCENC_ERR_BAD_SWIZZLE ||
				validate_decompression_swz(swizzle.g) == ASTCEncError.ASTCENC_ERR_BAD_SWIZZLE ||
				validate_decompression_swz(swizzle.b) == ASTCEncError.ASTCENC_ERR_BAD_SWIZZLE ||
				validate_decompression_swz(swizzle.a) == ASTCEncError.ASTCENC_ERR_BAD_SWIZZLE)
			{
				return ASTCEncError.ASTCENC_ERR_BAD_SWIZZLE;
			}

			return ASTCEncError.ASTCENC_SUCCESS;
		}

		public static ASTCEncError validate_config(ASTCEncConfig config) 
		{
			ASTCEncError status;

			status = validate_profile(config.profile);
			if (status != ASTCEncError.ASTCENC_SUCCESS)
			{
				return status;
			}

			status = validate_flags(config.flags);
			if (status != ASTCEncError.ASTCENC_SUCCESS)
			{
				return status;
			}

			status = validate_block_size(config.block_x, config.block_y, config.block_z);
			if (status != ASTCEncError.ASTCENC_SUCCESS)
			{
				return status;
			}

		#if ASTCENC_DECOMPRESS_ONLY
			// Decompress-only builds only support decompress-only contexts
			if (!(config.flags & ASTCENC_FLG_DECOMPRESS_ONLY))
			{
				return ASTCEncError.ASTCENC_ERR_BAD_PARAM;
			}
		#endif

			config.rgbm_m_scale = ASTCMath.max(config.rgbm_m_scale, 1.0f);

			config.tune_partition_count_limit = ASTCMath.clamp(config.tune_partition_count_limit, 1u, 4u);
			config.tune_2partition_index_limit = ASTCMath.clamp(config.tune_2partition_index_limit, 1u, Constants.BLOCK_MAX_PARTITIONINGS);
			config.tune_3partition_index_limit = ASTCMath.clamp(config.tune_3partition_index_limit, 1u, Constants.BLOCK_MAX_PARTITIONINGS);
			config.tune_4partition_index_limit = ASTCMath.clamp(config.tune_4partition_index_limit, 1u, Constants.BLOCK_MAX_PARTITIONINGS);
			config.tune_block_mode_limit = ASTCMath.clamp(config.tune_block_mode_limit, 1u, 100u);
			config.tune_refinement_limit = ASTCMath.max(config.tune_refinement_limit, 1u);
			config.tune_candidate_limit = ASTCMath.clamp(config.tune_candidate_limit, 1u, Constants.Constants.TUNE_MAX_TRIAL_CANDIDATES);
			config.tune_2partitioning_candidate_limit = ASTCMath.clamp(config.tune_2partitioning_candidate_limit, 1u, Constants.TUNE_MAX_PARTITIIONING_CANDIDATES);
			config.tune_3partitioning_candidate_limit = ASTCMath.clamp(config.tune_3partitioning_candidate_limit, 1u, Constants.TUNE_MAX_PARTITIIONING_CANDIDATES);
			config.tune_4partitioning_candidate_limit = ASTCMath.clamp(config.tune_4partitioning_candidate_limit, 1u, Constants.TUNE_MAX_PARTITIIONING_CANDIDATES);
			config.tune_db_limit = ASTCMath.max(config.tune_db_limit, 0.0f);
			config.tune_mode0_mse_overshoot = ASTCMath.max(config.tune_mode0_mse_overshoot, 1.0f);
			config.tune_refinement_mse_overshoot = ASTCMath.max(config.tune_refinement_mse_overshoot, 1.0f);
			config.tune_2_partition_early_out_limit_factor = ASTCMath.max(config.tune_2_partition_early_out_limit_factor, 0.0f);
			config.tune_3_partition_early_out_limit_factor = ASTCMath.max(config.tune_3_partition_early_out_limit_factor, 0.0f);
			config.tune_2_plane_early_out_limit_correlation = ASTCMath.max(config.tune_2_plane_early_out_limit_correlation, 0.0f);

			// Specifying a zero weight color component is not allowed; force to small value
			float max_weight = ASTCMath.max(ASTCMath.max(config.cw_r_weight, config.cw_g_weight),
										ASTCMath.max(config.cw_b_weight, config.cw_a_weight));
			if (max_weight > 0.0f)
			{
				max_weight /= 1000.0f;
				config.cw_r_weight = ASTCMath.max(config.cw_r_weight, max_weight);
				config.cw_g_weight = ASTCMath.max(config.cw_g_weight, max_weight);
				config.cw_b_weight = ASTCMath.max(config.cw_b_weight, max_weight);
				config.cw_a_weight = ASTCMath.max(config.cw_a_weight, max_weight);
			}
			// If all color components error weights are zero then return an error
			else
			{
				return ASTCEncError.ASTCENC_ERR_BAD_PARAM;
			}

			return ASTCEncError.ASTCENC_SUCCESS;
		}

		public ASTCEncError astcenc_config_init(ASTCEncProfile profile, uint block_x, uint block_y, uint block_z, float quality, uint flags, ASTCEncConfig config) 
		{
			ASTCEncError status;

			// Check basic library compatibility options here so they are checked early. Note, these checks
			// are repeated in context_alloc for cases where callers use a manually defined config struct
			status = validate_cpu_isa();
			if (status != ASTCEncError.ASTCENC_SUCCESS)
			{
				return status;
			}

			status = validate_cpu_float();
			if (status != ASTCEncError.ASTCENC_SUCCESS)
			{
				return status;
			}

			// Zero init all config fields; although most of will be over written
			config = default;

			// Process the block size
			block_z = ASTCMath.max(block_z, 1u); // For 2D blocks Z==0 is accepted, but convert to 1
			status = validate_block_size(block_x, block_y, block_z);
			if (status != ASTCEncError.ASTCENC_SUCCESS)
			{
				return status;
			}

			config.block_x = block_x;
			config.block_y = block_y;
			config.block_z = block_z;

			float texels = (float)(block_x * block_y * block_z);
			float ltexels = logf(texels) / logf(10.0f);

			// Process the performance quality level or preset; note that this must be done before we
			// process any additional settings, such as color profile and flags, which may replace some of
			// these settings with more use case tuned values
			if (quality < ASTCEnc.ASTCENC_PRE_FASTEST ||
				quality > ASTCEnc.ASTCENC_PRE_EXHAUSTIVE)
			{
				return ASTCEncError.ASTCENC_ERR_BAD_QUALITY;
			}

			AstcencPresetConfig[] preset_configs;
			int texels_int = (int)(block_x * block_y * block_z);
			if (texels_int < 25)
			{
				preset_configs = preset_configs_high;
			}
			else if (texels_int < 64)
			{
				preset_configs = preset_configs_mid;
			}
			else
			{
				preset_configs = preset_configs_low;
			}

			// Determine which preset to use, or which pair to interpolate
			int start;
			int end;
			for (end = 0; end < preset_configs.Length; end++)
			{
				if (preset_configs[end].quality >= quality)
				{
					break;
				}
			}

			start = end == 0 ? 0 : end - 1;

			// Start and end node are the same - so just transfer the values.
			if (start == end)
			{
				config.tune_partition_count_limit = preset_configs[start].tune_partition_count_limit;
				config.tune_2partition_index_limit = preset_configs[start].tune_2partition_index_limit;
				config.tune_3partition_index_limit = preset_configs[start].tune_3partition_index_limit;
				config.tune_4partition_index_limit = preset_configs[start].tune_4partition_index_limit;
				config.tune_block_mode_limit = preset_configs[start].tune_block_mode_limit;
				config.tune_refinement_limit = preset_configs[start].tune_refinement_limit;
				config.tune_candidate_limit = ASTCMath.min(preset_configs[start].tune_candidate_limit, Constants.TUNE_MAX_TRIAL_CANDIDATES);
				config.tune_2partitioning_candidate_limit = ASTCMath.min(preset_configs[start].tune_2partitioning_candidate_limit, Constants.TUNE_MAX_PARTITIIONING_CANDIDATES);
				config.tune_3partitioning_candidate_limit = ASTCMath.min(preset_configs[start].tune_3partitioning_candidate_limit, Constants.TUNE_MAX_PARTITIIONING_CANDIDATES);
				config.tune_4partitioning_candidate_limit = ASTCMath.min(preset_configs[start].tune_4partitioning_candidate_limit, Constants.TUNE_MAX_PARTITIIONING_CANDIDATES);
				config.tune_db_limit = ASTCMath.max(preset_configs[start].tune_db_limit_a_base - 35 * ltexels,
												preset_configs[start].tune_db_limit_b_base - 19 * ltexels);

				config.tune_mode0_mse_overshoot = preset_configs[start].tune_mode0_mse_overshoot;
				config.tune_refinement_mse_overshoot = preset_configs[start].tune_refinement_mse_overshoot;

				config.tune_2_partition_early_out_limit_factor = preset_configs[start].tune_2_partition_early_out_limit_factor;
				config.tune_3_partition_early_out_limit_factor = preset_configs[start].tune_3_partition_early_out_limit_factor;
				config.tune_2_plane_early_out_limit_correlation = preset_configs[start].tune_2_plane_early_out_limit_correlation;
			}
			// Start and end node are not the same - so interpolate between them
			else
			{
				AstcencPresetConfig node_a = preset_configs[start];
				AstcencPresetConfig node_b = preset_configs[end];

				float wt_range = node_b.quality - node_a.quality;
				Debug.Assert(wt_range > 0);

				// Compute interpolation factors
				float wt_node_a = (node_b.quality - quality) / wt_range;
				float wt_node_b = (quality - node_a.quality) / wt_range;

				float LERP(float a, float b)
				{
					return (a * wt_node_a) + (b * wt_node_b);
				}

				int LERPI(float a, float b)
				{
					return ASTCMath.flt2int_rtn((float)a * wt_node_a + (float)b * wt_node_b);
				}

				uint LERPUI(float a, float b) 
				{
					return (uint)(LERPI(a, b));
				}

				config.tune_partition_count_limit = (uint)LERPI(node_a.tune_partition_count_limit, node_b.tune_partition_count_limit);
				config.tune_2partition_index_limit = (uint)LERPI(node_a.tune_2partition_index_limit, node_b.tune_2partition_index_limit);
				config.tune_3partition_index_limit = (uint)LERPI(node_a.tune_3partition_index_limit, node_b.tune_3partition_index_limit);
				config.tune_4partition_index_limit = (uint)LERPI(node_a.tune_4partition_index_limit, node_b.tune_4partition_index_limit);
				config.tune_block_mode_limit = (uint)LERPI(node_a.tune_block_mode_limit, node_b.tune_block_mode_limit);
				config.tune_refinement_limit = (uint)LERPI(node_a.tune_refinement_limit, node_b.tune_refinement_limit);
				config.tune_candidate_limit = ASTCMath.min(LERPUI(node_a.tune_candidate_limit, node_b.tune_candidate_limit),
														Constants.TUNE_MAX_TRIAL_CANDIDATES);
				config.tune_2partitioning_candidate_limit = ASTCMath.min(LERPUI(node_a.tune_2partitioning_candidate_limit, node_b.tune_2partitioning_candidate_limit),
																	Constants.BLOCK_MAX_PARTITIONINGS);
				config.tune_3partitioning_candidate_limit = ASTCMath.min(LERPUI(node_a.tune_3partitioning_candidate_limit, node_b.tune_3partitioning_candidate_limit),
																	Constants.BLOCK_MAX_PARTITIONINGS);
				config.tune_4partitioning_candidate_limit = ASTCMath.min(LERPUI(node_a.tune_4partitioning_candidate_limit, node_b.tune_4partitioning_candidate_limit),
																	Constants.BLOCK_MAX_PARTITIONINGS);
				config.tune_db_limit = ASTCMath.max(LERP(node_a.tune_db_limit_a_base, node_b.tune_db_limit_a_base) - 35 * ltexels,
												LERP(node_a.tune_db_limit_b_base, node_b.tune_db_limit_b_base) - 19 * ltexels);

				config.tune_mode0_mse_overshoot = LERP(node_a.tune_mode0_mse_overshoot, node_b.tune_mode0_mse_overshoot);
				config.tune_refinement_mse_overshoot = LERP(node_a.tune_refinement_mse_overshoot, node_b.tune_refinement_mse_overshoot);

				config.tune_2_partition_early_out_limit_factor = LERP(node_a.tune_2_partition_early_out_limit_factor, node_b.tune_2_partition_early_out_limit_factor);
				config.tune_3_partition_early_out_limit_factor = LERP(node_a.tune_3_partition_early_out_limit_factor, node_b.tune_3_partition_early_out_limit_factor);
				config.tune_2_plane_early_out_limit_correlation = LERP(node_a.tune_2_plane_early_out_limit_correlation, node_b.tune_2_plane_early_out_limit_correlation);
			}

			// Set heuristics to the defaults for each color profile
			config.cw_r_weight = 1.0f;
			config.cw_g_weight = 1.0f;
			config.cw_b_weight = 1.0f;
			config.cw_a_weight = 1.0f;

			config.a_scale_radius = 0;

			config.rgbm_m_scale = 0.0f;

			config.profile = profile;

			// Values in this enum are from an external user, so not guaranteed to be
			// bounded to the enum values
			switch (profile)
			{
				case ASTCEncProfile.ASTCENC_PRF_LDR:
				case ASTCEncProfile.ASTCENC_PRF_LDR_SRGB:
					break;
				case ASTCEncProfile.ASTCENC_PRF_HDR_RGB_LDR_A:
				case ASTCEncProfile.ASTCENC_PRF_HDR:
					config.tune_db_limit = 999.0f;
					break;
				default:
					return ASTCEncError.ASTCENC_ERR_BAD_PROFILE;
			}

			// Flags field must not contain any unknown flag bits
			status = validate_flags(flags);
			if (status != ASTCEncError.ASTCENC_SUCCESS)
			{
				return status;
			}

			if ((flags & ASTCEnc.ASTCENC_FLG_MAP_NORMAL) == 1)
			{
				// Normal map encoding uses L+A blocks, so allow one more partitioning
				// than normal. We need need fewer bits for endpoints, so more likely
				// to be able to use more partitions than an RGB/RGBA block
				config.tune_partition_count_limit = ASTCMath.min(config.tune_partition_count_limit + 1u, 4u);

				config.cw_g_weight = 0.0f;
				config.cw_b_weight = 0.0f;
				config.tune_2_partition_early_out_limit_factor *= 1.5f;
				config.tune_3_partition_early_out_limit_factor *= 1.5f;
				config.tune_2_plane_early_out_limit_correlation = 0.99f;

				// Normals are prone to blocking artifacts on smooth curves
				// so force compressor to try harder here ...
				config.tune_db_limit *= 1.03f;
			}
			else if ((flags & ASTCEnc.ASTCENC_FLG_MAP_MASK) == 1)
			{
				// Masks are prone to blocking artifacts on mask edges
				// so force compressor to try harder here ...
				config.tune_db_limit *= 1.03f;
			}
			else if ((flags & ASTCEnc.ASTCENC_FLG_MAP_RGBM) == 1)
			{
				config.rgbm_m_scale = 5.0f;
				config.cw_a_weight = 2.0f * config.rgbm_m_scale;
			}
			else // (This is color data)
			{
				// This is a very basic perceptual metric for RGB color data, which weights error
				// significance by the perceptual luminance contribution of each color channel. For
				// luminance the usual weights to compute luminance from a linear RGB value are as
				// follows:
				//
				//     l = r * 0.3 + g * 0.59 + b * 0.11
				//
				// ... but we scale these up to keep a better balance between color and alpha. Note
				// that if the content is using alpha we'd recommend using the -a option to weight
				// the color contribution by the alpha transparency.
				if ((flags & ASTCEnc.ASTCENC_FLG_USE_PERCEPTUAL) == 1)
				{
					config.cw_r_weight = 0.30f * 2.25f;
					config.cw_g_weight = 0.59f * 2.25f;
					config.cw_b_weight = 0.11f * 2.25f;
				}
			}
			config.flags = flags;

			return ASTCEncError.ASTCENC_SUCCESS;
		}

		public ASTCEncError astcenc_context_alloc(
			ASTCEncConfig config,
			uint thread_count,
			astcenc_context context
		) {
			ASTCEncError status;

			status = validate_cpu_isa();
			if (status != ASTCEncError.ASTCENC_SUCCESS)
			{
				return status;
			}

			status = validate_cpu_float();
			if (status != ASTCEncError.ASTCENC_SUCCESS)
			{
				return status;
			}

			if (thread_count == 0)
			{
				return ASTCEncError.ASTCENC_ERR_BAD_PARAM;
			}

		#if ASTCENC_DIAGNOSTICS
			// Force single threaded compressor use in diagnostic mode.
			if (thread_count != 1)
			{
				return ASTCEncError.ASTCENC_ERR_BAD_PARAM;
			}
		#endif

			astcenc_context ctxo = new astcenc_context();
			astcenc_contexti ctx = ctxo.context;
			ctx.thread_count = thread_count;
			ctx.config = config;
			ctx.working_buffers = null;

			// These are allocated per-compress, as they depend on image size
			ctx.input_alpha_averages = null;

			// Copy the config first and validate the copy (we may modify it)
			status = validate_config(ctx.config);
			if (status != ASTCEncError.ASTCENC_SUCCESS)
			{
				return status;
			}

			ctx.bsd = aligned_malloc<block_size_descriptor>(sizeof(block_size_descriptor), ASTCENC_VECALIGN);
			bool can_omit_modes = (bool)(config.flags & ASTCENC_FLG_SELF_DECOMPRESS_ONLY);
			init_block_size_descriptor(config.block_x, config.block_y, config.block_z,
									can_omit_modes,
									config.tune_partition_count_limit,
									(float)(config.tune_block_mode_limit) / 100.0f,
									ctx.bsd);

		#if !ASTCENC_DECOMPRESS_ONLY
			// Do setup only needed by compression
			if (!(status & ASTCEnc.ASTCENC_FLG_DECOMPRESS_ONLY))
			{
				// Turn a dB limit into a per-texel error for faster use later
				if ((ctx.config.profile == ASTCEncProfile.ASTCENC_PRF_LDR) || (ctx.config.profile == ASTCEncProfile.ASTCENC_PRF_LDR_SRGB))
				{
					ctx.config.tune_db_limit = astc::pow(0.1f, ctx.config.tune_db_limit * 0.1f) * 65535.0f * 65535.0f;
				}
				else
				{
					ctx.config.tune_db_limit = 0.0f;
				}

				size_t worksize = sizeof(compression_working_buffers) * thread_count;
				ctx.working_buffers = aligned_malloc<compression_working_buffers>(worksize, ASTCENC_VECALIGN);
				static_assert((sizeof(compression_working_buffers) % ASTCENC_VECALIGN) == 0,
							"compression_working_buffers size must be multiple of vector alignment");
				if (!ctx.working_buffers)
				{
					
					return ASTCEncError.ASTCENC_ERR_OUT_OF_MEM;
				}
			}
		#endif

		#if ASTCENC_DIAGNOSTICS
			ctx.trace_log = new TraceLog(ctx.config.trace_file_path);
			if (!ctx.trace_log->m_file)
			{
				return ASTCENC_ERR_DTRACE_FAILURE;
			}

			trace_add_data("block_x", config.block_x);
			trace_add_data("block_y", config.block_y);
			trace_add_data("block_z", config.block_z);
		#endif

			*context = ctxo;

		#if !ASTCENC_DECOMPRESS_ONLY
			prepare_angular_tables();
		#endif

			return ASTCEncError.ASTCENC_SUCCESS;
		}
	}
}