using System.Collections.Generic;

namespace ASTCEnc
{
	public struct btq_count 
	{
		/**< The quantization level. */
		public byte quant;

		/**< The number of bits. */
		public byte bits;

		/**< The number of trits. */
		public byte trits;

		/**< The number of quints. */
		public byte quints;

		public btq_count(QuantMethod quantMethod, byte bits, byte trits, byte quints)
		{
			this.quant = (byte)quantMethod;
			this.bits = bits;
			this.trits = trits;
			this.quints = quints;
		}
	}

	public static class IntegerSequence
	{
		// unpacked quint triplets <low,middle,high> for each packed-quint value
		public static readonly byte[][] quints_of_integer = new byte[128][] {
			new byte[3] {0, 0, 0}, new byte[3] {1, 0, 0}, new byte[3] {2, 0, 0}, new byte[3] {3, 0, 0},
			new byte[3] {4, 0, 0}, new byte[3] {0, 4, 0}, new byte[3] {4, 4, 0}, new byte[3] {4, 4, 4},
			new byte[3] {0, 1, 0}, new byte[3] {1, 1, 0}, new byte[3] {2, 1, 0}, new byte[3] {3, 1, 0},
			new byte[3] {4, 1, 0}, new byte[3] {1, 4, 0}, new byte[3] {4, 4, 1}, new byte[3] {4, 4, 4},
			new byte[3] {0, 2, 0}, new byte[3] {1, 2, 0}, new byte[3] {2, 2, 0}, new byte[3] {3, 2, 0},
			new byte[3] {4, 2, 0}, new byte[3] {2, 4, 0}, new byte[3] {4, 4, 2}, new byte[3] {4, 4, 4},
			new byte[3] {0, 3, 0}, new byte[3] {1, 3, 0}, new byte[3] {2, 3, 0}, new byte[3] {3, 3, 0},
			new byte[3] {4, 3, 0}, new byte[3] {3, 4, 0}, new byte[3] {4, 4, 3}, new byte[3] {4, 4, 4},
			new byte[3] {0, 0, 1}, new byte[3] {1, 0, 1}, new byte[3] {2, 0, 1}, new byte[3] {3, 0, 1},
			new byte[3] {4, 0, 1}, new byte[3] {0, 4, 1}, new byte[3] {4, 0, 4}, new byte[3] {0, 4, 4},
			new byte[3] {0, 1, 1}, new byte[3] {1, 1, 1}, new byte[3] {2, 1, 1}, new byte[3] {3, 1, 1},
			new byte[3] {4, 1, 1}, new byte[3] {1, 4, 1}, new byte[3] {4, 1, 4}, new byte[3] {1, 4, 4},
			new byte[3] {0, 2, 1}, new byte[3] {1, 2, 1}, new byte[3] {2, 2, 1}, new byte[3] {3, 2, 1},
			new byte[3] {4, 2, 1}, new byte[3] {2, 4, 1}, new byte[3] {4, 2, 4}, new byte[3] {2, 4, 4},
			new byte[3] {0, 3, 1}, new byte[3] {1, 3, 1}, new byte[3] {2, 3, 1}, new byte[3] {3, 3, 1},
			new byte[3] {4, 3, 1}, new byte[3] {3, 4, 1}, new byte[3] {4, 3, 4}, new byte[3] {3, 4, 4},
			new byte[3] {0, 0, 2}, new byte[3] {1, 0, 2}, new byte[3] {2, 0, 2}, new byte[3] {3, 0, 2},
			new byte[3] {4, 0, 2}, new byte[3] {0, 4, 2}, new byte[3] {2, 0, 4}, new byte[3] {3, 0, 4},
			new byte[3] {0, 1, 2}, new byte[3] {1, 1, 2}, new byte[3] {2, 1, 2}, new byte[3] {3, 1, 2},
			new byte[3] {4, 1, 2}, new byte[3] {1, 4, 2}, new byte[3] {2, 1, 4}, new byte[3] {3, 1, 4},
			new byte[3] {0, 2, 2}, new byte[3] {1, 2, 2}, new byte[3] {2, 2, 2}, new byte[3] {3, 2, 2},
			new byte[3] {4, 2, 2}, new byte[3] {2, 4, 2}, new byte[3] {2, 2, 4}, new byte[3] {3, 2, 4},
			new byte[3] {0, 3, 2}, new byte[3] {1, 3, 2}, new byte[3] {2, 3, 2}, new byte[3] {3, 3, 2},
			new byte[3] {4, 3, 2}, new byte[3] {3, 4, 2}, new byte[3] {2, 3, 4}, new byte[3] {3, 3, 4},
			new byte[3] {0, 0, 3}, new byte[3] {1, 0, 3}, new byte[3] {2, 0, 3}, new byte[3] {3, 0, 3},
			new byte[3] {4, 0, 3}, new byte[3] {0, 4, 3}, new byte[3] {0, 0, 4}, new byte[3] {1, 0, 4},
			new byte[3] {0, 1, 3}, new byte[3] {1, 1, 3}, new byte[3] {2, 1, 3}, new byte[3] {3, 1, 3},
			new byte[3] {4, 1, 3}, new byte[3] {1, 4, 3}, new byte[3] {0, 1, 4}, new byte[3] {1, 1, 4},
			new byte[3] {0, 2, 3}, new byte[3] {1, 2, 3}, new byte[3] {2, 2, 3}, new byte[3] {3, 2, 3},
			new byte[3] {4, 2, 3}, new byte[3] {2, 4, 3}, new byte[3] {0, 2, 4}, new byte[3] {1, 2, 4},
			new byte[3] {0, 3, 3}, new byte[3] {1, 3, 3}, new byte[3] {2, 3, 3}, new byte[3] {3, 3, 3},
			new byte[3] {4, 3, 3}, new byte[3] {3, 4, 3}, new byte[3] {0, 3, 4}, new byte[3] {1, 3, 4}
		};

		// packed quint-value for every unpacked quint-triplet
		// indexed by [high][middle][low]
		public static readonly byte[][][] integer_of_quints = new byte[5][][] {
			new byte[5][]{
				new byte[5]{0, 1, 2, 3, 4},
				new byte[5]{8, 9, 10, 11, 12},
				new byte[5]{16, 17, 18, 19, 20},
				new byte[5]{24, 25, 26, 27, 28},
				new byte[5]{5, 13, 21, 29, 6}
			},
			new byte[5][]{
				new byte[5]{32, 33, 34, 35, 36},
				new byte[5]{40, 41, 42, 43, 44},
				new byte[5]{48, 49, 50, 51, 52},
				new byte[5]{56, 57, 58, 59, 60},
				new byte[5]{37, 45, 53, 61, 14}
			},
			new byte[5][]{
				new byte[5]{64, 65, 66, 67, 68},
				new byte[5]{72, 73, 74, 75, 76},
				new byte[5]{80, 81, 82, 83, 84},
				new byte[5]{88, 89, 90, 91, 92},
				new byte[5]{69, 77, 85, 93, 22}
			},
			new byte[5][]{
				new byte[5]{96, 97, 98, 99, 100},
				new byte[5]{104, 105, 106, 107, 108},
				new byte[5]{112, 113, 114, 115, 116},
				new byte[5]{120, 121, 122, 123, 124},
				new byte[5]{101, 109, 117, 125, 30}
			},
			new byte[5][]{
				new byte[5]{102, 103, 70, 71, 38},
				new byte[5]{110, 111, 78, 79, 46},
				new byte[5]{118, 119, 86, 87, 54},
				new byte[5]{126, 127, 94, 95, 62},
				new byte[5]{39, 47, 55, 63, 31}
			}
		};

		// unpacked trit quintuplets <low,_,_,_,high> for each packed-quint value
		public static readonly byte[][] trits_of_integer = new byte[256][] {
			new byte[5]{0, 0, 0, 0, 0}, new byte[5]{1, 0, 0, 0, 0}, new byte[5]{2, 0, 0, 0, 0}, new byte[5]{0, 0, 2, 0, 0},
			new byte[5]{0, 1, 0, 0, 0}, new byte[5]{1, 1, 0, 0, 0}, new byte[5]{2, 1, 0, 0, 0}, new byte[5]{1, 0, 2, 0, 0},
			new byte[5]{0, 2, 0, 0, 0}, new byte[5]{1, 2, 0, 0, 0}, new byte[5]{2, 2, 0, 0, 0}, new byte[5]{2, 0, 2, 0, 0},
			new byte[5]{0, 2, 2, 0, 0}, new byte[5]{1, 2, 2, 0, 0}, new byte[5]{2, 2, 2, 0, 0}, new byte[5]{2, 0, 2, 0, 0},
			new byte[5]{0, 0, 1, 0, 0}, new byte[5]{1, 0, 1, 0, 0}, new byte[5]{2, 0, 1, 0, 0}, new byte[5]{0, 1, 2, 0, 0},
			new byte[5]{0, 1, 1, 0, 0}, new byte[5]{1, 1, 1, 0, 0}, new byte[5]{2, 1, 1, 0, 0}, new byte[5]{1, 1, 2, 0, 0},
			new byte[5]{0, 2, 1, 0, 0}, new byte[5]{1, 2, 1, 0, 0}, new byte[5]{2, 2, 1, 0, 0}, new byte[5]{2, 1, 2, 0, 0},
			new byte[5]{0, 0, 0, 2, 2}, new byte[5]{1, 0, 0, 2, 2}, new byte[5]{2, 0, 0, 2, 2}, new byte[5]{0, 0, 2, 2, 2},
			new byte[5]{0, 0, 0, 1, 0}, new byte[5]{1, 0, 0, 1, 0}, new byte[5]{2, 0, 0, 1, 0}, new byte[5]{0, 0, 2, 1, 0},
			new byte[5]{0, 1, 0, 1, 0}, new byte[5]{1, 1, 0, 1, 0}, new byte[5]{2, 1, 0, 1, 0}, new byte[5]{1, 0, 2, 1, 0},
			new byte[5]{0, 2, 0, 1, 0}, new byte[5]{1, 2, 0, 1, 0}, new byte[5]{2, 2, 0, 1, 0}, new byte[5]{2, 0, 2, 1, 0},
			new byte[5]{0, 2, 2, 1, 0}, new byte[5]{1, 2, 2, 1, 0}, new byte[5]{2, 2, 2, 1, 0}, new byte[5]{2, 0, 2, 1, 0},
			new byte[5]{0, 0, 1, 1, 0}, new byte[5]{1, 0, 1, 1, 0}, new byte[5]{2, 0, 1, 1, 0}, new byte[5]{0, 1, 2, 1, 0},
			new byte[5]{0, 1, 1, 1, 0}, new byte[5]{1, 1, 1, 1, 0}, new byte[5]{2, 1, 1, 1, 0}, new byte[5]{1, 1, 2, 1, 0},
			new byte[5]{0, 2, 1, 1, 0}, new byte[5]{1, 2, 1, 1, 0}, new byte[5]{2, 2, 1, 1, 0}, new byte[5]{2, 1, 2, 1, 0},
			new byte[5]{0, 1, 0, 2, 2}, new byte[5]{1, 1, 0, 2, 2}, new byte[5]{2, 1, 0, 2, 2}, new byte[5]{1, 0, 2, 2, 2},
			new byte[5]{0, 0, 0, 2, 0}, new byte[5]{1, 0, 0, 2, 0}, new byte[5]{2, 0, 0, 2, 0}, new byte[5]{0, 0, 2, 2, 0},
			new byte[5]{0, 1, 0, 2, 0}, new byte[5]{1, 1, 0, 2, 0}, new byte[5]{2, 1, 0, 2, 0}, new byte[5]{1, 0, 2, 2, 0},
			new byte[5]{0, 2, 0, 2, 0}, new byte[5]{1, 2, 0, 2, 0}, new byte[5]{2, 2, 0, 2, 0}, new byte[5]{2, 0, 2, 2, 0},
			new byte[5]{0, 2, 2, 2, 0}, new byte[5]{1, 2, 2, 2, 0}, new byte[5]{2, 2, 2, 2, 0}, new byte[5]{2, 0, 2, 2, 0},
			new byte[5]{0, 0, 1, 2, 0}, new byte[5]{1, 0, 1, 2, 0}, new byte[5]{2, 0, 1, 2, 0}, new byte[5]{0, 1, 2, 2, 0},
			new byte[5]{0, 1, 1, 2, 0}, new byte[5]{1, 1, 1, 2, 0}, new byte[5]{2, 1, 1, 2, 0}, new byte[5]{1, 1, 2, 2, 0},
			new byte[5]{0, 2, 1, 2, 0}, new byte[5]{1, 2, 1, 2, 0}, new byte[5]{2, 2, 1, 2, 0}, new byte[5]{2, 1, 2, 2, 0},
			new byte[5]{0, 2, 0, 2, 2}, new byte[5]{1, 2, 0, 2, 2}, new byte[5]{2, 2, 0, 2, 2}, new byte[5]{2, 0, 2, 2, 2},
			new byte[5]{0, 0, 0, 0, 2}, new byte[5]{1, 0, 0, 0, 2}, new byte[5]{2, 0, 0, 0, 2}, new byte[5]{0, 0, 2, 0, 2},
			new byte[5]{0, 1, 0, 0, 2}, new byte[5]{1, 1, 0, 0, 2}, new byte[5]{2, 1, 0, 0, 2}, new byte[5]{1, 0, 2, 0, 2},
			new byte[5]{0, 2, 0, 0, 2}, new byte[5]{1, 2, 0, 0, 2}, new byte[5]{2, 2, 0, 0, 2}, new byte[5]{2, 0, 2, 0, 2},
			new byte[5]{0, 2, 2, 0, 2}, new byte[5]{1, 2, 2, 0, 2}, new byte[5]{2, 2, 2, 0, 2}, new byte[5]{2, 0, 2, 0, 2},
			new byte[5]{0, 0, 1, 0, 2}, new byte[5]{1, 0, 1, 0, 2}, new byte[5]{2, 0, 1, 0, 2}, new byte[5]{0, 1, 2, 0, 2},
			new byte[5]{0, 1, 1, 0, 2}, new byte[5]{1, 1, 1, 0, 2}, new byte[5]{2, 1, 1, 0, 2}, new byte[5]{1, 1, 2, 0, 2},
			new byte[5]{0, 2, 1, 0, 2}, new byte[5]{1, 2, 1, 0, 2}, new byte[5]{2, 2, 1, 0, 2}, new byte[5]{2, 1, 2, 0, 2},
			new byte[5]{0, 2, 2, 2, 2}, new byte[5]{1, 2, 2, 2, 2}, new byte[5]{2, 2, 2, 2, 2}, new byte[5]{2, 0, 2, 2, 2},
			new byte[5]{0, 0, 0, 0, 1}, new byte[5]{1, 0, 0, 0, 1}, new byte[5]{2, 0, 0, 0, 1}, new byte[5]{0, 0, 2, 0, 1},
			new byte[5]{0, 1, 0, 0, 1}, new byte[5]{1, 1, 0, 0, 1}, new byte[5]{2, 1, 0, 0, 1}, new byte[5]{1, 0, 2, 0, 1},
			new byte[5]{0, 2, 0, 0, 1}, new byte[5]{1, 2, 0, 0, 1}, new byte[5]{2, 2, 0, 0, 1}, new byte[5]{2, 0, 2, 0, 1},
			new byte[5]{0, 2, 2, 0, 1}, new byte[5]{1, 2, 2, 0, 1}, new byte[5]{2, 2, 2, 0, 1}, new byte[5]{2, 0, 2, 0, 1},
			new byte[5]{0, 0, 1, 0, 1}, new byte[5]{1, 0, 1, 0, 1}, new byte[5]{2, 0, 1, 0, 1}, new byte[5]{0, 1, 2, 0, 1},
			new byte[5]{0, 1, 1, 0, 1}, new byte[5]{1, 1, 1, 0, 1}, new byte[5]{2, 1, 1, 0, 1}, new byte[5]{1, 1, 2, 0, 1},
			new byte[5]{0, 2, 1, 0, 1}, new byte[5]{1, 2, 1, 0, 1}, new byte[5]{2, 2, 1, 0, 1}, new byte[5]{2, 1, 2, 0, 1},
			new byte[5]{0, 0, 1, 2, 2}, new byte[5]{1, 0, 1, 2, 2}, new byte[5]{2, 0, 1, 2, 2}, new byte[5]{0, 1, 2, 2, 2},
			new byte[5]{0, 0, 0, 1, 1}, new byte[5]{1, 0, 0, 1, 1}, new byte[5]{2, 0, 0, 1, 1}, new byte[5]{0, 0, 2, 1, 1},
			new byte[5]{0, 1, 0, 1, 1}, new byte[5]{1, 1, 0, 1, 1}, new byte[5]{2, 1, 0, 1, 1}, new byte[5]{1, 0, 2, 1, 1},
			new byte[5]{0, 2, 0, 1, 1}, new byte[5]{1, 2, 0, 1, 1}, new byte[5]{2, 2, 0, 1, 1}, new byte[5]{2, 0, 2, 1, 1},
			new byte[5]{0, 2, 2, 1, 1}, new byte[5]{1, 2, 2, 1, 1}, new byte[5]{2, 2, 2, 1, 1}, new byte[5]{2, 0, 2, 1, 1},
			new byte[5]{0, 0, 1, 1, 1}, new byte[5]{1, 0, 1, 1, 1}, new byte[5]{2, 0, 1, 1, 1}, new byte[5]{0, 1, 2, 1, 1},
			new byte[5]{0, 1, 1, 1, 1}, new byte[5]{1, 1, 1, 1, 1}, new byte[5]{2, 1, 1, 1, 1}, new byte[5]{1, 1, 2, 1, 1},
			new byte[5]{0, 2, 1, 1, 1}, new byte[5]{1, 2, 1, 1, 1}, new byte[5]{2, 2, 1, 1, 1}, new byte[5]{2, 1, 2, 1, 1},
			new byte[5]{0, 1, 1, 2, 2}, new byte[5]{1, 1, 1, 2, 2}, new byte[5]{2, 1, 1, 2, 2}, new byte[5]{1, 1, 2, 2, 2},
			new byte[5]{0, 0, 0, 2, 1}, new byte[5]{1, 0, 0, 2, 1}, new byte[5]{2, 0, 0, 2, 1}, new byte[5]{0, 0, 2, 2, 1},
			new byte[5]{0, 1, 0, 2, 1}, new byte[5]{1, 1, 0, 2, 1}, new byte[5]{2, 1, 0, 2, 1}, new byte[5]{1, 0, 2, 2, 1},
			new byte[5]{0, 2, 0, 2, 1}, new byte[5]{1, 2, 0, 2, 1}, new byte[5]{2, 2, 0, 2, 1}, new byte[5]{2, 0, 2, 2, 1},
			new byte[5]{0, 2, 2, 2, 1}, new byte[5]{1, 2, 2, 2, 1}, new byte[5]{2, 2, 2, 2, 1}, new byte[5]{2, 0, 2, 2, 1},
			new byte[5]{0, 0, 1, 2, 1}, new byte[5]{1, 0, 1, 2, 1}, new byte[5]{2, 0, 1, 2, 1}, new byte[5]{0, 1, 2, 2, 1},
			new byte[5]{0, 1, 1, 2, 1}, new byte[5]{1, 1, 1, 2, 1}, new byte[5]{2, 1, 1, 2, 1}, new byte[5]{1, 1, 2, 2, 1},
			new byte[5]{0, 2, 1, 2, 1}, new byte[5]{1, 2, 1, 2, 1}, new byte[5]{2, 2, 1, 2, 1}, new byte[5]{2, 1, 2, 2, 1},
			new byte[5]{0, 2, 1, 2, 2}, new byte[5]{1, 2, 1, 2, 2}, new byte[5]{2, 2, 1, 2, 2}, new byte[5]{2, 1, 2, 2, 2},
			new byte[5]{0, 0, 0, 1, 2}, new byte[5]{1, 0, 0, 1, 2}, new byte[5]{2, 0, 0, 1, 2}, new byte[5]{0, 0, 2, 1, 2},
			new byte[5]{0, 1, 0, 1, 2}, new byte[5]{1, 1, 0, 1, 2}, new byte[5]{2, 1, 0, 1, 2}, new byte[5]{1, 0, 2, 1, 2},
			new byte[5]{0, 2, 0, 1, 2}, new byte[5]{1, 2, 0, 1, 2}, new byte[5]{2, 2, 0, 1, 2}, new byte[5]{2, 0, 2, 1, 2},
			new byte[5]{0, 2, 2, 1, 2}, new byte[5]{1, 2, 2, 1, 2}, new byte[5]{2, 2, 2, 1, 2}, new byte[5]{2, 0, 2, 1, 2},
			new byte[5]{0, 0, 1, 1, 2}, new byte[5]{1, 0, 1, 1, 2}, new byte[5]{2, 0, 1, 1, 2}, new byte[5]{0, 1, 2, 1, 2},
			new byte[5]{0, 1, 1, 1, 2}, new byte[5]{1, 1, 1, 1, 2}, new byte[5]{2, 1, 1, 1, 2}, new byte[5]{1, 1, 2, 1, 2},
			new byte[5]{0, 2, 1, 1, 2}, new byte[5]{1, 2, 1, 1, 2}, new byte[5]{2, 2, 1, 1, 2}, new byte[5]{2, 1, 2, 1, 2},
			new byte[5]{0, 2, 2, 2, 2}, new byte[5]{1, 2, 2, 2, 2}, new byte[5]{2, 2, 2, 2, 2}, new byte[5]{2, 1, 2, 2, 2}
		};

		// packed trit-value for every unpacked trit-quintuplet
		// indexed by [high][][][][low]
		public static readonly byte[][][][][] integer_of_trits = new byte[3][][][][] {
			new byte[3][][][]{
				new byte[3][][]{
					new byte[3][]{
						new byte[3]{0, 1, 2},
						new byte[3]{4, 5, 6},
						new byte[3]{8, 9, 10}
					},
					new byte[3][]{
						new byte[3]{16, 17, 18},
						new byte[3]{20, 21, 22},
						new byte[3]{24, 25, 26}
					},
					new byte[3][]{
						new byte[3]{3, 7, 15},
						new byte[3]{19, 23, 27},
						new byte[3]{12, 13, 14}
					}
				},
				new byte[3][][]{
					new byte[3][]{
						new byte[3]{32, 33, 34},
						new byte[3]{36, 37, 38},
						new byte[3]{40, 41, 42}
					},
					new byte[3][]{
						new byte[3]{48, 49, 50},
						new byte[3]{52, 53, 54},
						new byte[3]{56, 57, 58}
					},
					new byte[3][]{
						new byte[3]{35, 39, 47},
						new byte[3]{51, 55, 59},
						new byte[3]{44, 45, 46}
					}
				},
				new byte[3][][]{
					new byte[3][]{
						new byte[3]{64, 65, 66},
						new byte[3]{68, 69, 70},
						new byte[3]{72, 73, 74}
					},
					new byte[3][]{
						new byte[3]{80, 81, 82},
						new byte[3]{84, 85, 86},
						new byte[3]{88, 89, 90}
					},
					new byte[3][]{
						new byte[3]{67, 71, 79},
						new byte[3]{83, 87, 91},
						new byte[3]{76, 77, 78}
					}
				}
			},
			new byte[3][][][]{
				new byte[3][][]{
					new byte[3][]{
						new byte[3]{128, 129, 130},
						new byte[3]{132, 133, 134},
						new byte[3]{136, 137, 138}
					},
					new byte[3][]{
						new byte[3]{144, 145, 146},
						new byte[3]{148, 149, 150},
						new byte[3]{152, 153, 154}
					},
					new byte[3][]{
						new byte[3]{131, 135, 143},
						new byte[3]{147, 151, 155},
						new byte[3]{140, 141, 142}
					}
				},
				new byte[3][][]{
					new byte[3][]{
						new byte[3]{160, 161, 162},
						new byte[3]{164, 165, 166},
						new byte[3]{168, 169, 170}
					},
					new byte[3][]{
						new byte[3]{176, 177, 178},
						new byte[3]{180, 181, 182},
						new byte[3]{184, 185, 186}
					},
					new byte[3][]{
						new byte[3]{163, 167, 175},
						new byte[3]{179, 183, 187},
						new byte[3]{172, 173, 174}
					}
				},
				new byte[3][][]{
					new byte[3][]{
						new byte[3]{192, 193, 194},
						new byte[3]{196, 197, 198},
						new byte[3]{200, 201, 202}
					},
					new byte[3][]{
						new byte[3]{208, 209, 210},
						new byte[3]{212, 213, 214},
						new byte[3]{216, 217, 218}
					},
					new byte[3][]{
						new byte[3]{195, 199, 207},
						new byte[3]{211, 215, 219},
						new byte[3]{204, 205, 206}
					}
				}
			},
			new byte[3][][][]{
				new byte[3][][]{
					new byte[3][]{
						new byte[3]{96, 97, 98},
						new byte[3]{100, 101, 102},
						new byte[3]{104, 105, 106}
					},
					new byte[3][]{
						new byte[3]{112, 113, 114},
						new byte[3]{116, 117, 118},
						new byte[3]{120, 121, 122}
					},
					new byte[3][]{
						new byte[3]{99, 103, 111},
						new byte[3]{115, 119, 123},
						new byte[3]{108, 109, 110}
					}
				},
				new byte[3][][]{
					new byte[3][]{
						new byte[3]{224, 225, 226},
						new byte[3]{228, 229, 230},
						new byte[3]{232, 233, 234}
					},
					new byte[3][]{
						new byte[3]{240, 241, 242},
						new byte[3]{244, 245, 246},
						new byte[3]{248, 249, 250}
					},
					new byte[3][]{
						new byte[3]{227, 231, 239},
						new byte[3]{243, 247, 251},
						new byte[3]{236, 237, 238}
					}
				},
				new byte[3][][]{
					new byte[3][]{
						new byte[3]{28, 29, 30},
						new byte[3]{60, 61, 62},
						new byte[3]{92, 93, 94}
					},
					new byte[3][]{
						new byte[3]{156, 157, 158},
						new byte[3]{188, 189, 190},
						new byte[3]{220, 221, 222}
					},
					new byte[3][]{
						new byte[3]{31, 63, 127},
						new byte[3]{159, 191, 255},
						new byte[3]{252, 253, 254}
					}
				}
			}
		};

		private static readonly List<btq_count> btq_counts = new List<btq_count>()
		{
			new btq_count(QuantMethod.QUANT_2, 1, 0, 0),
			new btq_count(QuantMethod.QUANT_3, 0, 1, 0 ),
			new btq_count(QuantMethod.QUANT_4, 2, 0, 0 ),
			new btq_count(QuantMethod.QUANT_5, 0, 0, 1 ),
			new btq_count(QuantMethod.QUANT_6, 1, 1, 0 ),
			new btq_count(QuantMethod.QUANT_8, 3, 0, 0 ),
			new btq_count(QuantMethod.QUANT_10, 1, 0, 1 ),
			new btq_count(QuantMethod.QUANT_12, 2, 1, 0 ),
			new btq_count(QuantMethod.QUANT_16, 4, 0, 0 ),
			new btq_count(QuantMethod.QUANT_20, 2, 0, 1 ),
			new btq_count(QuantMethod.QUANT_24, 3, 1, 0 ),
			new btq_count(QuantMethod.QUANT_32, 5, 0, 0 ),
			new btq_count(QuantMethod.QUANT_40, 3, 0, 1 ),
			new btq_count(QuantMethod.QUANT_48, 4, 1, 0 ),
			new btq_count(QuantMethod.QUANT_64, 6, 0, 0 ),
			new btq_count(QuantMethod.QUANT_80, 4, 0, 1 ),
			new btq_count(QuantMethod.QUANT_96, 5, 1, 0 ),
			new btq_count(QuantMethod.QUANT_128, 7, 0, 0 ),
			new btq_count(QuantMethod.QUANT_160, 5, 0, 1 ),
			new btq_count(QuantMethod.QUANT_192, 6, 1, 0 ),
			new btq_count(QuantMethod.QUANT_256, 8, 0, 0 )
		};

		/* See header for documentation. */
		public static int get_ise_sequence_bitcount(int items, QuantMethod quant) {
			// Cope with out-of bounds values - input might be invalid
			if (static_cast<size_t>(quant) >= ise_sizes.size())
			{
				// Arbitrary large number that's more than an ASTC block can hold
				return 1024;
			}

			auto& entry = ise_sizes[quant];
			return (entry.scale * items + entry.round) / entry.divisor;
		}

		// routine to write up to 8 bits
		public static void write_bits(int value, int bitcount, int bitoffset, byte[] ptr) 
		{
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

		// routine to read up to 8 bits
		public static int read_bits(int bitcount, int bitoffset, byte[] ptr) 
		{
			int mask = (1 << bitcount) - 1;
			ptr += bitoffset >> 3;
			bitoffset &= 7;
			int value = ptr[0] | (ptr[1] << 8);
			value >>= bitoffset;
			value &= mask;
			return value;
		}

		public static void encode_ise(int quant_level, int elements, byte[] input_data, byte[] output_data, int bit_offset) 
		{
			int bits = btq_counts[quant_level].bits;
			int trits = btq_counts[quant_level].trits;
			int quints = btq_counts[quant_level].quints;
			int mask = (1 << bits) - 1;

			// Write out trits and bits
			if (trits)
			{
				int i = 0;
				int full_trit_blocks = elements / 5;

				for (int j = 0; j < full_trit_blocks; j++)
				{
					int i4 = input_data[i + 4] >> bits;
					int i3 = input_data[i + 3] >> bits;
					int i2 = input_data[i + 2] >> bits;
					int i1 = input_data[i + 1] >> bits;
					int i0 = input_data[i + 0] >> bits;

					byte T = integer_of_trits[i4][i3][i2][i1][i0];

					// The max size of a trit bit count is 6, so we can always safely
					// pack a single MX value with the following 1 or 2 T bits.
					byte pack;

					// Element 0 + T0 + T1
					pack = (input_data[i++] & mask) | (((T >> 0) & 0x3) << bits);
					write_bits(pack, bits + 2, bit_offset, output_data);
					bit_offset += bits + 2;

					// Element 1 + T2 + T3
					pack = (input_data[i++] & mask) | (((T >> 2) & 0x3) << bits);
					write_bits(pack, bits + 2, bit_offset, output_data);
					bit_offset += bits + 2;

					// Element 2 + T4
					pack = (input_data[i++] & mask) | (((T >> 4) & 0x1) << bits);
					write_bits(pack, bits + 1, bit_offset, output_data);
					bit_offset += bits + 1;

					// Element 3 + T5 + T6
					pack = (input_data[i++] & mask) | (((T >> 5) & 0x3) << bits);
					write_bits(pack, bits + 2, bit_offset, output_data);
					bit_offset += bits + 2;

					// Element 4 + T7
					pack = (input_data[i++] & mask) | (((T >> 7) & 0x1) << bits);
					write_bits(pack, bits + 1, bit_offset, output_data);
					bit_offset += bits + 1;
				}

				// Loop tail for a partial block
				if (i != elements)
				{
					// i4 cannot be present - we know the block is partial
					// i0 must be present - we know the block isn't empty
					int i4 =                         0;
					int i3 = i + 3 >= elements ? 0 : input_data[i + 3] >> bits;
					int i2 = i + 2 >= elements ? 0 : input_data[i + 2] >> bits;
					int i1 = i + 1 >= elements ? 0 : input_data[i + 1] >> bits;
					int i0 =                         input_data[i + 0] >> bits;

					byte T = integer_of_trits[i4][i3][i2][i1][i0];

					for (int j = 0; i < elements; i++, j++)
					{
						// Truncated table as this iteration is always partital
						byte[] tbits = new byte[4] { 2, 2, 1, 2 };
						byte[] tshift = new byte[4] { 0, 2, 4, 5 };

						byte pack = (input_data[i] & mask) |
									(((T >> tshift[j]) & ((1 << tbits[j]) - 1)) << bits);

						write_bits(pack, bits + tbits[j], bit_offset, output_data);
						bit_offset += bits + tbits[j];
					}
				}
			}
			// Write out quints and bits
			else if (quints)
			{
				int i = 0;
				int full_quint_blocks = elements / 3;

				for (int j = 0; j < full_quint_blocks; j++)
				{
					int i2 = input_data[i + 2] >> bits;
					int i1 = input_data[i + 1] >> bits;
					int i0 = input_data[i + 0] >> bits;

					byte T = integer_of_quints[i2][i1][i0];

					// The max size of a quint bit count is 5, so we can always safely
					// pack a single M value with the following 2 or 3 T bits.
					byte pack;

					// Element 0
					pack = (input_data[i++] & mask) | (((T >> 0) & 0x7) << bits);
					write_bits(pack, bits + 3, bit_offset, output_data);
					bit_offset += bits + 3;

					// Element 1
					pack = (input_data[i++] & mask) | (((T >> 3) & 0x3) << bits);
					write_bits(pack, bits + 2, bit_offset, output_data);
					bit_offset += bits + 2;

					// Element 2
					pack = (input_data[i++] & mask) | (((T >> 5) & 0x3) << bits);
					write_bits(pack, bits + 2, bit_offset, output_data);
					bit_offset += bits + 2;
				}

				// Loop tail for a partial block
				if (i != elements)
				{
					// i2 cannot be present - we know the block is partial
					// i0 must be present - we know the block isn't empty
					int i2 =                         0;
					int i1 = i + 1 >= elements ? 0 : input_data[i + 1] >> bits;
					int i0 =                         input_data[i + 0] >> bits;

					byte T = integer_of_quints[i2][i1][i0];

					for (int j = 0; i < elements; i++, j++)
					{
						// Truncated table as this iteration is always partital
						byte[] tbits = new byte[2]  { 3, 2 };
						byte[] tshift = new byte[2] { 0, 3 };

						byte pack = (input_data[i] & mask) |
									(((T >> tshift[j]) & ((1 << tbits[j]) - 1)) << bits);

						write_bits(pack, bits + tbits[j], bit_offset, output_data);
						bit_offset += bits + tbits[j];
					}
				}
			}
			// Write out just bits
			else
			{
				promise(elements > 0);
				for (int i = 0; i < elements; i++)
				{
					write_bits(input_data[i], bits, bit_offset, output_data);
					bit_offset += bits;
				}
			}
		}

		public static void decode_ise(int quant_level, int elements, byte[] input_data, byte[] output_data, int bit_offset) 
		{
			// note: due to how the trit/quint-block unpacking is done in this function,
			// we may write more temporary results than the number of outputs
			// The maximum actual number of results is 64 bit, but we keep 4 additional elements
			// of padding.
			byte[] results = new byte[68];
			byte[] tq_blocks = new byte[22];		// trit-blocks or quint-blocks

			int bits = btq_counts[quant_level].bits;
			int trits = btq_counts[quant_level].trits;
			int quints = btq_counts[quant_level].quints;

			int lcounter = 0;
			int hcounter = 0;

			// trit-blocks or quint-blocks must be zeroed out before we collect them in the loop below.
			for (int i = 0; i < 22; i++)
			{
				tq_blocks[i] = 0;
			}

			// collect bits for each element, as well as bits for any trit-blocks and quint-blocks.
			for (int i = 0; i < elements; i++)
			{
				results[i] = read_bits(bits, bit_offset, input_data);
				bit_offset += bits;

				if (trits)
				{
					int[] bits_to_read = new int[5]  { 2, 2, 1, 2, 1 };
					int[] block_shift = new int[5]   { 0, 2, 4, 5, 7 };
					int[] next_lcounter = new int[5] { 1, 2, 3, 4, 0 };
					int[] hcounter_incr = new int[5] { 0, 0, 0, 0, 1 };
					int tdata = read_bits(bits_to_read[lcounter], bit_offset, input_data);
					bit_offset += bits_to_read[lcounter];
					tq_blocks[hcounter] |= tdata << block_shift[lcounter];
					hcounter += hcounter_incr[lcounter];
					lcounter = next_lcounter[lcounter];
				}

				if (quints)
				{
					int[] bits_to_read = new int[3]  { 3, 2, 2 };
					int[] block_shift = new int[3]   { 0, 3, 5 };
					int[] next_lcounter = new int[3] { 1, 2, 0 };
					int[] hcounter_incr = new int[3] { 0, 0, 1 };
					int tdata = read_bits(bits_to_read[lcounter], bit_offset, input_data);
					bit_offset += bits_to_read[lcounter];
					tq_blocks[hcounter] |= tdata << block_shift[lcounter];
					hcounter += hcounter_incr[lcounter];
					lcounter = next_lcounter[lcounter];
				}
			}

			// unpack trit-blocks or quint-blocks as needed
			if (trits)
			{
				int trit_blocks = (elements + 4) / 5;
				for (int i = 0; i < trit_blocks; i++)
				{
					byte[] tritptr = trits_of_integer[tq_blocks[i]];
					results[5 * i    ] |= tritptr[0] << bits;
					results[5 * i + 1] |= tritptr[1] << bits;
					results[5 * i + 2] |= tritptr[2] << bits;
					results[5 * i + 3] |= tritptr[3] << bits;
					results[5 * i + 4] |= tritptr[4] << bits;
				}
			}

			if (quints)
			{
				int quint_blocks = (elements + 2) / 3;
				for (int i = 0; i < quint_blocks; i++)
				{
					byte[] quintptr = quints_of_integer[tq_blocks[i]];
					results[3 * i    ] |= quintptr[0] << bits;
					results[3 * i + 1] |= quintptr[1] << bits;
					results[3 * i + 2] |= quintptr[2] << bits;
				}
			}

			for (int i = 0; i < elements; i++)
			{
				output_data[i] = results[i];
			}
		}

	}

	
	/**
	* @brief The number of bits, trits, and quints needed for a quant level.
	*/
	public struct BtqCount 
	{
		/**< The quantization level. */
		public byte quant;

		/**< The number of bits. */
		public byte bits;

		/**< The number of trits. */
		public byte trits;

		/**< The number of quints. */
		public byte quints;
	}

	/**
	* @brief The sequence scale, round, and divisors needed to compute sizing.
	*
	* The length of a quantized sequence in bits is:
	*     (scale * <sequence_len> + round) / divisor
	*/
	public struct IseSize 
	{
		/**< The quantization level. */
		public byte quant;

		/**< The scaling parameter. */
		public byte scale;

		/**< The rounding parameter. */
		public byte round;

		/**< The divisor parameter. */
		public byte divisor;
	}
}