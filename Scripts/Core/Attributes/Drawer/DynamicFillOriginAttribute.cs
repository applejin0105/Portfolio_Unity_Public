using System;
using UnityEngine;

namespace Core.Attributes.Drawer
{
    [AttributeUsage(AttributeTargets.Field)]
    public class DynamicFillOriginAttribute : PropertyAttribute
    {
        public readonly string MethodFieldName;

        public DynamicFillOriginAttribute(string methodFieldName)
        {
            MethodFieldName = methodFieldName;
        }
    }
}