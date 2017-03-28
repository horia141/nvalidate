using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;


namespace NValidate
{
    public class BaseEnviron : Environ
    {
        readonly Dictionary<Type, Func<Environ, object>> _modelExtractors;
        readonly Dictionary<Type, object> _models;


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
}