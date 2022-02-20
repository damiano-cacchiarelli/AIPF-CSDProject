namespace AIPF.Utilities
{
    public static class NumberExtension
    {
        public static double GetValueOr(this double value, double otherValue)
        {
            if (double.IsNaN(value) || double.IsInfinity(value) || double.IsNegativeInfinity(value))
                return otherValue;
            return value;
        }
    }
}
