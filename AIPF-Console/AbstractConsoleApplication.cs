using AIPF_Console.MNIST_example;
using AIPF_Console.RobotLoccioni_example;
using AIPF_Console.TaxiFare_example;
using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AIPF_Console
{
    public abstract class AbstractConsoleApplication
    {   
        protected static IExample example = null;
        protected readonly Dictionary<string, Func<IExample, Task>> Commands = new Dictionary<string, Func<IExample, Task>>()
            {
                { "fit", async e => await e.Train() },
                { "predict", async e => await e.Predict() },
                { "metrics", async e => await e.Metrics() },
                { "opentelemetry", async e => await e.OpenTelemetry() },
                { "back", _ => { example = null; return Task.CompletedTask; } },
                { "exit", _ => Task.CompletedTask },
            };
        protected readonly Dictionary<string, Func<IExample>> Examples = new Dictionary<string, Func<IExample>>()
            {
                { "mnist-default", () => MnistDefault.Start() },
                { "mnist-custom", () => MnistCustom.Start() },
                { "taxi-fare-linear", () => TaxiFareLinear.Start() },
                { "taxi-fare-huber", () => TaxiFareHuber.Start() },
                { "taxi-fare-pca-linear", () => TaxiFarePcaLinear.Start() },
                { "taxi-fare-pca-huber", () => TaxiFarePcaHuber.Start() },
                { "robot-loccioni", () => RobotLoccioni.Start() }
            };

        public abstract Task Start();

        protected virtual IExample SelectExample()
        {
            var command = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Select the command you what to do")
                    .PageSize(Examples.Count)
                    .MoreChoicesText("[grey](Move up and down to reveal more options)[/]")
                    .AddChoices(Examples.Keys));

            return Examples[command].Invoke();
        }

        protected string DefaultText()
        {
            AnsiConsole.Write(
                new FigletText("AIPF - " + example.Name)
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

        protected void ExitText()
        {
            AnsiConsole.Clear();
            AnsiConsole.Write(
                new FigletText("AIPF")
                    .Centered()
                    .Color(Color.Blue));

            AnsiConsole.Write(new Rule("[bold white]Cacchiarelli, Cesetti, Romagnoli 10/01/2022[/]").RuleStyle("blue").Centered());
            AnsiConsole.WriteLine();
            AnsiConsole.WriteLine();
            AnsiConsole.WriteLine();
            AnsiConsole.Write(new Rule("Closing the application, please wait...").RuleStyle("white").Centered());
        }
    }
}
