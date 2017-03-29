using System;
using System.Reflection;
using System.Linq;

namespace NValidate
{
    /// <summary>
    /// Filter the results of a ForEachXAttribute.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple=true)]
    public class FilterAttribute : Attribute
    {
        Type _filterInfo;
        MethodInfo _filterMethodInfo;
        Filter _filterObject;

        public FilterAttribute(Type filterInfo)
        {
            _filterInfo = filterInfo;

            if (!_filterInfo.GetInterfaces().Contains(typeof(Filter)))
                throw new ArgumentException("Invalid argument to FilterAttribute");

            _filterMethodInfo = _filterInfo.GetMethod("Filter", BindingFlags.Public | BindingFlags.Instance);

            if (_filterMethodInfo == null)
                throw new ArgumentException("Filter must have a Filter method");
            if (_filterMethodInfo.ReturnType != typeof(bool))
                throw new ArgumentException("Filter method must return bool");

            var constructors = _filterInfo.GetConstructors();

            if (constructors.Length != 1)
                throw new ArgumentException("Filter type is supposed to have a single constructor");

            var constructor = constructors.First();

            if (constructor.GetParameters().Count() != 0)
                throw new ArgumentException("Filter object is not supposed to have arguments");

            _filterObject = (Filter)constructor.Invoke(new object[0]);
        }


        public bool IsAllowed(Environ environ)
        {
            return (bool)_filterMethodInfo.Invoke(_filterObject, environ.ResolveParameters(_filterMethodInfo.GetParameters()));
        }
    }
}