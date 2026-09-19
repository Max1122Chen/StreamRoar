using System;
using UnityEngine;

namespace StreamRoar.Infrastructure
{
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class ShowIfAttribute : PropertyAttribute
    {
        public ShowIfAttribute(string fieldName, object expectedValue)
        {
            FieldName = fieldName;
            ExpectedValue = expectedValue;
        }

        public string FieldName { get; }
        public object ExpectedValue { get; }
    }
}
