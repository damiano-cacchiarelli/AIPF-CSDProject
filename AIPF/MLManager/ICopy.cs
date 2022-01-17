﻿using System.Linq;

namespace AIPF.MLManager
{
    public interface ICopy<T>
    {
        public void Copy(ref T b)
        {
            var otherType = b.GetType();
            var otherProperties = otherType.GetProperties();

            var myInType = GetType();
            var myProperties = myInType.GetProperties();

            foreach (var other in otherProperties)
            {
                var p = myProperties.FirstOrDefault(p => p.Name == other.Name && p.PropertyType == other.PropertyType);
                if (p != null)
                {
                    other.SetValue(b, p.GetValue(this));
                }
            }
        }
    }
}
