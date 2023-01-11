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

	public struct QuantizationAndTransferTable
	{
		/** The quantization level used */
		public QuantMethod method;
		/** The unscrambled unquantized value. */
		public float[] unquantized_value_unsc;
		/** The scrambling order: value[map[i]] == value_unsc[i] */
		public int[] scramble_map;
		/** The scrambled unquantized values. */
		public byte[] unquantized_value;
		/**
		* An encoded table of previous-and-next weight values, indexed by the
		* current unquantized value.
		*  * bits 7:0 = previous-index, unquantized
		*  * bits 15:8 = next-index, unquantized
		*  * bits 23:16 = previous-index, quantized
		*  * bits 31:24 = next-index, quantized
		*/
		public uint[] prev_next_values;

		public QuantizationAndTransferTable(bool unused)
		{
			this.method = default;
			this.unquantized_value_unsc = new float[33];
			this.scramble_map = new int[32];
			this.unquantized_value = new byte[32];
			this.prev_next_values = new uint[65];
		}
	}

	public enum EndpointFormats
	{
		FMT_LUMINANCE = 0,
		FMT_LUMINANCE_DELTA = 1,
		FMT_HDR_LUMINANCE_LARGE_RANGE = 2,
		FMT_HDR_LUMINANCE_SMALL_RANGE = 3,
		FMT_LUMINANCE_ALPHA = 4,
		FMT_LUMINANCE_ALPHA_DELTA = 5,
		FMT_RGB_SCALE = 6,
		FMT_HDR_RGB_SCALE = 7,
		FMT_RGB = 8,
		FMT_RGB_DELTA = 9,
		FMT_RGB_SCALE_ALPHA = 10,
		FMT_HDR_RGB = 11,
		FMT_RGBA = 12,
		FMT_RGBA_DELTA = 13,
		FMT_HDR_RGB_LDR_ALPHA = 14,
		FMT_HDR_RGBA = 15
	}

	/**
	* @brief A symbolic representation of a compressed block.
	*
	* The symbolic representation stores the unpacked content of a single
	* @c physical_compressed_block, in a form which is much easier to access for
	* the rest of the compressor code.
	*/
	public struct SymbolicCompressedBlock
	{
		/** @brief The block type, one of the @c SYM_BTYPE_* constants. */
		public byte block_type;

		/** @brief The number of partitions; valid for @c NONCONST blocks. */
		public byte partition_count;

		/** @brief Non-zero if the color formats matched; valid for @c NONCONST blocks. */
		public byte color_formats_matched;

		/** @brief The plane 2 color component, or -1 if single plane; valid for @c NONCONST blocks. */
		public sbyte plane2_component;

		/** @brief The block mode; valid for @c NONCONST blocks. */
		public ushort block_mode;

		/** @brief The partition index; valid for @c NONCONST blocks if 2 or more partitions. */
		public ushort partition_index;

		/** @brief The endpoint color formats for each partition; valid for @c NONCONST blocks. */
		public byte[] color_formats = new byte[Constants.BLOCK_MAX_PARTITIONS];

		/** @brief The endpoint color quant mode; valid for @c NONCONST blocks. */
		public QuantMethod quant_mode;

		/** @brief The error of the current encoding; valid for @c NONCONST blocks. */
		public float errorval;


		/** @brief The constant color; valid for @c CONST blocks. */
		public int[] constant_color = new int[Constants.BLOCK_MAX_COMPONENTS];

		/** @brief The quantized endpoint color pairs; valid for @c NONCONST blocks. */
		public byte[,] color_values = new byte[Constants.BLOCK_MAX_PARTITIONS, 8];

		/** @brief The quantized and decimated weights.
		*
		* Weights are stored in the 0-64 unpacked range allowing them to be used
		* directly in encoding passes without per-use unpacking. Packing happens
		* when converting to/from the physical bitstream encoding.
		*
		* If dual plane, the second plane starts at @c weights[WEIGHTS_PLANE2_OFFSET].
		*/
		public byte[] weights = new byte[Constants.BLOCK_MAX_WEIGHTS];

		

		public SymbolicCompressedBlock()
		{
			
		}

		/**
		* @brief Get the weight quantization used by this block mode.
		*
		* @return The quantization level.
		*/
		public QuantMethod get_color_quant_mode()
		{
			return this.quant_mode;
		}
	}

	public struct PhysicalCompressedBlock
	{
		public byte[] data = new byte[16];
		public PhysicalCompressedBlock()
		{

		}
	}

	public static class Constants
	{
		/* ============================================================================
		Constants
		============================================================================ */
		public const uint ASTCENC_BLOCK_MAX_TEXELS = 216;

		/** @brief The maximum number of texels a block can support (6x6x6 block). */
		public const uint BLOCK_MAX_TEXELS = ASTCENC_BLOCK_MAX_TEXELS;
		
		/** @brief The maximum number of components a block can support. */
		public const uint BLOCK_MAX_COMPONENTS = 4;

		/** @brief The maximum number of partitions a block can support. */
		public const uint BLOCK_MAX_PARTITIONS = 4;

		/** @brief The number of partitionings, per partition count, suported by the ASTC format. */
		public const uint BLOCK_MAX_PARTITIONINGS= 1024;

		/** @brief The maximum number of weights used during partition selection for texel clustering. */
		public const byte BLOCK_MAX_KMEANS_TEXELS = 64;

		/** @brief The maximum number of weights a block can support. */
		public const uint BLOCK_MAX_WEIGHTS = 64;

		/** @brief The maximum number of weights a block can support per plane in 2 plane mode. */
		public const uint BLOCK_MAX_WEIGHTS_2PLANE = BLOCK_MAX_WEIGHTS / 2;

		/** @brief The minimum number of weight bits a candidate encoding must encode. */
		public const uint BLOCK_MIN_WEIGHT_BITS = 24;

		/** @brief The maximum number of weight bits a candidate encoding can encode. */
		public const uint BLOCK_MAX_WEIGHT_BITS = 96;

		/** @brief The index indicating a bad (unused) block mode in the remap array. */
		public const ushort BLOCK_BAD_BLOCK_MODE = 0xFFFFu;

		/** @brief The index indicating a bad (unused) partitioning in the remap array. */
		public const ushort BLOCK_BAD_PARTITIONING = 0xFFFFu;

		/** @brief The number of partition index bits supported by the ASTC format . */
		public const uint PARTITION_INDEX_BITS = 10 ;

		/** @brief The offset of the plane 2 weights in shared weight arrays. */
		public const uint WEIGHTS_PLANE2_OFFSET = BLOCK_MAX_WEIGHTS_2PLANE;

		/** @brief The sum of quantized weights for one texel. */
		public const float WEIGHTS_TEXEL_SUM = 16.0f;

		/** @brief The number of block modes supported by the ASTC format. */
		public const uint WEIGHTS_MAX_BLOCK_MODES = 2048;

		/** @brief The number of weight grid decimation modes supported by the ASTC format. */
		public const uint WEIGHTS_MAX_DECIMATION_MODES = 87;

		/** @brief The high default error used to initialize error trackers. */
		public const float ERROR_CALC_DEFAULT = 1e30f;

		/**
		* @brief The minimum texel count for a block to use the one partition fast path.
		*
		* This setting skips 4x4 and 5x4 block sizes.
		*/
		public const uint TUNE_MIN_TEXELS_MODE0_FASTPATH = 24;

		/**
		* @brief The maximum number of candidate encodings tested for each encoding mode.
		*
		* This can be dynamically reduced by the compression quality preset.
		*/
		public const uint TUNE_MAX_TRIAL_CANDIDATES = 8;

		/**
		* @brief The maximum number of candidate partitionings tested for each encoding mode.
		*
		* This can be dynamically reduced by the compression quality preset.
		*/
		public const uint TUNE_MAX_PARTITIIONING_CANDIDATES = 32;

		/**
		* @brief The maximum quant level using full angular endpoint search method.
		*
		* The angular endpoint search is used to find the min/max weight that should
		* be used for a given quantization level. It is effective but expensive, so
		* we only use it where it has the most value - low quant levels with wide
		* spacing. It is used below TUNE_MAX_ANGULAR_QUANT (inclusive). Above this we
		* assume the min weight is 0.0f, and the max weight is 1.0f.
		*
		* Note the angular algorithm is vectorized, and using QUANT_12 exactly fills
		* one 8-wide vector. Decreasing by one doesn't buy much performance, and
		* increasing by one is disproportionately expensive.
		*/
		public const uint TUNE_MAX_ANGULAR_QUANT = 7; /* QUANT_12 */

		public const int ASTCENC_SIMD_WIDTH = 4;
	}

	public struct PartitionMetrics
	{
		public vfloat4 avg;
		public vfloat4 dir;
	};

	public struct PartitionLines3
	{
		public Line3 uncor_line;
		public Line3 samec_line;

		public ProcessedLine3 uncor_pline;
		public ProcessedLine3 samec_pline;

		public float uncor_line_len;
		public float samec_line_len;
	}


	public struct PartitionInfo
	{
		public ushort partition_count;
		public ushort partition_index;
		public byte[] partition_texel_count = new byte[Constants.BLOCK_MAX_PARTITIONS];
		public byte[] partition_of_texel = new byte[Constants.BLOCK_MAX_TEXELS];
		public byte[][] texels_of_partition = new byte[Constants.BLOCK_MAX_PARTITIONS][];


		public PartitionInfo()
		{
			for (int i = 0; i < Constants.BLOCK_MAX_PARTITIONS; i++)
			{
				texels_of_partition[i] = new byte[Constants.BLOCK_MAX_TEXELS];
			}
		}
	}

	public struct DecimationInfo
	{
		/** @brief The total number of texels in the block. */
		public byte texel_count;

		/** @brief The maximum number of stored weights that contribute to each texel, between 1 and 4. */
		public byte max_texel_weight_count;

		/** @brief The total number of weights stored. */
		public byte weight_count;

		/** @brief The number of stored weights in the X dimension. */
		public byte weight_x;

		/** @brief The number of stored weights in the Y dimension. */
		public byte weight_y;

		/** @brief The number of stored weights in the Z dimension. */
		public byte weight_z;

		/** @brief The number of stored weights that contribute to each texel, between 1 and 4. */
		public byte[] texel_weight_count = new byte[Constants.BLOCK_MAX_TEXELS];

		/** @brief The weight index of the N weights that need to be interpolated for each texel. */
		public byte[,] texel_weights_4t = new byte[4, Constants.BLOCK_MAX_TEXELS];

		/** @brief The bilinear interpolation weighting of the N input weights for each texel, between 0 and 16. */
		public byte[,] texel_weights_int_4t = new byte[4, Constants.BLOCK_MAX_TEXELS];

		/** @brief The bilinear interpolation weighting of the N input weights for each texel, between 0 and 1. */
		public float[,] texel_weights_float_4t = new float[4, Constants.BLOCK_MAX_TEXELS];

		/** @brief The number of texels that each stored weight contributes to. */
		public byte[] weight_texel_count = new byte[Constants.BLOCK_MAX_WEIGHTS];

		/** @brief The list of weights that contribute to each texel. */
		public byte[,] weight_texel = new byte[Constants.BLOCK_MAX_TEXELS, Constants.BLOCK_MAX_WEIGHTS];

		/** @brief The list of weight indices that contribute to each texel. */
		public float[,] weights_flt = new float[Constants.BLOCK_MAX_TEXELS, Constants.BLOCK_MAX_WEIGHTS];

		/**
		* @brief Folded structure for faster access:
		*     texel_weights_texel[i][j][.] = texel_weights[.][weight_texel[i][j]]
		*/
		public byte[,,] texel_weights_texel = new byte[Constants.BLOCK_MAX_WEIGHTS, Constants.BLOCK_MAX_TEXELS, 4];

		/**
		* @brief Folded structure for faster access:
		*     texel_weights_float_texel[i][j][.] = texel_weights_float[.][weight_texel[i][j]]
		*/
		public float[,,] texel_weights_float_texel = new float[Constants.BLOCK_MAX_WEIGHTS, Constants.BLOCK_MAX_TEXELS, 4];
	}


	/**
	* @brief Metadata for single block mode for a specific BSD.
	*/
	public struct BlockMode
	{
		/** @brief The block mode index in the ASTC encoded form. */
		public ushort mode_index;

		/** @brief The decimation mode index in the compressor reindexed list. */
		public byte decimation_mode;

		/** @brief The weight quantization used by this block mode. */
		public byte quant_mode;

		/** @brief The weight quantization used by this block mode. */
		public byte weight_bits;

		/** @brief Is a dual weight plane used by this block mode? */
		public bool is_dual_plane;

		/**
		* @brief Get the weight quantization used by this block mode.
		*
		* @return The quantization level.
		*/
		public QuantMethod get_weight_quant_mode()
		{
			return (QuantMethod)(this.quant_mode);
		}
	}

	/**
	* @brief Metadata for single decimation mode for a specific BSD.
	*/
	public struct DecimationMode
	{
		/** @brief The max weight precision for 1 plane, or -1 if not supported. */
		public sbyte maxprec_1plane;

		/** @brief The max weight precision for 2 planes, or -1 if not supported. */
		public sbyte maxprec_2planes;

		/**
		* @brief Bitvector indicating weight quant modes used by active 1 plane block modes.
		*
		* Bit 0 = QUANT_2, Bit 1 = QUANT_3, etc.
		*/
		public ushort refprec_1_plane;

		/**
		* @brief Bitvector indicating weight quant methods used by active 2 plane block modes.
		*
		* Bit 0 = QUANT_2, Bit 1 = QUANT_3, etc.
		*/
		public ushort refprec_2_planes;

		/**
		* @brief Set a 1 plane weight quant as active.
		*
		* @param weight_quant   The quant method to set.
		*/
		public void set_ref_1_plane(QuantMethod weight_quant)
		{
			refprec_1_plane |= (1 << weight_quant);
		}

		/**
		* @brief Test if this mode is active below a given 1 plane weight quant (inclusive).
		*
		* @param max_weight_quant   The max quant method to test.
		*/
		public bool is_ref_1_plane(QuantMethod max_weight_quant)
		{
			ushort mask = (ushort)((1 << (max_weight_quant + 1)) - 1);
			return (refprec_1_plane & mask) != 0;
		}

		/**
		* @brief Set a 2 plane weight quant as active.
		*
		* @param weight_quant   The quant method to set.
		*/
		public void set_ref_2_plane(QuantMethod weight_quant)
		{
			refprec_2_planes |= (ushort)(1 << weight_quant);
		}

		/**
		* @brief Test if this mode is active below a given 2 plane weight quant (inclusive).
		*
		* @param max_weight_quant   The max quant method to test.
		*/
		public bool is_ref_2_plane(QuantMethod max_weight_quant)
		{
			ushort mask = (ushort)((1 << (max_weight_quant + 1)) - 1);
			return (refprec_2_planes & mask) != 0;
		}
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
		public byte xdim;

		/**< The block Y dimension, in texels. */
		public byte ydim;

		/**< The block Z dimension, in texels. */
		public byte zdim;

		/**< The block total texel count. */
		public byte texel_count;


		/**
		* @brief The number of stored decimation modes which are "always" modes.
		*
		* Always modes are stored at the start of the decimation_modes list.
		*/
		public uint decimation_mode_count_always;

		/** @brief The number of stored decimation modes for selected encodings. */
		public uint decimation_mode_count_selected;

		/** @brief The number of stored decimation modes for any encoding. */
		public uint decimation_mode_count_all;

		/**
		* @brief The number of stored block modes which are "always" modes.
		*
		* Always modes are stored at the start of the block_modes list.
		*/
		public uint block_mode_count_1plane_always;

		/** @brief The number of stored block modes for active 1 plane encodings. */
		public uint block_mode_count_1plane_selected;

		/** @brief The number of stored block modes for active 1 and 2 plane encodings. */
		public uint block_mode_count_1plane_2plane_selected;

		/** @brief The number of stored block modes for any encoding. */
		public uint block_mode_count_all;

		/** @brief The number of selected partitionings for 1/2/3/4 partitionings. */
		public uint[] partitioning_count_selected = new uint[Constants.BLOCK_MAX_PARTITIONS];

		/** @brief The number of partitionings for 1/2/3/4 partitionings. */
		public uint[] partitioning_count_all = new uint[Constants.BLOCK_MAX_PARTITIONS];

		/**< The active decimation modes, stored in low indices. */
		public DecimationMode[] decimation_modes = new DecimationMode[Constants.WEIGHTS_MAX_DECIMATION_MODES];

		/**< The active decimation tables, stored in low indices. */
		public DecimationInfo[] decimation_tables = new DecimationInfo[Constants.WEIGHTS_MAX_DECIMATION_MODES];

		/** @brief The packed block mode array index, or @c BLOCK_BAD_BLOCK_MODE if not active. */
		public ushort[] block_mode_packed_index = new ushort[Constants.WEIGHTS_MAX_BLOCK_MODES];

		/**< The active block modes, stored in low indices. */
		public BlockMode[] block_modes = new BlockMode[Constants.WEIGHTS_MAX_BLOCK_MODES];


		/**< The partion tables for all of the possible partitions. */
		public PartitionInfo[] partitionings = new PartitionInfo[(3 * Constants.BLOCK_MAX_PARTITIONINGS) + 1];

		/**
		* @brief The packed partition table array index, or @c BLOCK_BAD_PARTITIONING if not active.
		*
		* Indexed by partition_count - 2, containing 2, 3 and 4 partitions.
		*/
		public ushort[,] partitioning_packed_index = new ushort[3, Constants.BLOCK_MAX_PARTITIONINGS];

		public byte[] kmeans_texels = new byte[Constants.BLOCK_MAX_KMEANS_TEXELS];

		public ulong[,] coverage_bitmaps_2 = new ulong[Constants.BLOCK_MAX_PARTITIONINGS, 2];
		public ulong[,] coverage_bitmaps_3 = new ulong[Constants.BLOCK_MAX_PARTITIONINGS, 3]; 
		public ulong[,] coverage_bitmaps_4 = new ulong[Constants.BLOCK_MAX_PARTITIONINGS, 4]; 

		public BlockSizeDescriptor(bool notUsed)
		{
			this.xdim = 0;
			this.ydim = 0;
			this.zdim = 0;
			this.texel_count = 0;
		}

		public BlockMode get_block_mode(uint block_mode)
		{
			uint packed_index = this.block_mode_packed_index[block_mode];
			return this.block_modes[packed_index];
		}

		public DecimationMode get_decimation_mode(uint decimation_mode)
		{
			return this.decimation_modes[decimation_mode];
		}

		public DecimationInfo get_decimation_info(uint decimation_mode)
		{
			return this.decimation_tables[decimation_mode];
		}

		public PartitionInfo get_partition_table(uint partition_count)
		{
			if (partition_count == 1)
			{
				partition_count = 5;
			}
			uint index = (partition_count - 2) * Constants.BLOCK_MAX_PARTITIONINGS;
			return this.partitionings[index];
		}

		public PartitionInfo get_partition_info(uint partition_count, uint index)
		{
			uint packed_index = 0;
			if (partition_count >= 2)
			{
				packed_index = this.partitioning_packed_index[partition_count - 2, index];
			}

			//assert(packed_index != BLOCK_BAD_PARTITIONING && packed_index < this->partitioning_count_all[partition_count - 1]);
			var result = get_partition_table(partition_count)[packed_index];
			//assert(index == result.partition_index);
			return result;
		}

		public PartitionInfo get_raw_partition_info(uint partition_count, uint packed_index)
		{
			//assert(packed_index != BLOCK_BAD_PARTITIONING && packed_index < this->partitioning_count_all[partition_count - 1]);
			var result = get_partition_table(partition_count)[packed_index];
			return result;
		}
	}

	// data structure representing one block of an image.
	// it is expanded to float prior to processing to save some computation time
	// on conversions to/from uint8_t (this also allows us to handle HDR textures easily)
	public struct ImageBlock
	{
		public float[] data_r = new float[Constants.BLOCK_MAX_TEXELS]; 
		public float[] data_g = new float[Constants.BLOCK_MAX_TEXELS];
		public float[] data_b = new float[Constants.BLOCK_MAX_TEXELS];
		public float[] data_a = new float[Constants.BLOCK_MAX_TEXELS];

		public byte texel_count;

		public vfloat4 origin_texel;
		public vfloat4 data_min;
		public vfloat4 data_mean;
		public vfloat4 data_max;
		public vfloat4 channel_weight;
		public bool grayscale;

		public byte[] rgb_lns = new byte[Constants.BLOCK_MAX_TEXELS];      // 1 if RGB data are being treated as LNS
		public byte[] alpha_lns = new byte[Constants.BLOCK_MAX_TEXELS];    // 1 if Alpha data are being treated as LNS
	
		public uint xpos;
		public uint ypos;
		public uint zpos;

		public ImageBlock()
		{
			
		}

		public vfloat4 Texel(int index)
		{
			return new vfloat4(data_r[index],
		               data_g[index],
		               data_b[index],
		               data_a[index]);
		}

		public vfloat4 Texel3(int index)
		{
			return vfloat4.vfloat3(data_r[index],
		               data_g[index],
		               data_b[index]);
		}
	}

	public struct ErrorWeightBlock
	{
		public vfloat4[] error_weights;
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
			this.error_weights = new vfloat4[Constants.MAX_TEXELS_PER_BLOCK];
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

	public struct DtInitWorkingBuffers
	{
		public byte[] weight_count_of_texel = new byte[Constants.BLOCK_MAX_TEXELS];
		public byte[,] grid_weights_of_texel = new byte[Constants.BLOCK_MAX_TEXELS, 4];
		public byte[,] weights_of_texel = new byte[Constants.BLOCK_MAX_TEXELS, 4];

		public byte[] texel_count_of_weight = new byte[Constants.BLOCK_MAX_WEIGHTS];
		public byte[,] texels_of_weight = new byte[Constants.BLOCK_MAX_WEIGHTS, Constants.BLOCK_MAX_TEXELS];
		public byte[,] texel_weights_of_weight = new byte[Constants.BLOCK_MAX_WEIGHTS, Constants.BLOCK_MAX_TEXELS];

		public DtInitWorkingBuffers()
		{

		}
	}

	/**
	* @brief Data structure storing the color endpoints for a block.
	*/
	public struct Endpoints
	{
		public uint partition_count = default;
		public vfloat4[] endpt0 = new vfloat4[Constants.BLOCK_MAX_PARTITIONS];
		public vfloat4[] endpt1 = new vfloat4[Constants.BLOCK_MAX_PARTITIONS];

		public Endpoints()
		{

		}
	}

	public struct EndpointsAndWeights
	{
		/** @brief True if all active values in weight_error_scale are the same. */
		public bool is_constant_weight_error_scale;

		/** @brief The color endpoints. */
		public Endpoints ep;

		/** @brief The ideal weight for each texel; may be undecimated or decimated. */
		public float[] weights = new float[Constants.BLOCK_MAX_TEXELS];

		/** @brief The ideal weight error scaling for each texel; may be undecimated or decimated. */
		public float[] weight_error_scale = new float[Constants.BLOCK_MAX_TEXELS];

		public EndpointsAndWeights()
		{

		}
	}

	public struct CompressFixedPartitionBuffers
	{
		public EndpointsAndWeights ei1;
		public EndpointsAndWeights ei2;
		public EndpointsAndWeights[] eix1;
		public EndpointsAndWeights[] eix2;
		public float[] decimated_quantized_weights;
		public float[] decimated_weights;
		public float[] flt_quantized_decimated_quantized_weights;
		public byte[] u8_quantized_decimated_quantized_weights;

		public CompressFixedPartitionBuffers(bool notUsed)
		{
			this.ei1 = new EndpointsAndWeights(notUsed: true);
			this.ei2 = new EndpointsAndWeights(notUsed: true);

			this.eix1 = new EndpointsAndWeights[Constants.MAX_DECIMATION_MODES];
			this.eix2 = new EndpointsAndWeights[Constants.MAX_DECIMATION_MODES];

			this.decimated_quantized_weights = new float[2 * Constants.MAX_DECIMATION_MODES * Constants.MAX_WEIGHTS_PER_BLOCK];
			this.decimated_weights = new float[2 * Constants.MAX_DECIMATION_MODES * Constants.MAX_WEIGHTS_PER_BLOCK];

			this.flt_quantized_decimated_quantized_weights = new float[2 * Constants.MAX_WEIGHT_MODES * Constants.MAX_WEIGHTS_PER_BLOCK];
			this.u8_quantized_decimated_quantized_weights = new byte[2 * Constants.MAX_WEIGHT_MODES * Constants.MAX_WEIGHTS_PER_BLOCK];
		}
	}

	/**
	* @brief Preallocated working buffers, allocated per thread during context creation.
	*/
	public struct compression_working_buffers
	{
		/** @brief Ideal endpoints and weights for plane 1. */
		public EndpointsAndWeights ei1;

		/** @brief Ideal endpoints and weights for plane 2. */
		public EndpointsAndWeights ei2;

		/**
		* @brief Decimated ideal weight values in the ~0-1 range.
		*
		* Note that values can be slightly below zero or higher than one due to
		* endpoint extents being inside the ideal color representation.
		*
		* For two planes, second plane starts at @c WEIGHTS_PLANE2_OFFSET offsets.
		*/
		public float[] dec_weights_ideal = new float[Constants.WEIGHTS_MAX_DECIMATION_MODES * Constants.BLOCK_MAX_WEIGHTS];

		/**
		* @brief Decimated quantized weight values in the unquantized 0-64 range.
		*
		* For two planes, second plane starts at @c WEIGHTS_PLANE2_OFFSET offsets.
		*/
		public byte[] dec_weights_uquant = new byte[Constants.WEIGHTS_MAX_BLOCK_MODES * Constants.BLOCK_MAX_WEIGHTS];

		/** @brief Error of the best encoding combination for each block mode. */
		public float[] errors_of_best_combination = new float[Constants.WEIGHTS_MAX_BLOCK_MODES];

		/** @brief The best color quant for each block mode. */
		public byte[] best_quant_levels = new byte[Constants.WEIGHTS_MAX_BLOCK_MODES];

		/** @brief The best color quant for each block mode if modes are the same and we have spare bits. */
		public byte[] best_quant_levels_mod = new byte[Constants.WEIGHTS_MAX_BLOCK_MODES];

		/** @brief The best endpoint format for each partition. */
		public byte[,] best_ep_formats = new byte[Constants.WEIGHTS_MAX_BLOCK_MODES, Constants.BLOCK_MAX_PARTITIONS];

		/** @brief The total bit storage needed for quantized weights for each block mode. */
		public sbyte[] qwt_bitcounts = new sbyte[Constants.WEIGHTS_MAX_BLOCK_MODES];

		/** @brief The cumulative error for quantized weights for each block mode. */
		public float[] qwt_errors = new float[Constants.WEIGHTS_MAX_BLOCK_MODES];

		/** @brief The low weight value in plane 1 for each block mode. */
		public float[] weight_low_value1 = new float[Constants.WEIGHTS_MAX_BLOCK_MODES];

		/** @brief The high weight value in plane 1 for each block mode. */
		public float[] weight_high_value1 = new float[Constants.WEIGHTS_MAX_BLOCK_MODES];

		/** @brief The low weight value in plane 1 for each quant level and decimation mode. */
		public float[,] weight_low_values1 = new float[Constants.WEIGHTS_MAX_DECIMATION_MODES, Constants.TUNE_MAX_ANGULAR_QUANT + 1];

		/** @brief The high weight value in plane 1 for each quant level and decimation mode. */
		public float[,] weight_high_values1 = new float[Constants.WEIGHTS_MAX_DECIMATION_MODES, Constants.TUNE_MAX_ANGULAR_QUANT + 1];

		/** @brief The low weight value in plane 2 for each block mode. */
		public float[] weight_low_value2 = new float[Constants.WEIGHTS_MAX_BLOCK_MODES];

		/** @brief The high weight value in plane 2 for each block mode. */
		public float[] weight_high_value2 = new float[Constants.WEIGHTS_MAX_BLOCK_MODES];

		/** @brief The low weight value in plane 2 for each quant level and decimation mode. */
		public float[,] weight_low_values2 = new float[Constants.WEIGHTS_MAX_DECIMATION_MODES, Constants.TUNE_MAX_ANGULAR_QUANT + 1];

		/** @brief The high weight value in plane 2 for each quant level and decimation mode. */
		public float[,] weight_high_values2 = new float[Constants.WEIGHTS_MAX_DECIMATION_MODES, Constants.TUNE_MAX_ANGULAR_QUANT + 1];
	}

	/**
	* @brief Parameter structure for @c compute_pixel_region_variance().
	*
	* This function takes a structure to avoid spilling arguments to the stack on every function
	* invocation, as there are a lot of parameters.
	*/
	public struct pixel_region_args
	{
		/** @brief The image to analyze. */
		public ASTCEncImage img; 

		/** @brief The component swizzle pattern. */
		public ASTCEncSwizzle swz;

		/** @brief Should the algorithm bother with Z axis processing? */
		public bool have_z;

		/** @brief The kernel radius for alpha processing. */
		public uint alpha_kernel_radius;

		/** @brief The X dimension of the working data to process. */
		public uint size_x;

		/** @brief The Y dimension of the working data to process. */
		public uint size_y;

		/** @brief The Z dimension of the working data to process. */
		public uint size_z;

		/** @brief The X position of first src and dst data in the data set. */
		public uint offset_x;

		/** @brief The Y position of first src and dst data in the data set. */
		public uint offset_y;

		/** @brief The Z position of first src and dst data in the data set. */
		public uint offset_z;

		/** @brief The working memory buffer. */
		public vfloat4[] work_memory;
	}

	/**
	* @brief Parameter structure for @c compute_averages_proc().
	*/
	public struct avg_args
	{
		/** @brief The arguments for the nested variance computation. */
		public pixel_region_args arg;

		/** @brief The image X dimensions. */
		public uint img_size_x;

		/** @brief The image Y dimensions. */
		public uint img_size_y;

		/** @brief The image Z dimensions. */
		public uint img_size_z;

		/** @brief The maximum working block dimensions in X and Y dimensions. */
		public uint blk_size_xy;

		/** @brief The maximum working block dimensions in Z dimensions. */
		public uint blk_size_z;

		/** @brief The working block memory size. */
		public uint work_memory_size;
	}

	public struct astcenc_contexti
	{
		public ASTCEncConfig config;
		public uint thread_count;
		public BlockSizeDescriptor bsd;

		// Fields below here are not needed in a decompress-only build, but some
		// remain as they are small and it avoids littering the code with #ifdefs.
		// The most significant contributors to large structure size are omitted.

		public float[] input_alpha_averages;

		public compression_working_buffers[] working_buffers;

		#if !ASTCENC_DECOMPRESS_ONLY
		/** @brief The pixel region and variance worker arguments. */
		public avg_args avg_preprocess_args;
		#endif
	}

	// *********************************************************
	// functions and data pertaining to images and imageblocks
	// *********************************************************

	/**
	* @brief Parameter structure for compute_pixel_region_variance().
	*
	* This function takes a structure to avoid spilling arguments to the stack
	* on every function invocation, as there are a lot of parameters.
	*/
	public struct pixel_region_variance_args
	{
		/** The image to analyze. */
		public ASTCEncImage img;
		/** The RGB channel power adjustment. */
		public float rgb_power;
		/** The alpha channel power adjustment. */
		public float alpha_power;
		/** The channel swizzle pattern. */
		public ASTCEncSwizzle swz;
		/** Should the algorithm bother with Z axis processing? */
		public bool have_z;
		/** The kernel radius for average and variance. */
		public int avg_var_kernel_radius;
		/** The kernel radius for alpha processing. */
		public int alpha_kernel_radius;
		/** The size of the working data to process. */
		public int size_x;
		public int size_y;
		public int size_z;
		/** The position of first src and dst data in the data set. */
		public int offset_x;
		public int offset_y;
		public int offset_z;
		/** The working memory buffer. */
		public vfloat4[] work_memory;
	};

	/**
	* @brief Parameter structure for compute_averages_and_variances_proc().
	*/
	public struct avg_var_args
	{
		/** The arguments for the nested variance computation. */
		public pixel_region_variance_args arg;
		/** The image dimensions. */
		public int img_size_x;
		public int img_size_y;
		public int img_size_z;
		/** The maximum working block dimensions. */
		public int blk_size_xy;
		public int blk_size_z;
		/** The working block memory size. */
		public int work_memory_size;
	}

	public struct EncodingChoiceErrors
	{
		// Error of using LDR RGB-scale instead of complete endpoints.
		public float rgb_scale_error;
		// Error of using HDR RGB-scale instead of complete endpoints.
		public float rgb_luma_error;
		// Error of using luminance instead of RGB.
		public float luminance_error;
		// Error of discarding alpha.
		public float alpha_drop_error;
		// Validity of using offset encoding.
		public bool can_offset_encode;
		// Validity of using blue contraction encoding.
		public bool can_blue_contract;
	}
}