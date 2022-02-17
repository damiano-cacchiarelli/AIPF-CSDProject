using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Threading.Tasks;

namespace AIPF_Console.Utils
{
    public static class ProfilerHelper
    {
        private static DateTime lastTime;
        private static TimeSpan lastTotalProcessorTime;
        private static DateTime curTime;
        private static TimeSpan curTotalProcessorTime;

        public static Measurement<long> GetMemoryFree(Process process)
        {
            var usedMemory = 0L;
            if (!process.HasExited)
            {
                usedMemory = process.WorkingSet64;
            }

            return new Measurement<long>(usedMemory);
        }

        public static Measurement<double> GetCPUPercentage(Process process)
        {
            double CPUUsage = 0;
            if (!process.HasExited)
            {
                if (lastTime == null || lastTime == new DateTime())
                {
                    lastTime = DateTime.Now;
                    lastTotalProcessorTime = process.TotalProcessorTime;
                }
                else
                {
                    curTime = DateTime.Now;
                    curTotalProcessorTime = process.TotalProcessorTime;

                    CPUUsage = (curTotalProcessorTime.TotalMilliseconds - lastTotalProcessorTime.TotalMilliseconds) / curTime.Subtract(lastTime).TotalMilliseconds / Convert.ToDouble(Environment.ProcessorCount);
                    CPUUsage *= 100;
                    //Console.WriteLine("{0} CPU: {1:0.0}%", process.ProcessName, CPUUsage);

                    lastTime = curTime;
                    lastTotalProcessorTime = curTotalProcessorTime;
                }
            }

            return new Measurement<double>(CPUUsage);
        }

        public static Measurement<double> GetProcessCpuTime(Process process)
        {
            if (!process.HasExited)
            {
                return new Measurement<double>(process.TotalProcessorTime.TotalMilliseconds);
            }

            return default;
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

        public static Task CheckCPU(Process process, Histogram<float> cpuHistogram)
        {
            //var cpuCounter = MyMeter.CreateHistogram<double>("CPU.percentage.counter");
            //var ramCounter = MyMeter.CreateCounter<Measurement<double>>("RAM.percentage.counter");

            return Task.Run(() =>
            {
                bool done = false;
                PerformanceCounter total_cpu = new PerformanceCounter("Process", "% Processor Time", "_Total");
                PerformanceCounter process_cpu = new PerformanceCounter("Process", "% Processor Time", process.ProcessName);
                while (!done)
                {
                    float t = total_cpu.NextValue();
                    float p = process_cpu.NextValue();
                    //Console.WriteLine(String.Format("_Total = {0}  App = {1} {2}%\n", t, p, p / t * 100));

                    cpuHistogram.Record(p);

                    System.Threading.Thread.Sleep(1000);
                }
            });
        }
    }
}
