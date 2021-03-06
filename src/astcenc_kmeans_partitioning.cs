using System;

namespace ASTCEnc
{
	public static class KmeansPartitioning
	{
		// for k++ means, we need pseudo-random numbers, however using random numbers
		// directly results in unreproducible encoding results. As such, we will
		// instead just supply a handful of numbers from random.org, and apply an
		// algorithm similar to XKCD #221. (http://xkcd.com/221/)

		// cluster the texels using the k++ means clustering initialization algorithm.
		public static void kmeans_init(int texels_per_block, int partition_count, ImageBlock blk, vfloat4[] cluster_centers) 
		{
			int[] cluster_center_samples = new int[4];
			// pick a random sample as first center-point.
			cluster_center_samples[0] = 145897 /* number from random.org */  % texels_per_block;
			int samples_selected = 1;

			float[] distances = new float[Constants.MAX_TEXELS_PER_BLOCK];

			// compute the distance to the first point.
			int sample = cluster_center_samples[0];
			vfloat4 center_color = blk.texel(sample);

			float distance_sum = 0.0f;
			for (int i = 0; i < texels_per_block; i++)
			{
				vfloat4 color = blk.texel(i);
				vfloat4 diff = color - center_color;
				float distance = vfloat4.dot_s(diff, diff);
				distance_sum += distance;
				distances[i] = distance;
			}

			// more numbers from random.org
			float[] cluster_cutoffs = new float[25] {
				0.952312f, 0.206893f, 0.835984f, 0.507813f, 0.466170f,
				0.872331f, 0.488028f, 0.866394f, 0.363093f, 0.467905f,
				0.812967f, 0.626220f, 0.932770f, 0.275454f, 0.832020f,
				0.362217f, 0.318558f, 0.240113f, 0.009190f, 0.983995f,
				0.566812f, 0.347661f, 0.731960f, 0.156391f, 0.297786f
			};

			while (true)
			{
				// pick a point in a weighted-random fashion.
				float summa = 0.0f;
				float distance_cutoff = distance_sum * cluster_cutoffs[samples_selected + 5 * partition_count];
				for (sample = 0; sample < texels_per_block; sample++)
				{
					summa += distances[sample];
					if (summa >= distance_cutoff)
					{
						break;
					}
				}

				if (sample >= texels_per_block)
				{
					sample = texels_per_block - 1;
				}

				cluster_center_samples[samples_selected] = sample;
				samples_selected++;
				if (samples_selected >= partition_count)
				{
					break;
				}

				// update the distances with the new point.
				center_color = blk.texel(sample);

				distance_sum = 0.0f;
				for (int i = 0; i < texels_per_block; i++)
				{
					vfloat4 color = blk.texel(i);
					vfloat4 diff = color - center_color;
					float distance = vfloat4.dot_s(diff, diff);
					distance = Math.Min(distance, distances[i]);
					distance_sum += distance;
					distances[i] = distance;
				}
			}

			// finally, gather up the results.
			for (int i = 0; i < partition_count; i++)
			{
				int center_sample = cluster_center_samples[i];
				cluster_centers[i] = blk.texel(center_sample);
			}
		}

		// basic K-means clustering: given a set of cluster centers,
		// assign each texel to a partition
		public static void kmeans_assign(int texels_per_block, int partition_count, ImageBlock blk, vfloat4[] cluster_centers, int[] partition_of_texel) 
		{
			float[] distances = new float[Constants.MAX_TEXELS_PER_BLOCK];

			int[] partition_texel_count = new int[4];

			partition_texel_count[0] = texels_per_block;
			for (int i = 1; i < partition_count; i++)
			{
				partition_texel_count[i] = 0;
			}

			for (int i = 0; i < texels_per_block; i++)
			{
				vfloat4 color = blk.texel(i);
				vfloat4 diff = color - cluster_centers[0];
				float distance = vfloat4.dot_s(diff, diff);
				distances[i] = distance;
				partition_of_texel[i] = 0;
			}

			for (int j = 1; j < partition_count; j++)
			{
				vfloat4 center_color = cluster_centers[j];

				for (int i = 0; i < texels_per_block; i++)
				{
					vfloat4 color = blk.texel(i);
					vfloat4 diff = color - center_color;
					float distance = vfloat4.dot_s(diff, diff);
					if (distance < distances[i])
					{
						distances[i] = distance;
						partition_texel_count[partition_of_texel[i]]--;
						partition_texel_count[j]++;
						partition_of_texel[i] = j;
					}
				}
			}

			// it is possible to get a situation where one of the partitions ends up
			// without any texels. In this case, we assign texel N to partition N;
			// this is silly, but ensures that every partition retains at least one texel.
			// Reassigning a texel in this manner may cause another partition to go empty,
			// so if we actually did a reassignment, we run the whole loop over again.
			int problem_case;
			do
			{
				problem_case = 0;
				for (int i = 0; i < partition_count; i++)
				{
					if (partition_texel_count[i] == 0)
					{
						partition_texel_count[partition_of_texel[i]]--;
						partition_texel_count[i]++;
						partition_of_texel[i] = i;
						problem_case = 1;
					}
				}
			}
			while (problem_case != 0);
		}

		// basic k-means clustering: given a set of cluster assignments
		// for the texels, find the center position of each cluster.
		public static void kmeans_update(int texels_per_block, int partition_count, ImageBlock blk, int[] partition_of_texel, vfloat4[] cluster_centers) 
		{
			vfloat4[] color_sum = new vfloat4[4];
			int[] weight_sum = new int[4];

			for (int i = 0; i < partition_count; i++)
			{
				color_sum[i] = vfloat4.zero();
				weight_sum[i] = 0;
			}

			// first, find the center-of-gravity in each cluster
			for (int i = 0; i < texels_per_block; i++)
			{
				vfloat4 color = blk.texel(i);
				int part = partition_of_texel[i];
				color_sum[part] = color_sum[part] + color;
				weight_sum[part]++;
			}

			for (int i = 0; i < partition_count; i++)
			{
				cluster_centers[i] = color_sum[i] * (1.0f / weight_sum[i]);
			}
		}

		// compute the bit-mismatch for a partitioning in 2-partition mode
		public static int partition_mismatch2(ulong a0, ulong a1, ulong b0, ulong b1) 
		{
			int v1 = ASTCMath.popcount(a0 ^ b0) + ASTCMath.popcount(a1 ^ b1);
			int v2 = ASTCMath.popcount(a0 ^ b1) + ASTCMath.popcount(a1 ^ b0);
			return Math.Min(v1, v2);
		}

		// compute the bit-mismatch for a partitioning in 3-partition mode
		public static int partition_mismatch3(ulong a0, ulong a1, ulong a2, ulong b0, ulong b1, ulong b2) 
		{
			int p00 = ASTCMath.popcount(a0 ^ b0);
			int p01 = ASTCMath.popcount(a0 ^ b1);
			int p02 = ASTCMath.popcount(a0 ^ b2);

			int p10 = ASTCMath.popcount(a1 ^ b0);
			int p11 = ASTCMath.popcount(a1 ^ b1);
			int p12 = ASTCMath.popcount(a1 ^ b2);

			int p20 = ASTCMath.popcount(a2 ^ b0);
			int p21 = ASTCMath.popcount(a2 ^ b1);
			int p22 = ASTCMath.popcount(a2 ^ b2);

			int s0 = p11 + p22;
			int s1 = p12 + p21;
			int v0 = Math.Min(s0, s1) + p00;

			int s2 = p10 + p22;
			int s3 = p12 + p20;
			int v1 = Math.Min(s2, s3) + p01;

			int s4 = p10 + p21;
			int s5 = p11 + p20;
			int v2 = Math.Min(s4, s5) + p02;

			return Math.Min(v0, v1, v2);
		}

		// compute the bit-mismatch for a partitioning in 4-partition mode
		public static int partition_mismatch4(ulong a0, ulong a1, ulong a2, ulong a3, ulong b0, ulong b1, ulong b2, ulong b3) 
		{
			int p00 = ASTCMath.popcount(a0 ^ b0);
			int p01 = ASTCMath.popcount(a0 ^ b1);
			int p02 = ASTCMath.popcount(a0 ^ b2);
			int p03 = ASTCMath.popcount(a0 ^ b3);

			int p10 = ASTCMath.popcount(a1 ^ b0);
			int p11 = ASTCMath.popcount(a1 ^ b1);
			int p12 = ASTCMath.popcount(a1 ^ b2);
			int p13 = ASTCMath.popcount(a1 ^ b3);

			int p20 = ASTCMath.popcount(a2 ^ b0);
			int p21 = ASTCMath.popcount(a2 ^ b1);
			int p22 = ASTCMath.popcount(a2 ^ b2);
			int p23 = ASTCMath.popcount(a2 ^ b3);

			int p30 = ASTCMath.popcount(a3 ^ b0);
			int p31 = ASTCMath.popcount(a3 ^ b1);
			int p32 = ASTCMath.popcount(a3 ^ b2);
			int p33 = ASTCMath.popcount(a3 ^ b3);

			int mx23 = Math.Min(p22 + p33, p23 + p32);
			int mx13 = Math.Min(p21 + p33, p23 + p31);
			int mx12 = Math.Min(p21 + p32, p22 + p31);
			int mx03 = Math.Min(p20 + p33, p23 + p30);
			int mx02 = Math.Min(p20 + p32, p22 + p30);
			int mx01 = Math.Min(p21 + p30, p20 + p31);

			int v0 = p00 + Math.Min(p11 + mx23, p12 + mx13, p13 + mx12);
			int v1 = p01 + Math.Min(p10 + mx23, p12 + mx03, p13 + mx02);
			int v2 = p02 + Math.Min(p11 + mx03, p10 + mx13, p13 + mx01);
			int v3 = p03 + Math.Min(p11 + mx02, p12 + mx01, p10 + mx12);

			return Math.Min(v0, v1, v2, v3);
		}

		public static void count_partition_mismatch_bits(BlockSizeDescriptor bsd, int partition_count, ulong[] bitmaps, int[] bitcounts)
		{
			PartitionInfo pt = get_partition_table(bsd, partition_count);

			if (partition_count == 2)
			{
				ulong bm0 = bitmaps[0];
				ulong bm1 = bitmaps[1];
				for (int i = 0; i < Constants.PARTITION_COUNT; i++)
				{
					if (pt.partition_count == 2)
					{
						bitcounts[i] = partition_mismatch2(bm0, bm1, pt.coverage_bitmaps[0], pt.coverage_bitmaps[1]);
					}
					else
					{
						bitcounts[i] = 255;
					}
					pt++;
				}
			}
			else if (partition_count == 3)
			{
				ulong bm0 = bitmaps[0];
				ulong bm1 = bitmaps[1];
				ulong bm2 = bitmaps[2];
				for (int i = 0; i < Constants.PARTITION_COUNT; i++)
				{
					if (pt.partition_count == 3)
					{
						bitcounts[i] = partition_mismatch3(bm0, bm1, bm2, pt.coverage_bitmaps[0], pt.coverage_bitmaps[1], pt.coverage_bitmaps[2]);
					}
					else
					{
						bitcounts[i] = 255;
					}
					pt++;
				}
			}
			else if (partition_count == 4)
			{
				ulong bm0 = bitmaps[0];
				ulong bm1 = bitmaps[1];
				ulong bm2 = bitmaps[2];
				ulong bm3 = bitmaps[3];
				for (int i = 0; i < Constants.PARTITION_COUNT; i++)
				{
					if (pt.partition_count == 4)
					{
						bitcounts[i] = partition_mismatch4(bm0, bm1, bm2, bm3, pt.coverage_bitmaps[0], pt.coverage_bitmaps[1], pt.coverage_bitmaps[2], pt.coverage_bitmaps[3]);
					}
					else
					{
						bitcounts[i] = 255;
					}
					pt++;
				}
			}

		}

		/**
		* @brief Use counting sort on the mismatch array to sort partition candidates.
		*/
		public static void get_partition_ordering_by_mismatch_bits(int[] mismatch_bits,int[] partition_ordering)
		{
			int[] mscount = new int[256];

			// Create the histogram of mismatch counts
			for (int i = 0; i < Constants.PARTITION_COUNT; i++)
			{
				mscount[mismatch_bits[i]]++;
			}

			// Create a running sum from the histogram array
			// Cells store previous values only; i.e. exclude self after sum
			int summa = 0;
			for (int i = 0; i < 256; i++)
			{
				int cnt = mscount[i];
				mscount[i] = summa;
				summa += cnt;
			}

			// Use the running sum as the index, incrementing after read to allow
			// sequential entries with the same count
			for (int i = 0; i < Constants.PARTITION_COUNT; i++)
			{
				int idx = mscount[mismatch_bits[i]]++;
				partition_ordering[idx] = i;
			}
		}

		public static void kmeans_compute_partition_ordering(BlockSizeDescriptor bsd, int partition_count, ImageBlock blk, int[] ordering) 
		{
			vfloat4[] cluster_centers = new vfloat4[4];
			int[] partition_of_texel = new int[Constants.MAX_TEXELS_PER_BLOCK];

			// Use three passes of k-means clustering to partition the block data
			for (int i = 0; i < 3; i++)
			{
				if (i == 0)
				{
					kmeans_init(bsd.texel_count, partition_count, blk, cluster_centers);
				}
				else
				{
					kmeans_update(bsd.texel_count, partition_count, blk, partition_of_texel, cluster_centers);
				}

				kmeans_assign(bsd.texel_count, partition_count, blk, cluster_centers, partition_of_texel);
			}

			// Construct the block bitmaps of texel assignments to each partition
			ulong[] bitmaps = new ulong[4];
			int texels_to_process = bsd.kmeans_texel_count;
			for (int i = 0; i < texels_to_process; i++)
			{
				int idx = bsd.kmeans_texels[i];
				bitmaps[partition_of_texel[idx]] |= 1UL << i;
			}

			// Count the mismatch between the block and the format's partition tables
			int[] mismatch_counts = new int[Constants.PARTITION_COUNT];
			count_partition_mismatch_bits(bsd, partition_count, bitmaps, mismatch_counts);

			// Sort the partitions based on the number of mismatched bits
			get_partition_ordering_by_mismatch_bits(mismatch_counts, ordering);
		}
	}
}