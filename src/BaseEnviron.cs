using System;
using System.Collections.Generic;


namespace NValidate
{
    public class BaseEnviron : Environ
    {
        public class Builder
        {
            readonly Dictionary<Type, Func<Environ, object>> _modelExtractors;
            readonly Dictionary<Type, object> _models;

            public Builder()
            {
                _modelExtractors = new Dictionary<Type, Func<Environ, object>>();
                _models = new Dictionary<Type, object>();
            }

            public Builder AddModel(object model)
            {
                _models[model.GetType()] = model;
                return this;
            }

            public Builder AddModelExtractor<T>(Func<Environ, object> modelExtractor)
            {
                _modelExtractors[typeof(T)] = modelExtractor;
                return this;
            }

            public BaseEnviron Build()
            {
                return new BaseEnviron(_modelExtractors, _models);
            }
        }


        readonly Dictionary<Type, Func<Environ, object>> _modelExtractors;
        readonly Dictionary<Type, object> _models;


        BaseEnviron(Dictionary<Type, Func<Environ, object>> modelExtractors, Dictionary<Type, object> models)
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
}