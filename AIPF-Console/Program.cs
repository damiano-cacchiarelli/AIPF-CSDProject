using AIPF_Console.MNIST_example;
using AIPF_Console.RobotLoccioni_example;
using AIPF_Console.TaxiFare_example;
using Spectre.Console;
using System;
using System.Collections.Generic;

namespace AIPF_Console
{
    class Program
    {
        private static IExample example = null;
        private static readonly Dictionary<string, Action<IExample>> Commands = new Dictionary<string, Action<IExample>>()
            {
                { "fit", e => e.Train() },
                { "predict", e => e.Predict() },
                { "metrics", e => e.Metrics() },
                { "back", _ => example = null },
                { "exit", _ => { } },
            };

        static void Main(string[] args)
        {
            string line = string.Empty;

            while (!line.Equals("exit"))
            {
                if (example == null)
                {
                    example = SelectExample();
                }
                line = DefaultText();

                try
                {
                    Commands[line].Invoke(example);
                }
                catch (Exception ex)
                {
                    AnsiConsole.WriteException(ex);
                }
                if (line.Equals("exit")) break;

                if (!line.Equals("back"))
                {
                    AnsiConsole.WriteLine();
                    AnsiConsole.Write(new Rule("--").RuleStyle("blue").Centered());

                    if (!AnsiConsole.Confirm("Continue?"))
                        break;
                }
                AnsiConsole.Clear();
            }
        }

        private static IExample SelectExample()
        {
            var examples = new Dictionary<string, Func<IExample>>()
            {
                { "mnist", () => Mnist.Start() },
                { "taxi-fare", () => TaxiFare.Start() },
                { "robot-loccioni", () => RobotLoccioni.Start() }
            };

            var command = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Select the command you what to do")
                    .PageSize(examples.Count)
                    .MoreChoicesText("[grey](Move up and down to reveal more options)[/]")
                    .AddChoices(examples.Keys));

            return examples[command].Invoke();
        }

        private static string DefaultText()
        {
            AnsiConsole.Write(
                new FigletText("AIPF - " + example.GetName())
                    .Centered()
                    .Color(Color.Blue));

            AnsiConsole.Write(new Rule("[bold white]Cacchiarelli, Cesetti, Romagnoli 10/01/2022[/]").RuleStyle("blue").Centered());
            AnsiConsole.WriteLine();

            return AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Select the command you what to do")
                    .PageSize(Commands.Count)
                    .MoreChoicesText("[grey](Move up and down to reveal more options)[/]")
                    .AddChoices(Commands.Keys));
        }
    }
}
