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

	public class Float4
	{
		public float r;
		public float g;
		public float b;
		public float a;

		public Float4()
		{
			this.r = default;
			this.g = default;
			this.b = default;
			this.a = default;
		}

		public Float4(float p)
		{
			this.r = p;
			this.g = p;
			this.b = p;
			this.a = p;
		}

		public Float4(float p, float q, float s, float t)
		{
			this.r = p;
			this.g = q;
			this.b = s;
			this.a = t;
		}

		public Float4(Float4 p)
		{
			this.r = p.r;
			this.g = p.g;
			this.b = p.b;
			this.a = p.a;
		}

		public static float dot(Float4 p, Float4 q)
		{
			return p.r * q.r + p.g * q.g + p.b * q.b + p.a * q.a;
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
	};

	// parametric line, 3D
	public struct Line3
	{
		public Float3 a;
		public Float3 b;
	};

	public struct Line4
	{
		public Float4 a;
		public Float4 b;
	};


	public struct ProcessedLine2
	{
		public Float2 amod;
		public Float2 bs;
		public Float2 bis;
	};

	public struct ProcessedLine3
	{
		public Float3 amod;
		public Float3 bs;
		public Float3 bis;
	};

	public struct ProcessedLine4
	{
		public Float4 amod;
		public Float4 bs;
		public Float4 bis;
	};

	public static class ASTCMath
	{
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
	}
}