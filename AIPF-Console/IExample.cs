using AIPF.MLManager;
using System;
using System.Collections.Generic;
using System.Text;

namespace AIPF_Console
{
    public interface IExample
    {
        public string GetName();

        public void Train();

        public void Predict();

        public void Metrics();

    }
}
