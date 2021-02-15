namespace ASTCEnc
{
	public struct vfloat4
	{
		private const int internalSize = 4;
		public float[] m;

		public vfloat4(bool unusedValue)
		{
			this.m = new float[internalSize];
		}

		public vfloat4(float[] p)
		{
			this.m = new float[internalSize];
			this.m[0] = p[0];
			this.m[1] = p[1];
			this.m[2] = p[2];
			this.m[3] = p[3];
		}

		public vfloat4(float a)
		{
			this.m = new float[internalSize];
			this.m[0] = a;
			this.m[1] = a;
			this.m[2] = a;
			this.m[3] = a;
		}
	}

	// ============================================================================
	// vint4 data type
	// ============================================================================

	/**
	* @brief Data type for 4-wide ints.
	*/
	public struct vint4
	{
		/**
		* @brief Construct from 4 values loaded from an unaligned address.
		*
		* Consider using vint4::loada() which is better with wider VLA vectors
		* if data is aligned.
		*/
		public vint4(int[] p)
		{
			this.m = new int[4];
			this.m[0] = p[0];
			this.m[1] = p[1];
			this.m[2] = p[2];
			this.m[3] = p[3];
		}

		/**
		* @brief Construct from 4 uint8_t loaded from an unaligned address.
		*/
		public vint4(byte[] p)
		{
			this.m = new int[4];
			this.m[0] = p[0];
			this.m[1] = p[1];
			this.m[2] = p[2];
			this.m[3] = p[3];
		}

		/**
		* @brief Construct from 4 scalar values.
		*
		* The value of @c a is stored to lane 0 (LSB) in the SIMD register.
		*/
		public vint4(int a, int b, int c, int d)
		{
			this.m = new int[4];
			this.m[0] = a;
			this.m[1] = b;
			this.m[2] = c;
			this.m[3] = d;
		}


		/**
		* @brief Construct from 4 scalar values replicated across all lanes.
		*
		* Consider using vint4::zero() for constexpr zeros.
		*/
		public vint4(int a)
		{
			this.m = new int[4];
			this.m[0] = a;
			this.m[1] = a;
			this.m[2] = a;
			this.m[3] = a;
		}

		/**
		* @brief Get the scalar value of a single lane.
		*/
		public int lane(int l)
		{
			return this.m[l];
		}

		/**
		* @brief Set the scalar value of a single lane.
		*/
		public void set_lane(int l, int a)
		{
			this.m[l] = a;
		}

		/**
		* @brief Factory that returns a vector of zeros.
		*/
		public static vint4 zero()
		{
			return new vint4(0);
		}

		/**
		* @brief Factory that returns a vector containing the lane IDs.
		*/
		public static vint4 lane_id()
		{
			return new vint4(0, 1, 2, 3);
		}

		/**
		* @brief The vector ...
		*/
		private readonly int[] m;
	}
}