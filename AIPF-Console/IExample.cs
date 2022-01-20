using System.IO;


namespace AIPF_Console
{
    public interface IExample
    {
        static string Dir => Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName;

        public string Name {  get;  }

        public void Train();

        public void Predict();

        public void Metrics();

    }
}
