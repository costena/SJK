using System;
using System.Collections.Generic;
using System.Text;

namespace SJK
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
    public class KeyAttribute : Attribute
    {
        private string keyName;

        public string KeyName
        {
            get
            {
                return keyName;
            }
        }

        public KeyAttribute(string keyName)
        {
            this.keyName = keyName;
        }
    }
}
