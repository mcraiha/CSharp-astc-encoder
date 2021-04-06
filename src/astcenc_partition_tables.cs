
namespace ASTCEnc
{
	public static class PartitionTables
	{
		/*
			Produce a canonicalized representation of a partition pattern
			The largest possible such representation is 432 bits, equal to 7 ulong values.
		*/
		public static void gen_canonicalized_partition_table(int texel_count, byte[] partition_table, ulong[] canonicalized)
		{
			for (int i = 0; i < 7; i++)
			{
				canonicalized[i] = 0;
			}

			int[] mapped_index = new int[4];
			int map_weight_count = 0;
			for (int i = 0; i < 4; i++)
			{
				mapped_index[i] = -1;
			}

			for (int i = 0; i < texel_count; i++)
			{
				int index = partition_table[i];
				if (mapped_index[index] == -1)
				{
					mapped_index[index] = map_weight_count++;
				}
				ulong xlat_index = (ulong)mapped_index[index];
				canonicalized[i >> 5] |= xlat_index << (2 * (i & 0x1F));
			}
		}

		public static int compare_canonicalized_partition_tables(ulong[] part1, ulong[] part2) 
		{
			if ((part1[0] != part2[0]) || (part1[1] != part2[1]) || (part1[2] != part2[2]) ||
				(part1[3] != part2[3]) || (part1[4] != part2[4]) || (part1[5] != part2[5]) ||
				(part1[6] != part2[6]))
			{
				return 0;
			}

			return 1;
		}

		/*
		For a partition table, detect partitions that are equivalent, then mark them
		as invalid. This reduces the number of partitions that the codec has to
		consider and thus improves encode performance. */
		public static void partition_table_zap_equal_elements(int texel_count, PartitionInfo pt) 
		{
			int partition_tables_zapped = 0;
			ulong[] canonicalizeds = new ulong[Constants.PARTITION_COUNT * 7];

			for (int i = 0; i < Constants.PARTITION_COUNT; i++)
			{
				gen_canonicalized_partition_table(texel_count, pt[i].partition_of_texel, canonicalizeds + i * 7);
			}

			for (int i = 0; i < Constants.PARTITION_COUNT; i++)
			{
				for (int j = 0; j < i; j++)
				{
					if (compare_canonicalized_partition_tables(canonicalizeds + 7 * i, canonicalizeds + 7 * j))
					{
						pt[i].partition_count = 0;
						partition_tables_zapped++;
						break;
					}
				}
			}
		}

		public static uint hash52(uint inp)
		{
			inp ^= inp >> 15;

			inp *= 0xEEDE0891;			// (2^4+1)*(2^7+1)*(2^17-1)
			inp ^= inp >> 5;
			inp += inp << 16;
			inp ^= inp >> 7;
			inp ^= inp >> 3;
			inp ^= inp << 6;
			inp ^= inp >> 17;
			return inp;
		}

		public static int select_partition(int seed, int x, int y, int z, int partitioncount, bool small_block) 
		{
			if (small_block)
			{
				x <<= 1;
				y <<= 1;
				z <<= 1;
			}

			seed += (partitioncount - 1) * 1024;

			uint rnum = hash52(seed);

			byte seed1 = (byte)(rnum & 0xF);
			byte seed2 = (rnum >> 4) & 0xF;
			byte seed3 = (rnum >> 8) & 0xF;
			byte seed4 = (rnum >> 12) & 0xF;
			byte seed5 = (rnum >> 16) & 0xF;
			byte seed6 = (rnum >> 20) & 0xF;
			byte seed7 = (rnum >> 24) & 0xF;
			byte seed8 = (rnum >> 28) & 0xF;
			byte seed9 = (rnum >> 18) & 0xF;
			byte seed10 = (rnum >> 22) & 0xF;
			byte seed11 = (rnum >> 26) & 0xF;
			byte seed12 = ((rnum >> 30) | (rnum << 2)) & 0xF;

			// squaring all the seeds in order to bias their distribution
			// towards lower values.
			seed1 *= seed1;
			seed2 *= seed2;
			seed3 *= seed3;
			seed4 *= seed4;
			seed5 *= seed5;
			seed6 *= seed6;
			seed7 *= seed7;
			seed8 *= seed8;
			seed9 *= seed9;
			seed10 *= seed10;
			seed11 *= seed11;
			seed12 *= seed12;

			int sh1, sh2;
			if ((seed & 1) == 1)
			{
				sh1 = ((seed & 2) == 1 ? 4 : 5);
				sh2 = (partitioncount == 3 ? 6 : 5);
			}
			else
			{
				sh1 = (partitioncount == 3 ? 6 : 5);
				sh2 = ((seed & 2) == 1 ? 4 : 5);
			}

			int sh3 = (seed & 0x10) == 1 ? sh1 : sh2;

			seed1 >>= sh1;
			seed2 >>= sh2;
			seed3 >>= sh1;
			seed4 >>= sh2;
			seed5 >>= sh1;
			seed6 >>= sh2;
			seed7 >>= sh1;
			seed8 >>= sh2;

			seed9 >>= sh3;
			seed10 >>= sh3;
			seed11 >>= sh3;
			seed12 >>= sh3;

			int a = seed1 * x + seed2 * y + seed11 * z + (rnum >> 14);
			int b = seed3 * x + seed4 * y + seed12 * z + (rnum >> 10);
			int c = seed5 * x + seed6 * y + seed9 * z + (rnum >> 6);
			int d = seed7 * x + seed8 * y + seed10 * z + (rnum >> 2);

			// apply the saw
			a &= 0x3F;
			b &= 0x3F;
			c &= 0x3F;
			d &= 0x3F;

			// remove some of the components if we are to output < 4 partitions.
			if (partitioncount <= 3)
			{
				d = 0;
			}

			if (partitioncount <= 2)
			{
				c = 0;
			}

			if (partitioncount <= 1)
			{
				b = 0;
			}

			int partition;
			if (a >= b && a >= c && a >= d)
			{
				partition = 0;
			}
			else if (b >= c && b >= d)
			{
				partition = 1;
			}
			else if (c >= d)
			{
				partition = 2;
			}
			else
			{
				partition = 3;
			}

			return partition;
		}

		public static void generate_one_partition_table(BlockSizeDescriptor bsd, int partition_count, int partition_index, PartitionInfo pt)
		{
			int texels_per_block = bsd.texel_count;
			bool small_block = texels_per_block < 32;

			byte[] partition_of_texel = pt.partition_of_texel;

			for (int z = 0; z < bsd.zdim; z++)
			{
				for (int y = 0; y <  bsd.ydim; y++)
				{
					for (int x = 0; x <  bsd.xdim; x++)
					{
						byte part = select_partition(partition_index, x, y, z, partition_count, small_block);
						*partition_of_texel++ = part;
					}
				}
			}

			int[] counts = new int[4];
			for (int i = 0; i < 4; i++)
			{
				counts[i] = 0;
			}

			for (int i = 0; i < texels_per_block; i++)
			{
				int partition = pt.partition_of_texel[i];
				pt.texels_of_partition[partition, counts[partition]++] = i;
			}

			for (int i = 0; i < 4; i++)
			{
				pt.partition_texel_count[i] = counts[i];
			}

			if (counts[0] == 0)
			{
				pt.partition_count = 0;
			}
			else if (counts[1] == 0)
			{
				pt.partition_count = 1;
			}
			else if (counts[2] == 0)
			{
				pt.partition_count = 2;
			}
			else if (counts[3] == 0)
			{
				pt.partition_count = 3;
			}
			else
			{
				pt.partition_count = 4;
			}

			for (int i = 0; i < 4; i++)
			{
				pt.coverage_bitmaps[i] = 0UL;
			}

			int texels_to_process = bsd.kmeans_texel_count;
			for (int i = 0; i < texels_to_process; i++)
			{
				int idx = bsd.kmeans_texels[i];
				pt.coverage_bitmaps[pt.partition_of_texel[idx]] |= 1UL << i;
			}
		}

		/* Public function, see header file for detailed documentation */
		public static void init_partition_tables(BlockSizeDescriptor bsd) 
		{
			PartitionInfo[] par_tab2 = bsd.partitions;
			PartitionInfo par_tab3 = par_tab2 + PARTITION_COUNT;
			PartitionInfo par_tab4 = par_tab3 + PARTITION_COUNT;
			PartitionInfo par_tab1 = par_tab4 + PARTITION_COUNT;

			generate_one_partition_table(bsd, 1, 0, par_tab1);
			for (int i = 0; i < 1024; i++)
			{
				generate_one_partition_table(bsd, 2, i, par_tab2 + i);
				generate_one_partition_table(bsd, 3, i, par_tab3 + i);
				generate_one_partition_table(bsd, 4, i, par_tab4 + i);
			}

			partition_table_zap_equal_elements(bsd.texel_count, par_tab2);
			partition_table_zap_equal_elements(bsd.texel_count, par_tab3);
			partition_table_zap_equal_elements(bsd.texel_count, par_tab4);
		}
	}
}