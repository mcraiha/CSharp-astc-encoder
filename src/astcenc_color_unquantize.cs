
namespace ASTCEnc
{
	public static class ColorUnquantize
	{
		static int rgb_delta_unpack(in int[] input, int quant_level, ref vint4 output0, ref vint4 output1) 
		{
			// unquantize the color endpoints
			int r0 = color_unquant_tables[quant_level][input[0]];
			int g0 = color_unquant_tables[quant_level][input[2]];
			int b0 = color_unquant_tables[quant_level][input[4]];

			int r1 = color_unquant_tables[quant_level][input[1]];
			int g1 = color_unquant_tables[quant_level][input[3]];
			int b1 = color_unquant_tables[quant_level][input[5]];

			// perform the bit-transfer procedure
			r0 |= (r1 & 0x80) << 1;
			g0 |= (g1 & 0x80) << 1;
			b0 |= (b1 & 0x80) << 1;
			r1 &= 0x7F;
			g1 &= 0x7F;
			b1 &= 0x7F;
			if ((r1 & 0x40) == 1)
				r1 -= 0x80;
			if ((g1 & 0x40) == 1)
				g1 -= 0x80;
			if ((b1 & 0x40) == 1)
				b1 -= 0x80;

			r0 >>= 1;
			g0 >>= 1;
			b0 >>= 1;
			r1 >>= 1;
			g1 >>= 1;
			b1 >>= 1;

			int rgbsum = r1 + g1 + b1;

			r1 += r0;
			g1 += g0;
			b1 += b0;

			int retval;

			int r0e, g0e, b0e;
			int r1e, g1e, b1e;

			if (rgbsum >= 0)
			{
				r0e = r0;
				g0e = g0;
				b0e = b0;

				r1e = r1;
				g1e = g1;
				b1e = b1;

				retval = 0;
			}
			else
			{
				r0e = (r1 + b1) >> 1;
				g0e = (g1 + b1) >> 1;
				b0e = b1;

				r1e = (r0 + b0) >> 1;
				g1e = (g0 + b0) >> 1;
				b1e = b0;

				retval = 1;
			}

			r0e = ASTCMath.clamp(r0e, 0, 255);
			g0e = ASTCMath.clamp(g0e, 0, 255);
			b0e = ASTCMath.clamp(b0e, 0, 255);

			r1e = ASTCMath.clamp(r1e, 0, 255);
			g1e = ASTCMath.clamp(g1e, 0, 255);
			b1e = ASTCMath.clamp(b1e, 0, 255);

			output0 = vint4(r0e, g0e, b0e, 0xFF);
			output1 = vint4(r1e, g1e, b1e, 0xFF);

			return retval;
		}
	}
}