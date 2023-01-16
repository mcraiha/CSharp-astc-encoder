namespace ASTCEnc
{
    /// <summary>
    /// Common utils to avoid some C++ -> C# translation issues
    /// </summary>
    public static class Utils
    {
        public static T[] CreateAndInitArray<T>(int len, T[] initialValues)
        {
            var temp = new T[len];
            initialValues.CopyTo(temp, 0);
            return temp;
        }
    }
}