using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;


namespace NValidate
{
    /// <summary>
    /// Indicates that an association between a type and a value, or between a type with an attribute and a value
    /// is already present in the builder.
    /// </summary>
    public class AssociationExistsException : Exception { }

    /// <summary>
    /// Indicates that the value for a type or a type and an attribute could not be found.
    /// </summary>
    public class CannotFindAssociationException : Exception
    {
        /// <summary>
        /// Construct a <see cref="CannotFindAssociationException"/>
        /// </summary>
        /// <param name="message">An informative message</param>
        public CannotFindAssociationException(string message) : base(message) { }
    }

    /// <summary>
    /// Construct an initial <see cref="Environ"/> for use in the validation process.
    /// </summary>
    /// <example>
    /// For example, this would be used as:
    /// <code>
    /// var environ = new EnvironBuilder()
    ///     .Add{string}("hello")
    ///     .Add{SomeAttribute, string}("world")
    ///     .Add{DateTime}(DateTime.UtcNow)
    ///     .Build();
    /// // Pass on environ to a ValidationRunner.
    /// </code>
    /// </example>
    public class EnvironBuilder
    {
        readonly Dictionary<ValueTuple<Type, Type>, object> _entities;
        readonly Dictionary<ValueTuple<Type, Type>, Func<Environ, object>> _entityExtractors;

        /// <summary>
        /// Construct an instance of <see cref="EnvironBuilder"/>.
        /// </summary>
        public EnvironBuilder()
        {
            _entities = new Dictionary<ValueTuple<Type, Type>, object>();
            _entityExtractors = new Dictionary<ValueTuple<Type, Type>, Func<Environ, object>>();
        }

        /// <summary>
        /// Add a new simple value to the builder, under type <typeparamref name="T"/>.
        /// </summary>
        /// <remarks>If <typeparamref name="T"/> is already associated with a value or extractor, an exception is thrown.</remarks>
        /// <typeparam name="T">The type to associate with <paramref name="entity"/></typeparam>
        /// <param name="entity">The value to use for type <typeparamref name="T"/></param>
        /// <returns>A reference to the current <see cref="EnvironBuilder"/>, for use in chaining.</returns>
        public EnvironBuilder Add<T>(T entity)
        {
            if (_entities.ContainsKey((null, typeof(T))) || _entityExtractors.ContainsKey((null, typeof(T))))
                throw new AssociationExistsException();

            _entities[(null, typeof(T))] = entity;
            return this;
        }

        /// <summary>
        /// Add a new simple value to the builder, under type <typeparamref name="T"/> with an attribute <typeparamref name="A"/>.
        /// </summary>
        /// <remarks>
        /// If <typeparamref name="T"/> with an attribute <typeparamref name="A"/> is already associated with a value or extractor,
        /// an exception is thrown.
        /// </remarks>
        /// <typeparam name="A">The attribute for type <typeparamref name="T"/> to associate with <paramref name="entity"/></typeparam>
        /// <typeparam name="T">The type to associate with <paramref name="entity"/></typeparam>
        /// <param name="entity">The value to use for type <typeparamref name="T"/> and attribute <typeparamref name="A"/></param>
        /// <returns>A reference to the current <see cref="EnvironBuilder"/>, for use in chaining.</returns>
        public EnvironBuilder Add<A, T>(T entity) where A : Attribute 
        {
            if (_entities.ContainsKey((typeof(A), typeof(T))) || _entityExtractors.ContainsKey((typeof(A), typeof(T))))
                throw new AssociationExistsException();

            _entities[(typeof(A), typeof(T))] = entity;
            return this;
        }

        /// <summary>
        /// Add a new extractor to the builder, under type <typeparamref name="T"/>.
        /// </summary>
        /// <remarks>
        /// The extractor provides more complex values for users of the <see cref="Environ"/>, derived from the simple ones already present.
        /// But it keeps the same retrieval semantics via <see cref="Environ.Get{T}"/>/<see cref="Environ.Get{A, T}"/>.
        /// </remarks>
        /// <remarks>If <typeparamref name="T"/> is already associated with a value or extractor, an exception is thrown.</remarks>
        /// <typeparam name="T">The type to associate with <paramref name="extractor"/></typeparam>
        /// <param name="extractor">The extractor to use for type <typeparamref name="T"/></param>
        /// <returns>A reference to the current <see cref="EnvironBuilder"/>, for use in chaining.</returns>
        public EnvironBuilder AddExtractor<T>(Func<Environ, object> extractor)
        {
            if (_entities.ContainsKey((null, typeof(T))) || _entityExtractors.ContainsKey((null, typeof(T))))
                throw new AssociationExistsException();

            _entityExtractors[(null, typeof(T))] = extractor;
            return this;
        }

        /// <summary>
        /// Add a new extractor to the builder, under type <typeparamref name="T"/> with an attribute <typeparamref name="A"/>.
        /// </summary>
        /// <remarks>
        /// The extractor provides more complex values for users of the <see cref="Environ"/>, derived from the simple ones already present.
        /// But it keeps the same retrieval semantics via <see cref="Environ.Get{T}"/>/<see cref="Environ.Get{A, T}"/>.
        /// </remarks>
        /// <remarks>
        /// If <typeparamref name="T"/> with an attribute <typeparamref name="A"/> is already associated with a value or extractor,
        /// an exception is thrown.
        /// </remarks>
        /// <typeparam name="A">The attribute for <typeparamref name="T"/> to associate with <paramref name="extractor"/></typeparam>
        /// <typeparam name="T">The type to associate with <paramref name="extractor"/></typeparam>
        /// <param name="extractor">The value to use for type <typeparamref name="T"/> with attribute <typeparamref name="A"/></param>
        /// <returns>A reference to the current <see cref="EnvironBuilder"/>, for use in chaining.</returns>
        public EnvironBuilder AddExtractor<A, T>(Func<Environ, object> extractor) where A : Attribute
        {
            if (_entities.ContainsKey((typeof(A), typeof(T))) || _entityExtractors.ContainsKey((typeof(A), typeof(T))))
                throw new AssociationExistsException();

            _entityExtractors[(typeof(A), typeof(T))] = extractor;
            return this;
        }

        /// <summary>
        /// Construct an <see cref="Environ"/> from all the values and extractors accumulated.
        /// </summary>
        /// <returns>A new <see cref="Environ"/> from all the values and extractors accumulated</returns>
        public Environ Build()
        {
            return new BaseEnviron(_entities, _entityExtractors);
        }
    }

    /// <summary>
    /// A mapping between types and values.
    /// </summary>
    /// <remarks>
    /// This is the core object involved in the mini-dependency injection we're doing in order to run the validation process.
    /// The user is supposed to provide their data to validate as part of an <see cref="Environ"/>, and the library transforms,
    /// projects, filters it in order to run the various validator fixtures, templates and instances.
    /// </remarks>
    /// <example>
    /// The class is relatively straight-forward to use. Unlike most dependency injection containers, you even have access to the
    /// <see cref="Environ"/> itself in certain circumstances. That is, you're not just building it and shipping it to the library,
    /// but rather expected to interact with it in certain ways. For example:
    /// <code>
    /// var environ = new EnvironBuilder()
    ///     .Add{OneAttribute,string}("hello")
    ///     .Add{TwoAttribute,string}("world")
    ///     .AddExtractor{string}((env) => $"{env.Get{OneAttribute, string}()}-{env.Get{TwoAttribute, string}()}")
    ///     .Build();
    /// </code>
    /// </example>
    public abstract class Environ
    {

        /// <summary>
        /// Construct an <see cref="Environ"/>.
        /// </summary>
        protected Environ() { }

        /// <summary>
        /// Retrieve the value associated with type <typeparamref name="T"/>.
        /// </summary>
        /// <remarks>An exact match of the type is required. Subclasses will not work.</remarks>
        /// <typeparam name="T">The type to retrieve the value for</typeparam>
        /// <returns>The value associated with type <typeparamref name="T"/>, or <c>default(T)</c> if no such value exists</returns>
        public T Get<T>()
        {
            var result = GetByAttributeAndType(null, typeof(T));
            if (result == null)
                return default(T);
            return (T)result;
        }

        /// <summary>
        /// Retrieve the value associated with type <typeparamref name="T"/> with an attribute <typeparamref name="A"/>.
        /// </summary>
        /// <remarks>An exact match of the type is required. Subclasses will not work.</remarks>
        /// <typeparam name="A">The attribute for type <typeparamref name="T"/> to retrieve the value for</typeparam>
        /// <typeparam name="T">The type to retrieve the value for</typeparam>
        /// <returns>The value associated with type <typeparamref name="T"/> with an attribute <typeparamref name="A"/>, or <c>default(T)</c> if no such value exists</returns>
        public T Get<A, T>() where A : Attribute
        {
            var result = GetByAttributeAndType(typeof(A), typeof(T));
            if (result == null)
                return default(T);
            return (T)result;
        }

        /// <summary>
        /// Construct a new <see cref="Environ"/>, based on this one, but with a new value associated to <typeparamref name="T"/>.
        /// </summary>
        /// <remarks>If <typeparamref name="T"/> is already associated with a value, that association will be shadowed.</remarks>
        /// <typeparam name="T">The type to associate with value <paramref name="entity"/></typeparam>
        /// <param name="entity">The value to associate with type <typeparamref name="T"/></param>
        /// <returns>
        /// A new <see cref="Environ"/>, containing all of the associations in the current object, as well as a new one
        /// between type <typeparamref name="T"/> and value <paramref name="entity"/>
        /// </returns>
        public Environ Extend<T>(T entity) => ExtendByAttributeAndType(null, typeof(T), entity);
        /// <summary>
        /// Construct a new <see cref="Environ"/>, based on this one, but with a new value associated to <typeparamref name="T"/>
        /// with attribute <typeparamref name="A"/>.
        /// </summary>
        /// <remarks>If <typeparamref name="T"/> is already associated with a value, that association will be shadowed.</remarks>
        /// <typeparam name="A">The attribute of type <typeparamref name="T"/> to associate with value <paramref name="entity"/></typeparam>
        /// <typeparam name="T">The type to associate with value <paramref name="entity"/></typeparam>
        /// <param name="entity">The value to associate with type <typeparamref name="T"/></param>
        /// <returns>
        /// A new <see cref="Environ"/>, containing all of the associations in the current object, as well as a new one
        /// between type <typeparamref name="T"/> with attributue <typeparamref name="A"/> and value <paramref name="entity"/>
        /// </returns>
        public Environ Extend<A, T>(T entity) where A : Attribute => ExtendByAttributeAndType(typeof(A), typeof(T), entity);

        /// <summary>
        /// Utility method which obtains values for the parameters of a method, based on the current values in this
        /// <see cref="Environ"/>. The values are returned in order, taking account of annotations and the types which
        /// are specified, but without handling inheritence. If a type or type and annotation can't be retrieve, then
        /// <see cref="CannotFindAssociationException"/> is thrown.
        /// </summary>
        /// <param name="parameters">An array of <see cref="ParameterInfo"/> objects</param>
        /// <returns>
        /// An array of objects. The object at position <c>i</c> corresponds to the <see cref="ParameterInfo"/> object
        /// at position <c>i</c> in <paramref name="parameters"/>.
        /// </returns>
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

            throw new CannotFindAssociationException($"Cannot translate parameter \"{parameter.Name}\"");
        }
    }
    
    /// <summary>
    /// A bulk mapping between types or types and attributes to values and extractors.
    /// </summary>
    /// <remarks>
    /// This is meant to be the base <see cref="Environ"/> for the validation process, and it should be the one
    /// provided by users. Hence <see cref="EnvironBuilder"/> actually returns an instance of this. Later calls
    /// to <see cref="Environ.Extend{T}(T)"/>/<see cref="Environ.Extend{A, T}(T)"/> derive off this one, but use
    /// <see cref="LinkedEnviron"/>
    /// </remarks>
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

    /// <summary>
    /// A lightweight <see cref="Environ"/>, meant to be based on another one. Contains a single association, and refers to its
    /// linked environment for all other lookups.
    /// </summary>
    /// <remarks>
    /// In order to have the semantics of <see cref="Environ.Extend{T}(T)"/>/<see cref="Environ.Extend{A, T}(T)"/> which are
    /// really important for the validation process, but still keep things fast, there's a single big <see cref="BaseEnviron"/>,
    /// and any extension of it produces chains of <see cref="LinkedEnviron"/>. Since the chains are expected to be small, the
    /// <c>O(n)</c> lookup cost doesn't really add to much.
    /// </remarks>
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