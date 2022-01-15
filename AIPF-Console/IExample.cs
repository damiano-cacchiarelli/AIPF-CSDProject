using AIPF.MLManager;
using System;
using System.Collections.Generic;
using System.Text;

namespace AIPF_Console
{
    public interface IExample
    {
        public void train();

        public void predict();

        public void metrics();

    }
}
