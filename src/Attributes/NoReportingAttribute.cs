using System;
using System.Linq;
using System.Reflection;


namespace NValidate.Attributes
{
    /// <summary>
    /// Marks a validator template / fixture as not for reporting.
    /// </summary>
    /// <remarks>
    /// What this means precisely is up to the user. But conceptually you need to present the
    /// results of validation somehow - either in a UI, or by sending an email to a particular
    /// group etc. This attribute will make the runners like <see cref="ValidatorRunResult"/>
    /// have their "NotForReporting" field set to true, which can then be interpreted by any
    /// coordinator process in the way it needs. Display a tag in the UI, not include it in
    /// an email etc.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public class NoReportingAttribute : Attribute
    {
        /// <summary>
        /// Convenience method for checking whether a class has the attribute.
        /// </summary>
        /// <param name="typeInfo">The type info for a particular class</param>
        /// <returns>Whether the class has the attribute or not</returns>
        public static bool HasAttribute(TypeInfo typeInfo) => typeInfo.GetCustomAttributes(typeof(NoReportingAttribute), true).Count() > 0;

        /// <summary>
        /// Convenience method for checking whether a method has the attribute.
        /// </summary>
        /// <param name="methodInfo">The method info for a particular class</param>
        /// <returns>Whether the method has the attribute or not</returns>
        public static bool HasAttribute(MethodInfo methodInfo) => methodInfo.GetCustomAttributes(typeof(NoReportingAttribute), true).Count() > 0;
    }
}
