using Google.Protobuf.Reflection;
using System;
using System.Collections.Generic;
using System.Text;

namespace SJK
{
    public class Condition
    {
        private ConditionType type;
        private string field;
        private object reference;

        private Condition(ConditionType type, string field, object reference)
        {
            this.type = type;
            this.field = field;
            this.reference = reference;
        }

        public static Condition Equal(string field, object reference)
        {
            return new Condition(ConditionType.Equal, field, reference);
        }

        public static Condition NotEqual(string field, object reference)
        {
            return new Condition(ConditionType.NotEqual, field, reference);
        }

        public override string ToString()
        {
            switch (type)
            {
                case ConditionType.Equal:
                    return $"`{field}` = '{reference}'";
                case ConditionType.NotEqual:
                    return $"`{field}` <> '{reference}'";
                case ConditionType.Less:
                    return $"`{field}` < '{reference}'";
                case ConditionType.More:
                    return $"`{field}` > '{reference}'";
                case ConditionType.LessEqual:
                    return $"`{field}` <= '{reference}'";
                case ConditionType.MoreEqual:
                    return $"`{field}` >= '{reference}'";
                default:
                    throw new Exception("");
            }
        }
    }
}
