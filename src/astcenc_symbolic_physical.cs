using System.Diagnostics;

namespace ASTCEnc
{
    public static class ASTCSymbolicPhysical
    {
        /**
        * @brief Write up to 8 bits at an arbitrary bit offset.
        *
        * The stored value is at most 8 bits, but can be stored at an offset of between 0 and 7 bits so
        * may span two separate bytes in memory.
        *
        * @param         value       The value to write.
        * @param         bitcount    The number of bits to write, starting from LSB.
        * @param         bitoffset   The bit offset to store at, between 0 and 7.
        * @param[in,out] ptr         The data pointer to write to.
        */
        static void write_bits(
            int value,
            int bitcount,
            int bitoffset,
            byte[] ptr
        ) {
            int mask = (1 << bitcount) - 1;
            value &= mask;
            ptr += bitoffset >> 3;
            bitoffset &= 7;
            value <<= bitoffset;
            mask <<= bitoffset;
            mask = ~mask;

            ptr[0] &= mask;
            ptr[0] |= value;
            ptr[1] &= mask >> 8;
            ptr[1] |= value >> 8;
        }

        /**
        * @brief Read up to 8 bits at an arbitrary bit offset.
        *
        * The stored value is at most 8 bits, but can be stored at an offset of between 0 and 7 bits so may
        * span two separate bytes in memory.
        *
        * @param         bitcount    The number of bits to read.
        * @param         bitoffset   The bit offset to read from, between 0 and 7.
        * @param[in,out] ptr         The data pointer to read from.
        *
        * @return The read value.
        */
        static int read_bits(
            int bitcount,
            int bitoffset,
            byte[] ptr
        ) {
            int mask = (1 << bitcount) - 1;
            ptr += bitoffset >> 3;
            bitoffset &= 7;
            int value = ptr[0] | (ptr[1] << 8);
            value >>= bitoffset;
            value &= mask;
            return value;
        }

        /**
        * @brief Reverse bits in a byte.
        *
        * @param p   The value to reverse.
        *
        * @return The reversed result.
        */
        static int bitrev8(int p)
        {
            p = ((p & 0x0F) << 4) | ((p >> 4) & 0x0F);
            p = ((p & 0x33) << 2) | ((p >> 2) & 0x33);
            p = ((p & 0x55) << 1) | ((p >> 1) & 0x55);
            return p;
        }

        /* See header for documentation. */
        static void symbolic_to_physical(
            BlockSizeDescriptor bsd,
            SymbolicCompressedBlock scb,
            PhysicalCompressedBlock pcb
        ) {
            Debug.Assert(scb.block_type != SYM_BTYPE.SYM_BTYPE_ERROR);

            // Constant color block using UNORM16 colors
            if (scb.block_type == SYM_BTYPE.SYM_BTYPE_CONST_U16)
            {
                // There is currently no attempt to coalesce larger void-extents
                byte[] cbytes = new byte[8] { 0xFC, 0xFD, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF };
                for (uint i = 0; i < 8; i++)
                {
                    pcb.data[i] = cbytes[i];
                }

                for (uint i = 0; i < Constants.BLOCK_MAX_COMPONENTS; i++)
                {
                    pcb.data[2 * i + 8] = scb.constant_color[i] & 0xFF;
                    pcb.data[2 * i + 9] = (scb.constant_color[i] >> 8) & 0xFF;
                }

                return;
            }

            // Constant color block using FP16 colors
            if (scb.block_type == SYM_BTYPE.SYM_BTYPE_CONST_F16)
            {
                // There is currently no attempt to coalesce larger void-extents
                byte[] cbytes = new byte[8] { 0xFC, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF };
                for (uint i = 0; i < 8; i++)
                {
                    pcb.data[i] = cbytes[i];
                }

                for (uint i = 0; i < Constants.BLOCK_MAX_COMPONENTS; i++)
                {
                    pcb.data[2 * i + 8] = scb.constant_color[i] & 0xFF;
                    pcb.data[2 * i + 9] = (scb.constant_color[i] >> 8) & 0xFF;
                }

                return;
            }

            uint partition_count = scb.partition_count;

            // Compress the weights.
            // They are encoded as an ordinary integer-sequence, then bit-reversed
            byte[] weightbuf = new byte[16];

            BlockMode bm = bsd.get_block_mode(scb.block_mode);
            DecimationInfo di = bsd.get_decimation_info(bm.decimation_mode);
            int weight_count = di.weight_count;
            QuantMethod weight_QuantMethod = bm.get_weight_quant_mode();
            float weight_quant_levels = (float)(get_quant_level(weight_QuantMethod));
            bool is_dual_plane = bm.is_dual_plane;

            QuantAndTransferTable qat = ASTCEncWeightQuantXferTables.quant_and_xfer_tables[(int)weight_QuantMethod];

            int real_weight_count = is_dual_plane ? 2 * weight_count : weight_count;

            int bits_for_weights = get_ise_sequence_bitcount(real_weight_count, weight_QuantMethod);

            byte[] weights = new byte[64];
            if (is_dual_plane)
            {
                for (int i = 0; i < weight_count; i++)
                {
                    float uqw = (float)(scb.weights[i]);
                    float qw = (uqw / 64.0f) * (weight_quant_levels - 1.0f);
                    int qwi = (int)(qw + 0.5f);
                    weights[2 * i] = qat.scramble_map[qwi];

                    uqw = (float)(scb.weights[i + WEIGHTS_PLANE2_OFFSET]);
                    qw = (uqw / 64.0f) * (weight_quant_levels - 1.0f);
                    qwi = (int)(qw + 0.5f);
                    weights[2 * i + 1] = qat.scramble_map[qwi];
                }
            }
            else
            {
                for (int i = 0; i < weight_count; i++)
                {
                    float uqw = (float)(scb.weights[i]);
                    float qw = (uqw / 64.0f) * (weight_quant_levels - 1.0f);
                    int qwi = (int)(qw + 0.5f);
                    weights[i] = qat.scramble_map[qwi];
                }
            }

            encode_ise(weight_QuantMethod, real_weight_count, weights, weightbuf, 0);

            for (int i = 0; i < 16; i++)
            {
                pcb.data[i] = (byte)(bitrev8(weightbuf[15 - i]));
            }

            write_bits(scb.block_mode, 11, 0, pcb.data);
            write_bits(partition_count - 1, 2, 11, pcb.data);

            int below_weights_pos = 128 - bits_for_weights;

            // Encode partition index and color endpoint types for blocks with 2+ partitions
            if (partition_count > 1)
            {
                write_bits(scb.partition_index, 6, 13, pcb.data);
                write_bits(scb.partition_index >> 6, Constants.PARTITION_INDEX_BITS - 6, 19, pcb.data);

                if (scb.color_formats_matched)
                {
                    write_bits(scb.color_formats[0] << 2, 6, 13 + Constants.PARTITION_INDEX_BITS, pcb.data);
                }
                else
                {
                    // Check endpoint types for each partition to determine the lowest class present
                    int low_class = 4;

                    for (uint i = 0; i < partition_count; i++)
                    {
                        int class_of_format = scb.color_formats[i] >> 2;
                        low_class = ASTCMath.min(class_of_format, low_class);
                    }

                    if (low_class == 3)
                    {
                        low_class = 2;
                    }

                    int encoded_type = low_class + 1;
                    int bitpos = 2;

                    for (uint i = 0; i < partition_count; i++)
                    {
                        int classbit_of_format = (scb.color_formats[i] >> 2) - low_class;
                        encoded_type |= classbit_of_format << bitpos;
                        bitpos++;
                    }

                    for (uint i = 0; i < partition_count; i++)
                    {
                        int lowbits_of_format = scb.color_formats[i] & 3;
                        encoded_type |= lowbits_of_format << bitpos;
                        bitpos += 2;
                    }

                    int encoded_type_lowpart = encoded_type & 0x3F;
                    int encoded_type_highpart = encoded_type >> 6;
                    int encoded_type_highpart_size = (3 * partition_count) - 4;
                    int encoded_type_highpart_pos = 128 - bits_for_weights - encoded_type_highpart_size;
                    write_bits(encoded_type_lowpart, 6, 13 + Constants.PARTITION_INDEX_BITS, pcb.data);
                    write_bits(encoded_type_highpart, encoded_type_highpart_size, encoded_type_highpart_pos, pcb.data);
                    below_weights_pos -= encoded_type_highpart_size;
                }
            }
            else
            {
                write_bits(scb.color_formats[0], 4, 13, pcb.data);
            }

            // In dual-plane mode, encode the color component of the second plane of weights
            if (is_dual_plane)
            {
                write_bits(scb.plane2_component, 2, below_weights_pos - 2, pcb.data);
            }

            // Encode the color components
            byte[] values_to_encode = new byte[32];
            int valuecount_to_encode = 0;
            for (uint i = 0; i < scb.partition_count; i++)
            {
                int vals = 2 * (scb.color_formats[i] >> 2) + 2;
                Debug.Assert(vals <= 8);
                for (int j = 0; j < vals; j++)
                {
                    values_to_encode[j + valuecount_to_encode] = scb.color_values[i, j];
                }
                valuecount_to_encode += vals;
            }

            encode_ise(scb.get_color_quant_mode(), valuecount_to_encode, values_to_encode, pcb.data,
                    scb.partition_count == 1 ? 17 : 19 + Constants.PARTITION_INDEX_BITS);
        }

        /* See header for documentation. */
        void physical_to_symbolic(
            BlockSizeDescriptor bsd,
            PhysicalCompressedBlock pcb,
            SymbolicCompressedBlock scb
        ) {
            byte[] bswapped = new byte[16];

            scb.block_type = SYM_BTYPE.SYM_BTYPE_NONCONST;

            // Extract header fields
            int block_mode = read_bits(11, 0, pcb.data);
            if ((block_mode & 0x1FF) == 0x1FC)
            {
                // Constant color block

                // Check what format the data has
                if (block_mode & 0x200)
                {
                    scb.block_type = SYM_BTYPE.SYM_BTYPE_CONST_F16;
                }
                else
                {
                    scb.block_type = SYM_BTYPE.SYM_BTYPE_CONST_U16;
                }

                scb.partition_count = 0;
                for (int i = 0; i < 4; i++)
                {
                    scb.constant_color[i] = pcb.data[2 * i + 8] | (pcb.data[2 * i + 9] << 8);
                }

                // Additionally, check that the void-extent
                if (bsd.zdim == 1)
                {
                    // 2D void-extent
                    int rsvbits = read_bits(2, 10, pcb.data);
                    if (rsvbits != 3)
                    {
                        scb.block_type = SYM_BTYPE.SYM_BTYPE_ERROR;
                        return;
                    }

                    int vx_low_s = read_bits(8, 12, pcb.data) | (read_bits(5, 12 + 8, pcb.data) << 8);
                    int vx_high_s = read_bits(8, 25, pcb.data) | (read_bits(5, 25 + 8, pcb.data) << 8);
                    int vx_low_t = read_bits(8, 38, pcb.data) | (read_bits(5, 38 + 8, pcb.data) << 8);
                    int vx_high_t = read_bits(8, 51, pcb.data) | (read_bits(5, 51 + 8, pcb.data) << 8);

                    int all_ones = vx_low_s == 0x1FFF && vx_high_s == 0x1FFF && vx_low_t == 0x1FFF && vx_high_t == 0x1FFF;

                    if ((vx_low_s >= vx_high_s || vx_low_t >= vx_high_t) && !all_ones)
                    {
                        scb.block_type = SYM_BTYPE.SYM_BTYPE_ERROR;
                        return;
                    }
                }
                else
                {
                    // 3D void-extent
                    int vx_low_s = read_bits(9, 10, pcb.data);
                    int vx_high_s = read_bits(9, 19, pcb.data);
                    int vx_low_t = read_bits(9, 28, pcb.data);
                    int vx_high_t = read_bits(9, 37, pcb.data);
                    int vx_low_p = read_bits(9, 46, pcb.data);
                    int vx_high_p = read_bits(9, 55, pcb.data);

                    int all_ones = vx_low_s == 0x1FF && vx_high_s == 0x1FF && vx_low_t == 0x1FF && vx_high_t == 0x1FF && vx_low_p == 0x1FF && vx_high_p == 0x1FF;

                    if ((vx_low_s >= vx_high_s || vx_low_t >= vx_high_t || vx_low_p >= vx_high_p) && !all_ones)
                    {
                        scb.block_type = SYM_BTYPE.SYM_BTYPE_ERROR;
                        return;
                    }
                }

                return;
            }

            uint packed_index = bsd.block_mode_packed_index[block_mode];
            if (packed_index == BLOCK_BAD_BLOCK_MODE)
            {
                scb.block_type = SYM_BTYPE.SYM_BTYPE_ERROR;
                return;
            }

            BlockMode bm = bsd.get_block_mode(block_mode);
            DecimationInfo di = bsd.get_decimation_info(bm.decimation_mode);

            int weight_count = di.weight_count;
            QuantMethod weight_QuantMethod = (QuantMethod)(bm.quant_mode);
            bool is_dual_plane = bm.is_dual_plane;

            int real_weight_count = is_dual_plane ? 2 * weight_count : weight_count;

            int partition_count = read_bits(2, 11, pcb.data) + 1;

            scb.block_mode = (ushort)(block_mode);
            scb.partition_count = (byte)(partition_count);

            for (int i = 0; i < 16; i++)
            {
                bswapped[i] = (byte)(bitrev8(pcb.data[15 - i]));
            }

            int bits_for_weights = get_ise_sequence_bitcount(real_weight_count, weight_QuantMethod);

            int below_weights_pos = 128 - bits_for_weights;

            byte[] indices =new byte[64];
            QuantAndTransferTable qat = ASTCEncWeightQuantXferTables.quant_and_xfer_tables[(int)weight_QuantMethod];

            decode_ise(weight_QuantMethod, real_weight_count, bswapped, indices, 0);

            if (is_dual_plane)
            {
                for (int i = 0; i < weight_count; i++)
                {
                    scb.weights[i] = qat.unscramble_and_unquant_map[indices[2 * i]];
                    scb.weights[i + WEIGHTS_PLANE2_OFFSET] = qat.unscramble_and_unquant_map[indices[2 * i + 1]];
                }
            }
            else
            {
                for (int i = 0; i < weight_count; i++)
                {
                    scb.weights[i] = qat.unscramble_and_unquant_map[indices[i]];
                }
            }

            if (is_dual_plane && partition_count == 4)
            {
                scb.block_type = SYM_BTYPE_ERROR;
                return;
            }

            scb.color_formats_matched = 0;

            // Determine the format of each endpoint pair
            int[] color_formats = new int[Constants.BLOCK_MAX_PARTITIONS];
            int encoded_type_highpart_size = 0;
            if (partition_count == 1)
            {
                color_formats[0] = read_bits(4, 13, pcb.data);
                scb.partition_index = 0;
            }
            else
            {
                encoded_type_highpart_size = (3 * partition_count) - 4;
                below_weights_pos -= encoded_type_highpart_size;
                int encoded_type = read_bits(6, 13 + Constants.PARTITION_INDEX_BITS, pcb.data) | (read_bits(encoded_type_highpart_size, below_weights_pos, pcb.data) << 6);
                int baseclass = encoded_type & 0x3;
                if (baseclass == 0)
                {
                    for (int i = 0; i < partition_count; i++)
                    {
                        color_formats[i] = (encoded_type >> 2) & 0xF;
                    }

                    below_weights_pos += encoded_type_highpart_size;
                    scb.color_formats_matched = 1;
                    encoded_type_highpart_size = 0;
                }
                else
                {
                    int bitpos = 2;
                    baseclass--;

                    for (int i = 0; i < partition_count; i++)
                    {
                        color_formats[i] = (((encoded_type >> bitpos) & 1) + baseclass) << 2;
                        bitpos++;
                    }

                    for (int i = 0; i < partition_count; i++)
                    {
                        color_formats[i] |= (encoded_type >> bitpos) & 3;
                        bitpos += 2;
                    }
                }
                scb.partition_index = (ushort)(read_bits(6, 13, pcb.data) | (read_bits(Constants.PARTITION_INDEX_BITS - 6, 19, pcb.data) << 6));
            }

            for (int i = 0; i < partition_count; i++)
            {
                scb.color_formats[i] = (byte)(color_formats[i]);
            }

            // Determine number of color endpoint integers
            int color_integer_count = 0;
            for (int i = 0; i < partition_count; i++)
            {
                int endpoint_class = color_formats[i] >> 2;
                color_integer_count += (endpoint_class + 1) * 2;
            }

            if (color_integer_count > 18)
            {
                scb.block_type = SYM_BTYPE_ERROR;
                return;
            }

            // Determine the color endpoint format to use
            int[] color_bits_arr = new int[5] { -1, 115 - 4, 113 - 4 - Constants.PARTITION_INDEX_BITS, 113 - 4 - Constants.PARTITION_INDEX_BITS, 113 - 4 - Constants.PARTITION_INDEX_BITS };
            int color_bits = color_bits_arr[partition_count] - bits_for_weights - encoded_type_highpart_size;
            if (is_dual_plane)
            {
                color_bits -= 2;
            }

            if (color_bits < 0)
            {
                color_bits = 0;
            }

            int color_quant_level = quant_mode_table[color_integer_count >> 1][color_bits];
            if (color_quant_level < QUANT_6)
            {
                scb.block_type = SYM_BTYPE.SYM_BTYPE_ERROR;
                return;
            }

            // Unpack the integer color values and assign to endpoints
            scb.quant_mode = (QuantMethod)(color_quant_level);
            byte[] values_to_decode = new byte[32];
            decode_ise((QuantMethod)(color_quant_level), color_integer_count, pcb.data,
                    values_to_decode, (partition_count == 1 ? 17 : 19 + Constants.PARTITION_INDEX_BITS));

            int valuecount_to_decode = 0;
            for (int i = 0; i < partition_count; i++)
            {
                int vals = 2 * (color_formats[i] >> 2) + 2;
                for (int j = 0; j < vals; j++)
                {
                    scb.color_values[i][j] = values_to_decode[j + valuecount_to_decode];
                }
                valuecount_to_decode += vals;
            }

            // Fetch component for second-plane in the case of dual plane of weights.
            if (is_dual_plane)
            {
                scb.plane2_component = (sbyte)(read_bits(2, below_weights_pos - 2, pcb.data));
            }
        }
    }
}