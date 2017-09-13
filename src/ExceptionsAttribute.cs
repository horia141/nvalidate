using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;


namespace NValidate
{
    /// <summary>
    /// Identifies which of the entities produced by a projector should be ignored as exceptions to the "rule".
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class ExceptionsAttribute : Attribute
    {
        object[] _exceptionsSet;


        public ExceptionsAttribute(params object[] exceptionsSet)
	    {
	        _exceptionsSet = exceptionsSet;
	    }
	

        public HashSet<object> GetExceptionsSet() => new HashSet<object>(_exceptionsSet);
	
	
        public static bool HasAttribute(MethodInfo methodInfo) => methodInfo.GetCustomAttributes(typeof(ExceptionsAttribute), true).Count() > 0;
    }
}