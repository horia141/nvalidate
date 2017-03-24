using System;
using System.Reflection;
using System.Linq;

namespace NValidate.Internal
{
    /// <summary>
    /// Marks a method which performs some validation. Similar to TestAttribute from NUnit.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    class ValidatorTemplateAttribute : Attribute
    {
        public static bool HasAttribute(MethodInfo methodInfo) => methodInfo.GetCustomAttributes(typeof(ValidatorTemplateAttribute), true).Count() > 0;
    }
}