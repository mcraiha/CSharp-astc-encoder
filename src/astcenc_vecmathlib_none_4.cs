namespace ASTCEnc
{
	// ============================================================================
	// vfloat1 data type
	// ============================================================================

	/**
	* @brief Data type for 1-wide floats.
	*/
	public struct vfloat1
	{
		/**
		* @brief Construct from 1 value loaded from an unaligned address.
		*
		* Consider using loada() which is better with wider VLA vectors if data is
		* aligned to vector length.
		*/
		public vfloat1(vfloat1 p)
		{
			this.m = p.m;
		}

		/**
		* @brief Construct from 1 scalar value replicated across all lanes.
		*
		* Consider using zero() for constexpr zeros.
		*/
		public vfloat1(float a)
		{
			this.m = a;
		}

		/**
		* @brief Get the scalar value of a single lane.
		*/
		public float lane()
		{
			return this.m;
		}

		/**
		* @brief Factory that returns a vector of zeros.
		*/
		public static vfloat1 zero()
		{
			return new vfloat1(0.0f);
		}

		/**
		* @brief Factory that returns a vector containing the lane IDs.
		*/
		public static vfloat1 lane_id()
		{
			return new vfloat1(0.0f);
		}

		/**
		* @brief The vector ...
		*/
		public float m;
	}

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

		public vfloat4(float v0, float v1, float v2, float v3)
		{
			this.m = new float[internalSize];
			this.m[0] = v0;
			this.m[1] = v1;
			this.m[2] = v2;
			this.m[3] = v3;
		}

		public vfloat4(float a)
		{
			this.m = new float[internalSize];
			this.m[0] = a;
			this.m[1] = a;
			this.m[2] = a;
			this.m[3] = a;
		}

		/**
		* @brief Get the scalar value of a single lane.
		*/
		public float lane(int l)
		{
			return this.m[l];
		}

		/**
		* @brief Set the scalar value of a single lane.
		*/
		public void set_lane(int l, float a)
		{
			m[l] = a;
		}

		public static vfloat4 operator +(vfloat4 a, vfloat4 b)
		{
			return new vfloat4(a.m[0] + b.m[0],
	             a.m[1] + b.m[1],
	             a.m[2] + b.m[2],
	             a.m[3] + b.m[3]);
		}

		public static vfloat4 operator -(vfloat4 a, vfloat4 b)
		{
			return new vfloat4(a.m[0] - b.m[0],
	             a.m[1] - b.m[1],
	             a.m[2] - b.m[2],
	             a.m[3] - b.m[3]);
		}

		public static vfloat4 operator *(vfloat4 a, vfloat4 b)
		{
			return new vfloat4(a.m[0] * b.m[0],
	             a.m[1] * b.m[1],
	             a.m[2] * b.m[2],
	             a.m[3] * b.m[3]);
		}

		/**
		* @brief Overload: vector by scalar multiplication.
		*/
		public static vfloat4 operator*(vfloat4 a, float b)
		{
			return new vfloat4(a.m[0] * b,
						a.m[1] * b,
						a.m[2] * b,
						a.m[3] * b);
		}

		/**
		* @brief Overload: scalar by vector multiplication.
		*/
		public static vfloat4 operator*(float a, vfloat4 b)
		{
			return new vfloat4(a * b.m[0],
						a * b.m[1],
						a * b.m[2],
						a * b.m[3]);
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

		public static vint4 operator +(vint4 a, vint4 b)
		{
			return new vint4(a.m[0] + b.m[0],
	             a.m[1] + b.m[1],
	             a.m[2] + b.m[2],
	             a.m[3] + b.m[3]);
		}

		public static vint4 operator -(vint4 a, vint4 b)
		{
			return new vint4(a.m[0] - b.m[0],
	             a.m[1] - b.m[1],
	             a.m[2] - b.m[2],
	             a.m[3] - b.m[3]);
		}

		public static vint4 operator *(vint4 a, vint4 b)
		{
			return new vint4(a.m[0] * b.m[0],
	             a.m[1] * b.m[1],
	             a.m[2] * b.m[2],
	             a.m[3] * b.m[3]);
		}

		public static vint4 operator *(vint4 a, int b)
		{
			return new vint4(a.m[0] * b,
	             a.m[1] * b,
	             a.m[2] * b,
	             a.m[3] * b);
		}

		/**
		* @brief Logical shift right.
		*/
		public static vint4 lsr(int s, vint4 a)
		{
			return new vint4(a.m[0] >> s,
						a.m[1] >> s,
						a.m[2] >> s,
						a.m[3] >> s);
		}

		/**
		* @brief Return the min vector of two vectors.
		*/
		public static vint4 min(vint4 a, vint4 b)
		{
			return new vint4(a.m[0] < b.m[0] ? a.m[0] : b.m[0],
						a.m[1] < b.m[1] ? a.m[1] : b.m[1],
						a.m[2] < b.m[2] ? a.m[2] : b.m[2],
						a.m[3] < b.m[3] ? a.m[3] : b.m[3]);
		}

		/**
		* @brief Return the min vector of two vectors.
		*/
		public static vint4 max(vint4 a, vint4 b)
		{
			return new vint4(a.m[0] > b.m[0] ? a.m[0] : b.m[0],
						a.m[1] > b.m[1] ? a.m[1] : b.m[1],
						a.m[2] > b.m[2] ? a.m[2] : b.m[2],
						a.m[3] > b.m[3] ? a.m[3] : b.m[3]);
		}

		/**
		* @brief Return lanes from @c b if MSB of @c cond is set, else @c a.
		*/
		public static vint4 select(vint4 a, vint4 b, vmask4 cond)
		{
			return new vint4((cond.m[0] & 0x80000000) == 1 ? b.m[0] : a.m[0],
						(cond.m[1] & 0x80000000) == 1 ? b.m[1] : a.m[1],
						(cond.m[2] & 0x80000000) == 1 ? b.m[2] : a.m[2],
						(cond.m[3] & 0x80000000) == 1 ? b.m[3] : a.m[3]);
		}
	}

	// ============================================================================
	// vmask4 data type
	// ============================================================================

	/**
	* @brief Data type for 4-wide control plane masks.
	*/
	public struct vmask4
	{
		/**
		* @brief Construct from an existing mask value.
		*/
		public vmask4(int[] p)
		{
			m = new int[4];
			m[0] = p[0];
			m[1] = p[1];
			m[2] = p[2];
			m[3] = p[3];
		}

		/**
		* @brief Construct from 4 scalar values.
		*
		* The value of @c a is stored to lane 0 (LSB) in the SIMD register.
		*/
		public vmask4(int a, int b, int c, int d)
		{
			m = new int[4];
			m[0] = a;
			m[1] = b;
			m[2] = c;
			m[3] = d;
		}

		/**
		* @brief The vector ...
		*/
		public readonly int[] m;
	}
}