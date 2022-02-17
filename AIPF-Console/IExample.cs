using System.IO;
using System.Threading.Tasks;

namespace AIPF_Console
{
    public interface IExample
    {
        static string Dir => Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName;

        public string Name { get; }

        public Task Train();

        public Task Predict(PredictionMode predictionMode = PredictionMode.USER_VALUE, int error = 0);

        public Task Metrics();

    }

    public enum PredictionMode {
        DEFAULT_VALUE,
        RANDOM_VALUE,
        USER_VALUE
    }
}
