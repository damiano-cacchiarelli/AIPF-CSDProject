using System.IO;


namespace AIPF_Console
{
    public interface IExample
    {
        static string Dir => Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName;

        public string GetName();

        public void Train();

        public void Predict();

        public void Metrics();

    }
}
