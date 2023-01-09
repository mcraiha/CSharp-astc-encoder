using System;

namespace ASTCEnc
{
	public static class ColorUnquantize
	{
		public static readonly byte[][] color_unquant_tables = new byte[21][] 
		{
			new byte[] {
				0, 255
			},
			new byte[] {
				0, 128, 255
			},
			new byte[] {
				0, 85, 170, 255
			},
			new byte[] {
				0, 64, 128, 192, 255
			},
			new byte[] {
				0, 255, 51, 204, 102, 153
			},
			new byte[] {
				0, 36, 73, 109, 146, 182, 219, 255
			},
			new byte[] {
				0, 255, 28, 227, 56, 199, 84, 171, 113, 142
			},
			new byte[] {
				0, 255, 69, 186, 23, 232, 92, 163, 46, 209, 116, 139
			},
			new byte[] {
				0, 17, 34, 51, 68, 85, 102, 119, 136, 153, 170, 187, 204, 221, 238, 255
			},
			new byte[] {
				0, 255, 67, 188, 13, 242, 80, 175, 27, 228, 94, 161, 40, 215, 107, 148,
				54, 201, 121, 134
			},
			new byte[] {
				0, 255, 33, 222, 66, 189, 99, 156, 11, 244, 44, 211, 77, 178, 110, 145,
				22, 233, 55, 200, 88, 167, 121, 134
			},
			new byte[] {
				0, 8, 16, 24, 33, 41, 49, 57, 66, 74, 82, 90, 99, 107, 115, 123,
				132, 140, 148, 156, 165, 173, 181, 189, 198, 206, 214, 222, 231, 239, 247, 255
			},
			new byte[] {
				0, 255, 32, 223, 65, 190, 97, 158, 6, 249, 39, 216, 71, 184, 104, 151,
				13, 242, 45, 210, 78, 177, 110, 145, 19, 236, 52, 203, 84, 171, 117, 138,
				26, 229, 58, 197, 91, 164, 123, 132
			},
			new byte[] {
				0, 255, 16, 239, 32, 223, 48, 207, 65, 190, 81, 174, 97, 158, 113, 142,
				5, 250, 21, 234, 38, 217, 54, 201, 70, 185, 86, 169, 103, 152, 119, 136,
				11, 244, 27, 228, 43, 212, 59, 196, 76, 179, 92, 163, 108, 147, 124, 131
			},
			new byte[] {
				0, 4, 8, 12, 16, 20, 24, 28, 32, 36, 40, 44, 48, 52, 56, 60,
				65, 69, 73, 77, 81, 85, 89, 93, 97, 101, 105, 109, 113, 117, 121, 125,
				130, 134, 138, 142, 146, 150, 154, 158, 162, 166, 170, 174, 178, 182, 186, 190,
				195, 199, 203, 207, 211, 215, 219, 223, 227, 231, 235, 239, 243, 247, 251, 255
			},
			new byte[] {
				0, 255, 16, 239, 32, 223, 48, 207, 64, 191, 80, 175, 96, 159, 112, 143,
				3, 252, 19, 236, 35, 220, 51, 204, 67, 188, 83, 172, 100, 155, 116, 139,
				6, 249, 22, 233, 38, 217, 54, 201, 71, 184, 87, 168, 103, 152, 119, 136,
				9, 246, 25, 230, 42, 213, 58, 197, 74, 181, 90, 165, 106, 149, 122, 133,
				13, 242, 29, 226, 45, 210, 61, 194, 77, 178, 93, 162, 109, 146, 125, 130
			},
			new byte[] {
				0, 255, 8, 247, 16, 239, 24, 231, 32, 223, 40, 215, 48, 207, 56, 199,
				64, 191, 72, 183, 80, 175, 88, 167, 96, 159, 104, 151, 112, 143, 120, 135,
				2, 253, 10, 245, 18, 237, 26, 229, 35, 220, 43, 212, 51, 204, 59, 196,
				67, 188, 75, 180, 83, 172, 91, 164, 99, 156, 107, 148, 115, 140, 123, 132,
				5, 250, 13, 242, 21, 234, 29, 226, 37, 218, 45, 210, 53, 202, 61, 194,
				70, 185, 78, 177, 86, 169, 94, 161, 102, 153, 110, 145, 118, 137, 126, 129
			},
			new byte[] {
				0, 2, 4, 6, 8, 10, 12, 14, 16, 18, 20, 22, 24, 26, 28, 30,
				32, 34, 36, 38, 40, 42, 44, 46, 48, 50, 52, 54, 56, 58, 60, 62,
				64, 66, 68, 70, 72, 74, 76, 78, 80, 82, 84, 86, 88, 90, 92, 94,
				96, 98, 100, 102, 104, 106, 108, 110, 112, 114, 116, 118, 120, 122, 124, 126,
				129, 131, 133, 135, 137, 139, 141, 143, 145, 147, 149, 151, 153, 155, 157, 159,
				161, 163, 165, 167, 169, 171, 173, 175, 177, 179, 181, 183, 185, 187, 189, 191,
				193, 195, 197, 199, 201, 203, 205, 207, 209, 211, 213, 215, 217, 219, 221, 223,
				225, 227, 229, 231, 233, 235, 237, 239, 241, 243, 245, 247, 249, 251, 253, 255
			},
			new byte[] {
				0, 255, 8, 247, 16, 239, 24, 231, 32, 223, 40, 215, 48, 207, 56, 199,
				64, 191, 72, 183, 80, 175, 88, 167, 96, 159, 104, 151, 112, 143, 120, 135,
				1, 254, 9, 246, 17, 238, 25, 230, 33, 222, 41, 214, 49, 206, 57, 198,
				65, 190, 73, 182, 81, 174, 89, 166, 97, 158, 105, 150, 113, 142, 121, 134,
				3, 252, 11, 244, 19, 236, 27, 228, 35, 220, 43, 212, 51, 204, 59, 196,
				67, 188, 75, 180, 83, 172, 91, 164, 99, 156, 107, 148, 115, 140, 123, 132,
				4, 251, 12, 243, 20, 235, 28, 227, 36, 219, 44, 211, 52, 203, 60, 195,
				68, 187, 76, 179, 84, 171, 92, 163, 100, 155, 108, 147, 116, 139, 124, 131,
				6, 249, 14, 241, 22, 233, 30, 225, 38, 217, 46, 209, 54, 201, 62, 193,
				70, 185, 78, 177, 86, 169, 94, 161, 102, 153, 110, 145, 118, 137, 126, 129
			},
			new byte[] {
				0, 255, 4, 251, 8, 247, 12, 243, 16, 239, 20, 235, 24, 231, 28, 227,
				32, 223, 36, 219, 40, 215, 44, 211, 48, 207, 52, 203, 56, 199, 60, 195,
				64, 191, 68, 187, 72, 183, 76, 179, 80, 175, 84, 171, 88, 167, 92, 163,
				96, 159, 100, 155, 104, 151, 108, 147, 112, 143, 116, 139, 120, 135, 124, 131,
				1, 254, 5, 250, 9, 246, 13, 242, 17, 238, 21, 234, 25, 230, 29, 226,
				33, 222, 37, 218, 41, 214, 45, 210, 49, 206, 53, 202, 57, 198, 61, 194,
				65, 190, 69, 186, 73, 182, 77, 178, 81, 174, 85, 170, 89, 166, 93, 162,
				97, 158, 101, 154, 105, 150, 109, 146, 113, 142, 117, 138, 121, 134, 125, 130,
				2, 253, 6, 249, 10, 245, 14, 241, 18, 237, 22, 233, 26, 229, 30, 225,
				34, 221, 38, 217, 42, 213, 46, 209, 50, 205, 54, 201, 58, 197, 62, 193,
				66, 189, 70, 185, 74, 181, 78, 177, 82, 173, 86, 169, 90, 165, 94, 161,
				98, 157, 102, 153, 106, 149, 110, 145, 114, 141, 118, 137, 122, 133, 126, 129
			},
			new byte[] {
				0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15,
				16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31,
				32, 33, 34, 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47,
				48, 49, 50, 51, 52, 53, 54, 55, 56, 57, 58, 59, 60, 61, 62, 63,
				64, 65, 66, 67, 68, 69, 70, 71, 72, 73, 74, 75, 76, 77, 78, 79,
				80, 81, 82, 83, 84, 85, 86, 87, 88, 89, 90, 91, 92, 93, 94, 95,
				96, 97, 98, 99, 100, 101, 102, 103, 104, 105, 106, 107, 108, 109, 110, 111,
				112, 113, 114, 115, 116, 117, 118, 119, 120, 121, 122, 123, 124, 125, 126, 127,
				128, 129, 130, 131, 132, 133, 134, 135, 136, 137, 138, 139, 140, 141, 142, 143,
				144, 145, 146, 147, 148, 149, 150, 151, 152, 153, 154, 155, 156, 157, 158, 159,
				160, 161, 162, 163, 164, 165, 166, 167, 168, 169, 170, 171, 172, 173, 174, 175,
				176, 177, 178, 179, 180, 181, 182, 183, 184, 185, 186, 187, 188, 189, 190, 191,
				192, 193, 194, 195, 196, 197, 198, 199, 200, 201, 202, 203, 204, 205, 206, 207,
				208, 209, 210, 211, 212, 213, 214, 215, 216, 217, 218, 219, 220, 221, 222, 223,
				224, 225, 226, 227, 228, 229, 230, 231, 232, 233, 234, 235, 236, 237, 238, 239,
				240, 241, 242, 243, 244, 245, 246, 247, 248, 249, 250, 251, 252, 253, 254, 255
			}
		};

		public static vint4 unquant_color(QuantMethod quant_level, vint4 inputq) 
		{
			byte[] unq = color_unquant_tables[quant_level - QuantMethod.QUANT_6];
			return new vint4(unq[inputq.lane(0)], unq[inputq.lane(1)],
	             unq[inputq.lane(2)], unq[inputq.lane(3)]);
		}

		public static byte unquant_color(QuantMethod quant_level, int value) 
		{
			return color_unquant_tables[quant_level - QuantMethod.QUANT_6][value];
		}

		public static vint4 uncontract_color(vint4 input) 
		{
			vmask4 mask = new vmask(true, true, false, false);
			vint4 bc0 = asr<1>(input + input.lane<2>());
			return vint4.select(input, bc0, mask);
		}

		/**
		* @brief Unpack an LDR RGBA color that uses delta encoding.
		*
		* @param      input0q       The raw quantized endpoint 0 color.
		* @param      input1q       The raw quantized endpoint 1 color deltas.
		* @param      quant_level   The quantization level to use.
		* @param[out] output0       The unpacked and unquantized endpoint 0 color.
		* @param[out] output1       The unpacked and unquantized endpoint 1 color.
		*/
		static void rgba_delta_unpack(
			vint4 input0q,
			vint4 input1q,
			QuantMethod quant_level,
			out vint4 output0,
			out vint4 output1
		) {
			// Unquantize color endpoints
			vint4 input0 = unquant_color(quant_level, input0q);
			vint4 input1 = unquant_color(quant_level, input1q);

			// Apply bit transfer
			bit_transfer_signed(input1, input0);

			// Apply blue-uncontraction if needed
			int rgb_sum = hadd_rgb_s(input1);
			input1 = input1 + input0;
			if (rgb_sum < 0)
			{
				input0 = uncontract_color(input0);
				input1 = uncontract_color(input1);
				std::swap(input0, input1);
			}

			output0 = clamp(0, 255, input0);
			output1 = clamp(0, 255, input1);
		}

		/**
		* @brief Unpack an LDR RGB color that uses delta encoding.
		*
		* Output alpha set to 255.
		*
		* @param      input0q       The raw quantized endpoint 0 color.
		* @param      input1q       The raw quantized endpoint 1 color deltas.
		* @param      quant_level   The quantization level to use.
		* @param[out] output0       The unpacked and unquantized endpoint 0 color.
		* @param[out] output1       The unpacked and unquantized endpoint 1 color.
		*/
		static void rgb_delta_unpack(
			vint4 input0q,
			vint4 input1q,
			QuantMethod quant_level,
			out vint4 output0,
			out vint4 output1
		) {
			rgba_delta_unpack(input0q, input1q, quant_level, out output0, out output1);
			output0.set_lane(3, 255);
			output1.set_lane(3, 255);
		}

		/**
		* @brief Unpack an LDR RGBA color that uses direct encoding.
		*
		* @param      input0q       The raw quantized endpoint 0 color.
		* @param      input1q       The raw quantized endpoint 1 color.
		* @param      quant_level   The quantization level to use.
		* @param[out] output0       The unpacked and unquantized endpoint 0 color.
		* @param[out] output1       The unpacked and unquantized endpoint 1 color.
		*/
		static void rgba_unpack(
			vint4 input0q,
			vint4 input1q,
			QuantMethod quant_level,
			out vint4 output0,
			out vint4 output1
		) {
			// Unquantize color endpoints
			vint4 input0 = unquant_color(quant_level, input0q);
			vint4 input1 = unquant_color(quant_level, input1q);

			// Apply blue-uncontraction if needed
			if (hadd_rgb_s(input0) > hadd_rgb_s(input1))
			{
				input0 = uncontract_color(input0);
				input1 = uncontract_color(input1);
				std::swap(input0, input1);
			}

			output0 = input0;
			output1 = input1;
		}

		/**
		* @brief Unpack an LDR RGB color that uses direct encoding.
		*
		* Output alpha set to 255.
		*
		* @param      input0q       The raw quantized endpoint 0 color.
		* @param      input1q       The raw quantized endpoint 1 color.
		* @param      quant_level   The quantization level to use.
		* @param[out] output0       The unpacked and unquantized endpoint 0 color.
		* @param[out] output1       The unpacked and unquantized endpoint 1 color.
		*/
		static void rgb_unpack(
			vint4 input0q,
			vint4 input1q,
			QuantMethod quant_level,
			out vint4 output0,
			out vint4 output1
		) {
			rgba_unpack(input0q, input1q, quant_level, out output0, out output1);
			output0.set_lane(3, 255);
			output1.set_lane(3, 255);
		}

		/**
		* @brief Unpack an LDR RGBA color that uses scaled encoding.
		*
		* Note only the RGB channels use the scaled encoding, alpha uses direct.
		*
		* @param      input0q       The raw quantized endpoint 0 color.
		* @param      alpha1q       The raw quantized endpoint 1 alpha value.
		* @param      scaleq        The raw quantized scale.
		* @param      quant_level   The quantization level to use.
		* @param[out] output0       The unpacked and unquantized endpoint 0 color.
		* @param[out] output1       The unpacked and unquantized endpoint 1 color.
		*/
		static void rgb_scale_alpha_unpack(
			vint4 input0q,
			byte alpha1q,
			byte scaleq,
			QuantMethod quant_level,
			out vint4 output0,
			out vint4 output1
		) {
			// Unquantize color endpoints
			vint4 input = unquant_color(quant_level, input0q);
			byte alpha1 = unquant_color(quant_level, alpha1q);
			byte scale = unquant_color(quant_level, scaleq);

			output1 = input;
			output1.set_lane(3, alpha1);

			output0 = asr<8>(input * scale);
			output0.set_lane(3, input.lane(3));
		}

		/**
		* @brief Unpack an LDR RGB color that uses scaled encoding.
		*
		* Output alpha is 255.
		*
		* @param      input0q       The raw quantized endpoint 0 color.
		* @param      scaleq        The raw quantized scale.
		* @param      quant_level   The quantization level to use.
		* @param[out] output0       The unpacked and unquantized endpoint 0 color.
		* @param[out] output1       The unpacked and unquantized endpoint 1 color.
		*/
		static void rgb_scale_unpack(
			vint4 input0q,
			int scaleq,
			QuantMethod quant_level,
			out vint4 output0,
			out vint4 output1
		) {
			vint4 input = unquant_color(quant_level, input0q);
			int scale = unquant_color(quant_level, scaleq);

			output1 = input;
			output1.set_lane(3, 255);

			output0 = asr<8>(input * scale);
			output0.set_lane(3, 255);
		}

		/**
		* @brief Unpack an LDR L color that uses direct encoding.
		*
		* Output alpha is 255.
		*
		* @param      input         The raw quantized endpoints.
		* @param      quant_level   The quantization level to use.
		* @param[out] output0       The unpacked and unquantized endpoint 0 color.
		* @param[out] output1       The unpacked and unquantized endpoint 1 color.
		*/
		static void luminance_unpack(
			byte[] input,
			QuantMethod quant_level,
			out vint4 output0,
			out vint4 output1
		) {
			int lum0 = unquant_color(quant_level, input[0]);
			int lum1 = unquant_color(quant_level, input[1]);
			output0 = new vint4(lum0, lum0, lum0, 255);
			output1 = new vint4(lum1, lum1, lum1, 255);
		}

		/**
		* @brief Unpack an LDR L color that uses delta encoding.
		*
		* Output alpha is 255.
		*
		* @param      input         The raw quantized endpoints (L0, L1).
		* @param      quant_level   The quantization level to use.
		* @param[out] output0       The unpacked and unquantized endpoint 0 color.
		* @param[out] output1       The unpacked and unquantized endpoint 1 color.
		*/
		static void luminance_delta_unpack(
			byte[] input,
			QuantMethod quant_level,
			out vint4 output0,
			out vint4 output1
		) {
			int v0 = unquant_color(quant_level, input[0]);
			int v1 = unquant_color(quant_level, input[1]);
			int l0 = (v0 >> 2) | (v1 & 0xC0);
			int l1 = l0 + (v1 & 0x3F);

			l1 = astc::min(l1, 255);

			output0 = new vint4(l0, l0, l0, 255);
			output1 = new vint4(l1, l1, l1, 255);
		}

		/**
		* @brief Unpack an LDR LA color that uses direct encoding.
		*
		* @param      input         The raw quantized endpoints (L0, L1, A0, A1).
		* @param      quant_level   The quantization level to use.
		* @param[out] output0       The unpacked and unquantized endpoint 0 color.
		* @param[out] output1       The unpacked and unquantized endpoint 1 color.
		*/
		static void luminance_alpha_unpack(
			byte[] input,
			QuantMethod quant_level,
			out vint4 output0,
			out vint4 output1
		) {
			int lum0 =   unquant_color(quant_level, input[0]);
			int lum1 =   unquant_color(quant_level, input[1]);
			int alpha0 = unquant_color(quant_level, input[2]);
			int alpha1 = unquant_color(quant_level, input[3]);
			output0 = new vint4(lum0, lum0, lum0, alpha0);
			output1 = new vint4(lum1, lum1, lum1, alpha1);
		}

		/**
		* @brief Unpack an LDR LA color that uses delta encoding.
		*
		* @param      input         The raw quantized endpoints (L0, L1, A0, A1).
		* @param      quant_level   The quantization level to use.
		* @param[out] output0       The unpacked and unquantized endpoint 0 color.
		* @param[out] output1       The unpacked and unquantized endpoint 1 color.
		*/
		static void luminance_alpha_delta_unpack(
			byte[] input,
			QuantMethod quant_level,
			out vint4 output0,
			out vint4 output1
		) {
			int lum0 =   unquant_color(quant_level, input[0]);
			int lum1 =   unquant_color(quant_level, input[1]);
			int alpha0 = unquant_color(quant_level, input[2]);
			int alpha1 = unquant_color(quant_level, input[3]);

			lum0 |= (lum1 & 0x80) << 1;
			alpha0 |= (alpha1 & 0x80) << 1;
			lum1 &= 0x7F;
			alpha1 &= 0x7F;
			if ((lum1 & 0x40) == 1)
				lum1 -= 0x80;
			if ((alpha1 & 0x40) == 1)
				alpha1 -= 0x80;

			lum0 >>= 1;
			lum1 >>= 1;
			alpha0 >>= 1;
			alpha1 >>= 1;
			lum1 += lum0;
			alpha1 += alpha0;

			lum1 = ASTCMath.clamp(lum1, 0, 255);
			alpha1 = ASTCMath.clamp(alpha1, 0, 255);

			output0 = new vint4(lum0, lum0, lum0, alpha0);
			output1 = new vint4(lum1, lum1, lum1, alpha1);
		}

		/**
		* @brief Unpack an HDR RGB + offset encoding.
		*
		* @param      input         The raw quantized endpoints (packed and modal).
		* @param      quant_level   The quantization level to use.
		* @param[out] output0       The unpacked and unquantized endpoint 0 color.
		* @param[out] output1       The unpacked and unquantized endpoint 1 color.
		*/
		static void hdr_rgbo_unpack(
			byte[] input,
			QuantMethod quant_level,
			out vint4 output0,
			out vint4 output1
		) {
			int v0 = unquant_color(quant_level, input[0]);
			int v1 = unquant_color(quant_level, input[1]);
			int v2 = unquant_color(quant_level, input[2]);
			int v3 = unquant_color(quant_level, input[3]);

			int modeval = ((v0 & 0xC0) >> 6) | (((v1 & 0x80) >> 7) << 2) | (((v2 & 0x80) >> 7) << 3);

			int majcomp;
			int mode;
			if ((modeval & 0xC) != 0xC)
			{
				majcomp = modeval >> 2;
				mode = modeval & 3;
			}
			else if (modeval != 0xF)
			{
				majcomp = modeval & 3;
				mode = 4;
			}
			else
			{
				majcomp = 0;
				mode = 5;
			}

			int red = v0 & 0x3F;
			int green = v1 & 0x1F;
			int blue = v2 & 0x1F;
			int scale = v3 & 0x1F;

			int bit0 = (v1 >> 6) & 1;
			int bit1 = (v1 >> 5) & 1;
			int bit2 = (v2 >> 6) & 1;
			int bit3 = (v2 >> 5) & 1;
			int bit4 = (v3 >> 7) & 1;
			int bit5 = (v3 >> 6) & 1;
			int bit6 = (v3 >> 5) & 1;

			int ohcomp = 1 << mode;

			if ((ohcomp & 0x30) == 1)
				green |= bit0 << 6;
			if ((ohcomp & 0x3A) == 1)
				green |= bit1 << 5;
			if ((ohcomp & 0x30) == 1)
				blue |= bit2 << 6;
			if ((ohcomp & 0x3A) == 1)
				blue |= bit3 << 5;

			if ((ohcomp & 0x3D) == 1)
				scale |= bit6 << 5;
			if ((ohcomp & 0x2D) == 1)
				scale |= bit5 << 6;
			if ((ohcomp & 0x04) == 1)
				scale |= bit4 << 7;

			if ((ohcomp & 0x3B) == 1)
				red |= bit4 << 6;
			if ((ohcomp & 0x04) == 1)
				red |= bit3 << 6;

			if ((ohcomp & 0x10) == 1)
				red |= bit5 << 7;
			if ((ohcomp & 0x0F) == 1)
				red |= bit2 << 7;

			if ((ohcomp & 0x05) == 1)
				red |= bit1 << 8;
			if ((ohcomp & 0x0A) == 1)
				red |= bit0 << 8;

			if ((ohcomp & 0x05) == 1)
				red |= bit0 << 9;
			if ((ohcomp & 0x02) == 1)
				red |= bit6 << 9;

			if ((ohcomp & 0x01) == 1)
				red |= bit3 << 10;
			if ((ohcomp & 0x02) == 1)
				red |= bit5 << 10;

			// expand to 12 bits.
			int[] shamts = new int[] { 1, 1, 2, 3, 4, 5 };
			int shamt = shamts[mode];
			red <<= shamt;
			green <<= shamt;
			blue <<= shamt;
			scale <<= shamt;

			// on modes 0 to 4, the values stored for "green" and "blue" are differentials,
			// not absolute values.
			if (mode != 5)
			{
				green = red - green;
				blue = red - blue;
			}

			// switch around components.
			int temp;
			switch (majcomp)
			{
			case 1:
				temp = red;
				red = green;
				green = temp;
				break;
			case 2:
				temp = red;
				red = blue;
				blue = temp;
				break;
			default:
				break;
			}

			int red0 = red - scale;
			int green0 = green - scale;
			int blue0 = blue - scale;

			// clamp to [0,0xFFF].
			if (red < 0)
				red = 0;
			if (green < 0)
				green = 0;
			if (blue < 0)
				blue = 0;

			if (red0 < 0)
				red0 = 0;
			if (green0 < 0)
				green0 = 0;
			if (blue0 < 0)
				blue0 = 0;

			output0 = new vint4(red0 << 4, green0 << 4, blue0 << 4, 0x7800);
			output1 = new vint4(red << 4, green << 4, blue << 4, 0x7800);
		}

		/**
		* @brief Unpack an HDR RGB direct encoding.
		*
		* @param      input         The raw quantized endpoints (packed and modal).
		* @param      quant_level   The quantization level to use.
		* @param[out] output0       The unpacked and unquantized endpoint 0 color.
		* @param[out] output1       The unpacked and unquantized endpoint 1 color.
		*/
		static void hdr_rgb_unpack(
			byte[] input,
			QuantMethod quant_level,
			out vint4 output0,
			out vint4 output1
		) {

			int v0 = unquant_color(quant_level, input[0]);
			int v1 = unquant_color(quant_level, input[1]);
			int v2 = unquant_color(quant_level, input[2]);
			int v3 = unquant_color(quant_level, input[3]);
			int v4 = unquant_color(quant_level, input[4]);
			int v5 = unquant_color(quant_level, input[5]);

			// extract all the fixed-placement bitfields
			int modeval = ((v1 & 0x80) >> 7) | (((v2 & 0x80) >> 7) << 1) | (((v3 & 0x80) >> 7) << 2);

			int majcomp = ((v4 & 0x80) >> 7) | (((v5 & 0x80) >> 7) << 1);

			if (majcomp == 3)
			{
				output0 = new vint4(v0 << 8, v2 << 8, (v4 & 0x7F) << 9, 0x7800);
				output1 = new vint4(v1 << 8, v3 << 8, (v5 & 0x7F) << 9, 0x7800);
				return;
			}

			int a = v0 | ((v1 & 0x40) << 2);
			int b0 = v2 & 0x3f;
			int b1 = v3 & 0x3f;
			int c = v1 & 0x3f;
			int d0 = v4 & 0x7f;
			int d1 = v5 & 0x7f;

			// get hold of the number of bits in 'd0' and 'd1'
			int[] dbits_tab = new int[8] { 7, 6, 7, 6, 5, 6, 5, 6 };
			int dbits = dbits_tab[modeval];

			// extract six variable-placement bits
			int bit0 = (v2 >> 6) & 1;
			int bit1 = (v3 >> 6) & 1;
			int bit2 = (v4 >> 6) & 1;
			int bit3 = (v5 >> 6) & 1;
			int bit4 = (v4 >> 5) & 1;
			int bit5 = (v5 >> 5) & 1;

			// and prepend the variable-placement bits depending on mode.
			int ohmod = 1 << modeval;	// one-hot-mode
			if ((ohmod & 0xA4) == 1)
				a |= bit0 << 9;
			if ((ohmod & 0x8) == 1)
				a |= bit2 << 9;
			if ((ohmod & 0x50) == 1)
				a |= bit4 << 9;

			if ((ohmod & 0x50) == 1)
				a |= bit5 << 10;
			if ((ohmod & 0xA0) == 1)
				a |= bit1 << 10;

			if ((ohmod & 0xC0) == 1)
				a |= bit2 << 11;

			if ((ohmod & 0x4) == 1)
				c |= bit1 << 6;
			if ((ohmod & 0xE8) == 1)
				c |= bit3 << 6;

			if ((ohmod & 0x20) == 1)
				c |= bit2 << 7;

			if ((ohmod & 0x5B) == 1)
			{
				b0 |= bit0 << 6;
				b1 |= bit1 << 6;
			}

			if ((ohmod & 0x12) == 1)
			{
				b0 |= bit2 << 7;
				b1 |= bit3 << 7;
			}

			if ((ohmod & 0xAF) == 1)
			{
				d0 |= bit4 << 5;
				d1 |= bit5 << 5;
			}

			if ((ohmod & 0x5) == 1)
			{
				d0 |= bit2 << 6;
				d1 |= bit3 << 6;
			}

			// sign-extend 'd0' and 'd1'
			// note: this code assumes that signed right-shift actually sign-fills, not zero-fills.
			int d0x = d0;
			int d1x = d1;
			int sx_shamt = 32 - dbits;
			d0x <<= sx_shamt;
			d0x >>= sx_shamt;
			d1x <<= sx_shamt;
			d1x >>= sx_shamt;
			d0 = d0x;
			d1 = d1x;

			// expand all values to 12 bits, with left-shift as needed.
			int val_shamt = (modeval >> 1) ^ 3;
			a <<= val_shamt;
			b0 <<= val_shamt;
			b1 <<= val_shamt;
			c <<= val_shamt;
			d0 <<= val_shamt;
			d1 <<= val_shamt;

			// then compute the actual color values.
			int red1 = a;
			int green1 = a - b0;
			int blue1 = a - b1;
			int red0 = a - c;
			int green0 = a - b0 - c - d0;
			int blue0 = a - b1 - c - d1;

			// clamp the color components to [0,2^12 - 1]
			red0 = ASTCMath.clamp(red0, 0, 4095);
			green0 = ASTCMath.clamp(green0, 0, 4095);
			blue0 = ASTCMath.clamp(blue0, 0, 4095);

			red1 = ASTCMath.clamp(red1, 0, 4095);
			green1 = ASTCMath.clamp(green1, 0, 4095);
			blue1 = ASTCMath.clamp(blue1, 0, 4095);

			// switch around the color components
			int temp0, temp1;
			switch (majcomp)
			{
			case 1:					// switch around red and green
				temp0 = red0;
				temp1 = red1;
				red0 = green0;
				red1 = green1;
				green0 = temp0;
				green1 = temp1;
				break;
			case 2:					// switch around red and blue
				temp0 = red0;
				temp1 = red1;
				red0 = blue0;
				red1 = blue1;
				blue0 = temp0;
				blue1 = temp1;
				break;
			case 0:					// no switch
				break;
			}

			output0 = new vint4(red0 << 4, green0 << 4, blue0 << 4, 0x7800);
			output1 = new vint4(red1 << 4, green1 << 4, blue1 << 4, 0x7800);
		}

		/**
		* @brief Unpack an HDR RGB + LDR A direct encoding.
		*
		* @param      input         The raw quantized endpoints (packed and modal).
		* @param      quant_level   The quantization level to use.
		* @param[out] output0       The unpacked and unquantized endpoint 0 color.
		* @param[out] output1       The unpacked and unquantized endpoint 1 color.
		*/
		static void hdr_rgb_ldr_alpha_unpack(
			byte[] input,
			QuantMethod quant_level,
			out vint4 output0,
			out vint4 output1
		) {
			hdr_rgb_unpack(input, quant_level, out output0, out output1);

			int v6 = unquant_color(quant_level, input[6]);
			int v7 = unquant_color(quant_level, input[7]);
			output0.set_lane(3, v6);
			output1.set_lane(3, v7);
		}

		/**
		* @brief Unpack an HDR L (small range) direct encoding.
		*
		* @param      input         The raw quantized endpoints (packed and modal).
		* @param      quant_level   The quantization level to use.
		* @param[out] output0       The unpacked and unquantized endpoint 0 color.
		* @param[out] output1       The unpacked and unquantized endpoint 1 color.
		*/
		static void hdr_luminance_small_range_unpack(
			byte[] input,
			QuantMethod quant_level,
			out vint4 output0,
			out vint4 output1
		) {
			int v0 = unquant_color(quant_level, input[0]);
			int v1 = unquant_color(quant_level, input[1]);

			int y0, y1;
			if ((v0 & 0x80) == 1)
			{
				y0 = ((v1 & 0xE0) << 4) | ((v0 & 0x7F) << 2);
				y1 = (v1 & 0x1F) << 2;
			}
			else
			{
				y0 = ((v1 & 0xF0) << 4) | ((v0 & 0x7F) << 1);
				y1 = (v1 & 0xF) << 1;
			}

			y1 += y0;
			if (y1 > 0xFFF)
				y1 = 0xFFF;

			output0 = new vint4(y0 << 4, y0 << 4, y0 << 4, 0x7800);
			output1 = new vint4(y1 << 4, y1 << 4, y1 << 4, 0x7800);
		}

		/**
		* @brief Unpack an HDR L (large range) direct encoding.
		*
		* @param      input         The raw quantized endpoints (packed and modal).
		* @param      quant_level   The quantization level to use.
		* @param[out] output0       The unpacked and unquantized endpoint 0 color.
		* @param[out] output1       The unpacked and unquantized endpoint 1 color.
		*/
		static void hdr_luminance_large_range_unpack(
			byte[]  input,
			QuantMethod quant_level,
			out vint4 output0,
			out vint4 output1
		) {
			int v0 = unquant_color(quant_level, input[0]);
			int v1 = unquant_color(quant_level, input[1]);

			int y0, y1;
			if (v1 >= v0)
			{
				y0 = v0 << 4;
				y1 = v1 << 4;
			}
			else
			{
				y0 = (v1 << 4) + 8;
				y1 = (v0 << 4) - 8;
			}

			output0 = new vint4(y0 << 4, y0 << 4, y0 << 4, 0x7800);
			output1 = new vint4(y1 << 4, y1 << 4, y1 << 4, 0x7800);
		}

		/**
		* @brief Unpack an HDR A direct encoding.
		*
		* @param      input         The raw quantized endpoints (packed and modal).
		* @param      quant_level   The quantization level to use.
		* @param[out] output0       The unpacked and unquantized endpoint 0 color.
		* @param[out] output1       The unpacked and unquantized endpoint 1 color.
		*/
		static void hdr_alpha_unpack(
			byte[]  input,
			QuantMethod quant_level,
			out int output0,
			out int output1
		) {

			int v6 = unquant_color(quant_level, input[0]);
			int v7 = unquant_color(quant_level, input[1]);

			int selector = ((v6 >> 7) & 1) | ((v7 >> 6) & 2);
			v6 &= 0x7F;
			v7 &= 0x7F;
			if (selector == 3)
			{
				output0 = v6 << 5;
				output1 = v7 << 5;
			}
			else
			{
				v6 |= (v7 << (selector + 1)) & 0x780;
				v7 &= (0x3f >> selector);
				v7 ^= 32 >> selector;
				v7 -= 32 >> selector;
				v6 <<= (4 - selector);
				v7 <<= (4 - selector);
				v7 += v6;

				if (v7 < 0)
					v7 = 0;
				else if (v7 > 0xFFF)
					v7 = 0xFFF;

				output0 = v6;
				output1 = v7;
			}

			output0 <<= 4;
			output1 <<= 4;
		}

		/**
		* @brief Unpack an HDR RGBA direct encoding.
		*
		* @param      input         The raw quantized endpoints (packed and modal).
		* @param      quant_level   The quantization level to use.
		* @param[out] output0       The unpacked and unquantized endpoint 0 color.
		* @param[out] output1       The unpacked and unquantized endpoint 1 color.
		*/
		static void hdr_rgb_hdr_alpha_unpack(
			byte[] input,
			QuantMethod quant_level,
			out vint4 output0,
			out vint4 output1
		) {
			hdr_rgb_unpack(input, quant_level, out output0, out output1);

			int alpha0, alpha1;
			hdr_alpha_unpack(input + 6, quant_level, out alpha0, out alpha1);

			output0.set_lane(3, alpha0);
			output1.set_lane(3, alpha1);
		}

		/* See header for documentation. */
		void unpack_color_endpoints(
			ASTCEncProfile decode_mode,
			EndpointFormats format,
			QuantMethod quant_level,
			byte[]  input,
			out bool rgb_hdr,
			out bool alpha_hdr,
			out vint4 output0,
			out vint4 output1
		) {
			// Assume no NaNs and LDR endpoints unless set later
			rgb_hdr = false;
			alpha_hdr = false;

			bool alpha_hdr_default = false;

			switch (format)
			{
			case EndpointFormats.FMT_LUMINANCE:
				luminance_unpack(input, quant_level, out output0, out output1);
				break;

			case EndpointFormats.FMT_LUMINANCE_DELTA:
				luminance_delta_unpack(input, quant_level, out output0, out output1);
				break;

			case EndpointFormats.FMT_HDR_LUMINANCE_SMALL_RANGE:
				rgb_hdr = true;
				alpha_hdr_default = true;
				hdr_luminance_small_range_unpack(input, quant_level, out output0, out output1);
				break;

			case EndpointFormats.FMT_HDR_LUMINANCE_LARGE_RANGE:
				rgb_hdr = true;
				alpha_hdr_default = true;
				hdr_luminance_large_range_unpack(input, quant_level, out output0, out output1);
				break;

			case EndpointFormats.FMT_LUMINANCE_ALPHA:
				luminance_alpha_unpack(input, quant_level, out output0, out output1);
				break;

			case EndpointFormats.FMT_LUMINANCE_ALPHA_DELTA:
				luminance_alpha_delta_unpack(input, quant_level, out output0, out output1);
				break;

			case EndpointFormats.FMT_RGB_SCALE:
				{
					vint4 input0q = new vint4(input[0], input[1], input[2], 0);
					byte scale = input[3];
					rgb_scale_unpack(input0q, scale, quant_level, out output0, out output1);
				}
				break;

			case EndpointFormats.FMT_RGB_SCALE_ALPHA:
				{
					vint4 input0q = new vint4(input[0], input[1], input[2], input[4]);
					byte alpha1q = input[5];
					byte scaleq = input[3];
					rgb_scale_alpha_unpack(input0q, alpha1q, scaleq, quant_level, out output0, out output1);
				}
				break;

			case EndpointFormats.FMT_HDR_RGB_SCALE:
				rgb_hdr = true;
				alpha_hdr_default = true;
				hdr_rgbo_unpack(input, quant_level, out output0, out output1);
				break;

			case EndpointFormats.FMT_RGB:
				{
					vint4 input0q = new vint4(input[0], input[2], input[4], 0);
					vint4 input1q = new vint4(input[1], input[3], input[5], 0);
					rgb_unpack(input0q, input1q, quant_level, out output0, out output1);
				}
				break;

			case EndpointFormats.FMT_RGB_DELTA:
				{
					vint4 input0q = new vint4(input[0], input[2], input[4], 0);
					vint4 input1q = new vint4(input[1], input[3], input[5], 0);
					rgb_delta_unpack(input0q, input1q, quant_level, out output0, out output1);
				}
				break;

			case EndpointFormats.FMT_HDR_RGB:
				rgb_hdr = true;
				alpha_hdr_default = true;
				hdr_rgb_unpack(input, quant_level, out output0, out output1);
				break;

			case EndpointFormats.FMT_RGBA:
				{
					vint4 input0q = new vint4(input[0], input[2], input[4], input[6]);
					vint4 input1q = new vint4(input[1], input[3], input[5], input[7]);
					rgba_unpack(input0q, input1q, quant_level, out output0, out output1);
				}
				break;

			case EndpointFormats.FMT_RGBA_DELTA:
				{
					vint4 input0q = new vint4(input[0], input[2], input[4], input[6]);
					vint4 input1q = new vint4(input[1], input[3], input[5], input[7]);
					rgba_delta_unpack(input0q, input1q, quant_level, out output0, out output1);
				}
				break;

			case EndpointFormats.FMT_HDR_RGB_LDR_ALPHA:
				rgb_hdr = true;
				hdr_rgb_ldr_alpha_unpack(input, quant_level, out output0, out output1);
				break;

			case EndpointFormats.FMT_HDR_RGBA:
				rgb_hdr = true;
				alpha_hdr = true;
				hdr_rgb_hdr_alpha_unpack(input, quant_level, out output0, out output1);
				break;
			}

			// Assign a correct default alpha
			if (alpha_hdr_default)
			{
				if (decode_mode == ASTCEncProfile.ASTCENC_PRF_HDR)
				{
					output0.set_lane(3, 0x7800);
					output1.set_lane(3, 0x7800);
					alpha_hdr = true;
				}
				else
				{
					output0.set_lane(3, 0x00FF);
					output1.set_lane(3, 0x00FF);
					alpha_hdr = false;
				}
			}

			vint4 ldr_scale = new vint4(257);
			vint4 hdr_scale = new vint4(1);
			vint4 output_scale = ldr_scale;

			// An LDR profile image
			if ((decode_mode == ASTCEncProfile.ASTCENC_PRF_LDR) ||
				(decode_mode == ASTCEncProfile.ASTCENC_PRF_LDR_SRGB))
			{
				// Also matches HDR alpha, as cannot have HDR alpha without HDR RGB
				if (rgb_hdr == true)
				{
					output0 = new vint4(0xFF00, 0x0000, 0xFF00, 0xFF00);
					output1 = new vint4(0xFF00, 0x0000, 0xFF00, 0xFF00);
					output_scale = hdr_scale;

					rgb_hdr = false;
					alpha_hdr = false;
				}
			}
			// An HDR profile image
			else
			{
				vmask4 hdr_lanes = new vmask4(rgb_hdr, rgb_hdr, rgb_hdr, alpha_hdr);
				output_scale = vint4.select(ldr_scale, hdr_scale, hdr_lanes);
			}

			output0 = output0 * output_scale;
			output1 = output1 * output_scale;
		}
	}
}