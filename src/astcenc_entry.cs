
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
	public struct AstcencPresetConfig {
		public float quality;
		public uint tune_partition_count_limit;
		public uint tune_partition_index_limit;
		public uint tune_block_mode_limit;
		public uint tune_refinement_limit;
		public uint tune_candidate_limit;
		public float tune_db_limit_a_base;
		public float tune_db_limit_b_base;
		public float tune_mode0_mse_overshoot;
		public float tune_refinement_mse_overshoot;
		public float tune_partition_early_out_limit;
		public float tune_two_plane_early_out_limit;

		public AstcencPresetConfig(float quality, uint tune_partition_count_limit, uint tune_partition_index_limit, uint tune_block_mode_limit, uint tune_refinement_limit, uint tune_candidate_limit, float tune_db_limit_a_base, float tune_db_limit_b_base, float tune_mode0_mse_overshoot, float tune_refinement_mse_overshoot, float tune_partition_early_out_limit, float tune_two_plane_early_out_limit)
		{
			this.quality = quality;
			this.tune_partition_count_limit = tune_partition_count_limit;
			this.tune_partition_index_limit = tune_partition_index_limit;
			this.tune_block_mode_limit = tune_block_mode_limit;
			this.tune_refinement_limit = tune_refinement_limit;
			this.tune_candidate_limit = tune_candidate_limit;
			this.tune_db_limit_a_base = tune_db_limit_a_base;
			this.tune_db_limit_b_base = tune_db_limit_b_base;
			this.tune_mode0_mse_overshoot = tune_mode0_mse_overshoot;
			this.tune_refinement_mse_overshoot = tune_refinement_mse_overshoot;
			this.tune_partition_early_out_limit = tune_partition_early_out_limit;
			this.tune_two_plane_early_out_limit = tune_two_plane_early_out_limit;
		}
	}

	public static class Entry
	{
		public static readonly AstcencPresetConfig[] preset_configs = new AstcencPresetConfig[] {
			new AstcencPresetConfig(ASTCEnc.ASTCENC_PRE_FASTEST, 4, 2, 25, 1, 1, 75, 53, 1.0f, 1.0f, 1.0f, 0.5f),
			new AstcencPresetConfig(ASTCEnc.ASTCENC_PRE_FAST,4, 4, 50, 1, 2, 85, 63, 2.5f, 2.5f, 1.0f, 0.5f),
			new AstcencPresetConfig(ASTCEnc.ASTCENC_PRE_MEDIUM, 4, 25, 75, 2, 2,  95, 70, 1.75f, 1.75f, 1.2f, 0.75f),
			new AstcencPresetConfig(ASTCEnc.ASTCENC_PRE_THOROUGH, 4, 75, 92, 4, 4, 105, 77, 10.0f, 10.0f, 2.5f, 0.95f),
			new AstcencPresetConfig(ASTCEnc.ASTCENC_PRE_EXHAUSTIVE, 4, 1024, 100, 4, 4, 200, 200, 10.0f, 10.0f, 10.0f, 0.99f)
		};
	}
}