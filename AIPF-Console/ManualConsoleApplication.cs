using Spectre.Console;
using System;
using System.Threading.Tasks;

namespace AIPF_Console
{
    public class ManualConsoleApplication : AbstractConsoleApplication
    {
        private static ManualConsoleApplication instance;
        public static ManualConsoleApplication Instance
        {
            get
            {
                if (instance == null) instance = new ManualConsoleApplication();
                return instance;
            }
        }

        protected ManualConsoleApplication() { }

        public override async Task Start()
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
            }

            ExitText();
        }
    }
}
