using AIPF.Models.Taxi;
using AIPF_Console.TaxiFare_example;
using Spectre.Console;
using System;

namespace AIPF_Console
{
    class Program
    {

        static void Main(string[] args)
        {
            var e = TaxiFare.Start();

            string line = string.Empty;
            while (!line.Equals("exit"))
            {
                line = defaultText();

                try
                {
                    switch (line)
                    {
                        case "fit":
                            e.train();
                            AnsiConsole.WriteLine();
                            break;
                        case "predict":
                            e.predict();
                            AnsiConsole.WriteLine();
                            break;
                        case "metrics":
                            e.metrics();
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

        private static string defaultText()
        {
            var commands = new string[] { "fit", "predict", "metrics", "exit" };

            AnsiConsole.Write(
                new FigletText("AIPF")
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
