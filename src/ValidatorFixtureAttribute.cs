using System;
using System.Reflection;
using System.Linq;

namespace NValidate
{
    /// <summary>
    /// Marks classes which hold validators. Similar to TestFixtureAttribute from NUnit.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class ValidatorFixtureAttribute : Attribute
    {
        public string Name { get; private set; }

        public ValidatorFixtureAttribute(string name)
        {
            Name = name;
        }

        public static bool HasAttribute(TypeInfo typeInfo) => typeInfo.GetCustomAttributes(typeof(ValidatorFixtureAttribute), true).Count() > 0;
    }
}
