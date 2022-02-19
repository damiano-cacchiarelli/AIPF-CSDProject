using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace AIPF_Console.Utils
{
    public static class ProfilerHelper
    {
        private static PerformanceCounter CpuCounter = new PerformanceCounter("Process", "% Processor Time", Process.GetCurrentProcess().ProcessName, true);

        public static Measurement<double> GetCPUUsage(float multipler)
        {
            return new Measurement<double>(CpuCounter.NextValue() * multipler);
        }

        public static Measurement<double> GetMemoryUsage()
        {
            return new Measurement<double>(Process.GetCurrentProcess().WorkingSet64);
        }

        public static IEnumerable<Measurement<double>> GetThreadCpuTime(Process process)
        {
            if (!process.HasExited)
            {
                foreach (ProcessThread thread in process.Threads)
                {
                    Measurement<double> m = default;
                    try
                    {
                        m = new Measurement<double>(thread.TotalProcessorTime.TotalMilliseconds, new KeyValuePair<string, object?>("ProcessId", process.Id),
                                new KeyValuePair<string, object?>("ThreadId", thread.Id));
                    }
                    catch (Exception) { }

                    yield return m;

                }
            }
        }

        public static IEnumerable<Measurement<int>> GetThreadState(Process process)
        {
            foreach (ProcessThread thread in process.Threads)
            {
                Measurement<int> m = default;
                try
                {
                    m = new Measurement<int>((int)thread.ThreadState, new KeyValuePair<string, object?>("ProcessId", process.Id),
                    new KeyValuePair<string, object?>("ThreadId", thread.Id));
                }
                catch (Exception) { }

                yield return m;
            }
        }
    }
}
