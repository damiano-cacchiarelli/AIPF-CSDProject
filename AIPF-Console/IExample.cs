using Spectre.Console;
using System;
using System.IO;
using System.Threading.Tasks;

namespace AIPF_Console
{
    public interface IExample
    {
        static readonly Random random = new Random();
        static string Dir => Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName;

        public string Name { get; }

        public Task Train(int? numberOfIterations = null, int? numberOfElementsForTrain = null);

        public Task Predict(PredictionMode predictionMode = PredictionMode.USER_VALUE, int error = 0);

        public Task Metrics(int? numberOfElementsForEvaluate = null);

        public async Task OpenTelemetry(TaskConfig taskConfig = null)
        {
            bool inserted = taskConfig != null;
            bool wantToInsert = false;

            if(!inserted)
            {
                taskConfig = new TaskConfig();
                wantToInsert = AnsiConsole.Confirm("Do you want to insert the task configuration?", false);
            }
            if (wantToInsert)
            {
                taskConfig.NumberOfIterations = AnsiConsole.Ask("Number of iterations during the training phase ", 1);
                taskConfig.NumberOfElementsForTrain = AnsiConsole.Ask<int?>("Number of elements used for train (null means all) ", null);
            }

            await Train(taskConfig.NumberOfIterations, taskConfig.NumberOfElementsForTrain);

            if (wantToInsert)
            {
                taskConfig.NumberOfPredictions = AnsiConsole.Ask("Number of predictions ", 100);
                taskConfig.PercentageOfIncorrectPredictions = AnsiConsole.Ask("Percentage of incorrect predictions ", 0);
                taskConfig.MinMillisecondsToSleepBetweenPredictions = AnsiConsole.Ask("Minimum milliseconds to wait between two predictions ", 0);
                do
                {
                    taskConfig.MaxMillisecondsToSleepBetweenPredictions = AnsiConsole.Ask("Maximum milliseconds to wait between two predictions (must be greater or equals than the minimum)", 0);
                } while (taskConfig.MaxMillisecondsToSleepBetweenPredictions < taskConfig.MinMillisecondsToSleepBetweenPredictions);
            }
            for (int i = 0; i < taskConfig.NumberOfPredictions; i++)
            {
                try
                {
                    await Predict(PredictionMode.RANDOM_VALUE, taskConfig.PercentageOfIncorrectPredictions);
                    var del = random.Next(taskConfig.MinMillisecondsToSleepBetweenPredictions, taskConfig.MaxMillisecondsToSleepBetweenPredictions);
                    AnsiConsole.WriteLine($"Prediction number: {i}/{taskConfig.NumberOfPredictions}, delay: {del}\n");
                    await Task.Delay(del);
                }
                catch (Exception ex) { AnsiConsole.WriteException(ex); }
            }

            if (wantToInsert)
            {
                taskConfig.NumberOfElementsForEvaluate = AnsiConsole.Ask<int?>("Number of elements used for evaluate the model (null means all) ", null);
            }
            await Metrics(taskConfig.NumberOfElementsForEvaluate);
        }
    }

    public enum PredictionMode {
        DEFAULT_VALUE,
        RANDOM_VALUE,
        USER_VALUE
    }

    public class TaskConfig
    {
        public int NumberOfIterations = 1;
        public int? NumberOfElementsForTrain = null;

        public int NumberOfPredictions = 100;
        public int PercentageOfIncorrectPredictions = 0;
        public int MinMillisecondsToSleepBetweenPredictions = 0;
        public int MaxMillisecondsToSleepBetweenPredictions = 0;

        public int? NumberOfElementsForEvaluate = null;

        public override string ToString()
        {
            return $"NumberOfIterations={NumberOfIterations}, " +
                   $"NumberOfElementsForTrain={NumberOfElementsForTrain}, " +
                   $"NumberOfPredictions={NumberOfPredictions}, " +
                   $"PercentageOfIncorrectPredictions={PercentageOfIncorrectPredictions}, " +
                   $"MinMillisecondsToSleepBetweenPredictions={MinMillisecondsToSleepBetweenPredictions}, " +
                   $"MaxMillisecondsToSleepBetweenPredictions={MaxMillisecondsToSleepBetweenPredictions}, "+
                   $"NumberOfElementsForEvaluate={NumberOfElementsForEvaluate}";
        }
    }
}
