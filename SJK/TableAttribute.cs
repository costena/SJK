using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace SJK
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class TableAttribute : Attribute
    {
        private string tableName;

        public string TableName
        {
            get
            {
                return tableName;
            }
        }

        public TableAttribute(string tableName)
        {
            this.tableName = tableName;
        }
    }
}
