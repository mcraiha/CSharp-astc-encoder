
namespace ASTCEnc
{
	/**
	* @brief A codec API error code.
	*/
	public enum ASTCEncError 
	{
		/** @brief The call was successful. */
		ASTCENC_SUCCESS = 0,
		/** @brief The call failed due to low memory, or undersized I/O buffers. */
		ASTCENC_ERR_OUT_OF_MEM,
		/** @brief The call failed due to the build using fast math. */
		ASTCENC_ERR_BAD_CPU_FLOAT,
		/** @brief The call failed due to the build using an unsupported ISA. */
		ASTCENC_ERR_BAD_CPU_ISA,
		/** @brief The call failed due to an out-of-spec parameter. */
		ASTCENC_ERR_BAD_PARAM,
		/** @brief The call failed due to an out-of-spec block size. */
		ASTCENC_ERR_BAD_BLOCK_SIZE,
		/** @brief The call failed due to an out-of-spec color profile. */
		ASTCENC_ERR_BAD_PROFILE,
		/** @brief The call failed due to an out-of-spec quality value. */
		ASTCENC_ERR_BAD_QUALITY,
		/** @brief The call failed due to an out-of-spec channel swizzle. */
		ASTCENC_ERR_BAD_SWIZZLE,
		/** @brief The call failed due to an out-of-spec flag set. */
		ASTCENC_ERR_BAD_FLAGS,
		/** @brief The call failed due to the context not supporting the operation. */
		ASTCENC_ERR_BAD_CONTEXT,
		/** @brief The call failed due to unimplemented functionality. */
		ASTCENC_ERR_NOT_IMPLEMENTED,
	}

	public enum ASTCEncProfile 
	{
		/** @brief The LDR sRGB color profile. */
		ASTCENC_PRF_LDR_SRGB = 0,
		/** @brief The LDR linear color profile. */
		ASTCENC_PRF_LDR,
		/** @brief The HDR RGB with LDR alpha color profile. */
		ASTCENC_PRF_HDR_RGB_LDR_A,
		/** @brief The HDR RGBA color profile. */
		ASTCENC_PRF_HDR
	}

	/**
	* @brief A codec channel swizzle selector.
	*/
	public enum ASTCEncSwizzleChannel
	{
		/** @brief Select the red channel. */
		ASTCENC_SWZ_R = 0,
		/** @brief Select the green channel. */
		ASTCENC_SWZ_G = 1,
		/** @brief Select the blue channel. */
		ASTCENC_SWZ_B = 2,
		/** @brief Select the alpha channel. */
		ASTCENC_SWZ_A = 3,
		/** @brief Use a constant zero channel. */
		ASTCENC_SWZ_0 = 4,
		/** @brief Use a constant one channel. */
		ASTCENC_SWZ_1 = 5,
		/** @brief Use a reconstructed normal vector Z channel. */
		ASTCENC_SWZ_Z = 6
	}

	/**
	* @brief A texel channel swizzle.
	*/
	public struct ASTCEncSwizzle
	{
		/** @brief The red channel selector. */
		public ASTCEncSwizzleChannel r;
		/** @brief The green channel selector. */
		public ASTCEncSwizzleChannel g;
		/** @brief The blue channel selector. */
		public ASTCEncSwizzleChannel b;
		/** @brief The alpha channel selector. */
		public ASTCEncSwizzleChannel a;
	}

	/**
	* @brief A texel channel data format.
	*/
	public enum ASTCEncType 
	{
		/** @brief Unorm 8-bit data per channel. */
		ASTCENC_TYPE_U8 = 0,
		/** @brief 16-bit float per channel. */
		ASTCENC_TYPE_F16 = 1,
		/** @brief 32-bit float per channel. */
		ASTCENC_TYPE_F32 = 2
	};

	/**
	* @brief The config structure.
	*
	* This structure will initially be populated by a call to astcenc_config_init,
	* but power users may modify it before calling astcenc_context_alloc. See
	* astcenccli_toplevel_help.cpp for full user documentation of the power-user
	* settings.
	*
	* Note for any settings which are associated with a specific color channel,
	* the value in the config applies to the channel that exists after any
	* compression data swizzle is applied.
	*/
	public struct ASTCEncConfig 
	{
		/** @brief The color profile. */
		public ASTCEncProfile profile;

		/** @brief The set of set flags. */
		public uint flags;

		/** @brief The ASTC block size X dimension. */
		public uint block_x;

		/** @brief The ASTC block size Y dimension. */
		public uint block_y;

		/** @brief The ASTC block size Z dimension. */
		public uint block_z;

		/** @brief The size of the texel kernel for error weighting (-v). */
		public uint v_rgba_radius;

		/** @brief The mean and stdev channel mix for error weighting (-v). */
		public float v_rgba_mean_stdev_mix;

		/** @brief The texel RGB power for error weighting (-v). */
		public float v_rgb_power;

		/** @brief The texel RGB base weight for error weighting (-v). */
		public float v_rgb_base;

		/** @brief The texel RGB mean weight for error weighting (-v). */
		public float v_rgb_mean;

		/** @brief The texel RGB stdev for error weighting (-v). */
		public float v_rgb_stdev;

		/** @brief The texel A power for error weighting (-va). */
		public float v_a_power;

		/** @brief The texel A base weight for error weighting (-va). */
		public float v_a_base;

		/** @brief The texel A mean weight for error weighting (-va). */
		public float v_a_mean;

		/** @brief The texel A stdev for error weighting (-va). */
		public float v_a_stdev;

		/** @brief The red channel weight scale for error weighting (-cw). */
		public float cw_r_weight;

		/** @brief The green channel weight scale for error weighting (-cw). */
		public float cw_g_weight;

		/** @brief The blue channel weight scale for error weighting (-cw). */
		public float cw_b_weight;

		/** @brief The alpha channel weight scale for error weighting (-cw). */
		public float cw_a_weight;

		/**
		* @brief The radius for any alpha-weight scaling (-a).
		*
		* It is recommended that this is set to 1 when using FLG_USE_ALPHA_WEIGHT
		* on a texture that will be sampled using linear texture filtering to
		* minimize color bleed out of transparent texels that are adjcent to
		* non-transparent texels.
		*/
		public uint a_scale_radius;

		/**
		* @brief The additional weight for block edge texels (-b).
		*
		* This is generic tool for reducing artefacts visible on block changes.
		*/
		public float b_deblock_weight;

		/**
		* @brief The maximum number of partitions searched (-partitionlimit).
		*
		* Valid values are between 1 and 1024.
		*/
		public uint tune_partition_limit;

		/**
		* @brief The maximum centile for block modes searched (-blockmodelimit).
		*
		* Valid values are between 1 and 100.
		*/
		public uint tune_block_mode_limit;

		/**
		* @brief The maximum iterative refinements applied (-refinementlimit).
		*
		* Valid values are between 1 and N; there is no technical upper limit
		* but little benefit is expected after N=4.
		*/
		public uint tune_refinement_limit;

		/**
		* @brief The number of trial candidates per mode search (-candidatelimit).
		*
		* Valid values are between 1 and TUNE_MAX_TRIAL_CANDIDATES (default 4).
		*/
		public uint tune_candidate_limit;

		/**
		* @brief The dB threshold for stopping block search (-dblimit).
		*
		* This option is ineffective for HDR textures.
		*/
		public float tune_db_limit;

		/**
		* @brief The amount of overshoot needed to early-out mode 0 fast path.
		*
		* We have a fast-path for mode 0 (1 partition, 1 plane) which uses only
		* essential block modes as an initital search. This can short-cut
		* compression for simple blocks, but to avoid shortcutting too much we
		* force this to overshoot the MSE threshold needed to hit the block-local
		* db_limit e.g. 1.0 = no overshoot, 2.0 = need half the error to trigger.
		*/
		public float tune_mode0_mse_overshoot;

		/**
		* @brief The amount of overshoot needed to early-out refinement.
		*
		* The codec will refine block candidates iteratively to improve the
		* encoding, based on the @c tune_refinement_limit count. Earlier
		* implementations will use all refinement iterations, even if the target
		* threshold is reached. This tuning parameter allows an early out, but
		* with an overshoot MSE threshold. Setting this to 1.0 will early-out as
		* soon as the target is hit, but does reduce image quality vs the
		* default behavior of over-refinement.
		*/
		public float tune_refinement_mse_overshoot;

		/**
		* @brief The threshold for skipping 3+ partitions (-partitionearlylimit).
		*
		* This option is ineffective for normal maps.
		*/
		public float tune_partition_early_out_limit;

		/**
		* @brief The threshold for skipping 2 weight planess (-planecorlimit).
		*
		* This option is ineffective for normal maps.
		*/
		public float tune_two_plane_early_out_limit;
	}

	/**
	* @brief An uncompressed 2D or 3D image.
	*
	* 3D image are passed in as an array of 2D slices. Each slice has identical
	* size and color format.
	*/
	public struct ASTCEncImage {
		/** @brief The X dimension of the image, in texels. */
		public uint dim_x;
		/** @brief The Y dimension of the image, in texels. */
		public uint dim_y;
		/** @brief The Z dimension of the image, in texels. */
		public uint dim_z;
		/** @brief The data type per channel. */
		public ASTCEncType data_type;
		/** @brief The array of 2D slices, of length @c dim_z. */
		public byte[] data;
	}

	public struct ASTCBlockInfo
	{
		/** @brief The block encoding color profile. */
		public ASTCEncProfile profile;

		/** @brief The number of texels in the X dimension. */
		public uint block_x;

		/** @brief The number of texels in the Y dimension. */
		public uint block_y;

		/** @brief The number of texels in the Z dimension. */
		public uint block_z;

		/** @brief The number of texels in the block. */
		public uint texel_count;

		/** @brief True if this block is an error block. */
		public bool is_error_block;

		/** @brief True if this block is a constant color block. */
		public bool is_constant_block;

		/** @brief True if this block is an HDR block. */
		public bool is_hdr_block;

		/** @brief True if this block uses two weight planes. */
		public bool is_dual_plane_block;

		/** @brief The number of partitions if not constant color. */
		public uint partition_count;

		/** @brief The partition index if 2 - 4 partitions used. */
		public uint partition_index;

		/** @brief The component index of the second plane if dual plane. */
		public uint dual_plane_component;

		/** @brief The color endpoint encoding mode for each partition. */
		public uint[] color_endpoint_modes = new uint[4];

		/** @brief The number of color endpoint quantization levels. */
		public uint color_level_count;

		/** @brief The number of weight quantization levels. */
		public uint weight_level_count;

		/** @brief The number of weights in the X dimension. */
		public uint weight_x;

		/** @brief The number of weights in the Y dimension. */
		public uint weight_y;

		/** @brief The number of weights in the Z dimension. */
		public uint weight_z;

		/** @brief The unpacked color endpoints for each partition. */
		public float[,,] color_endpoints = new float[4, 2, 4];

		/** @brief The per-texel interpolation weights for the block. */
		public float[] weight_values_plane1 = new float[216];

		/** @brief The per-texel interpolation weights for the block. */
		public float[] weight_values_plane2 = new float[216];

		/** @brief The per-texel partition assignments for the block. */
		public byte[] partition_assignment = new byte[216];
	}

	/**
	* @brief A codec color profile.
	*/
	public class ASTCEnc
	{
		/** @brief The fastest, lowest quality, search preset. */
		public const float ASTCENC_PRE_FASTEST = 0.0f;

		/** @brief The fast search preset. */
		public const float ASTCENC_PRE_FAST = 10.0f;

		/** @brief The medium quality search preset. */
		public const float ASTCENC_PRE_MEDIUM = 60.0f;

		/** @brief The throrough quality search preset. */
		public const float ASTCENC_PRE_THOROUGH = 98.0f;

		/** @brief The exhaustive, highest quality, search preset. */
		public const float ASTCENC_PRE_EXHAUSTIVE = 100.0f;

		/**
		* @brief Enable normal map compression.
		*
		* Input data will be treated a two channel normal map, storing X and Y, and
		* the codec will optimize for angular error rather than simple linear PSNR.
		* In this mode the input swizzle should be e.g. rrrg (the default ordering for
		* ASTC normals on the command line) or gggr (the ordering used by BC5).
		*/
		public const uint ASTCENC_FLG_MAP_NORMAL          = 1 << 0;

		/**
		* @brief Enable mask map compression.
		*
		* Input data will be treated a multi-layer mask map, where is is desirable for
		* the color channels to be treated independently for the purposes of error
		* analysis.
		*/
		public const uint ASTCENC_FLG_MAP_MASK             = 1 << 1;

		/**
		* @brief Enable alpha weighting.
		*
		* The input alpha value is used for transparency, so errors in the RGB
		* channels are weighted by the transparency level. This allows the codec to
		* more accurately encode the alpha value in areas where the color value
		* is less significant.
		*/
		public const uint ASTCENC_FLG_USE_ALPHA_WEIGHT     = 1 << 2;

		/**
		* @brief Enable perceptual error metrics.
		*
		* This mode enables perceptual compression mode, which will optimize for
		* perceptual error rather than best PSNR. Only some input modes support
		* perceptual error metrics.
		*/
		public const uint ASTCENC_FLG_USE_PERCEPTUAL       = 1 << 3;

		/**
		* @brief Create a decompression-only context.
		*
		* This mode disables support for compression. This enables context allocation
		* to skip some transient buffer allocation, resulting in lower memory usage.
		*/
		public const uint ASTCENC_FLG_DECOMPRESS_ONLY      = 1 << 4;

		/**
		* @brief Create a self-decompression context.
		*
		* This mode configures the compressor so that it is only guaranteed to be
		* able to decompress images that were actually created using the current
		* context. This is the common case for compression use cases, and setting this
		* flag enables additional optimizations, but does mean that the context cannot
		* reliably decompress arbitrary ASTC images.
		*/
		public const uint ASTCENC_FLG_SELF_DECOMPRESS_ONLY = 1 << 5;

		/**
		* @brief The bit mask of all valid flags.
		*/
		public const uint ASTCENC_ALL_FLAGS =
									ASTCENC_FLG_MAP_NORMAL |
									ASTCENC_FLG_MAP_MASK |
									ASTCENC_FLG_USE_ALPHA_WEIGHT |
									ASTCENC_FLG_USE_PERCEPTUAL |
									ASTCENC_FLG_DECOMPRESS_ONLY |
									ASTCENC_FLG_SELF_DECOMPRESS_ONLY;
			}

	
}