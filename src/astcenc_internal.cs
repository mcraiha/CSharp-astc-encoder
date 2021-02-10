namespace ASTCEnc
{
	// enumeration of all the quantization methods we support under this format.
	public enum QuantMethod
	{
		QUANT_2 = 0,
		QUANT_3 = 1,
		QUANT_4 = 2,
		QUANT_5 = 3,
		QUANT_6 = 4,
		QUANT_8 = 5,
		QUANT_10 = 6,
		QUANT_12 = 7,
		QUANT_16 = 8,
		QUANT_20 = 9,
		QUANT_24 = 10,
		QUANT_32 = 11,
		QUANT_40 = 12,
		QUANT_48 = 13,
		QUANT_64 = 14,
		QUANT_80 = 15,
		QUANT_96 = 16,
		QUANT_128 = 17,
		QUANT_160 = 18,
		QUANT_192 = 19,
		QUANT_256 = 20
	}

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
			this.texels_of_partition = new byte[4][Constants.MAX_TEXELS_PER_BLOCK];
			this.coverage_bitmaps = new ulong[4];
		}
	}

	/*
	In ASTC, we don't necessarily provide a weight for every texel.
	As such, for each block size, there are a number of patterns where some texels
	have their weights computed as a weighted average of more than 1 weight.
	As such, the codec uses a data structure that tells us: for each texel, which
	weights it is a combination of for each weight, which texels it contributes to.
	The decimation_table is this data structure.
	*/
	public struct DecimationTable
	{
		// TODO: Make these byte values
		public int texel_count;
		public int weight_count;
		public int weight_x;
		public int weight_y;
		public int weight_z;

		public byte[] texel_weight_count;	// number of indices that go into the calculation for a texel

		// The 4t and t4 tables are the same data, but transposed to allow optimal
		// data access patterns depending on how we can unroll loops
		public float[,] texel_weights_float_4t;	// the weight to assign to each weight
		public byte[,] texel_weights_4t;	// the weights that go into a texel calculation

		public float[,] texel_weights_float_t4;	// the weight to assign to each weight
		public byte[,] texel_weights_t4;	// the weights that go into a texel calculation

		public byte[,] texel_weights_int_t4;	// the weight to assign to each weight

		public byte[] weight_texel_count;	// the number of texels that a given weight contributes to
		public byte[,] weight_texel;	// the texels that the weight contributes to
		public byte[,] weights_int;	// the weights that the weight contributes to a texel.
		public float[,] weights_flt;	// the weights that the weight contributes to a texel.

		// folded data structures:
		//  * texel_weights_texel[i][j] = texel_weights[weight_texel[i][j]];
		//  * texel_weights_float_texel[i][j] = texel_weights_float[weight_texel[i][j]]
		public byte[,,] texel_weights_texel;
		public float[,,] texel_weights_float_texel;

		public DecimationTable(bool unused)
		{
			this.texel_weight_count = new byte[Constants.MAX_TEXELS_PER_BLOCK];
			this.texel_weights_float_4t = new float[4, Constants.MAX_TEXELS_PER_BLOCK];
			this.texel_weights_4t = new byte[4, Constants.MAX_TEXELS_PER_BLOCK];
			this.texel_weights_float_t4 = new float[Constants.MAX_TEXELS_PER_BLOCK, 4];
			this.texel_weights_t4 = new byte[Constants.MAX_TEXELS_PER_BLOCK, 4];
			this.texel_weights_int_t4 = new byte[Constants.MAX_TEXELS_PER_BLOCK, 4];
			this.weight_texel_count = new byte[Constants.MAX_WEIGHTS_PER_BLOCK];
			this.weight_texel = new byte[Constants.MAX_WEIGHTS_PER_BLOCK, Constants.MAX_TEXELS_PER_BLOCK];
			this.weights_int = new byte[Constants.MAX_WEIGHTS_PER_BLOCK, Constants.MAX_TEXELS_PER_BLOCK];
			this.weights_flt = new float[Constants.MAX_WEIGHTS_PER_BLOCK, Constants.MAX_TEXELS_PER_BLOCK];
			this.texel_weights_texel = new byte[Constants.MAX_WEIGHTS_PER_BLOCK, Constants.MAX_TEXELS_PER_BLOCK, 4];
			this.texel_weights_float_texel = new float[Constants.MAX_WEIGHTS_PER_BLOCK, Constants.AX_TEXELS_PER_BLOCK, 4];
		}
	}

	/**
	* @brief Metadata for single block mode for a specific BSD.
	*/
	public struct BlockMode
	{
		public sbyte decimation_mode;
		public sbyte quant_mode;
		public byte is_dual_plane : 1;
		public byte percentile_hit : 1;
		public byte percentile_always : 1;
		public short mode_index;
	}

	/**
	* @brief Metadata for single decimation mode for a specific BSD.
	*/
	public struct DecimationMode
	{
		public sbyte maxprec_1plane;
		public sbyte maxprec_2planes;
		public byte percentile_hit : 1;
		public byte percentile_always : 1;
	}

	/**
	* @brief Data tables for a single block size.
	*
	* The decimation tables store the information to apply weight grid dimension
	* reductions. We only store the decimation modes that are actually needed by
	* the current context; many of the possible modes will be unused (too many
	* weights for the current block size or disabled by heuristics). The actual
	* number of weights stored is @c decimation_mode_count, and the
	* @c decimation_modes and @c decimation_tables arrays store the active modes
	* contiguously at the start of the array. These entries are not stored in any
	* particuar order.
	*
	* The block mode tables store the unpacked block mode settings. Block modes
	* are stored in the compressed block as an 11 bit field, but for any given
	* block size and set of compressor heuristics, only a subset of the block
	* modes will be used. The actual number of block modes stored is indicated in
	* @c block_mode_count, and the @c block_modes array store the active modes
	* contiguously at the start of the array. These entries are stored in
	* incrementing "packed" value order, which doesn't mean much once unpacked.
	* To allow decompressors to reference the packed data efficiently the
	* @c block_mode_packed_index array stores the mapping between physical ID and
	* the actual remapped array index.
	*/
	public struct BlockSizeDescriptor
	{
		/**< The block X dimension, in texels. */
		public int xdim;

		/**< The block Y dimension, in texels. */
		public int ydim;

		/**< The block Z dimension, in texels. */
		public int zdim;

		/**< The block total texel count. */
		public int texel_count;


		/**< The number of stored decimation modes. */
		public int decimation_mode_count;

		/**< The active decimation modes, stored in low indices. */
		public DecimationMode[] decimation_modes;

		/**< The active decimation tables, stored in low indices. */
		public DecimationTable[] decimation_tables;


		/**< The number of stored block modes. */
		public int block_mode_count;

		/**< The active block modes, stored in low indices. */
		BlockMode[] block_modes;

		/**< The block mode array index, or -1 if not valid in current config. */
		public short[] block_mode_packed_index;


		/**< The texel count for k-means partition selection. */
		public int kmeans_texel_count;

		/**< The active texels for k-means partition selection. */
		public int[] kmeans_texels;

		/**< The partion tables for all of the possible partitions. */
		PartitionInfo[] partitions;

		public BlockSizeDescriptor(bool notUsed)
		{
			this.xdim = 0;
			this.ydim = 0;
			this.zdim = 0;
			this.texel_count = 0;
			this.decimation_mode_count = 0;
			this.decimation_modes = new DecimationMode[Constants.MAX_DECIMATION_MODES];
			this.decimation_tables = new DecimationTable[Constants.MAX_DECIMATION_MODES];
			this.block_mode_count = 0;
			this.block_modes = new BlockMode[Constants.MAX_WEIGHT_MODES];
			this.block_mode_packed_index = new short[Constants.MAX_WEIGHT_MODES];
			this.kmeans_texel_count = 0;
			this.kmeans_texels = new int[Constants.MAX_KMEANS_TEXELS];
			this.partitions = new PartitionInfo[(3 * Constants.PARTITION_COUNT) + 1]
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
		public Float4 origin_texel;
		public vfloat4 data_min;
		public vfloat4 data_max;
		public bool grayscale;

		public byte[] rgb_lns;      // 1 if RGB data are being treated as LNS
		public byte[] alpha_lns;    // 1 if Alpha data are being treated as LNS
		public byte[] nan_texel;    // 1 if the texel is a NaN-texel.
		int xpos, ypos, zpos;

		public ImageBlock(bool notUsed)
		{
			this.data_r = new float[Constants.MAX_TEXELS_PER_BLOCK];
			this.data_g = new float[Constants.MAX_TEXELS_PER_BLOCK];
			this.data_b = new float[Constants.MAX_TEXELS_PER_BLOCK];
			this.data_a = new float[Constants.MAX_TEXELS_PER_BLOCK];

			this.rgb_lns = new byte[Constants.MAX_TEXELS_PER_BLOCK];
			this.alpha_lns = new byte[Constants.MAX_TEXELS_PER_BLOCK];
			this.nan_texel = new byte[Constants.MAX_TEXELS_PER_BLOCK];

			this.xpos = 0;
			this.ypos = 0;
			this.zpos = 0;
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