namespace ASTCEnc
{
	public struct vfloat4
	{
		private const int internalSize = 4;
		public float[] m;

		public vfloat4()
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
}