using System;

namespace ASTCEnc
{
	public class Float2
	{
		public float r;
		public float g;

		public Float2()
		{
			this.r = default;
			this.g = default;
		}

		public Float2(float p)
		{
			this.r = p;
			this.g = p;
		}

		public Float2(float p, float q)
		{
			this.r = p;
			this.g = q;
		}

		public Float2(Float2 p)
		{
			this.r = p.r;
			this.g = p.g;
		}

		public static float dot(Float2 p, Float2 q)
		{
			return p.r * q.r + p.g * q.g;
		}
	}

	public class Float3
	{
		public float r;
		public float g;
		public float b;

		public Float3()
		{
			this.r = default;
			this.g = default;
			this.b = default;
		}

		public Float3(float p)
		{
			this.r = p;
			this.g = p;
			this.b = p;
		}

		public Float3(float p, float q, float s)
		{
			this.r = p;
			this.g = q;
			this.b = s;
		}

		public Float3(Float3 p)
		{
			this.r = p.r;
			this.g = p.g;
			this.b = p.b;
		}

		public static float dot(Float3 p, Float3 q)
		{
			return p.r * q.r + p.g * q.g + p.b * q.b;
		}
	}


	/*********************************
	Declaration of line types
	*********************************/
	// parametric line, 2D: The line is given by line = a + b * t.

	public struct Line2
	{
		public Float2 a;
		public Float2 b;

		public Line2(Float2 aa, Float2 bb)
		{
			a = aa;
			b = bb;
		}
	};

	// parametric line, 3D
	public struct Line3
	{
		public vfloat4 a;
		public vfloat4 b;
	};

	public struct Line4
	{
		public vfloat4 a;
		public vfloat4 b;

		public Line4(vfloat4 aa, vfloat4 bb)
		{
			this.a = aa;
			this.b = bb;
		}
	};


	public struct ProcessedLine2
	{
		public Float2 amod;
		public Float2 bs;
	};

	public struct ProcessedLine3
	{
		public vfloat4 amod;
		public vfloat4 bs;
	};

	public struct ProcessedLine4
	{
		public vfloat4 amod;
		public vfloat4 bs;
	};

	public static class ASTCMath
	{
		/*public static T min<T>(T p, T q)
		{
			return p < q ? p : q;
		}*/

		public static byte min(byte p, byte q)
		{
			return p < q ? p : q;
		}

		public static byte min(byte p, byte q, byte r)
		{
			return min(min(p, q), r);
		}

		public static byte min(byte p, byte q, byte r, byte s)
		{
			return min(min(p, q), min(r, s));
		}

		public static uint min(uint p, uint q)
		{
			return p < q ? p : q;
		}

		public static uint min(uint p, uint q, uint r)
		{
			return min(min(p, q), r);
		}

		public static uint min(uint p, uint q, uint r, uint s)
		{
			return min(min(p, q), min(r, s));
		}

		public static int min(int p, int q)
		{
			return p < q ? p : q;
		}

		public static int min(int p, int q, int r)
		{
			return min(min(p, q), r);
		}

		public static int min(int p, int q, int r, int s)
		{
			return min(min(p, q), min(r, s));
		}

		public static float min(float p, float q)
		{
			return p < q ? p : q;
		}

		public static uint max(uint p, uint q)
		{
			return p > q ? p : q;
		}

		public static int max(int p, int q)
		{
			return p > q ? p : q;
		}

		public static float max(float p, float q)
		{
			return p > q ? p : q;
		}

		/**
		* @brief Clamp a value value between @c mn and @c mx.
		*
		* For floats, NaNs are turned into @c mn.
		*
		* @param v      The value to clamp.
		* @param mn     The min value (inclusive).
		* @param mx     The max value (inclusive).
		*
		* @return The clamped value.
		*/
		public static int clamp(int v, int mn, int mx)
		{
			// Do not reorder; correct NaN handling relies on the fact that comparison
			// with NaN returns false and will fall-though to the "min" value.
			if (v > mx) return mx;
			if (v > mn) return v;
			return mn;
		}

		public static uint clamp(uint v, uint mn, uint mx)
		{
			// Do not reorder; correct NaN handling relies on the fact that comparison
			// with NaN returns false and will fall-though to the "min" value.
			if (v > mx) return mx;
			if (v > mn) return v;
			return mn;
		}

		public static float clamp(float v, float mn, float mx)
		{
			// Do not reorder; correct NaN handling relies on the fact that comparison
			// with NaN returns false and will fall-though to the "min" value.
			if (v > mx) return mx;
			if (v > mn) return v;
			return mn;
		}

		public static float clamp1f(float value)
		{
			return (value < 0.0f) ? 0.0f : (value > 1.0f) ? 1.0f : value; 
		}

		/**
		* @brief Clamp a float value between 0.0f and 255.0f.
		*
		* NaNs are turned into 0.0f.
		*
		* @param v  The value to clamp.
		*
		* @return The clamped value.
		*/
		public static float clamp255f(float value)
		{
			return (value < 0.0f) ? 0.0f : (value > 255.0f) ? 255.0f : value; 
		}

		/**
		* @brief Clamp a float value between 0.0f and 65504.0f.
		*
		* NaNs are turned into 0.0f.
		*
		* @param v   The value to clamp
		*
		* @return The clamped value
		*/
		public static float clamp64Kf(float value)
		{
			return (value < 0.0f) ? 0.0f : (value > 65504.0f) ? 65504.0f : value; 
		}


		/**
		* @brief SP float round-to-nearest.
		*
		* @param v   The value to round.
		*
		* @return The rounded value.
		*/
		public static float flt_rte(float v)
		{
			return (float)Math.Floor(v + 0.5f);
		}

		/**
		* @brief SP float round-down.
		*
		* @param v   The value to round.
		*
		* @return The rounded value.
		*/
		public static float flt_rd(float v)
		{
			return (float)Math.Floor(v);
		}

		/**
		* @brief SP float round-to-nearest and convert to integer.
		*
		* @param v   The value to round.
		*
		* @return The rounded value.
		*/
		public static int flt2int_rtn(float v)
		{

			return (int)(v + 0.5f);
		}

		/**
		* @brief SP float round down and convert to integer.
		*
		* @param v   The value to round.
		*
		* @return The rounded value.
		*/
		public static int flt2int_rd(float v)
		{
			return (int)(v);
		}

		/**
		* @brief Population bit count.
		*
		* @param v   The value to population count.
		*
		* @return The number of 1 bits.
		*/
		public static int popcount(ulong v)
		{
			ulong mask1 = 0x5555555555555555;
			ulong mask2 = 0x3333333333333333;
			ulong mask3 = 0x0F0F0F0F0F0F0F0F;
			v -= (v >> 1) & mask1;
			v = (v & mask2) + ((v >> 2) & mask2);
			v += v >> 4;
			v &= mask3;
			v *= 0x0101010101010101;
			v >>= 56;
			return (int)v;
		}
	}
}