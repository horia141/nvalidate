using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;


namespace NValidate
{
    public class EnvironBuilder
    {
        readonly Dictionary<ValueTuple<Type, Type>, object> _entities;
        readonly Dictionary<ValueTuple<Type, Type>, Func<Environ, object>> _entityExtractors;

        public EnvironBuilder()
        {
            _entities = new Dictionary<ValueTuple<Type, Type>, object>();
            _entityExtractors = new Dictionary<ValueTuple<Type, Type>, Func<Environ, object>>();
        }

        public EnvironBuilder Add<T>(T entity)
        {
            _entities[(null, typeof(T))] = entity;
            return this;
        }

        public EnvironBuilder Add<A, T>(T entity) where A : Attribute 
        {
            _entities[(typeof(A), typeof(T))] = entity;
            return this;
        }

        public EnvironBuilder AddExtractor<T>(Func<Environ, object> extractor)
        {
            _entityExtractors[(null, typeof(T))] = extractor;
            return this;
        }

        public EnvironBuilder AddExtractor<A, T>(Func<Environ, object> extractor) where A : Attribute
        {
            _entityExtractors[(typeof(A), typeof(T))] = extractor;
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

        public T Get<T>()
        {
            var result = GetByAttributeAndType(null, typeof(T));
            if (result == null)
                return default(T);
            return (T)result;
        }

        public T Get<A, T>() where A : Attribute
        {
            var result = GetByAttributeAndType(typeof(A), typeof(T));
            if (result == null)
                return default(T);
            return (T)result;
        }

        public Environ Extend<T>(T entity) => ExtendByAttributeAndType(null, typeof(T), entity);
        public Environ Extend<A, T>(T entity) where A : Attribute => ExtendByAttributeAndType(typeof(A), typeof(T), entity);
        public object[] ResolveParameters(ParameterInfo[] parameters) => parameters.Select(p => FillInParameter(p)).ToArray();

        internal abstract object GetByAttributeAndType(Type attributeType, Type entityType, Environ topEnviron = null);
        internal abstract Environ ExtendByAttributeAndType(Type attributeType, Type entityType, object entity);

        private object FillInParameter(ParameterInfo parameter)
        {
            Type attributeType = null;
            if (parameter.CustomAttributes.Count() == 1)
                attributeType = parameter.CustomAttributes.First().AttributeType;
            object result = GetByAttributeAndType(attributeType, parameter.ParameterType);

            if (result != null)
                return result;

            throw new Exception($"Cannot translate parameter \"{parameter.Name}\"");
        }
    }
    
    class BaseEnviron : Environ
    {
        private readonly Dictionary<ValueTuple<Type, Type>, object> _entities;
        private readonly Dictionary<ValueTuple<Type, Type>, Func<Environ, object>> _entityExtractors;

        public BaseEnviron(Dictionary<ValueTuple<Type, Type>, object> entities, Dictionary<ValueTuple<Type, Type>, Func<Environ, object>> entityExtractors)
        {
            _entities = entities;
            _entityExtractors = entityExtractors;
        }

        internal override object GetByAttributeAndType(Type attributeType, Type entityType, Environ topEnviron = null)
        {
            object result = null;
            _entities.TryGetValue((attributeType, entityType), out result);

            if (result != null)
            {
                return result;
            }
            else if (_entityExtractors != null)
            {
                Func<Environ, object> extractor = null;
                if (!_entityExtractors.TryGetValue((attributeType, entityType), out extractor))
                    return null;

                return extractor(topEnviron ?? this);
            }
            else
            {
                return null;
            }
        }

        internal override Environ ExtendByAttributeAndType(Type attributeType, Type entityType, object enitity)
        {
            return new LinkedEnviron(attributeType, entityType, enitity, this);
        }
    }

    class LinkedEnviron : Environ
    {
        private readonly Type _attributeType;
        private readonly Type _entityType;
        private readonly object _entity;
        private readonly Environ _previousEnviron;

        public LinkedEnviron(Type attributeType, Type entityType, object value, Environ previousEnviron)
        {
            _attributeType = attributeType;
            _entityType = entityType;
            _entity = value;
            _previousEnviron = previousEnviron;
        }

        internal override object GetByAttributeAndType(Type attributeType, Type entityType, Environ topEnviron = null)
        {
            if (attributeType == _attributeType && entityType == _entityType)
                return _entity;

            if (entityType == typeof(Environ) && _entity.GetType() == typeof(LinkedEnviron))
                return _entity;

            if (topEnviron == null)
                return _previousEnviron.GetByAttributeAndType(attributeType, entityType, this);
            else
                return _previousEnviron.GetByAttributeAndType(attributeType, entityType, topEnviron);
        }

        internal override Environ ExtendByAttributeAndType(Type attributeType, Type entityType, object entity)
        {
            return new LinkedEnviron(attributeType, entityType, entity, this);
        }
    }
}