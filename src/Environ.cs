using System;
using System.Linq;
using System.Reflection;


namespace NValidate
{
    public abstract class Environ
    {
        public abstract Environ Add(object entity);
        public abstract object GetByType(Type type, Environ topEnviron = null);
        public T Get<T>() => (T)GetByType(typeof(T));


        public object[] ResolveParameters(ParameterInfo[] parameters) => parameters.Select(p => FillInParameter(p)).ToArray();
        
        object FillInParameter(ParameterInfo parameter)
        {
            object result = GetByType(parameter.ParameterType);

            if (result != null)
                return result;

            throw new Exception($"Cannot translate parameter \"{parameter.Name}\"");
        }
    }
}