using System;
using System.Collections.Generic;
using System.Text;

namespace SJK
{
    public class PrimaryKeyAttribute : KeyAttribute
    {
        private bool autoIncrement;

        public bool AutoIncrement
        {
            get
            {
                return autoIncrement;
            }
        }

        public PrimaryKeyAttribute(string keyName, bool autoIncrement = true) : base(keyName)
        {
            this.autoIncrement = autoIncrement;
        }
    }
}
