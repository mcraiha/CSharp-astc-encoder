
namespace ASTCEnc
{
    /**
    * @brief The astcenc compression context.
    */
    public struct astcenc_context
    {
        /** @brief The context internal state. */
        public astcenc_contexti context;

    #if !ASTCENC_DECOMPRESS_ONLY
        /** @brief The parallel manager for averages computation. */
        ParallelManager manage_avg;

        /** @brief The parallel manager for compression. */
        ParallelManager manage_compress;
    #endif

        /** @brief The parallel manager for decompression. */
        ParallelManager manage_decompress;
    }
}