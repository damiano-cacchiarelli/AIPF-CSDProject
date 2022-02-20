using AIPF.Utilities;
using System;
using System.Diagnostics;

namespace AIPF.Telemetry
{
    public static class ActivityExtension
    {
        public static void AddTags<I>(this Activity activity, string tagPrefix, I instance)
        {
            Array.ForEach(
                typeof(I).GetProperties(),
                p =>
                {
                    object v = p.GetValue(instance);
                    if (long.TryParse(v.ToString(), out long l))
                    {
                        activity?.AddTag($"{tagPrefix}.{p.Name}", l);
                    }
                    else if (double.TryParse(v.ToString(), out double d))
                    {
                        activity?.AddTag($"{tagPrefix}.{p.Name}", d.GetValueOr(0));
                    }
                    else if (bool.TryParse(v.ToString(), out bool b))
                    {
                        activity?.AddTag($"{tagPrefix}.{p.Name}", b);
                    }
                    else if (char.TryParse(v.ToString(), out char c))
                    {
                        activity?.AddTag($"{tagPrefix}.{p.Name}", c);
                    }
                    else
                    {
                        activity?.AddTag($"{tagPrefix}.{p.Name}", v);
                    }
                }
            );
        }
    }
}