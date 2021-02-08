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
			return p.r * q.r + p.g * q.g + p.b * q.b  + p.a * q.a;
		}
	}
}