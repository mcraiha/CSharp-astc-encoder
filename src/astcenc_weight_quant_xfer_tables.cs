
namespace ASTCEnc
{
    public static class ASTCEncWeightQuantXferTables
    {
        public static readonly QuantAndTransferTable[] quant_and_xfer_tables = new QuantAndTransferTable[12]
        {
            // Quantization method 0, range 0..1
            new QuantAndTransferTable(
                QuantMethod.QUANT_2,
                Utils.CreateAndInitArray<sbyte>(32, new sbyte[] {0, 64}),
                Utils.CreateAndInitArray<sbyte>(32, new sbyte[] {0, 1}),
                Utils.CreateAndInitArray<sbyte>(32, new sbyte[] {0, 64}),
                Utils.CreateAndInitArray<ushort>(65, new ushort[] {0x4000, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 
                0x4000})
            ),
            // Quantization method 1, range 0..2
            new QuantAndTransferTable(
                QuantMethod.QUANT_3,
                Utils.CreateAndInitArray<sbyte>(32, new sbyte[] {0, 32, 64}),
                Utils.CreateAndInitArray<sbyte>(32, new sbyte[] {0, 1, 2}),
                Utils.CreateAndInitArray<sbyte>(32, new sbyte[] {0, 32, 64}),
                Utils.CreateAndInitArray<ushort>(65, new ushort[] {0x2000, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 
                0, 0, 0x4000, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 
                0, 0, 0, 0, 0x4020})
            ),
            // Quantization method 2, range 0..3
            new QuantAndTransferTable(
                QuantMethod.QUANT_4,
                Utils.CreateAndInitArray<sbyte>(32, new sbyte[] {0, 21, 43, 64}),
                Utils.CreateAndInitArray<sbyte>(32, new sbyte[] {0, 1, 2, 3}),
                Utils.CreateAndInitArray<sbyte>(32, new sbyte[] {0, 21, 43, 64}),
                Utils.CreateAndInitArray<ushort>(65, new ushort[] {0x1500, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0x2b00, 0, 0, 0, 0, 
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0x4015, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 
                0, 0, 0, 0, 0, 0, 0, 0, 0x402b})
            ),
            // Quantization method 3, range 0..4
            new QuantAndTransferTable(
                QuantMethod.QUANT_5,
                Utils.CreateAndInitArray<sbyte>(32, new sbyte[] {0, 16, 32, 48, 64}),
                Utils.CreateAndInitArray<sbyte>(32, new sbyte[] {0, 1, 2, 3, 4}),
                Utils.CreateAndInitArray<sbyte>(32, new sbyte[] {0, 16, 32, 48, 64}),
                Utils.CreateAndInitArray<ushort>(65, new ushort[] {0x1000, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0x2000, 0, 0, 0, 0, 0, 0, 0, 0, 0, 
                0, 0, 0, 0, 0, 0, 0x3010, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0x4020, 0, 0, 0, 
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0x4030})
            ),
            // Quantization method 4, range 0..5
            new QuantAndTransferTable(
                QuantMethod.QUANT_6,
                Utils.CreateAndInitArray<sbyte>(32, new sbyte[] {0, 12, 25, 39, 52, 64}),
                Utils.CreateAndInitArray<sbyte>(32, new sbyte[] {0, 2, 4, 5, 3, 1}),
                Utils.CreateAndInitArray<sbyte>(32, new sbyte[] {0, 64, 12, 52, 25, 39}),
                Utils.CreateAndInitArray<ushort>(65, new ushort[] {0x0c00, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0x1900, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 
                0x270c, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0x3419, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 
                0, 0, 0x4027, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0x4034})
            ),
            // Quantization method 5, range 0..7
            new QuantAndTransferTable(
                QuantMethod.QUANT_8,
                Utils.CreateAndInitArray<sbyte>(32, new sbyte[] {0, 9, 18, 27, 37, 46, 55, 64}),
                Utils.CreateAndInitArray<sbyte>(32, new sbyte[] {0, 1, 2, 3, 4, 5, 6, 7}),
                Utils.CreateAndInitArray<sbyte>(32, new sbyte[] {0, 9, 18, 27, 37, 46, 55, 64}),
                Utils.CreateAndInitArray<ushort>(65, new ushort[] {0x0900, 0, 0, 0, 0, 0, 0, 0, 0, 0x1200, 0, 0, 0, 0, 0, 0, 0, 0, 0x1b09, 0, 0, 
                0, 0, 0, 0, 0, 0, 0x2512, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0x2e1b, 0, 0, 0, 0, 0, 0, 0, 0, 
                0x3725, 0, 0, 0, 0, 0, 0, 0, 0, 0x402e, 0, 0, 0, 0, 0, 0, 0, 0, 0x4037})
            ),
            // Quantization method 6, range 0..9
            new QuantAndTransferTable(
                QuantMethod.QUANT_10,
                Utils.CreateAndInitArray<sbyte>(32, new sbyte[] {0, 7, 14, 21, 28, 36, 43, 50, 57, 64}),
                Utils.CreateAndInitArray<sbyte>(32, new sbyte[] {0, 2, 4, 6, 8, 9, 7, 5, 3, 1}),
                Utils.CreateAndInitArray<sbyte>(32, new sbyte[] {0, 64, 7, 57, 14, 50, 21, 43, 28, 36}),
                Utils.CreateAndInitArray<ushort>(65, new ushort[] {0x0700, 0, 0, 0, 0, 0, 0, 0x0e00, 0, 0, 0, 0, 0, 0, 0x1507, 0, 0, 0, 0, 0, 0, 
                0x1c0e, 0, 0, 0, 0, 0, 0, 0x2415, 0, 0, 0, 0, 0, 0, 0, 0x2b1c, 0, 0, 0, 0, 0, 
                0, 0x3224, 0, 0, 0, 0, 0, 0, 0x392b, 0, 0, 0, 0, 0, 0, 0x4032, 0, 0, 0, 0, 0, 
                0, 0x4039})
            ),
            // Quantization method 7, range 0..11
            new QuantAndTransferTable(
                QuantMethod.QUANT_12,
                Utils.CreateAndInitArray<sbyte>(32, new sbyte[] {0, 5, 11, 17, 23, 28, 36, 41, 47, 53, 59, 64}),
                Utils.CreateAndInitArray<sbyte>(32, new sbyte[] {0, 4, 8, 2, 6, 10, 11, 7, 3, 9, 5, 1}),
                Utils.CreateAndInitArray<sbyte>(32, new sbyte[] {0, 64, 17, 47, 5, 59, 23, 41, 11, 53, 28, 36}),
                Utils.CreateAndInitArray<ushort>(65, new ushort[] {0x0500, 0, 0, 0, 0, 0x0b00, 0, 0, 0, 0, 0, 0x1105, 0, 0, 0, 0, 0, 
                0x170b, 0, 0, 0, 0, 0, 0x1c11, 0, 0, 0, 0, 0x2417, 0, 0, 0, 0, 0, 0, 0, 
                0x291c, 0, 0, 0, 0, 0x2f24, 0, 0, 0, 0, 0, 0x3529, 0, 0, 0, 0, 0, 
                0x3b2f, 0, 0, 0, 0, 0, 0x4035, 0, 0, 0, 0, 0x403b})
            ),
            // Quantization method 8, range 0..15
            new QuantAndTransferTable(
                QuantMethod.QUANT_16,
                Utils.CreateAndInitArray<sbyte>(32, new sbyte[] {0, 4, 8, 12, 17, 21, 25, 29, 35, 39, 43, 47, 52, 56, 60, 64}),
                Utils.CreateAndInitArray<sbyte>(32, new sbyte[] {0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15}),
                Utils.CreateAndInitArray<sbyte>(32, new sbyte[] {0, 4, 8, 12, 17, 21, 25, 29, 35, 39, 43, 47, 52, 56, 60, 64}),
                Utils.CreateAndInitArray<ushort>(65, new ushort[] {0x0400, 0, 0, 0, 0x0800, 0, 0, 0, 0x0c04, 0, 0, 0, 0x1108, 0, 0, 0, 0, 
                0x150c, 0, 0, 0, 0x1911, 0, 0, 0, 0x1d15, 0, 0, 0, 0x2319, 0, 0, 0, 0, 
                0, 0x271d, 0, 0, 0, 0x2b23, 0, 0, 0, 0x2f27, 0, 0, 0, 0x342b, 0, 0, 0, 
                0, 0x382f, 0, 0, 0, 0x3c34, 0, 0, 0, 0x4038, 0, 0, 0, 0x403c})
            ),
            // Quantization method 9, range 0..19
            new QuantAndTransferTable(
                QuantMethod.QUANT_20,
                Utils.CreateAndInitArray<sbyte>(32, new sbyte[] {0, 3, 6, 9, 13, 16, 19, 23, 26, 29, 35, 38, 41, 45, 48, 51, 55, 58, 61, 64}),
                Utils.CreateAndInitArray<sbyte>(32, new sbyte[] {0, 4, 8, 12, 16, 2, 6, 10, 14, 18, 19, 15, 11, 7, 3, 17, 13, 9, 5, 1}),
                Utils.CreateAndInitArray<sbyte>(32, new sbyte[] {0, 64, 16, 48, 3, 61, 19, 45, 6, 58, 23, 41, 9, 55, 26, 38, 13, 51, 29, 35}),
                Utils.CreateAndInitArray<ushort>(65, new ushort[] {0x0300, 0, 0, 0x0600, 0, 0, 0x0903, 0, 0, 0x0d06, 0, 0, 0, 
                0x1009, 0, 0, 0x130d, 0, 0, 0x1710, 0, 0, 0, 0x1a13, 0, 0, 
                0x1d17, 0, 0, 0x231a, 0, 0, 0, 0, 0, 0x261d, 0, 0, 0x2923, 0, 0, 
                0x2d26, 0, 0, 0, 0x3029, 0, 0, 0x332d, 0, 0, 0x3730, 0, 0, 0, 
                0x3a33, 0, 0, 0x3d37, 0, 0, 0x403a, 0, 0, 0x403d})
            ),
            // Quantization method 10, range 0..23
            new QuantAndTransferTable(
                QuantMethod.QUANT_24,
                Utils.CreateAndInitArray<sbyte>(32, new sbyte[] {0, 2, 5, 8, 11, 13, 16, 19, 22, 24, 27, 30, 34, 37, 40, 42, 45, 48, 51, 53, 56, 59, 62, 64}),
                Utils.CreateAndInitArray<sbyte>(32, new sbyte[] {0, 8, 16, 2, 10, 18, 4, 12, 20, 6, 14, 22, 23, 15, 7, 21, 13, 5, 19, 11, 3, 17, 9, 1}),
                Utils.CreateAndInitArray<sbyte>(32, new sbyte[] {0, 64, 8, 56, 16, 48, 24, 40, 2, 62, 11, 53, 19, 45, 27, 37, 5, 59, 13, 51, 22, 42, 30, 34}),
                Utils.CreateAndInitArray<ushort>(65, new ushort[] {0x0200, 0, 0x0500, 0, 0, 0x0802, 0, 0, 0x0b05, 0, 0, 0x0d08,
                0, 0x100b, 0, 0, 0x130d, 0, 0, 0x1610, 0, 0, 0x1813, 0, 
                0x1b16, 0, 0, 0x1e18, 0, 0, 0x221b, 0, 0, 0, 0x251e, 0, 0, 
                0x2822, 0, 0, 0x2a25, 0, 0x2d28, 0, 0, 0x302a, 0, 0, 0x332d,
                0, 0, 0x3530, 0, 0x3833, 0, 0, 0x3b35, 0, 0, 0x3e38, 0, 0, 
                0x403b, 0, 0x403e})
            ),
            // Quantization method 11, range 0..31
            new QuantAndTransferTable(
                QuantMethod.QUANT_32,
                Utils.CreateAndInitArray<sbyte>(32, new sbyte[] {0, 2, 4, 6, 8, 10, 12, 14, 16, 18, 20, 22, 24, 26, 28, 30, 34, 36, 38, 40, 42, 44, 46, 48, 50, 52, 54, 56, 58, 60, 62, 64}),
                Utils.CreateAndInitArray<sbyte>(32, new sbyte[] {0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31}),
                Utils.CreateAndInitArray<sbyte>(32, new sbyte[] {0, 2, 4, 6, 8, 10, 12, 14, 16, 18, 20, 22, 24, 26, 28, 30, 34, 36, 38, 40, 42, 44, 46, 48, 50, 52, 54, 56, 58, 60, 62, 64}),
                Utils.CreateAndInitArray<ushort>(65, new ushort[] {0x0200, 0, 0x0400, 0, 0x0602, 0, 0x0804, 0, 0x0a06, 0, 
                0x0c08, 0, 0x0e0a, 0, 0x100c, 0, 0x120e, 0, 0x1410, 0, 
                0x1612, 0, 0x1814, 0, 0x1a16, 0, 0x1c18, 0, 0x1e1a, 0, 
                0x221c, 0, 0, 0, 0x241e, 0, 0x2622, 0, 0x2824, 0, 0x2a26, 0, 
                0x2c28, 0, 0x2e2a, 0, 0x302c, 0, 0x322e, 0, 0x3430, 0, 
                0x3632, 0, 0x3834, 0, 0x3a36, 0, 0x3c38, 0, 0x3e3a, 0, 
                0x403c, 0, 0x403e})
            )
        };
    }
}