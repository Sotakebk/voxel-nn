namespace RealMode.Generation
{
    public static class DoubleExtensions
    {
        public static double To01Range(this double value)
        {
            return (value + 1.0) / 2.0;
        }
    }
}