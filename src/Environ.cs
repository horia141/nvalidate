using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;


namespace NValidate
{
    public class EnvironBuilder
    {
        readonly Dictionary<Type, Func<Environ, object>> _modelExtractors;
        readonly Dictionary<Type, object> _models;

        public EnvironBuilder()
        {
            _modelExtractors = new Dictionary<Type, Func<Environ, object>>();
            _models = new Dictionary<Type, object>();
        }

        public EnvironBuilder AddModel(object model)
        {
            _models[model.GetType()] = model;
            return this;
        }

        public EnvironBuilder AddModelExtractor<T>(Func<Environ, object> modelExtractor)
        {
            _modelExtractors[typeof(T)] = modelExtractor;
            return this;
        }

        public Environ Build()
        {
            return new BaseEnviron(_modelExtractors, _models);
        }
    }

    public abstract class Environ
    {
        protected Environ() { }

        public Environ Add<T>(T entity) => Add(entity);
        public T Get<T>() => (T)GetByType(typeof(T));
        public object[] ResolveParameters(ParameterInfo[] parameters) => parameters.Select(p => FillInParameter(p)).ToArray();

        public abstract Environ Add(object entity);
        public abstract object GetByType(Type type, Environ topEnviron = null);

        private object FillInParameter(ParameterInfo parameter)
        {
            object result = GetByType(parameter.ParameterType);

            if (result != null)
                return result;

            throw new Exception($"Cannot translate parameter \"{parameter.Name}\"");
        }
    }
    
    class BaseEnviron : Environ
    {
        private readonly Dictionary<Type, Func<Environ, object>> _modelExtractors;
        private readonly Dictionary<Type, object> _models;

        public BaseEnviron(Dictionary<Type, Func<Environ, object>> modelExtractors, Dictionary<Type, object> models)
        {
            _modelExtractors = modelExtractors;
            _models = models;
        }

        public override Environ Add(object model)
        {
            return new LinkedEnviron(model, this);
        }

        public override object GetByType(Type type, Environ topEnviron = null)
        {
            object result = null;
            _models.TryGetValue(type, out result);

            if (result != null)
            {
                return result;
            }
            else if (_modelExtractors != null)
            {
                Func<Environ, object> extractor = null;
                if (!_modelExtractors.TryGetValue(type, out extractor))
                    return null;

                return extractor(topEnviron ?? this);
            }
            else
            {
                return null;
            }
        }
    }

    class LinkedEnviron : Environ
    {
        private readonly object _value;
        private readonly Environ _previousEnviron;

        public LinkedEnviron(object value, Environ previousEnviron)
        {
            _value = value;
            _previousEnviron = previousEnviron;
        }

        public override Environ Add(object entity)
        {
            return new LinkedEnviron(entity, this);
        }

        public override object GetByType(Type type, Environ topEnviron = null)
        {
            if (type == _value.GetType())
                return _value;

            if (type == typeof(Environ) && _value.GetType() == typeof(LinkedEnviron))
                return _value;

            if (topEnviron == null)
                return _previousEnviron.GetByType(type, this);
            else
                return _previousEnviron.GetByType(type, topEnviron);
        }
    }
}