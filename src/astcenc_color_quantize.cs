using System;
using System.Diagnostics;

namespace ASTCEnc
{
	public static class ColorQuantize
	{
		private static readonly byte[,] color_quant_tables = new byte[21, 256] 
		{
			{
				0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
				0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
				0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
				0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
				0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
				0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
				0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
				0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
				1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
				1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
				1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
				1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
				1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
				1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
				1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
				1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1
			},
			{
				0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
				0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
				0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
				0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
				0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
				1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
				1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
				1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
				1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
				1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
				1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
				1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
				2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2,
				2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2,
				2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2,
				2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2
			},
			{
				0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
				0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
				0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 1,
				1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
				1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
				1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
				1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
				1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
				2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2,
				2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2,
				2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2,
				2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2,
				2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2,
				2, 2, 2, 2, 2, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3,
				3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3,
				3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3
			},
			{
				0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
				0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
				0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
				1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
				1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
				1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
				1, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2,
				2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2,
				2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2,
				2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2,
				3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3,
				3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3,
				3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3,
				3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3,
				4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4,
				4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4
			},
			{
				0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
				0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2, 2, 2, 2, 2, 2,
				2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2,
				2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2,
				2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 4, 4, 4,
				4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4,
				4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4,
				4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4,
				5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5,
				5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5,
				5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5,
				5, 5, 5, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3,
				3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3,
				3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3,
				3, 3, 3, 3, 3, 3, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
				1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1
			},
			{
				0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
				0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
				1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
				1, 1, 1, 1, 1, 1, 1, 2, 2, 2, 2, 2, 2, 2, 2, 2,
				2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2,
				2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 3, 3, 3, 3,
				3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3,
				3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3,
				4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4,
				4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4,
				4, 4, 4, 4, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5,
				5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5,
				5, 5, 5, 5, 5, 5, 5, 5, 5, 6, 6, 6, 6, 6, 6, 6,
				6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6,
				6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 7, 7, 7,
				7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7
			},
			{
				0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2,
				2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2,
				2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 4, 4, 4, 4, 4,
				4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4,
				4, 4, 4, 4, 4, 4, 4, 6, 6, 6, 6, 6, 6, 6, 6, 6,
				6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6,
				6, 6, 6, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8,
				8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8,
				9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9,
				9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 7, 7, 7,
				7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7,
				7, 7, 7, 7, 7, 7, 7, 7, 7, 5, 5, 5, 5, 5, 5, 5,
				5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5,
				5, 5, 5, 5, 5, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3,
				3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3,
				3, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1
			},
			{
				0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 4, 4, 4, 4,
				4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4,
				4, 4, 4, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8,
				8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 2, 2, 2, 2, 2, 2,
				2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2,
				2, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6,
				6, 6, 6, 6, 6, 6, 6, 6, 6, 10, 10, 10, 10, 10, 10, 10,
				10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10,
				11, 11, 11, 11, 11, 11, 11, 11, 11, 11, 11, 11, 11, 11, 11, 11,
				11, 11, 11, 11, 11, 11, 11, 7, 7, 7, 7, 7, 7, 7, 7, 7,
				7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 3,
				3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3,
				3, 3, 3, 3, 3, 3, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9,
				9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 5, 5, 5,
				5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5,
				5, 5, 5, 5, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1
			},
			{
				0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 1,
				1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 2, 2, 2, 2, 2, 2,
				2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 3, 3, 3, 3, 3,
				3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 4, 4, 4, 4,
				4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 5, 5, 5,
				5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 6, 6,
				6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 7,
				7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7,
				8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8,
				8, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9,
				9, 9, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10,
				10, 10, 10, 11, 11, 11, 11, 11, 11, 11, 11, 11, 11, 11, 11, 11,
				11, 11, 11, 11, 12, 12, 12, 12, 12, 12, 12, 12, 12, 12, 12, 12,
				12, 12, 12, 12, 12, 13, 13, 13, 13, 13, 13, 13, 13, 13, 13, 13,
				13, 13, 13, 13, 13, 13, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14,
				14, 14, 14, 14, 14, 14, 14, 15, 15, 15, 15, 15, 15, 15, 15, 15
			},
			{
				0, 0, 0, 0, 0, 0, 0, 4, 4, 4, 4, 4, 4, 4, 4, 4,
				4, 4, 4, 4, 4, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8,
				8, 8, 12, 12, 12, 12, 12, 12, 12, 12, 12, 12, 12, 12, 12, 12,
				16, 16, 16, 16, 16, 16, 16, 16, 16, 16, 16, 16, 16, 2, 2, 2,
				2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 6, 6, 6, 6, 6, 6,
				6, 6, 6, 6, 6, 6, 6, 6, 10, 10, 10, 10, 10, 10, 10, 10,
				10, 10, 10, 10, 10, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14,
				14, 14, 14, 18, 18, 18, 18, 18, 18, 18, 18, 18, 18, 18, 18, 18,
				19, 19, 19, 19, 19, 19, 19, 19, 19, 19, 19, 19, 19, 15, 15, 15,
				15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 11, 11, 11, 11, 11,
				11, 11, 11, 11, 11, 11, 11, 11, 7, 7, 7, 7, 7, 7, 7, 7,
				7, 7, 7, 7, 7, 7, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3,
				3, 3, 3, 17, 17, 17, 17, 17, 17, 17, 17, 17, 17, 17, 17, 17,
				13, 13, 13, 13, 13, 13, 13, 13, 13, 13, 13, 13, 13, 13, 9, 9,
				9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 5, 5, 5, 5, 5,
				5, 5, 5, 5, 5, 5, 5, 5, 5, 1, 1, 1, 1, 1, 1, 1
			},
			{
				0, 0, 0, 0, 0, 0, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8,
				8, 16, 16, 16, 16, 16, 16, 16, 16, 16, 16, 16, 2, 2, 2, 2,
				2, 2, 2, 2, 2, 2, 2, 10, 10, 10, 10, 10, 10, 10, 10, 10,
				10, 10, 18, 18, 18, 18, 18, 18, 18, 18, 18, 18, 18, 4, 4, 4,
				4, 4, 4, 4, 4, 4, 4, 4, 12, 12, 12, 12, 12, 12, 12, 12,
				12, 12, 12, 20, 20, 20, 20, 20, 20, 20, 20, 20, 20, 20, 6, 6,
				6, 6, 6, 6, 6, 6, 6, 6, 6, 14, 14, 14, 14, 14, 14, 14,
				14, 14, 14, 14, 22, 22, 22, 22, 22, 22, 22, 22, 22, 22, 22, 22,
				23, 23, 23, 23, 23, 23, 23, 23, 23, 23, 23, 23, 15, 15, 15, 15,
				15, 15, 15, 15, 15, 15, 15, 7, 7, 7, 7, 7, 7, 7, 7, 7,
				7, 7, 21, 21, 21, 21, 21, 21, 21, 21, 21, 21, 21, 13, 13, 13,
				13, 13, 13, 13, 13, 13, 13, 13, 5, 5, 5, 5, 5, 5, 5, 5,
				5, 5, 5, 19, 19, 19, 19, 19, 19, 19, 19, 19, 19, 19, 11, 11,
				11, 11, 11, 11, 11, 11, 11, 11, 11, 3, 3, 3, 3, 3, 3, 3,
				3, 3, 3, 3, 17, 17, 17, 17, 17, 17, 17, 17, 17, 17, 17, 9,
				9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 1, 1, 1, 1, 1, 1
			},
			{
				0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 2, 2, 2,
				2, 2, 2, 2, 2, 3, 3, 3, 3, 3, 3, 3, 3, 4, 4, 4,
				4, 4, 4, 4, 4, 4, 5, 5, 5, 5, 5, 5, 5, 5, 6, 6,
				6, 6, 6, 6, 6, 6, 7, 7, 7, 7, 7, 7, 7, 7, 8, 8,
				8, 8, 8, 8, 8, 8, 8, 9, 9, 9, 9, 9, 9, 9, 9, 10,
				10, 10, 10, 10, 10, 10, 10, 11, 11, 11, 11, 11, 11, 11, 11, 12,
				12, 12, 12, 12, 12, 12, 12, 12, 13, 13, 13, 13, 13, 13, 13, 13,
				14, 14, 14, 14, 14, 14, 14, 14, 15, 15, 15, 15, 15, 15, 15, 15,
				16, 16, 16, 16, 16, 16, 16, 16, 17, 17, 17, 17, 17, 17, 17, 17,
				18, 18, 18, 18, 18, 18, 18, 18, 19, 19, 19, 19, 19, 19, 19, 19,
				19, 20, 20, 20, 20, 20, 20, 20, 20, 21, 21, 21, 21, 21, 21, 21,
				21, 22, 22, 22, 22, 22, 22, 22, 22, 23, 23, 23, 23, 23, 23, 23,
				23, 23, 24, 24, 24, 24, 24, 24, 24, 24, 25, 25, 25, 25, 25, 25,
				25, 25, 26, 26, 26, 26, 26, 26, 26, 26, 27, 27, 27, 27, 27, 27,
				27, 27, 27, 28, 28, 28, 28, 28, 28, 28, 28, 29, 29, 29, 29, 29,
				29, 29, 29, 30, 30, 30, 30, 30, 30, 30, 30, 31, 31, 31, 31, 31
			},
			{
				0, 0, 0, 0, 8, 8, 8, 8, 8, 8, 16, 16, 16, 16, 16, 16,
				16, 24, 24, 24, 24, 24, 24, 32, 32, 32, 32, 32, 32, 32, 2, 2,
				2, 2, 2, 2, 10, 10, 10, 10, 10, 10, 10, 18, 18, 18, 18, 18,
				18, 26, 26, 26, 26, 26, 26, 26, 34, 34, 34, 34, 34, 34, 4, 4,
				4, 4, 4, 4, 4, 12, 12, 12, 12, 12, 12, 20, 20, 20, 20, 20,
				20, 20, 28, 28, 28, 28, 28, 28, 36, 36, 36, 36, 36, 36, 36, 6,
				6, 6, 6, 6, 6, 14, 14, 14, 14, 14, 14, 14, 22, 22, 22, 22,
				22, 22, 30, 30, 30, 30, 30, 30, 30, 38, 38, 38, 38, 38, 38, 38,
				39, 39, 39, 39, 39, 39, 39, 31, 31, 31, 31, 31, 31, 31, 23, 23,
				23, 23, 23, 23, 15, 15, 15, 15, 15, 15, 15, 7, 7, 7, 7, 7,
				7, 37, 37, 37, 37, 37, 37, 37, 29, 29, 29, 29, 29, 29, 21, 21,
				21, 21, 21, 21, 21, 13, 13, 13, 13, 13, 13, 5, 5, 5, 5, 5,
				5, 5, 35, 35, 35, 35, 35, 35, 27, 27, 27, 27, 27, 27, 27, 19,
				19, 19, 19, 19, 19, 11, 11, 11, 11, 11, 11, 11, 3, 3, 3, 3,
				3, 3, 33, 33, 33, 33, 33, 33, 33, 25, 25, 25, 25, 25, 25, 17,
				17, 17, 17, 17, 17, 17, 9, 9, 9, 9, 9, 9, 1, 1, 1, 1
			},
			{
				0, 0, 0, 16, 16, 16, 16, 16, 16, 32, 32, 32, 32, 32, 2, 2,
				2, 2, 2, 18, 18, 18, 18, 18, 18, 34, 34, 34, 34, 34, 4, 4,
				4, 4, 4, 4, 20, 20, 20, 20, 20, 36, 36, 36, 36, 36, 6, 6,
				6, 6, 6, 6, 22, 22, 22, 22, 22, 38, 38, 38, 38, 38, 38, 8,
				8, 8, 8, 8, 24, 24, 24, 24, 24, 24, 40, 40, 40, 40, 40, 10,
				10, 10, 10, 10, 26, 26, 26, 26, 26, 26, 42, 42, 42, 42, 42, 12,
				12, 12, 12, 12, 12, 28, 28, 28, 28, 28, 44, 44, 44, 44, 44, 14,
				14, 14, 14, 14, 14, 30, 30, 30, 30, 30, 46, 46, 46, 46, 46, 46,
				47, 47, 47, 47, 47, 47, 31, 31, 31, 31, 31, 15, 15, 15, 15, 15,
				15, 45, 45, 45, 45, 45, 29, 29, 29, 29, 29, 13, 13, 13, 13, 13,
				13, 43, 43, 43, 43, 43, 27, 27, 27, 27, 27, 27, 11, 11, 11, 11,
				11, 41, 41, 41, 41, 41, 25, 25, 25, 25, 25, 25, 9, 9, 9, 9,
				9, 39, 39, 39, 39, 39, 39, 23, 23, 23, 23, 23, 7, 7, 7, 7,
				7, 7, 37, 37, 37, 37, 37, 21, 21, 21, 21, 21, 5, 5, 5, 5,
				5, 5, 35, 35, 35, 35, 35, 19, 19, 19, 19, 19, 19, 3, 3, 3,
				3, 3, 33, 33, 33, 33, 33, 17, 17, 17, 17, 17, 17, 1, 1, 1
			},
			{
				0, 0, 0, 1, 1, 1, 1, 2, 2, 2, 2, 3, 3, 3, 3, 4,
				4, 4, 4, 5, 5, 5, 5, 6, 6, 6, 6, 7, 7, 7, 7, 8,
				8, 8, 8, 9, 9, 9, 9, 10, 10, 10, 10, 11, 11, 11, 11, 12,
				12, 12, 12, 13, 13, 13, 13, 14, 14, 14, 14, 15, 15, 15, 15, 16,
				16, 16, 16, 16, 17, 17, 17, 17, 18, 18, 18, 18, 19, 19, 19, 19,
				20, 20, 20, 20, 21, 21, 21, 21, 22, 22, 22, 22, 23, 23, 23, 23,
				24, 24, 24, 24, 25, 25, 25, 25, 26, 26, 26, 26, 27, 27, 27, 27,
				28, 28, 28, 28, 29, 29, 29, 29, 30, 30, 30, 30, 31, 31, 31, 31,
				32, 32, 32, 32, 33, 33, 33, 33, 34, 34, 34, 34, 35, 35, 35, 35,
				36, 36, 36, 36, 37, 37, 37, 37, 38, 38, 38, 38, 39, 39, 39, 39,
				40, 40, 40, 40, 41, 41, 41, 41, 42, 42, 42, 42, 43, 43, 43, 43,
				44, 44, 44, 44, 45, 45, 45, 45, 46, 46, 46, 46, 47, 47, 47, 47,
				47, 48, 48, 48, 48, 49, 49, 49, 49, 50, 50, 50, 50, 51, 51, 51,
				51, 52, 52, 52, 52, 53, 53, 53, 53, 54, 54, 54, 54, 55, 55, 55,
				55, 56, 56, 56, 56, 57, 57, 57, 57, 58, 58, 58, 58, 59, 59, 59,
				59, 60, 60, 60, 60, 61, 61, 61, 61, 62, 62, 62, 62, 63, 63, 63
			},
			{
				0, 0, 16, 16, 16, 32, 32, 32, 48, 48, 48, 48, 64, 64, 64, 2,
				2, 2, 18, 18, 18, 34, 34, 34, 50, 50, 50, 50, 66, 66, 66, 4,
				4, 4, 20, 20, 20, 36, 36, 36, 36, 52, 52, 52, 68, 68, 68, 6,
				6, 6, 22, 22, 22, 38, 38, 38, 38, 54, 54, 54, 70, 70, 70, 8,
				8, 8, 24, 24, 24, 24, 40, 40, 40, 56, 56, 56, 72, 72, 72, 10,
				10, 10, 26, 26, 26, 26, 42, 42, 42, 58, 58, 58, 74, 74, 74, 12,
				12, 12, 12, 28, 28, 28, 44, 44, 44, 60, 60, 60, 76, 76, 76, 14,
				14, 14, 14, 30, 30, 30, 46, 46, 46, 62, 62, 62, 78, 78, 78, 78,
				79, 79, 79, 79, 63, 63, 63, 47, 47, 47, 31, 31, 31, 15, 15, 15,
				15, 77, 77, 77, 61, 61, 61, 45, 45, 45, 29, 29, 29, 13, 13, 13,
				13, 75, 75, 75, 59, 59, 59, 43, 43, 43, 27, 27, 27, 27, 11, 11,
				11, 73, 73, 73, 57, 57, 57, 41, 41, 41, 25, 25, 25, 25, 9, 9,
				9, 71, 71, 71, 55, 55, 55, 39, 39, 39, 39, 23, 23, 23, 7, 7,
				7, 69, 69, 69, 53, 53, 53, 37, 37, 37, 37, 21, 21, 21, 5, 5,
				5, 67, 67, 67, 51, 51, 51, 51, 35, 35, 35, 19, 19, 19, 3, 3,
				3, 65, 65, 65, 49, 49, 49, 49, 33, 33, 33, 17, 17, 17, 1, 1
			},
			{
				0, 0, 32, 32, 64, 64, 64, 2, 2, 2, 34, 34, 66, 66, 66, 4,
				4, 4, 36, 36, 68, 68, 68, 6, 6, 6, 38, 38, 70, 70, 70, 8,
				8, 8, 40, 40, 40, 72, 72, 10, 10, 10, 42, 42, 42, 74, 74, 12,
				12, 12, 44, 44, 44, 76, 76, 14, 14, 14, 46, 46, 46, 78, 78, 16,
				16, 16, 48, 48, 48, 80, 80, 80, 18, 18, 50, 50, 50, 82, 82, 82,
				20, 20, 52, 52, 52, 84, 84, 84, 22, 22, 54, 54, 54, 86, 86, 86,
				24, 24, 56, 56, 56, 88, 88, 88, 26, 26, 58, 58, 58, 90, 90, 90,
				28, 28, 60, 60, 60, 92, 92, 92, 30, 30, 62, 62, 62, 94, 94, 94,
				95, 95, 95, 63, 63, 63, 31, 31, 93, 93, 93, 61, 61, 61, 29, 29,
				91, 91, 91, 59, 59, 59, 27, 27, 89, 89, 89, 57, 57, 57, 25, 25,
				87, 87, 87, 55, 55, 55, 23, 23, 85, 85, 85, 53, 53, 53, 21, 21,
				83, 83, 83, 51, 51, 51, 19, 19, 81, 81, 81, 49, 49, 49, 17, 17,
				17, 79, 79, 47, 47, 47, 15, 15, 15, 77, 77, 45, 45, 45, 13, 13,
				13, 75, 75, 43, 43, 43, 11, 11, 11, 73, 73, 41, 41, 41, 9, 9,
				9, 71, 71, 71, 39, 39, 7, 7, 7, 69, 69, 69, 37, 37, 5, 5,
				5, 67, 67, 67, 35, 35, 3, 3, 3, 65, 65, 65, 33, 33, 1, 1
			},
			{
				0, 0, 1, 1, 2, 2, 3, 3, 4, 4, 5, 5, 6, 6, 7, 7,
				8, 8, 9, 9, 10, 10, 11, 11, 12, 12, 13, 13, 14, 14, 15, 15,
				16, 16, 17, 17, 18, 18, 19, 19, 20, 20, 21, 21, 22, 22, 23, 23,
				24, 24, 25, 25, 26, 26, 27, 27, 28, 28, 29, 29, 30, 30, 31, 31,
				32, 32, 33, 33, 34, 34, 35, 35, 36, 36, 37, 37, 38, 38, 39, 39,
				40, 40, 41, 41, 42, 42, 43, 43, 44, 44, 45, 45, 46, 46, 47, 47,
				48, 48, 49, 49, 50, 50, 51, 51, 52, 52, 53, 53, 54, 54, 55, 55,
				56, 56, 57, 57, 58, 58, 59, 59, 60, 60, 61, 61, 62, 62, 63, 63,
				64, 64, 65, 65, 66, 66, 67, 67, 68, 68, 69, 69, 70, 70, 71, 71,
				72, 72, 73, 73, 74, 74, 75, 75, 76, 76, 77, 77, 78, 78, 79, 79,
				80, 80, 81, 81, 82, 82, 83, 83, 84, 84, 85, 85, 86, 86, 87, 87,
				88, 88, 89, 89, 90, 90, 91, 91, 92, 92, 93, 93, 94, 94, 95, 95,
				96, 96, 97, 97, 98, 98, 99, 99, 100, 100, 101, 101, 102, 102, 103, 103,
				104, 104, 105, 105, 106, 106, 107, 107, 108, 108, 109, 109, 110, 110, 111, 111,
				112, 112, 113, 113, 114, 114, 115, 115, 116, 116, 117, 117, 118, 118, 119, 119,
				120, 120, 121, 121, 122, 122, 123, 123, 124, 124, 125, 125, 126, 126, 127, 127
			},
			{
				0, 32, 32, 64, 96, 96, 128, 128, 2, 34, 34, 66, 98, 98, 130, 130,
				4, 36, 36, 68, 100, 100, 132, 132, 6, 38, 38, 70, 102, 102, 134, 134,
				8, 40, 40, 72, 104, 104, 136, 136, 10, 42, 42, 74, 106, 106, 138, 138,
				12, 44, 44, 76, 108, 108, 140, 140, 14, 46, 46, 78, 110, 110, 142, 142,
				16, 48, 48, 80, 112, 112, 144, 144, 18, 50, 50, 82, 114, 114, 146, 146,
				20, 52, 52, 84, 116, 116, 148, 148, 22, 54, 54, 86, 118, 118, 150, 150,
				24, 56, 56, 88, 120, 120, 152, 152, 26, 58, 58, 90, 122, 122, 154, 154,
				28, 60, 60, 92, 124, 124, 156, 156, 30, 62, 62, 94, 126, 126, 158, 158,
				159, 159, 127, 127, 95, 63, 63, 31, 157, 157, 125, 125, 93, 61, 61, 29,
				155, 155, 123, 123, 91, 59, 59, 27, 153, 153, 121, 121, 89, 57, 57, 25,
				151, 151, 119, 119, 87, 55, 55, 23, 149, 149, 117, 117, 85, 53, 53, 21,
				147, 147, 115, 115, 83, 51, 51, 19, 145, 145, 113, 113, 81, 49, 49, 17,
				143, 143, 111, 111, 79, 47, 47, 15, 141, 141, 109, 109, 77, 45, 45, 13,
				139, 139, 107, 107, 75, 43, 43, 11, 137, 137, 105, 105, 73, 41, 41, 9,
				135, 135, 103, 103, 71, 39, 39, 7, 133, 133, 101, 101, 69, 37, 37, 5,
				131, 131, 99, 99, 67, 35, 35, 3, 129, 129, 97, 97, 65, 33, 33, 1
			},
			{
				0, 64, 128, 128, 2, 66, 130, 130, 4, 68, 132, 132, 6, 70, 134, 134,
				8, 72, 136, 136, 10, 74, 138, 138, 12, 76, 140, 140, 14, 78, 142, 142,
				16, 80, 144, 144, 18, 82, 146, 146, 20, 84, 148, 148, 22, 86, 150, 150,
				24, 88, 152, 152, 26, 90, 154, 154, 28, 92, 156, 156, 30, 94, 158, 158,
				32, 96, 160, 160, 34, 98, 162, 162, 36, 100, 164, 164, 38, 102, 166, 166,
				40, 104, 168, 168, 42, 106, 170, 170, 44, 108, 172, 172, 46, 110, 174, 174,
				48, 112, 176, 176, 50, 114, 178, 178, 52, 116, 180, 180, 54, 118, 182, 182,
				56, 120, 184, 184, 58, 122, 186, 186, 60, 124, 188, 188, 62, 126, 190, 190,
				191, 191, 127, 63, 189, 189, 125, 61, 187, 187, 123, 59, 185, 185, 121, 57,
				183, 183, 119, 55, 181, 181, 117, 53, 179, 179, 115, 51, 177, 177, 113, 49,
				175, 175, 111, 47, 173, 173, 109, 45, 171, 171, 107, 43, 169, 169, 105, 41,
				167, 167, 103, 39, 165, 165, 101, 37, 163, 163, 99, 35, 161, 161, 97, 33,
				159, 159, 95, 31, 157, 157, 93, 29, 155, 155, 91, 27, 153, 153, 89, 25,
				151, 151, 87, 23, 149, 149, 85, 21, 147, 147, 83, 19, 145, 145, 81, 17,
				143, 143, 79, 15, 141, 141, 77, 13, 139, 139, 75, 11, 137, 137, 73, 9,
				135, 135, 71, 7, 133, 133, 69, 5, 131, 131, 67, 3, 129, 129, 65, 1
			},
			{
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

		

		private static readonly sbyte[,] quant_mode_table = new sbyte[17, 128];

		public static void build_quant_mode_table()
		{
			for (int i = 0; i <= 16; i++)
			{
				for (int j = 0; j < 128; j++)
				{
					quant_mode_table[i, j] = -1;
				}
			}

			for (int i = 0; i < 21; i++)
			{
				for (int j = 1; j <= 16; j++)
				{
					int p = get_ise_sequence_bitcount(2 * j, (QuantMethod)i);
					if (p < 128)
					{
						quant_mode_table[j, p] = i;
					}
				}
			}

			for (int i = 0; i <= 16; i++)
			{
				int largest_value_so_far = -1;
				for (int j = 0; j < 128; j++)
				{
					if (quant_mode_table[i, j] > largest_value_so_far)
					{
						largest_value_so_far = quant_mode_table[i, j];
					}
					else
					{
						quant_mode_table[i, j] = largest_value_so_far;
					}
				}
			}
		}

		private static int Clamp(int value, int min, int max)  
		{  
			return (value < min) ? min : (value > max) ? max : value;  
		}

		private static int cqt_lookup(int quant_level, int value) 
		{
			// TODO: Make this unsigned and avoid the low clamp
			value = Clamp(value, 0, 255);
			return color_quant_tables[quant_level, value];
		}

		public static void quantize_rgb(vfloat4 color0, vfloat4 color1, ref int[] output, int quant_level) 
		{
			float scale = 1.0f / 257.0f;

			float r0 = ASTCMath.clamp255f(color0.lane<0>() * scale);
			float g0 = ASTCMath.clamp255f(color0.lane(1) * scale);
			float b0 = ASTCMath.clamp255f(color0.lane(2) * scale);

			float r1 = ASTCMath.clamp255f(color1.lane<0>() * scale);
			float g1 = ASTCMath.clamp255f(color1.lane(1) * scale);
			float b1 = ASTCMath.clamp255f(color1.lane(2) * scale);

			int ri0, gi0, bi0, ri1, gi1, bi1;
			int ri0b, gi0b, bi0b, ri1b, gi1b, bi1b;
			float rgb0_addon = 0.5f;
			float rgb1_addon = 0.5f;
			int iters = 0;
			do
			{
				ri0 = cqt_lookup(quant_level, ASTCMath.flt2int_rd(r0 + rgb0_addon));
				gi0 = cqt_lookup(quant_level, ASTCMath.flt2int_rd(g0 + rgb0_addon));
				bi0 = cqt_lookup(quant_level, ASTCMath.flt2int_rd(b0 + rgb0_addon));
				ri1 = cqt_lookup(quant_level, ASTCMath.flt2int_rd(r1 + rgb1_addon));
				gi1 = cqt_lookup(quant_level, ASTCMath.flt2int_rd(g1 + rgb1_addon));
				bi1 = cqt_lookup(quant_level, ASTCMath.flt2int_rd(b1 + rgb1_addon));

				ri0b = ColorUnquantize.color_unquant_tables[quant_level][ri0];
				gi0b = ColorUnquantize.color_unquant_tables[quant_level][gi0];
				bi0b = ColorUnquantize.color_unquant_tables[quant_level][bi0];
				ri1b = ColorUnquantize.color_unquant_tables[quant_level][ri1];
				gi1b = ColorUnquantize.color_unquant_tables[quant_level][gi1];
				bi1b = ColorUnquantize.color_unquant_tables[quant_level][bi1];

				rgb0_addon -= 0.2f;
				rgb1_addon += 0.2f;
				iters++;
			} while (ri0b + gi0b + bi0b > ri1b + gi1b + bi1b);

			output[0] = ri0;
			output[1] = ri1;
			output[2] = gi0;
			output[3] = gi1;
			output[4] = bi0;
			output[5] = bi1;
		}

		/* quantize an RGBA color. */
		public static void quantize_rgba(vfloat4 color0, vfloat4 color1,ref int[] output, int quant_level) 
		{
			float scale = 1.0f / 257.0f;

			float a0 = ASTCMath.clamp255f(color0.lane(3) * scale);
			float a1 = ASTCMath.clamp255f(color1.lane(3) * scale);

			int ai0 = color_quant_tables[quant_level, ASTCMath.flt2int_rtn(a0)];
			int ai1 = color_quant_tables[quant_level, ASTCMath.flt2int_rtn(a1)];

			output[6] = ai0;
			output[7] = ai1;

			quantize_rgb(color0, color1, ref output, quant_level);
		}

		/* attempt to quantize RGB endpoint values with blue-contraction. Returns 1 on failure, 0 on success. */
		public static bool try_quantize_rgb_blue_contract(vfloat4 color0, vfloat4 color1, ref int[] output, int quant_level) 
		{
			float scale = 1.0f / 257.0f;

			float r0 = color0.lane<0>() * scale;
			float g0 = color0.lane(1) * scale;
			float b0 = color0.lane(2) * scale;

			float r1 = color1.lane<0>() * scale;
			float g1 = color1.lane(1) * scale;
			float b1 = color1.lane(2) * scale;

			// inverse blue-contraction. This can produce an overflow;
			// just bail out immediately if this is the case.
			r0 += (r0 - b0);
			g0 += (g0 - b0);
			r1 += (r1 - b1);
			g1 += (g1 - b1);

			if (r0 < 0.0f || r0 > 255.0f || g0 < 0.0f || g0 > 255.0f || b0 < 0.0f || b0 > 255.0f ||
				r1 < 0.0f || r1 > 255.0f || g1 < 0.0f || g1 > 255.0f || b1 < 0.0f || b1 > 255.0f)
			{
				return false;
			}

			// quantize the inverse-blue-contracted color
			int ri0 = color_quant_tables[quant_level, ASTCMath.flt2int_rtn(r0)];
			int gi0 = color_quant_tables[quant_level, ASTCMath.flt2int_rtn(g0)];
			int bi0 = color_quant_tables[quant_level, ASTCMath.flt2int_rtn(b0)];
			int ri1 = color_quant_tables[quant_level, ASTCMath.flt2int_rtn(r1)];
			int gi1 = color_quant_tables[quant_level, ASTCMath.flt2int_rtn(g1)];
			int bi1 = color_quant_tables[quant_level, ASTCMath.flt2int_rtn(b1)];

			// then unquantize again
			int ru0 = ColorUnquantize.color_unquant_tables[quant_level][ri0];
			int gu0 = ColorUnquantize.color_unquant_tables[quant_level][gi0];
			int bu0 = ColorUnquantize.color_unquant_tables[quant_level][bi0];
			int ru1 = ColorUnquantize.color_unquant_tables[quant_level][ri1];
			int gu1 = ColorUnquantize.color_unquant_tables[quant_level][gi1];
			int bu1 = ColorUnquantize.color_unquant_tables[quant_level][bi1];

			// if color #1 is not larger than color #0, then blue-contraction is not a valid approach.
			// note that blue-contraction and quantization may itself change this order, which is why
			// we must only test AFTER blue-contraction.
			if (ru1 + gu1 + bu1 <= ru0 + gu0 + bu0)
			{
				return false;
			}

			output[0] = ri1;
			output[1] = ri0;
			output[2] = gi1;
			output[3] = gi0;
			output[4] = bi1;
			output[5] = bi0;

			return true;
		}

		/* quantize an RGBA color with blue-contraction */
		public static bool try_quantize_rgba_blue_contract(vfloat4 color0, vfloat4 color1, ref int[] output, int quant_level) 
		{
			float scale = 1.0f / 257.0f;

			float a0 = ASTCMath.clamp255f(color0.lane(3) * scale);
			float a1 = ASTCMath.clamp255f(color1.lane(3) * scale);

			output[7] = color_quant_tables[quant_level, ASTCMath.flt2int_rtn(a0)];
			output[6] = color_quant_tables[quant_level, ASTCMath.flt2int_rtn(a1)];

			return try_quantize_rgb_blue_contract(color0, color1, ref output, quant_level);
		}

		// delta-encoding:
		// at decode time, we move one bit from the offset to the base and seize another bit as a sign bit;
		// we then unquantize both values as if they contain one extra bit.

		// if the sum of the offsets is nonnegative, then we encode a regular delta.

		/* attempt to quantize an RGB endpoint value with delta-encoding. */
		static bool try_quantize_rgb_delta(vfloat4 color0, vfloat4 color1, ref int[] output, int quant_level) 
		{
			float scale = 1.0f / 257.0f;

			float r0 = ASTCMath.clamp255f(color0.lane<0>() * scale);
			float g0 = ASTCMath.clamp255f(color0.lane(1) * scale);
			float b0 = ASTCMath.clamp255f(color0.lane(2) * scale);

			float r1 = ASTCMath.clamp255f(color1.lane<0>() * scale);
			float g1 = ASTCMath.clamp255f(color1.lane(1) * scale);
			float b1 = ASTCMath.clamp255f(color1.lane(2) * scale);

			// transform r0 to unorm9
			int r0a = ASTCMath.flt2int_rtn(r0);
			int g0a = ASTCMath.flt2int_rtn(g0);
			int b0a = ASTCMath.flt2int_rtn(b0);

			r0a <<= 1;
			g0a <<= 1;
			b0a <<= 1;

			// mask off the top bit
			int r0b = r0a & 0xFF;
			int g0b = g0a & 0xFF;
			int b0b = b0a & 0xFF;

			// quantize, then unquantize in order to get a value that we take
			// differences against.
			int r0be = color_quant_tables[quant_level, r0b];
			int g0be = color_quant_tables[quant_level, g0b];
			int b0be = color_quant_tables[quant_level, b0b];

			r0b = ColorUnquantize.color_unquant_tables[quant_level][r0be];
			g0b = ColorUnquantize.color_unquant_tables[quant_level][g0be];
			b0b = ColorUnquantize.color_unquant_tables[quant_level][b0be];
			r0b |= r0a & 0x100;			// final unquantized-values for endpoint 0.
			g0b |= g0a & 0x100;
			b0b |= b0a & 0x100;

			// then, get hold of the second value
			int r1d = ASTCMath.flt2int_rtn(r1);
			int g1d = ASTCMath.flt2int_rtn(g1);
			int b1d = ASTCMath.flt2int_rtn(b1);

			r1d <<= 1;
			g1d <<= 1;
			b1d <<= 1;
			// and take differences!
			r1d -= r0b;
			g1d -= g0b;
			b1d -= b0b;

			// check if the difference is too large to be encodable.
			if (r1d > 63 || g1d > 63 || b1d > 63 || r1d < -64 || g1d < -64 || b1d < -64)
			{
				return false;
			}

			// insert top bit of the base into the offset
			r1d &= 0x7F;
			g1d &= 0x7F;
			b1d &= 0x7F;

			r1d |= (r0b & 0x100) >> 1;
			g1d |= (g0b & 0x100) >> 1;
			b1d |= (b0b & 0x100) >> 1;

			// then quantize & unquantize; if this causes any of the top two bits to flip,
			// then encoding fails, since we have then corrupted either the top bit of the base
			// or the sign bit of the offset.
			int r1de = color_quant_tables[quant_level, r1d];
			int g1de = color_quant_tables[quant_level, g1d];
			int b1de = color_quant_tables[quant_level, b1d];

			int r1du = ColorUnquantize.color_unquant_tables[quant_level][r1de];
			int g1du = ColorUnquantize.color_unquant_tables[quant_level][g1de];
			int b1du = ColorUnquantize.color_unquant_tables[quant_level][b1de];

			if ((((r1d ^ r1du) | (g1d ^ g1du) | (b1d ^ b1du)) & 0xC0) == 1)
			{
				return false;
			}

			// check that the sum of the encoded offsets is nonnegative, else encoding fails
			r1du &= 0x7f;
			g1du &= 0x7f;
			b1du &= 0x7f;

			if ((r1du & 0x40) == 1)
			{
				r1du -= 0x80;
			}

			if ((g1du & 0x40) == 1)
			{
				g1du -= 0x80;
			}

			if ((b1du & 0x40) == 1)
			{
				b1du -= 0x80;
			}

			if (r1du + g1du + b1du < 0)
			{
				return false;
			}

			// check that the offsets produce legitimate sums as well.
			r1du += r0b;
			g1du += g0b;
			b1du += b0b;
			if (r1du < 0 || r1du > 0x1FF || g1du < 0 || g1du > 0x1FF || b1du < 0 || b1du > 0x1FF)
			{
				return false;
			}

			// OK, we've come this far; we can now encode legitimate values.
			output[0] = r0be;
			output[1] = r1de;
			output[2] = g0be;
			output[3] = g1de;
			output[4] = b0be;
			output[5] = b1de;

			return true;
		}

		static bool try_quantize_rgb_delta_blue_contract(vfloat4 color0, vfloat4 color1, ref int[] output, int quant_level) 
		{
			// Note: Switch around endpoint colors already at start
			float scale = 1.0f / 257.0f;

			float r1 = color0.lane<0>() * scale;
			float g1 = color0.lane(1) * scale;
			float b1 = color0.lane(2) * scale;

			float r0 = color1.lane<0>() * scale;
			float g0 = color1.lane(1) * scale;
			float b0 = color1.lane(2) * scale;

			// inverse blue-contraction. This step can perform an overflow, in which case
			// we will bail out immediately.
			r0 += (r0 - b0);
			g0 += (g0 - b0);
			r1 += (r1 - b1);
			g1 += (g1 - b1);

			if (r0 < 0.0f || r0 > 255.0f || g0 < 0.0f || g0 > 255.0f || b0 < 0.0f || b0 > 255.0f ||
				r1 < 0.0f || r1 > 255.0f || g1 < 0.0f || g1 > 255.0f || b1 < 0.0f || b1 > 255.0f)
			{
				return false;
			}

			// transform r0 to unorm9
			int r0a = ASTCMath.flt2int_rtn(r0);
			int g0a = ASTCMath.flt2int_rtn(g0);
			int b0a = ASTCMath.flt2int_rtn(b0);
			r0a <<= 1;
			g0a <<= 1;
			b0a <<= 1;

			// mask off the top bit
			int r0b = r0a & 0xFF;
			int g0b = g0a & 0xFF;
			int b0b = b0a & 0xFF;

			// quantize, then unquantize in order to get a value that we take
			// differences against.
			int r0be = color_quant_tables[quant_level, r0b];
			int g0be = color_quant_tables[quant_level, g0b];
			int b0be = color_quant_tables[quant_level, b0b];

			r0b = ColorUnquantize.color_unquant_tables[quant_level][r0be];
			g0b = ColorUnquantize.color_unquant_tables[quant_level][g0be];
			b0b = ColorUnquantize.color_unquant_tables[quant_level][b0be];
			r0b |= r0a & 0x100;			// final unquantized-values for endpoint 0.
			g0b |= g0a & 0x100;
			b0b |= b0a & 0x100;

			// then, get hold of the second value
			int r1d = ASTCMath.flt2int_rtn(r1);
			int g1d = ASTCMath.flt2int_rtn(g1);
			int b1d = ASTCMath.flt2int_rtn(b1);

			r1d <<= 1;
			g1d <<= 1;
			b1d <<= 1;
			// and take differences!
			r1d -= r0b;
			g1d -= g0b;
			b1d -= b0b;

			// check if the difference is too large to be encodable.
			if (r1d > 63 || g1d > 63 || b1d > 63 || r1d < -64 || g1d < -64 || b1d < -64)
			{
				return false;
			}

			// insert top bit of the base into the offset
			r1d &= 0x7F;
			g1d &= 0x7F;
			b1d &= 0x7F;

			r1d |= (r0b & 0x100) >> 1;
			g1d |= (g0b & 0x100) >> 1;
			b1d |= (b0b & 0x100) >> 1;

			// then quantize & unquantize; if this causes any of the top two bits to flip,
			// then encoding fails, since we have then corrupted either the top bit of the base
			// or the sign bit of the offset.
			int r1de = color_quant_tables[quant_level, r1d];
			int g1de = color_quant_tables[quant_level, g1d];
			int b1de = color_quant_tables[quant_level, b1d];

			int r1du = ColorUnquantize.color_unquant_tables[quant_level][r1de];
			int g1du = ColorUnquantize.color_unquant_tables[quant_level][g1de];
			int b1du = ColorUnquantize.color_unquant_tables[quant_level][b1de];

			if (((r1d ^ r1du) | (g1d ^ g1du) | (b1d ^ b1du)) & 0xC0)
			{
				return false;
			}

			// check that the sum of the encoded offsets is negative, else encoding fails
			// note that this is inverse of the test for non-blue-contracted RGB.
			r1du &= 0x7f;
			g1du &= 0x7f;
			b1du &= 0x7f;

			if (r1du & 0x40)
			{
				r1du -= 0x80;
			}

			if (g1du & 0x40)
			{
				g1du -= 0x80;
			}

			if (b1du & 0x40)
			{
				b1du -= 0x80;
			}

			if (r1du + g1du + b1du >= 0)
			{
				return false;
			}

			// check that the offsets produce legitimate sums as well.
			r1du += r0b;
			g1du += g0b;
			b1du += b0b;

			if (r1du < 0 || r1du > 0x1FF || g1du < 0 || g1du > 0x1FF || b1du < 0 || b1du > 0x1FF)
			{
				return false;
			}

			// OK, we've come this far; we can now encode legitimate values.
			output[0] = r0be;
			output[1] = r1de;
			output[2] = g0be;
			output[3] = g1de;
			output[4] = b0be;
			output[5] = b1de;

			return true;
		}

		static bool try_quantize_alpha_delta(vfloat4 color0, vfloat4 color1, ref int[] output, int quant_level) 
		{
			float scale = 1.0f / 257.0f;

			float a0 = ASTCMath.clamp255f(color0.lane(3) * scale);
			float a1 = ASTCMath.clamp255f(color1.lane(3) * scale);

			int a0a = ASTCMath.flt2int_rtn(a0);
			a0a <<= 1;
			int a0b = a0a & 0xFF;
			int a0be = color_quant_tables[quant_level, a0b];
			a0b = ColorUnquantize.color_unquant_tables[quant_level][a0be];
			a0b |= a0a & 0x100;
			int a1d = ASTCMath.flt2int_rtn(a1);
			a1d <<= 1;
			a1d -= a0b;
			if (a1d > 63 || a1d < -64)
			{
				return false;
			}
			a1d &= 0x7F;
			a1d |= (a0b & 0x100) >> 1;
			int a1de = color_quant_tables[quant_level, a1d];
			int a1du = ColorUnquantize.color_unquant_tables[quant_level][a1de];
			if (((a1d ^ a1du) & 0xC0) == 1)
			{
				return false;
			}
			a1du &= 0x7F;
			if ((a1du & 0x40) == 1)
			{
				a1du -= 0x80;
			}
			a1du += a0b;
			if (a1du < 0 || a1du > 0x1FF)
			{
				return false;
			}
			output[6] = a0be;
			output[7] = a1de;
			return true;
		}

		static bool try_quantize_luminance_alpha_delta(vfloat4 color0, vfloat4 color1, ref int[] output, int quant_level) 
		{
			float scale = 1.0f / 257.0f;

			float l0 = ASTCMath.clamp255f(hadd_rgb_s(color0) * ((1.0f / 3.0f) * scale));
			float l1 = ASTCMath.clamp255f(hadd_rgb_s(color1) * ((1.0f / 3.0f) * scale));

			float a0 = ASTCMath.clamp255f(color0.lane(3) * scale);
			float a1 = ASTCMath.clamp255f(color1.lane(3) * scale);

			int l0a = ASTCMath.flt2int_rtn(l0);
			int a0a = ASTCMath.flt2int_rtn(a0);
			l0a <<= 1;
			a0a <<= 1;
			int l0b = l0a & 0xFF;
			int a0b = a0a & 0xFF;
			int l0be = color_quant_tables[quant_level, l0b];
			int a0be = color_quant_tables[quant_level, a0b];
			l0b = ColorUnquantize.color_unquant_tables[quant_level][l0be];
			a0b = ColorUnquantize.color_unquant_tables[quant_level][a0be];
			l0b |= l0a & 0x100;
			a0b |= a0a & 0x100;
			int l1d = ASTCMath.flt2int_rtn(l1);
			int a1d = ASTCMath.flt2int_rtn(a1);
			l1d <<= 1;
			a1d <<= 1;
			l1d -= l0b;
			a1d -= a0b;
			if (l1d > 63 || l1d < -64)
			{
				return false;
			}
			if (a1d > 63 || a1d < -64)
			{
				return false;
			}
			l1d &= 0x7F;
			a1d &= 0x7F;
			l1d |= (l0b & 0x100) >> 1;
			a1d |= (a0b & 0x100) >> 1;

			int l1de = color_quant_tables[quant_level, l1d];
			int a1de = color_quant_tables[quant_level, a1d];
			int l1du = ColorUnquantize.color_unquant_tables[quant_level][l1de];
			int a1du = ColorUnquantize.color_unquant_tables[quant_level][a1de];
			if (((l1d ^ l1du) & 0xC0) == 1)
			{
				return false;
			}
			if (((a1d ^ a1du) & 0xC0) == 1)
			{
				return false;
			}
			l1du &= 0x7F;
			a1du &= 0x7F;
			if ((l1du & 0x40) == 1)
			{
				l1du -= 0x80;
			}
			if ((a1du & 0x40) == 1)
			{
				a1du -= 0x80;
			}
			l1du += l0b;
			a1du += a0b;
			if (l1du < 0 || l1du > 0x1FF)
			{
				return false;
			}
			if (a1du < 0 || a1du > 0x1FF)
			{
				return false;
			}
			output[0] = l0be;
			output[1] = l1de;
			output[2] = a0be;
			output[3] = a1de;

			return true;
		}

		static bool try_quantize_rgba_delta(vfloat4 color0, vfloat4 color1, ref int[] output, int quant_level) 
		{
			bool alpha_delta_res = try_quantize_alpha_delta(color0, color1, ref output, quant_level);

			if (alpha_delta_res == false)
			{
				return false;
			}

			return try_quantize_rgb_delta(color0, color1, ref output, quant_level);
		}

		static bool try_quantize_rgba_delta_blue_contract(vfloat4 color0, vfloat4 color1, ref int[] output, int quant_level) 
		{
			// notice that for the alpha encoding, we are swapping around color0 and color1;
			// this is because blue-contraction involves swapping around the two colors.
			int alpha_delta_res = try_quantize_alpha_delta(color1, color0, ref output, quant_level);

			if (alpha_delta_res == 0)
			{
				return false;
			}

			return try_quantize_rgb_delta_blue_contract(color0, color1, ref output, quant_level);
		}

		static void quantize_rgbs_new(vfloat4 rgbs_color, ref int[] output, int quant_level) 
		{
			float scale = 1.0f / 257.0f;

			float r = ASTCMath.clamp255f(rgbs_color.lane<0>() * scale);
			float g = ASTCMath.clamp255f(rgbs_color.lane(1) * scale);
			float b = ASTCMath.clamp255f(rgbs_color.lane(2) * scale);

			int ri = color_quant_tables[quant_level, ASTCMath.flt2int_rtn(r)];
			int gi = color_quant_tables[quant_level, ASTCMath.flt2int_rtn(g)];
			int bi = color_quant_tables[quant_level, ASTCMath.flt2int_rtn(b)];

			int ru = ColorUnquantize.color_unquant_tables[quant_level][ri];
			int gu = ColorUnquantize.color_unquant_tables[quant_level][gi];
			int bu = ColorUnquantize.color_unquant_tables[quant_level][bi];

			float oldcolorsum = hadd_rgb_s(rgbs_color) * scale;
			float newcolorsum = (float)(ru + gu + bu);

			float scalea = ASTCMath.clamp1f(rgbs_color.lane(3) * (oldcolorsum + 1e-10f) / (newcolorsum + 1e-10f));
			int scale_idx = ASTCMath.flt2int_rtn(scalea * 256.0f);
			scale_idx = ASTCMath.clamp(scale_idx, 0, 255);

			output[0] = ri;
			output[1] = gi;
			output[2] = bi;
			output[3] = color_quant_tables[quant_level, scale_idx];
		}

		static void quantize_rgbs_alpha_new(vfloat4 color0, vfloat4 color1, vfloat4 rgbs_color, ref int[] output, int quant_level) 
		{
			float scale = 1.0f / 257.0f;

			float a0 = ASTCMath.clamp255f(color0.lane(3) * scale);
			float a1 = ASTCMath.clamp255f(color1.lane(3) * scale);

			int ai0 = color_quant_tables[quant_level, ASTCMath.flt2int_rtn(a0)];
			int ai1 = color_quant_tables[quant_level, ASTCMath.flt2int_rtn(a1)];

			output[4] = ai0;
			output[5] = ai1;

			quantize_rgbs_new(rgbs_color, ref output, quant_level);
		}

		static void quantize_luminance(vfloat4 color0, vfloat4 color1, ref int[] output, int quant_level) 
		{
			float scale = 1.0f / 257.0f;

			color0 = color0 * scale;
			color1 = color1 * scale;

			float lum0 = ASTCMath.clamp255f(hadd_rgb_s(color0) * (1.0f / 3.0f));
			float lum1 = ASTCMath.clamp255f(hadd_rgb_s(color1) * (1.0f / 3.0f));

			if (lum0 > lum1)
			{
				float avg = (lum0 + lum1) * 0.5f;
				lum0 = avg;
				lum1 = avg;
			}

			output[0] = color_quant_tables[quant_level, ASTCMath.flt2int_rtn(lum0)];
			output[1] = color_quant_tables[quant_level, ASTCMath.flt2int_rtn(lum1)];
		}

		static void quantize_luminance_alpha(vfloat4 color0, vfloat4 color1, int[] output, int quant_level) 
		{
			float scale = 1.0f / 257.0f;

			color0 = color0 * scale;
			color1 = color1 * scale;

			float lum0 = ASTCMath.clamp255f(hadd_rgb_s(color0) * (1.0f / 3.0f));
			float lum1 = ASTCMath.clamp255f(hadd_rgb_s(color1) * (1.0f / 3.0f));

			float a0 = ASTCMath.clamp255f(color0.lane(3));
			float a1 = ASTCMath.clamp255f(color1.lane(3));

			// if the endpoints are *really* close, then pull them apart slightly;
			// this affords for >8 bits precision for normal maps.
			if (quant_level > 18)
			{
				if (Math.Abs(lum0 - lum1) < 3.0f)
				{
					if (lum0 < lum1)
					{
						lum0 -= 0.5f;
						lum1 += 0.5f;
					}
					else
					{
						lum0 += 0.5f;
						lum1 -= 0.5f;
					}
					lum0 = ASTCMath.clamp255f(lum0);
					lum1 = ASTCMath.clamp255f(lum1);
				}

				if (Math.Abs(a0 - a1) < 3.0f)
				{
					if (a0 < a1)
					{
						a0 -= 0.5f;
						a1 += 0.5f;
					}
					else
					{
						a0 += 0.5f;
						a1 -= 0.5f;
					}
					a0 = ASTCMath.clamp255f(a0);
					a1 = ASTCMath.clamp255f(a1);
				}
			}

			output[0] = color_quant_tables[quant_level, ASTCMath.flt2int_rtn(lum0)];
			output[1] = color_quant_tables[quant_level, ASTCMath.flt2int_rtn(lum1)];
			output[2] = color_quant_tables[quant_level, ASTCMath.flt2int_rtn(a0)];
			output[3] = color_quant_tables[quant_level, ASTCMath.flt2int_rtn(a1)];
		}

		// quantize and unquantize a number, wile making sure to retain the top two bits.
		static void quantize_and_unquantize_retain_top_two_bits(int quant_level, int value_to_quantize, ref int quantized_value, ref int unquantized_value) 
		{
			bool perform_loop;
			int quantval;
			int uquantval;

			do
			{
				quantval = color_quant_tables[quant_level, value_to_quantize];
				uquantval = ColorUnquantize.color_unquant_tables[quant_level][quantval];

				// perform looping if the top two bits were modified by quant/unquant
				perform_loop = (value_to_quantize & 0xC0) != (uquantval & 0xC0);

				if ((uquantval & 0xC0) > (value_to_quantize & 0xC0))
				{
					// quant/unquant rounded UP so that the top two bits changed;
					// decrement the input value in hopes that this will avoid rounding up.
					value_to_quantize--;
				}
				else if ((uquantval & 0xC0) < (value_to_quantize & 0xC0))
				{
					// quant/unquant rounded DOWN so that the top two bits changed;
					// decrement the input value in hopes that this will avoid rounding down.
					value_to_quantize--;
				}
			} while (perform_loop);

			quantized_value = quantval;
			unquantized_value = uquantval;
		}

		// quantize and unquantize a number, wile making sure to retain the top four bits.
		static void quantize_and_unquantize_retain_top_four_bits(int quant_level, int value_to_quantize, ref int quantized_value, ref int unquantized_value) 
		{
			bool perform_loop;
			int quantval;
			int uquantval;

			do
			{
				quantval = color_quant_tables[quant_level, value_to_quantize];
				uquantval = ColorUnquantize.color_unquant_tables[quant_level][quantval];

				// perform looping if the top two bits were modified by quant/unquant
				perform_loop = (value_to_quantize & 0xF0) != (uquantval & 0xF0);

				if ((uquantval & 0xF0) > (value_to_quantize & 0xF0))
				{
					// quant/unquant rounded UP so that the top two bits changed;
					// decrement the input value in hopes that this will avoid rounding up.
					value_to_quantize--;
				}
				else if ((uquantval & 0xF0) < (value_to_quantize & 0xF0))
				{
					// quant/unquant rounded DOWN so that the top two bits changed;
					// decrement the input value in hopes that this will avoid rounding down.
					value_to_quantize--;
				}
			} while (perform_loop);

			quantized_value = quantval;
			unquantized_value = uquantval;
		}

		/* HDR color encoding, take #3 */
		static void quantize_hdr_rgbo3(vfloat4 color, ref int[] output, int quant_level) 
		{
			color.set_lane<0>(color.lane<0>() + color.lane(3));
			color.set_lane<1>(color.lane(1) + color.lane(3));
			color.set_lane<2>(color.lane(2) + color.lane(3));

			color = clamp(0.0f, 65535.0f, color);

			vfloat4 color_bak = color;
			int majcomp;
			if (color.lane<0>() > color.lane(1) && color.lane<0>() > color.lane(2))
				majcomp = 0;			// red is largest component
			else if (color.lane(1) > color.lane(2))
				majcomp = 1;			// green is largest component
			else
				majcomp = 2;			// blue is largest component

			// swap around the red component and the largest component.
			switch (majcomp)
			{
			case 1:
				color = new vfloat4(color.lane(1), color.lane<0>(), color.lane(2), color.lane(3));
				break;
			case 2:
				color = new vfloat4(color.lane(2), color.lane(1), color.lane<0>(), color.lane(3));
				break;
			default:
				break;
			}

			int[,] mode_bits = new int[5, 3] {
				{11, 5, 7},
				{11, 6, 5},
				{10, 5, 8},
				{9, 6, 7},
				{8, 7, 6}
			};

			float[,] mode_cutoffs = new float[5, 2] {
				{1024, 4096},
				{2048, 1024},
				{2048, 16384},
				{8192, 16384},
				{32768, 16384}
			};

			float[] mode_rscales = new float[5] {
				32.0f,
				32.0f,
				64.0f,
				128.0f,
				256.0f,
			};

			float[] mode_scales = new float[5] {
				1.0f / 32.0f,
				1.0f / 32.0f,
				1.0f / 64.0f,
				1.0f / 128.0f,
				1.0f / 256.0f,
			};

			float r_base = color.lane<0>();
			float g_base = color.lane<0>() - color.lane(1) ;
			float b_base = color.lane<0>() - color.lane(2) ;
			float s_base = color.lane(3) ;

			for (int mode = 0; mode < 5; mode++)
			{
				if (g_base > mode_cutoffs[mode, 0] || b_base > mode_cutoffs[mode, 0] || s_base > mode_cutoffs[mode, 1])
				{
					continue;
				}

				// encode the mode into a 4-bit vector.
				int mode_enc = mode < 4 ? (mode | (majcomp << 2)) : (majcomp | 0xC);

				float mode_scale = mode_scales[mode];
				float mode_rscale = mode_rscales[mode];

				int gb_intcutoff = 1 << mode_bits[mode, 1];
				int s_intcutoff = 1 << mode_bits[mode, 2];

				// first, quantize and unquantize R.
				int r_intval = ASTCMath.flt2int_rtn(r_base * mode_scale);

				int r_lowbits = r_intval & 0x3f;

				r_lowbits |= (mode_enc & 3) << 6;

				int r_quantval = 0;
				int r_uquantval = 0;
				quantize_and_unquantize_retain_top_two_bits(quant_level, r_lowbits, ref r_quantval, ref r_uquantval);

				r_intval = (r_intval & ~0x3f) | (r_uquantval & 0x3f);
				float r_fval = r_intval * mode_rscale;

				// next, recompute G and B, then quantize and unquantize them.
				float g_fval = r_fval - color.lane(1) ;
				float b_fval = r_fval - color.lane(2) ;

				g_fval = ASTCMath.clamp(g_fval, 0.0f, 65535.0f);
				b_fval = ASTCMath.clamp(b_fval, 0.0f, 65535.0f);

				int g_intval = ASTCMath.flt2int_rtn(g_fval * mode_scale);
				int b_intval = ASTCMath.flt2int_rtn(b_fval * mode_scale);

				if (g_intval >= gb_intcutoff || b_intval >= gb_intcutoff)
				{
					continue;
				}

				int g_lowbits = g_intval & 0x1f;
				int b_lowbits = b_intval & 0x1f;

				int bit0 = 0;
				int bit1 = 0;
				int bit2 = 0;
				int bit3 = 0;

				switch (mode)
				{
				case 0:
				case 2:
					bit0 = (r_intval >> 9) & 1;
					break;
				case 1:
				case 3:
					bit0 = (r_intval >> 8) & 1;
					break;
				case 4:
				case 5:
					bit0 = (g_intval >> 6) & 1;
					break;
				}

				switch (mode)
				{
				case 0:
				case 1:
				case 2:
				case 3:
					bit2 = (r_intval >> 7) & 1;
					break;
				case 4:
				case 5:
					bit2 = (b_intval >> 6) & 1;
					break;
				}

				switch (mode)
				{
				case 0:
				case 2:
					bit1 = (r_intval >> 8) & 1;
					break;
				case 1:
				case 3:
				case 4:
				case 5:
					bit1 = (g_intval >> 5) & 1;
					break;
				}

				switch (mode)
				{
				case 0:
					bit3 = (r_intval >> 10) & 1;
					break;
				case 2:
					bit3 = (r_intval >> 6) & 1;
					break;
				case 1:
				case 3:
				case 4:
				case 5:
					bit3 = (b_intval >> 5) & 1;
					break;
				}

				g_lowbits |= (mode_enc & 0x4) << 5;
				b_lowbits |= (mode_enc & 0x8) << 4;

				g_lowbits |= bit0 << 6;
				g_lowbits |= bit1 << 5;
				b_lowbits |= bit2 << 6;
				b_lowbits |= bit3 << 5;

				int g_quantval = 0;
				int b_quantval = 0;
				int g_uquantval = 0;
				int b_uquantval = 0;

				quantize_and_unquantize_retain_top_four_bits(quant_level, g_lowbits, ref g_quantval, ref g_uquantval);

				quantize_and_unquantize_retain_top_four_bits(quant_level, b_lowbits, ref b_quantval, ref b_uquantval);

				g_intval = (g_intval & ~0x1f) | (g_uquantval & 0x1f);
				b_intval = (b_intval & ~0x1f) | (b_uquantval & 0x1f);

				g_fval = static_cast<float>(g_intval) * mode_rscale;
				b_fval = static_cast<float>(b_intval) * mode_rscale;

				// finally, recompute the scale value, based on the errors
				// introduced to red, green and blue.

				// If the error is positive, then the R,G,B errors combined have raised the color
				// value overall; as such, the scale value needs to be increased.
				float rgb_errorsum = (r_fval - color.lane<0>() ) + (r_fval - g_fval - color.lane(1) ) + (r_fval - b_fval - color.lane(2) );

				float s_fval = s_base + rgb_errorsum * (1.0f / 3.0f);
				s_fval = ASTCMath.clamp(s_fval, 0.0f, 1e9f);

				int s_intval = ASTCMath.flt2int_rtn(s_fval * mode_scale);

				if (s_intval >= s_intcutoff)
				{
					continue;
				}

				int s_lowbits = s_intval & 0x1f;

				int bit4;
				int bit5;
				int bit6;
				switch (mode)
				{
				case 1:
					bit6 = (r_intval >> 9) & 1;
					break;
				default:
					bit6 = (s_intval >> 5) & 1;
					break;
				}

				switch (mode)
				{
				case 4:
					bit5 = (r_intval >> 7) & 1;
					break;
				case 1:
					bit5 = (r_intval >> 10) & 1;
					break;
				default:
					bit5 = (s_intval >> 6) & 1;
					break;
				}

				switch (mode)
				{
				case 2:
					bit4 = (s_intval >> 7) & 1;
					break;
				default:
					bit4 = (r_intval >> 6) & 1;
					break;
				}

				s_lowbits |= bit6 << 5;
				s_lowbits |= bit5 << 6;
				s_lowbits |= bit4 << 7;

				int s_quantval = 0;
				int s_uquantval = 0;

				quantize_and_unquantize_retain_top_four_bits(quant_level, s_lowbits, ref s_quantval, ref s_uquantval);
				output[0] = r_quantval;
				output[1] = g_quantval;
				output[2] = b_quantval;
				output[3] = s_quantval;
				return;
			}

			// failed to encode any of the modes above? In that case,
			// encode using mode #5.
			float[] vals = new float[4];
			vals[0] = color_bak.lane<0>();
			vals[1] = color_bak.lane(1);
			vals[2] = color_bak.lane(2);
			vals[3] = color_bak.lane(3);

			int[] ivals = new int[4];
			float[] cvals = new float[3];

			for (int i = 0; i < 3; i++)
			{
				vals[i] = ASTCMath.clamp(vals[i], 0.0f, 65020.0f);
				ivals[i] = ASTCMath.flt2int_rtn(vals[i] * (1.0f / 512.0f));
				cvals[i] = static_cast<float>(ivals[i]) * 512.0f;
			}

			float rgb_errorsum = (cvals[0] - vals[0]) + (cvals[1] - vals[1]) + (cvals[2] - vals[2]);
			vals[3] += rgb_errorsum * (1.0f / 3.0f);

			vals[3] = ASTCMath.clamp(vals[3], 0.0f, 65020.0f);
			ivals[3] = ASTCMath.flt2int_rtn(vals[3] * (1.0f / 512.0f));

			int[] encvals = new int[4];
			encvals[0] = (ivals[0] & 0x3f) | 0xC0;
			encvals[1] = (ivals[1] & 0x7f) | 0x80;
			encvals[2] = (ivals[2] & 0x7f) | 0x80;
			encvals[3] = (ivals[3] & 0x7f) | ((ivals[0] & 0x40) << 1);

			for (int i = 0; i < 4; i++)
			{
				int dummy = 0;
				quantize_and_unquantize_retain_top_four_bits(quant_level, encvals[i], ref (output[i]), ref dummy);
			}

			return;
		}

		static void quantize_hdr_rgb3(vfloat4 color0, vfloat4 color1, ref int[] output, int quant_level) 
		{
			// Note: color*.lane<3> is not used so we can ignore it
			color0 = ASTCMath.clamp(0.0f, 65535.0f, color0);
			color1 = ASTCMath.clamp(0.0f, 65535.0f, color1);

			vfloat4 color0_bak = color0;
			vfloat4 color1_bak = color1;

			int majcomp;
			if (color1.lane<0>() > color1.lane(1) && color1.lane<0>() > color1.lane(2))
			{
				majcomp = 0;			// red is largest
			}
			else if (color1.lane(1) > color1.lane(2))
			{
				majcomp = 1;			// green is largest
			}
			else
			{
				majcomp = 2;			// blue is largest
			}

			// swizzle the components
			switch (majcomp)
			{
			case 1:					// red-green swap
				color0 = new vfloat4(color0.lane(1), color0.lane<0>(), color0.lane(2), color0.lane(3));
				color1 = new vfloat4(color1.lane(1), color1.lane<0>(), color1.lane(2), color1.lane(3));
				break;
			case 2:					// red-blue swap
				color0 = new vfloat4(color0.lane(2), color0.lane(1), color0.lane<0>(), color0.lane(3));
				color1 = new vfloat4(color1.lane(2), color1.lane(1), color1.lane<0>(), color1.lane(3));
				break;
			default:
				break;
			}

			float a_base = color1.lane<0>();
			a_base = ASTCMath.clamp(a_base, 0.0f, 65535.0f);

			float b0_base = a_base - color1.lane(1);
			float b1_base = a_base - color1.lane(2);
			float c_base = a_base - color0.lane<0>();
			float d0_base = a_base - b0_base - c_base - color0.lane(1);
			float d1_base = a_base - b1_base - c_base - color0.lane(2);

			// number of bits in the various fields in the various modes
			int[,] mode_bits = new int[8, 4] {
				{9, 7, 6, 7},
				{9, 8, 6, 6},
				{10, 6, 7, 7},
				{10, 7, 7, 6},
				{11, 8, 6, 5},
				{11, 6, 8, 6},
				{12, 7, 7, 5},
				{12, 6, 7, 6}
			};

			// cutoffs to use for the computed values of a,b,c,d, assuming the
			// range 0..65535 are LNS values corresponding to fp16.
			float[,] mode_cutoffs = new float[8, 4] {
				{16384, 8192, 8192, 8},	// mode 0: 9,7,6,7
				{32768, 8192, 4096, 8},	// mode 1: 9,8,6,6
				{4096, 8192, 4096, 4},	// mode 2: 10,6,7,7
				{8192, 8192, 2048, 4},	// mode 3: 10,7,7,6
				{8192, 2048, 512, 2},	// mode 4: 11,8,6,5
				{2048, 8192, 1024, 2},	// mode 5: 11,6,8,6
				{2048, 2048, 256, 1},	// mode 6: 12,7,7,5
				{1024, 2048, 512, 1},	// mode 7: 12,6,7,6
			};

			float[] mode_scales = new float[8] {
				1.0f / 128.0f,
				1.0f / 128.0f,
				1.0f / 64.0f,
				1.0f / 64.0f,
				1.0f / 32.0f,
				1.0f / 32.0f,
				1.0f / 16.0f,
				1.0f / 16.0f,
			};

			// scaling factors when going from what was encoded in the mode to 16 bits.
			float[] mode_rscales = new float[8] {
				128.0f,
				128.0f,
				64.0f,
				64.0f,
				32.0f,
				32.0f,
				16.0f,
				16.0f
			};

			// try modes one by one, with the highest-precision mode first.
			for (int mode = 7; mode >= 0; mode--)
			{
				// for each mode, test if we can in fact accommodate
				// the computed b,c,d values. If we clearly can't, then we skip to the next mode.

				float b_cutoff = mode_cutoffs[mode, 0];
				float c_cutoff = mode_cutoffs[mode, 1];
				float d_cutoff = mode_cutoffs[mode, 2];

				if (b0_base > b_cutoff || b1_base > b_cutoff || c_base > c_cutoff || Math.Abs(d0_base) > d_cutoff || Math.Abs(d1_base) > d_cutoff)
				{
					continue;
				}

				float mode_scale = mode_scales[mode];
				float mode_rscale = mode_rscales[mode];

				int b_intcutoff = 1 << mode_bits[mode, 1];
				int c_intcutoff = 1 << mode_bits[mode, 2];
				int d_intcutoff = 1 << (mode_bits[mode, 3] - 1);

				// first, quantize and unquantize A, with the assumption that its high bits can be handled safely.
				int a_intval = ASTCMath.flt2int_rtn(a_base * mode_scale);
				int a_lowbits = a_intval & 0xFF;

				int a_quantval = color_quant_tables[quant_level, a_lowbits];
				int a_uquantval = ColorUnquantize.color_unquant_tables[quant_level][a_quantval];
				a_intval = (a_intval & ~0xFF) | a_uquantval;
				float a_fval = static_cast<float>(a_intval) * mode_rscale;

				// next, recompute C, then quantize and unquantize it
				float c_fval = a_fval - color0.lane<0>();
				c_fval = ASTCMath.clamp(c_fval, 0.0f, 65535.0f);

				int c_intval = ASTCMath.flt2int_rtn(c_fval * mode_scale);

				if (c_intval >= c_intcutoff)
				{
					continue;
				}

				int c_lowbits = c_intval & 0x3f;

				c_lowbits |= (mode & 1) << 7;
				c_lowbits |= (a_intval & 0x100) >> 2;

				int c_quantval = 0;
				int c_uquantval = 0;
				quantize_and_unquantize_retain_top_two_bits(quant_level, c_lowbits, ref c_quantval, ref c_uquantval);
				c_intval = (c_intval & ~0x3F) | (c_uquantval & 0x3F);
				c_fval = static_cast<float>(c_intval) * mode_rscale;

				// next, recompute B0 and B1, then quantize and unquantize them
				float b0_fval = a_fval - color1.lane(1);
				float b1_fval = a_fval - color1.lane(2);

				b0_fval = ASTCMath.clamp(b0_fval, 0.0f, 65535.0f);
				b1_fval = ASTCMath.clamp(b1_fval, 0.0f, 65535.0f);
				int b0_intval = ASTCMath.flt2int_rtn(b0_fval * mode_scale);
				int b1_intval = ASTCMath.flt2int_rtn(b1_fval * mode_scale);

				if (b0_intval >= b_intcutoff || b1_intval >= b_intcutoff)
				{
					continue;
				}

				int b0_lowbits = b0_intval & 0x3f;
				int b1_lowbits = b1_intval & 0x3f;

				int bit0 = 0;
				int bit1 = 0;
				switch (mode)
				{
				case 0:
				case 1:
				case 3:
				case 4:
				case 6:
					bit0 = (b0_intval >> 6) & 1;
					break;
				case 2:
				case 5:
				case 7:
					bit0 = (a_intval >> 9) & 1;
					break;
				}

				switch (mode)
				{
				case 0:
				case 1:
				case 3:
				case 4:
				case 6:
					bit1 = (b1_intval >> 6) & 1;
					break;
				case 2:
					bit1 = (c_intval >> 6) & 1;
					break;
				case 5:
				case 7:
					bit1 = (a_intval >> 10) & 1;
					break;
				}

				b0_lowbits |= bit0 << 6;
				b1_lowbits |= bit1 << 6;

				b0_lowbits |= ((mode >> 1) & 1) << 7;
				b1_lowbits |= ((mode >> 2) & 1) << 7;

				int b0_quantval = 0;
				int b1_quantval = 0;
				int b0_uquantval = 0;
				int b1_uquantval = 0;

				quantize_and_unquantize_retain_top_two_bits(quant_level, b0_lowbits, ref b0_quantval, ref b0_uquantval);

				quantize_and_unquantize_retain_top_two_bits(quant_level, b1_lowbits, ref b1_quantval, ref b1_uquantval);

				b0_intval = (b0_intval & ~0x3f) | (b0_uquantval & 0x3f);
				b1_intval = (b1_intval & ~0x3f) | (b1_uquantval & 0x3f);
				b0_fval = static_cast<float>(b0_intval) * mode_rscale;
				b1_fval = static_cast<float>(b1_intval) * mode_rscale;

				// finally, recompute D0 and D1, then quantize and unquantize them
				float d0_fval = a_fval - b0_fval - c_fval - color0.lane(1);
				float d1_fval = a_fval - b1_fval - c_fval - color0.lane(2);

				d0_fval = ASTCMath.clamp(d0_fval, -65535.0f, 65535.0f);
				d1_fval = ASTCMath.clamp(d1_fval, -65535.0f, 65535.0f);

				int d0_intval = ASTCMath.flt2int_rtn(d0_fval * mode_scale);
				int d1_intval = ASTCMath.flt2int_rtn(d1_fval * mode_scale);

				if (Math.Abs(d0_intval) >= d_intcutoff || Math.Abs(d1_intval) >= d_intcutoff)
				{
					continue;
				}

				int d0_lowbits = d0_intval & 0x1f;
				int d1_lowbits = d1_intval & 0x1f;

				int bit2 = 0;
				int bit3 = 0;
				int bit4;
				int bit5;
				switch (mode)
				{
				case 0:
				case 2:
					bit2 = (d0_intval >> 6) & 1;
					break;
				case 1:
				case 4:
					bit2 = (b0_intval >> 7) & 1;
					break;
				case 3:
					bit2 = (a_intval >> 9) & 1;
					break;
				case 5:
					bit2 = (c_intval >> 7) & 1;
					break;
				case 6:
				case 7:
					bit2 = (a_intval >> 11) & 1;
					break;
				}
				switch (mode)
				{
				case 0:
				case 2:
					bit3 = (d1_intval >> 6) & 1;
					break;
				case 1:
				case 4:
					bit3 = (b1_intval >> 7) & 1;
					break;
				case 3:
				case 5:
				case 6:
				case 7:
					bit3 = (c_intval >> 6) & 1;
					break;
				}

				switch (mode)
				{
				case 4:
				case 6:
					bit4 = (a_intval >> 9) & 1;
					bit5 = (a_intval >> 10) & 1;
					break;
				default:
					bit4 = (d0_intval >> 5) & 1;
					bit5 = (d1_intval >> 5) & 1;
					break;
				}

				d0_lowbits |= bit2 << 6;
				d1_lowbits |= bit3 << 6;
				d0_lowbits |= bit4 << 5;
				d1_lowbits |= bit5 << 5;

				d0_lowbits |= (majcomp & 1) << 7;
				d1_lowbits |= ((majcomp >> 1) & 1) << 7;

				int d0_quantval = 0;
				int d1_quantval = 0;
				int d0_uquantval = 0;
				int d1_uquantval = 0;

				quantize_and_unquantize_retain_top_four_bits(quant_level, d0_lowbits, ref d0_quantval, ref d0_uquantval);

				quantize_and_unquantize_retain_top_four_bits(quant_level, d1_lowbits, ref d1_quantval, ref d1_uquantval);

				output[0] = a_quantval;
				output[1] = c_quantval;
				output[2] = b0_quantval;
				output[3] = b1_quantval;
				output[4] = d0_quantval;
				output[5] = d1_quantval;
				return;
			}

			// neither of the modes fit? In this case, we will use a flat representation
			// for storing data, using 8 bits for red and green, and 7 bits for blue.
			// This gives color accuracy roughly similar to LDR 4:4:3 which is not at all great
			// but usable. This representation is used if the light color is more than 4x the
			// color value of the dark color.
			float[] vals = new float[6];
			vals[0] = color0_bak.lane<0>();
			vals[1] = color1_bak.lane<0>();
			vals[2] = color0_bak.lane(1);
			vals[3] = color1_bak.lane(1);
			vals[4] = color0_bak.lane(2);
			vals[5] = color1_bak.lane(2);

			for (int i = 0; i < 6; i++)
			{
				vals[i] = ASTCMath.clamp(vals[i], 0.0f, 65020.0f);
			}

			for (int i = 0; i < 4; i++)
			{
				int idx = ASTCMath.flt2int_rtn(vals[i] * 1.0f / 256.0f);
				output[i] = color_quant_tables[quant_level, idx];
			}

			for (int i = 4; i < 6; i++)
			{
				int dummy = 0;
				int idx = ASTCMath.flt2int_rtn(vals[i] * 1.0f / 512.0f) + 128;
				quantize_and_unquantize_retain_top_two_bits(quant_level, idx, ref (output[i]), ref dummy);
			}

			return;
		}
		
		static void quantize_hdr_rgb_ldr_alpha3(vfloat4 color0, vfloat4 color1, ref int[] output, int quant_level) 
		{
			float scale = 1.0f / 257.0f;

			float a0 = ASTCMath.clamp255f(color0.lane(3) * scale);
			float a1 = ASTCMath.clamp255f(color1.lane(3) * scale);

			int ai0 = color_quant_tables[quant_level, ASTCMath.flt2int_rtn(a0)];
			int ai1 = color_quant_tables[quant_level, ASTCMath.flt2int_rtn(a1)];

			output[6] = ai0;
			output[7] = ai1;

			quantize_hdr_rgb3(color0, color1, ref output, quant_level);
		}

		static void quantize_hdr_luminance_large_range3(vfloat4 color0, vfloat4 color1, ref int[] output, int quant_level) 
		{
			float lum0 = hadd_rgb_s(color0) * (1.0f / 3.0f);
			float lum1 = hadd_rgb_s(color1) * (1.0f / 3.0f);

			if (lum1 < lum0)
			{
				float avg = (lum0 + lum1) * 0.5f;
				lum0 = avg;
				lum1 = avg;
			}

			int ilum1 = ASTCMath.flt2int_rtn(lum1);
			int ilum0 = ASTCMath.flt2int_rtn(lum0);

			// find the closest encodable point in the upper half of the code-point space
			int upper_v0 = (ilum0 + 128) >> 8;
			int upper_v1 = (ilum1 + 128) >> 8;

			upper_v0 = ASTCMath.clamp(upper_v0, 0, 255);
			upper_v1 = ASTCMath.clamp(upper_v1, 0, 255);

			// find the closest encodable point in the lower half of the code-point space
			int lower_v0 = (ilum1 + 256) >> 8;
			int lower_v1 = ilum0 >> 8;

			lower_v0 = ASTCMath.clamp(lower_v0, 0, 255);
			lower_v1 = ASTCMath.clamp(lower_v1, 0, 255);

			// determine the distance between the point in code-point space and the input value
			int upper0_dec = upper_v0 << 8;
			int upper1_dec = upper_v1 << 8;
			int lower0_dec = (lower_v1 << 8) + 128;
			int lower1_dec = (lower_v0 << 8) - 128;

			int upper0_diff = upper0_dec - ilum0;
			int upper1_diff = upper1_dec - ilum1;
			int lower0_diff = lower0_dec - ilum0;
			int lower1_diff = lower1_dec - ilum1;

			int upper_error = (upper0_diff * upper0_diff) + (upper1_diff * upper1_diff);
			int lower_error = (lower0_diff * lower0_diff) + (lower1_diff * lower1_diff);

			int v0, v1;
			if (upper_error < lower_error)
			{
				v0 = upper_v0;
				v1 = upper_v1;
			}
			else
			{
				v0 = lower_v0;
				v1 = lower_v1;
			}

			// OK; encode.
			output[0] = color_quant_tables[quant_level, v0];
			output[1] = color_quant_tables[quant_level, v1];
		}

		static bool try_quantize_hdr_luminance_small_range3(vfloat4 color0, vfloat4 color1, ref int[] output, int quant_level)
		{
			float lum0 = hadd_rgb_s(color0) * (1.0f / 3.0f);
			float lum1 = hadd_rgb_s(color1) * (1.0f / 3.0f);

			if (lum1 < lum0)
			{
				float avg = (lum0 + lum1) * 0.5f;
				lum0 = avg;
				lum1 = avg;
			}

			int ilum1 = ASTCMath.flt2int_rtn(lum1);
			int ilum0 = ASTCMath.flt2int_rtn(lum0);

			// difference of more than a factor-of-2 results in immediate failure.
			if (ilum1 - ilum0 > 2048)
			{
				return false;
			}

			int lowval, highval, diffval;
			int v0, v1;
			int v0e, v1e;
			int v0d, v1d;

			// first, try to encode the high-precision submode
			lowval = (ilum0 + 16) >> 5;
			highval = (ilum1 + 16) >> 5;

			lowval = ASTCMath.clamp(lowval, 0, 2047);
			highval = ASTCMath.clamp(highval, 0, 2047);

			v0 = lowval & 0x7F;
			v0e = color_quant_tables[quant_level, v0];
			v0d = ColorUnquantize.color_unquant_tables[quant_level][v0e];

			if (v0d < 0x80)
			{
				lowval = (lowval & ~0x7F) | v0d;
				diffval = highval - lowval;
				if (diffval >= 0 && diffval <= 15)
				{
					v1 = ((lowval >> 3) & 0xF0) | diffval;
					v1e = color_quant_tables[quant_level, v1];
					v1d = ColorUnquantize.color_unquant_tables[quant_level][v1e];
					if ((v1d & 0xF0) == (v1 & 0xF0))
					{
						output[0] = v0e;
						output[1] = v1e;
						return true;
					}
				}
			}

			// failed to encode the high-precision submode; well, then try to encode the
			// low-precision submode.

			lowval = (ilum0 + 32) >> 6;
			highval = (ilum1 + 32) >> 6;

			lowval = ASTCMath.clamp(lowval, 0, 1023);
			highval = ASTCMath.clamp(highval, 0, 1023);

			v0 = (lowval & 0x7F) | 0x80;
			v0e = color_quant_tables[quant_level, v0];
			v0d = ColorUnquantize.color_unquant_tables[quant_level][v0e];
			if ((v0d & 0x80) == 0)
			{
				return false;
			}

			lowval = (lowval & ~0x7F) | (v0d & 0x7F);
			diffval = highval - lowval;
			if (diffval < 0 || diffval > 31)
			{
				return false;
			}

			v1 = ((lowval >> 2) & 0xE0) | diffval;
			v1e = color_quant_tables[quant_level, v1];
			v1d = ColorUnquantize.color_unquant_tables[quant_level][v1e];
			if ((v1d & 0xE0) != (v1 & 0xE0))
			{
				return false;
			}

			output[0] = v0e;
			output[1] = v1e;
			return true;
		}

		static void quantize_hdr_alpha3(float alpha0, float alpha1, ref int[] output, int quant_level) 
		{
			alpha0 = ASTCMath.clamp(alpha0, 0.0f, 65280.0f);
			alpha1 = ASTCMath.clamp(alpha1, 0.0f, 65280.0f);

			int ialpha0 = ASTCMath.flt2int_rtn(alpha0);
			int ialpha1 = ASTCMath.flt2int_rtn(alpha1);

			int val0, val1, diffval;
			int v6, v7;
			int v6e, v7e;
			int v6d, v7d;

			// try to encode one of the delta submodes, in decreasing-precision order.
			for (int i = 2; i >= 0; i--)
			{
				val0 = (ialpha0 + (128 >> i)) >> (8 - i);
				val1 = (ialpha1 + (128 >> i)) >> (8 - i);

				v6 = (val0 & 0x7F) | ((i & 1) << 7);
				v6e = color_quant_tables[quant_level, v6];
				v6d = ColorUnquantize.color_unquant_tables[quant_level][v6e];

				if (((v6 ^ v6d) & 0x80) == 1)
				{
					continue;
				}

				val0 = (val0 & ~0x7f) | (v6d & 0x7f);
				diffval = val1 - val0;
				int cutoff = 32 >> i;
				int mask = 2 * cutoff - 1;

				if (diffval < -cutoff || diffval >= cutoff)
				{
					continue;
				}

				v7 = ((i & 2) << 6) | ((val0 >> 7) << (6 - i)) | (diffval & mask);
				v7e = color_quant_tables[quant_level, v7];
				v7d = ColorUnquantize.color_unquant_tables[quant_level][v7e];

				int[] testbits = new int[3] { 0xE0, 0xF0, 0xF8 };

				if (((v7 ^ v7d) & testbits[i]) == 1)
				{
					continue;
				}

				output[0] = v6e;
				output[1] = v7e;
				return;
			}

			// could not encode any of the delta modes; instead encode a flat value
			val0 = (ialpha0 + 256) >> 9;
			val1 = (ialpha1 + 256) >> 9;
			v6 = val0 | 0x80;
			v7 = val1 | 0x80;

			v6e = color_quant_tables[quant_level, v6];
			v7e = color_quant_tables[quant_level, v7];
			output[0] = v6e;
			output[1] = v7e;

			return;
		}

		static void quantize_hdr_rgb_alpha3(vfloat4 color0, vfloat4 color1, ref int[] output, int quant_level) 
		{
			quantize_hdr_rgb3(color0, color1, ref output, quant_level);
			quantize_hdr_alpha3(color0.lane(3), color1.lane(3), output + 6, quant_level);
		}

		/*
			Quantize a color. When quantizing an RGB or RGBA color, the quantizer may choose a
			delta-based representation; as such, it will report back the format it actually used.
		*/
		EndpointFormats pack_color_endpoints(vfloat4 color0, vfloat4 color1, vfloat4 rgbs_color, vfloat4 rgbo_color, EndpointFormats format, ref int[] output, int quant_level) 
		{
			Debug.Assert(quant_level >= 0 && quant_level < 21);

			// we do not support negative colors.
			color0 = max(color0, 0.0f);
			color1 = max(color1, 0.0f);

			EndpointFormats retval = 0;

			switch (format)
			{
			case EndpointFormats.FMT_RGB:
				if (quant_level <= 18)
				{
					if (try_quantize_rgb_delta_blue_contract(color0, color1, ref output, quant_level))
					{
						retval = EndpointFormats.FMT_RGB_DELTA;
						break;
					}
					if (try_quantize_rgb_delta(color0, color1, ref output, quant_level))
					{
						retval = EndpointFormats.FMT_RGB_DELTA;
						break;
					}
				}
				if (try_quantize_rgb_blue_contract(color0, color1, ref output, quant_level))
				{
					retval = EndpointFormats.FMT_RGB;
					break;
				}
				quantize_rgb(color0, color1, ref output, quant_level);
				retval = EndpointFormats.FMT_RGB;
				break;

			case EndpointFormats.FMT_RGBA:
				if (quant_level <= 18)
				{
					if (try_quantize_rgba_delta_blue_contract(color0, color1, ref output, quant_level))
					{
						retval = EndpointFormats.FMT_RGBA_DELTA;
						break;
					}
					if (try_quantize_rgba_delta(color0, color1, ref output, quant_level))
					{
						retval = EndpointFormats.FMT_RGBA_DELTA;
						break;
					}
				}
				if (try_quantize_rgba_blue_contract(color0, color1, ref output, quant_level))
				{
					retval = EndpointFormats.FMT_RGBA;
					break;
				}
				quantize_rgba(color0, color1, ref output, quant_level);
				retval = EndpointFormats.FMT_RGBA;
				break;

			case EndpointFormats.FMT_RGB_SCALE:
				quantize_rgbs_new(rgbs_color, ref output, quant_level);
				retval = EndpointFormats.FMT_RGB_SCALE;
				break;

			case EndpointFormats.FMT_HDR_RGB_SCALE:
				quantize_hdr_rgbo3(rgbo_color, ref output, quant_level);
				retval = EndpointFormats.FMT_HDR_RGB_SCALE;
				break;

			case EndpointFormats.FMT_HDR_RGB:
				quantize_hdr_rgb3(color0, color1, ref output, quant_level);
				retval = EndpointFormats.FMT_HDR_RGB;
				break;

			case EndpointFormats.FMT_RGB_SCALE_ALPHA:
				quantize_rgbs_alpha_new(color0, color1, rgbs_color, ref output, quant_level);
				retval = EndpointFormats.FMT_RGB_SCALE_ALPHA;
				break;

			case EndpointFormats.FMT_HDR_LUMINANCE_SMALL_RANGE:
			case EndpointFormats.FMT_HDR_LUMINANCE_LARGE_RANGE:
				if (try_quantize_hdr_luminance_small_range3(color0, color1, ref output, quant_level))
				{
					retval = EndpointFormats.FMT_HDR_LUMINANCE_SMALL_RANGE;
					break;
				}
				quantize_hdr_luminance_large_range3(color0, color1, ref output, quant_level);
				retval = EndpointFormats.FMT_HDR_LUMINANCE_LARGE_RANGE;
				break;

			case EndpointFormats.FMT_LUMINANCE:
				quantize_luminance(color0, color1, ref output, quant_level);
				retval = EndpointFormats.FMT_LUMINANCE;
				break;

			case EndpointFormats.FMT_LUMINANCE_ALPHA:
				if (quant_level <= 18)
				{
					if (try_quantize_luminance_alpha_delta(color0, color1, ref output, quant_level))
					{
						retval = EndpointFormats.FMT_LUMINANCE_ALPHA_DELTA;
						break;
					}
				}
				quantize_luminance_alpha(color0, color1, output, quant_level);
				retval = EndpointFormats.FMT_LUMINANCE_ALPHA;
				break;

			case EndpointFormats.FMT_HDR_RGB_LDR_ALPHA:
				quantize_hdr_rgb_ldr_alpha3(color0, color1, ref output, quant_level);
				retval = EndpointFormats.FMT_HDR_RGB_LDR_ALPHA;
				break;

			case EndpointFormats.FMT_HDR_RGBA:
				quantize_hdr_rgb_alpha3(color0, color1, ref output, quant_level);
				retval = EndpointFormats.FMT_HDR_RGBA;
				break;
			}

			return retval;
		}
	}
}