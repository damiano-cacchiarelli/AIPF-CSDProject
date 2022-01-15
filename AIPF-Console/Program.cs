using AIPF_Console.MNIST_example;
using AIPF_Console.TaxiFare_example;
using Spectre.Console;
using System;

namespace AIPF_Console
{
    class Program
    {

        static void Main(string[] args)
        {
            IExample example = null;
            string line = string.Empty;

            while (!line.Equals("exit"))
            {
                if (example == null) {
                 example = SelectExample();
                    if (example == null) continue;
                }
                line = DefaultText(example);

                try
                {
                    switch (line)
                    {
                        case "fit":
                            example.Train();
                            AnsiConsole.WriteLine();
                            break;
                        case "predict":
                            example.Predict();
                            AnsiConsole.WriteLine();
                            break;
                        case "metrics":
                            example.Metrics();
                            AnsiConsole.WriteLine();
                            break;
                        case "exit":
                            break;
                        default:
                            AnsiConsole.WriteLine("[red]Command not found![/]");
                            break;
                    }
                }
                catch (Exception ex)
                {
                    AnsiConsole.WriteException(ex);
                }
                if (line.Equals("exit")) break;

                AnsiConsole.Write(new Rule("--").RuleStyle("blue").Centered());

                if (!AnsiConsole.Confirm("Continue?"))
                    break;
                AnsiConsole.Clear();
            }
        }

        private static IExample SelectExample()
        {
            var commands = new string[] { "mnist", "taxi-fare" };

            var command = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Select the command you what to do")
                    .PageSize(commands.Length + 1)
                    .MoreChoicesText("[grey](Move up and down to reveal more options)[/]")
                    .AddChoices(commands));
            switch (command)
            {
                case "mnist":
                    return Mnist.Start();
                case "taxi-fare":
                    return TaxiFare.Start();
                default:
                    AnsiConsole.WriteLine("[red]Command not found![/]");
                    return null;
            }
            
        }

        private static string DefaultText(IExample example)
        {
            var commands = new string[] { "fit", "predict", "metrics", "exit" };

            AnsiConsole.Write(
                new FigletText("AIPF - " + example.GetName())
                    .Centered()
                    .Color(Color.Blue));
            AnsiConsole.Write(new Rule("[bold white]Cacchiarelli, Cesetti, Romagnoli 10/01/2022[/]").RuleStyle("blue").Centered());
            AnsiConsole.WriteLine();

            return AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Select the command you what to do")
                    .PageSize(commands.Length)
                    .MoreChoicesText("[grey](Move up and down to reveal more options)[/]")
                    .AddChoices(commands));
        }
    }
}
