namespace ASTCEnc
{
	public static class Constants
	{
		/* ============================================================================
		Constants
		============================================================================ */
		public const int MAX_TEXELS_PER_BLOCK = 216;
		public const int MAX_KMEANS_TEXELS = 64;
		public const int MAX_WEIGHTS_PER_BLOCK = 64;
		public const int PLANE2_WEIGHTS_OFFSET = (MAX_WEIGHTS_PER_BLOCK / 2);
		public const int MIN_WEIGHT_BITS_PER_BLOCK = 24;
		public const int MAX_WEIGHT_BITS_PER_BLOCK = 96;
		public const int PARTITION_BITS = 10;
		public const int PARTITION_COUNT = (1 << PARTITION_BITS);

		// the sum of weights for one texel.
		public const int TEXEL_WEIGHT_SUM = 16;
		public const int MAX_DECIMATION_MODES = 87;
		public const int MAX_WEIGHT_MODES = 2048;

		// A high default error value
		public const float ERROR_CALC_DEFAULT = 1e30f;

		/* ============================================================================
		Compile-time tuning parameters
		============================================================================ */
		// The max texel count in a block which can try the one partition fast path.
		// Default: enabled for 4x4 and 5x4 blocks.
		public const uint TUNE_MAX_TEXELS_MODE0_FASTPATH = 24;

		// The maximum number of candidate encodings returned for each encoding mode.
		// Default: depends on quality preset
		public const uint TUNE_MAX_TRIAL_CANDIDATES = 4;
	}

	public struct PartitionInfo
	{
		public int partition_count;
		public byte[] texels_per_partition;
		public byte[] partition_of_texel;
		public byte[][] texels_of_partition;
		public ulong[] coverage_bitmaps;

		public PartitionInfo()
		{
			this.texels_per_partition = new byte[4];
			this.partition_of_texel = new byte[Constants.MAX_TEXELS_PER_BLOCK];
			this.texels_of_partition = new byte[4][MAX_TEXELS_PER_BLOCK];
			this.coverage_bitmaps = new ulong[4];
		}
	}

	// data structure representing one block of an image.
	// it is expanded to float prior to processing to save some computation time
	// on conversions to/from uint8_t (this also allows us to handle HDR textures easily)
	public struct ImageBlock
	{
		public float[] data_r;  // the data that we will compress, either linear or LNS (0..65535 in both cases)
		public float[] data_g;
		public float[] data_b;
		public float[] data_a;

		// TODO: Migrate to vfloat4
		public float4 origin_texel;
		public vfloat4 data_min;
		public vfloat4 data_max;
		public bool    grayscale;

		public byte[] rgb_lns;      // 1 if RGB data are being treated as LNS
		public byte[] alpha_lns;    // 1 if Alpha data are being treated as LNS
		public byte[] nan_texel;    // 1 if the texel is a NaN-texel.
		int xpos, ypos, zpos;

		public ImageBlock()
		{
			this.data_r = new float[Constants.MAX_TEXELS_PER_BLOCK];
			this.data_g = new float[Constants.MAX_TEXELS_PER_BLOCK];
			this.data_b = new float[Constants.MAX_TEXELS_PER_BLOCK];
			this.data_a = new float[Constants.MAX_TEXELS_PER_BLOCK];

			this.rgb_lns = new byte[Constants.MAX_TEXELS_PER_BLOCK];
			this.alpha_lns = new byte[Constants.MAX_TEXELS_PER_BLOCK];
			this.nan_texel = new byte[Constants.MAX_TEXELS_PER_BLOCK];
		}
	}

	public struct ErrorWeightBlock
	{
		public Float4[] error_weights;
		public float[] texel_weight;
		public float[] texel_weight_gba;
		public float[] texel_weight_rba;
		public float[] texel_weight_rga;
		public float[] texel_weight_rgb;

		public float[] texel_weight_rg;
		public float[] texel_weight_rb;
		public float[] texel_weight_gb;
		public float[] texel_weight_ra;

		public float[] texel_weight_r;
		public float[] texel_weight_g;
		public float[] texel_weight_b;
		public float[] texel_weight_a;

		public ErrorWeightBlock()
		{
			this.error_weights = new Float4[Constants.MAX_TEXELS_PER_BLOCK];
			this.texel_weight = new float[Constants.MAX_TEXELS_PER_BLOCK];

			this.texel_weight_gba = new float[Constants.MAX_TEXELS_PER_BLOCK];
			this.texel_weight_rba = new float[Constants.MAX_TEXELS_PER_BLOCK];
			this.texel_weight_rga = new float[Constants.MAX_TEXELS_PER_BLOCK];
			this.texel_weight_rgb = new float[Constants.MAX_TEXELS_PER_BLOCK];

			this.texel_weight_rg = new float[Constants.MAX_TEXELS_PER_BLOCK];
			this.texel_weight_rb = new float[Constants.MAX_TEXELS_PER_BLOCK];
			this.texel_weight_gb = new float[Constants.MAX_TEXELS_PER_BLOCK];
			this.texel_weight_ra = new float[Constants.MAX_TEXELS_PER_BLOCK];

			this.texel_weight_r = new float[Constants.MAX_TEXELS_PER_BLOCK];
			this.texel_weight_g = new float[Constants.MAX_TEXELS_PER_BLOCK];
			this.texel_weight_b = new float[Constants.MAX_TEXELS_PER_BLOCK];
			this.texel_weight_a = new float[Constants.MAX_TEXELS_PER_BLOCK];
		}
	}
}