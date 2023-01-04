global using vfloat = ASTCEnc.vfloat4;
global using vfloatacc = ASTCEnc.vfloat4;
global using vint = ASTCEnc.vint4;
global using vmask = ASTCEnc.vmask4;
// ASTCENC_SIMD_WIDTH is defined in 

using System;



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
		public float[] m = new float[4];

		public vfloat4(float[] p)
		{
			this.m[0] = p[0];
			this.m[1] = p[1];
			this.m[2] = p[2];
			this.m[3] = p[3];
		}

		public vfloat4(float v0, float v1, float v2, float v3)
		{
			this.m[0] = v0;
			this.m[1] = v1;
			this.m[2] = v2;
			this.m[3] = v3;
		}

		public vfloat4(float a)
		{
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

		public vfloat4 swz2(int l0, int l1)
		{
			return new vfloat4(this.lane(l0), this.lane(l1), 0.0f, 0.0f);
		}

		public vfloat4 swz3(int l0, int l1, int l2)
		{
			return new vfloat4(this.lane(l0), this.lane(l1), this.lane(l2), 0.0f);
		}

		public vfloat4 swz4(int l0, int l1, int l2, int l3)
		{
			return new vfloat4(this.lane(l0), this.lane(l1), this.lane(l2), this.lane(l3));
		}

		/**
		* @brief Factory that returns a vector of zeros.
		*/
		public static vfloat4 zero()
		{
			return new vfloat4(0.0f);
		}

		/**
		* @brief Return the horizontal sum of a vector.
		*/
		public static float hadd_s(vfloat4 a)
		{
			// Use halving add, gives invariance with SIMD versions
			return (a.m[0] + a.m[2]) + (a.m[1] + a.m[3]);
		}

		/**
		* @brief Return the dot product for the full 4 lanes, returning scalar.
		*/
		public static float dot_s(vfloat4 a, vfloat4 b)
		{
			return a.m[0] * b.m[0] +
				a.m[1] * b.m[1] +
				a.m[2] * b.m[2] +
				a.m[3] * b.m[3];
		}

		/**
		* @brief Return the dot product for the full 4 lanes, returning vector.
		*/
		public static vfloat4 dot(vfloat4 a, vfloat4 b)
		{
			return new vfloat4(dot_s(a, b));
		}

		/**
		* @brief Return the dot product for first 3 lanes, returning scalar.
		*/
		public static float dot3_s(vfloat4 a, vfloat4 b)
		{
			return a.m[0] * b.m[0] +
				a.m[1] * b.m[1] +
				a.m[2] * b.m[2];
		}

		/**
		* @brief Return the dot product for first 3 lanes, returning vector.
		*/
		public static vfloat4 dot3(vfloat4 a, vfloat4 b)
		{
			float d3 = dot3_s(a, b);
			return new vfloat4(d3, d3, d3, 0.0f);
		}

		public static vfloat4 vfloat3(float a, float b, float c)
		{
			return new vfloat4(a, b, c, 0.0f);
		}

		public static vfloat4 vfloat2(float a, float b)
		{
			return new vfloat4(a, b, 0.0f, 0.0f);
		}

		/**
		* @brief Return lanes from @c b if MSB of @c cond is set, else @c a.
		*/
		public static vfloat4 select(vfloat4 a, vfloat4 b, vmask4 cond)
		{
			return new vfloat4((cond.m[0] & 0x80000000) == 1 ? b.m[0] : a.m[0],
						(cond.m[1] & 0x80000000) == 1 ? b.m[1] : a.m[1],
						(cond.m[2] & 0x80000000) == 1 ? b.m[2] : a.m[2],
						(cond.m[3] & 0x80000000) == 1 ? b.m[3] : a.m[3]);
		}

		/**
		* @brief Return the absolute value of the float vector.
		*/
		public static vfloat4 abs(vfloat4 a)
		{
			return new vfloat4(Math.Abs(a.m[0]),
						Math.Abs(a.m[1]),
						Math.Abs(a.m[2]),
						Math.Abs(a.m[3]));
		}

		/**
		* @brief Return the min vector of two vectors.
		*
		* If either lane value is NaN, @c b will be returned for that lane.
		*/
		public static vfloat4 min(vfloat4 a, vfloat4 b)
		{
			return new vfloat4(a.m[0] < b.m[0] ? a.m[0] : b.m[0],
						a.m[1] < b.m[1] ? a.m[1] : b.m[1],
						a.m[2] < b.m[2] ? a.m[2] : b.m[2],
						a.m[3] < b.m[3] ? a.m[3] : b.m[3]);
		}

		/**
		* @brief Return the max vector of two vectors.
		*
		* If either lane value is NaN, @c b will be returned for that lane.
		*/
		public static vfloat4 max(vfloat4 a, vfloat4 b)
		{
			return new vfloat4(a.m[0] > b.m[0] ? a.m[0] : b.m[0],
						a.m[1] > b.m[1] ? a.m[1] : b.m[1],
						a.m[2] > b.m[2] ? a.m[2] : b.m[2],
						a.m[3] > b.m[3] ? a.m[3] : b.m[3]);
		}

		/**
		* @brief Return the horizontal minimum of a vector.
		*/
		public static float hmin_s(vfloat4 a)
		{
			return hmin(a).lane(0);
		}

		/**
		* @brief Return the horizontal min of RGB vector lanes as a scalar.
		*/
		public static float hmin_rgb_s(vfloat4 a)
		{
			a.set_lane(3, a.lane(0));
			return hmin_s(a);
		}

		/**
		* @brief Return the horizontal maximum of a vector.
		*/
		public static float hmax_s(vfloat4 a)
		{
			return hmax(a).lane(0);
		}

		/**
		* @brief Return the horizontal minimum of a vector.
		*/
		public static vfloat4 hmin(vfloat4 a)
		{
			float tmp1 = Math.Min(a.m[0], a.m[1]);
			float tmp2 = Math.Min(a.m[2], a.m[3]);
			return new vfloat4(Math.Min(tmp1, tmp2));
		}

		public static void haccumulate(vfloat4 accum, vfloat4 a)
		{
			accum = accum + a;
		}

		public static void haccumulate(vfloat4 accum, vfloat4 a, vmask4 m)
		{
			a = select(vfloat4.zero(), a, m);
			haccumulate(accum, a);
		}

		/**
		* @brief Return the horizontal maximum of a vector.
		*/
		public static vfloat4 hmax(vfloat4 a)
		{
			float tmp1 = Math.Max(a.m[0], a.m[1]);
			float tmp2 = Math.Max(a.m[2], a.m[3]);
			return new vfloat4(Math.Max(tmp1, tmp2));
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

		public static vfloat4 operator/(vfloat4 a, vfloat4 b)
		{
			return new vfloat4(a.m[0] / b.m[0],
						a.m[1] / b.m[1],
						a.m[2] / b.m[2],
						a.m[3] / b.m[3]);
		}

		public static vfloat4 operator/(vfloat4 a, float b)
		{
			return a / new vfloat4(b);
		}

		public static vfloat4 operator/(float a, vfloat4 b)
		{
			return new vfloat4(a) / b;
		}

		public static vmask4 operator<(vfloat4 a, vfloat4 b)
		{
			return new vmask4(a.m[0] < b.m[0],
						a.m[1] < b.m[1],
						a.m[2] < b.m[2],
						a.m[3] < b.m[3]);
		}

		public static vmask4 operator>(vfloat4 a, vfloat4 b)
		{
			return new vmask4(a.m[0] > b.m[0],
						a.m[1] > b.m[1],
						a.m[2] > b.m[2],
						a.m[3] > b.m[3]);
		}

		public static vmask4 operator<=(vfloat4 a, vfloat4 b)
		{
			return new vmask4(a.m[0] <= b.m[0],
						a.m[1] <= b.m[1],
						a.m[2] <= b.m[2],
						a.m[3] <= b.m[3]);
		}

		public static vmask4 operator>=(vfloat4 a, vfloat4 b)
		{
			return new vmask4(a.m[0] >= b.m[0],
						a.m[1] >= b.m[1],
						a.m[2] >= b.m[2],
						a.m[3] >= b.m[3]);
		}

		public static vfloat4 loada(float[] p, uint offset)
		{
			return new vfloat4(p[offset], p[offset + 1], p[offset + 2], p[offset + 3]);
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
			this.m[0] = p[0];
			this.m[1] = p[1];
			this.m[2] = p[2];
			this.m[3] = p[3];
		}

		public vint4(byte[] p, uint offset)
		{
			this.m[0] = p[offset + 0];
			this.m[1] = p[offset + 1];
			this.m[2] = p[offset + 2];
			this.m[3] = p[offset + 3];
		}

		/**
		* @brief Construct from 4 scalar values.
		*
		* The value of @c a is stored to lane 0 (LSB) in the SIMD register.
		*/
		public vint4(int a, int b, int c, int d)
		{
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
		* @brief Return the horizontal sum of vector lanes as a scalar.
		*/
		public static int hadd_s(vint4 a)
		{
			return a.m[0] + a.m[1] + a.m[2] + a.m[3];
		}

		/**
		* @brief The vector ...
		*/
		private readonly int[] m = new int[4];

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
		* @brief Overload: vector by vector equality.
		*/
		public static vmask4 operator==(vint4 a, vint4 b)
		{
			return new vmask4(a.m[0] == b.m[0],
								a.m[1] == b.m[1],
								a.m[2] == b.m[2],
								a.m[3] == b.m[3]);
		}

		/**
		* @brief Overload: vector by vector inequality.
		*/
		public static vmask4 operator!=(vint4 a, vint4 b)
		{
			return new vmask4(a.m[0] != b.m[0],
								a.m[1] != b.m[1],
								a.m[2] != b.m[2],
								a.m[3] != b.m[3]);
		}

		public static vmask4 operator<(vint4 a, vint4 b)
		{
			return new vmask4(a.m[0] < b.m[0],
						a.m[1] < b.m[1],
						a.m[2] < b.m[2],
						a.m[3] < b.m[3]);
		}

		public static vmask4 operator>(vint4 a, vint4 b)
		{
			return new vmask4(a.m[0] > b.m[0],
						a.m[1] > b.m[1],
						a.m[2] > b.m[2],
						a.m[3] > b.m[3]);
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
			m[0] = p[0];
			m[1] = p[1];
			m[2] = p[2];
			m[3] = p[3];
		}

		public vmask4(bool a)
		{
			m[0] = a == false ? 0 : -1;
			m[1] = a == false ? 0 : -1;
			m[2] = a == false ? 0 : -1;
			m[3] = a == false ? 0 : -1;
		}

		public vmask4(bool a, bool b, bool c, bool d)
		{
			m[0] = a == false ? 0 : -1;
			m[1] = b == false ? 0 : -1;
			m[2] = c == false ? 0 : -1;
			m[3] = d == false ? 0 : -1;
		}

		/**
		* @brief The vector ...
		*/
		public readonly int[] m = new int[4];

		public static int mask(vmask4 a)
		{
			return ((a.m[0] >> 31) & 0x1) |
				((a.m[1] >> 30) & 0x2) |
				((a.m[2] >> 29) & 0x4) |
				((a.m[3] >> 28) & 0x8);
		}

		public static bool any(vmask4 a)
		{
			return mask(a) != 0;
		}

		public static bool all(vmask4 a)
		{
			return mask(a) == 0xF;
		}

		public static vmask4 operator|(vmask4 a, vmask4 b)
		{
			return new vmask4(a.m[0] | b.m[0],
						a.m[1] | b.m[1],
						a.m[2] | b.m[2],
						a.m[3] | b.m[3]);
		}

		public static vmask4 operator&(vmask4 a, vmask4 b)
		{
			return new vmask4(a.m[0] & b.m[0],
						a.m[1] & b.m[1],
						a.m[2] & b.m[2],
						a.m[3] & b.m[3]);
		}

		public static vmask4 operator^(vmask4 a, vmask4 b)
		{
			return new vmask4(a.m[0] ^ b.m[0],
						a.m[1] ^ b.m[1],
						a.m[2] ^ b.m[2],
						a.m[3] ^ b.m[3]);
		}

		public static vmask4 operator~(vmask4 a)
		{
			return new vmask4(~a.m[0],
						~a.m[1],
						~a.m[2],
						~a.m[3]);
		}

		public static vmask4 operator<(vfloat4 a, vfloat4 b)
		{
			return new vmask4(a.m[0] < b.m[0],
						a.m[1] < b.m[1],
						a.m[2] < b.m[2],
						a.m[3] < b.m[3]);
		}
	}
}