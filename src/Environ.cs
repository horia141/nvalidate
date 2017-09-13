using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;


namespace NValidate
{
    public class EnvironBuilder
    {
        readonly Dictionary<Type, object> _entities;
        readonly Dictionary<Type, Func<Environ, object>> _entityExtractors;

        public EnvironBuilder()
        {
            _entities = new Dictionary<Type, object>();
            _entityExtractors = new Dictionary<Type, Func<Environ, object>>();
        }

        public EnvironBuilder Add<T>(T entity)
        {
            _entities[typeof(T)] = entity;
            return this;
        }

        public EnvironBuilder AddExtractor<T>(Func<Environ, object> extractor)
        {
            _entityExtractors[typeof(T)] = extractor;
            return this;
        }

        public Environ Build()
        {
            return new BaseEnviron(_entities, _entityExtractors);
        }
    }

    public abstract class Environ
    {
        protected Environ() { }

        public T Get<T>() => (T)GetByType(typeof(T));
        public Environ Extend<T>(T entity) => ExtendByType(typeof(T), entity);
        public object[] ResolveParameters(ParameterInfo[] parameters) => parameters.Select(p => FillInParameter(p)).ToArray();

        internal abstract object GetByType(Type type, Environ topEnviron = null);
        internal abstract Environ ExtendByType(Type type, object entity);

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
        private readonly Dictionary<Type, object> _entities;
        private readonly Dictionary<Type, Func<Environ, object>> _entityExtractors;

        public BaseEnviron(Dictionary<Type, object> entities, Dictionary<Type, Func<Environ, object>> entityExtractors)
        {
            _entities = entities;
            _entityExtractors = entityExtractors;
        }

        internal override object GetByType(Type type, Environ topEnviron = null)
        {
            object result = null;
            _entities.TryGetValue(type, out result);

            if (result != null)
            {
                return result;
            }
            else if (_entityExtractors != null)
            {
                Func<Environ, object> extractor = null;
                if (!_entityExtractors.TryGetValue(type, out extractor))
                    return null;

                return extractor(topEnviron ?? this);
            }
            else
            {
                return null;
            }
        }

        internal override Environ ExtendByType(Type type, object enitity)
        {
            return new LinkedEnviron(type, enitity, this);
        }
    }

    class LinkedEnviron : Environ
    {
        private readonly Type _type;
        private readonly object _entity;
        private readonly Environ _previousEnviron;

        public LinkedEnviron(Type type, object value, Environ previousEnviron)
        {
            _type = type;
            _entity = value;
            _previousEnviron = previousEnviron;
        }

        internal override object GetByType(Type type, Environ topEnviron = null)
        {
            if (type == _type)
                return _entity;

            if (type == typeof(Environ) && _entity.GetType() == typeof(LinkedEnviron))
                return _entity;

            if (topEnviron == null)
                return _previousEnviron.GetByType(type, this);
            else
                return _previousEnviron.GetByType(type, topEnviron);
        }

        internal override Environ ExtendByType(Type type, object entity)
        {
            return new LinkedEnviron(type, entity, this);
        }
    }
}