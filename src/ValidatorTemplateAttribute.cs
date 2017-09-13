using System;
using System.Linq;
using System.Reflection;


namespace NValidate
{
    /// <summary>
    /// Marks a method which performs some validation. Similar to TestAttribute from NUnit.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class ValidatorTemplateAttribute : Attribute
    {
        public static bool HasAttribute(MethodInfo methodInfo) => methodInfo.GetCustomAttributes(typeof(ValidatorTemplateAttribute), true).Count() > 0;
    }
}