using AIPF.MLManager.Modifiers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AIPF.Reflection
{
    public interface ITestRef
    {
        public void Copy(ref TestCopy b)
        {
            var otherType = b.GetType();
            var otherProperties = otherType.GetProperties();

            var myInType = GetType();
            var myProperties = myInType.GetProperties();

            foreach(var other in otherProperties)
            {
                var p = myProperties.FirstOrDefault(p => p.Name == other.Name && p.PropertyType == other.PropertyType);
                if(p != null)
                {
                    other.SetValue(b, p.GetValue(this));
                }
            }
        }
    }
}
