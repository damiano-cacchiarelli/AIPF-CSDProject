using System;
using System.Collections.Generic;
using System.Text;

namespace AIPF.Reflection
{
    /*
        Main() {
            TestCopy input = new TestCopy() { A = 10, B = "ciao", C = 345.45f };
            TestCopy output = new TestCopy2() { D = 5 };
            ((ITestRef)input).Copy(ref output);
        }
     */
    public class TestCopy : ITestRef
    {
        public int A { get; set; }
        public string B { get; set; }
        public float C { get; set; }

        /*
        void ITestRef.Copy(ref TestCopy b)
        {
            b.A = A;
        }*/
    }

    public class TestCopy2 : TestCopy
    {
        public int D { get; set; }

        /*
        void ITestRef.Copy(ref TestCopy b)
        {
            b.A = A;
        }*/
    }
}
