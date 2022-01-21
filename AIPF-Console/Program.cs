using AIPF_Console.MNIST_example;
using AIPF_Console.RobotLoccioni_example;
using AIPF_Console.TaxiFare_example;
using Spectre.Console;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace AIPF_Console
{
    class Program
    {
        public static readonly bool REST = false;
        private static IExample example = null;
        private static readonly Dictionary<string, Func<IExample, Task>> Commands = new Dictionary<string, Func<IExample, Task>>()
            {
                { "fit", async e => await e.Train2() },
                { "predict", async e => e.Predict() },
                { "metrics", async e => e.Metrics() },
                { "back", async _ => example = null },
                { "exit", async _ => { } },
            };
        /*
        static async Task Main(string[] args)
        {
            HttpClient client = new HttpClient();
            client.Timeout = TimeSpan.FromSeconds(5);
            string stockSymbol = new Random().Next(1,10000).ToString();
            string url = $"http://localhost:5000/api/mlmanager/messages/sse/{stockSymbol}";

            while (true)
            {
                try
                {
                    Console.WriteLine("Establishing connection");
                    using (var streamReader = new StreamReader(await client.GetStreamAsync(url)))
                    {
                        while (!streamReader.EndOfStream)
                        {
                            var message = await streamReader.ReadLineAsync();
                            Console.WriteLine($"Received price update: {message}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    //Here you can check for 
                    //specific types of errors before continuing
                    //Since this is a simple example, i'm always going to retry
                    Console.WriteLine($"Error: {ex.Message}");
                    Console.WriteLine("Retrying in 5 seconds");
                    await Task.Delay(TimeSpan.FromSeconds(5));
                }
            }
        }*/
        

        public async static Task Main(string[] args)
        {
            example = Mnist.Start();
            await example.Train2();
            AnsiConsole.WriteLine("end!");
            /*
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
                    await Commands[line].Invoke(example);
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
            }*/
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
    }
}
