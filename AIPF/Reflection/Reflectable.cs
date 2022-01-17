using System;
using System.Collections.Generic;
using System.Text;

namespace AIPF.Reflection
{
    [AttributeUsage(AttributeTargets.Interface)]
    public class Reflectable : Attribute
    {
        private string reflectionCode;

        public Reflectable(string reflectionCode = null)
        {
            this.reflectionCode = reflectionCode;
        }

        public string GetReflectionCode()
        {
            return reflectionCode;
        }

        public bool HasReflectionCode()
        {
            return reflectionCode != null;
        }
    }
}
