using System;


namespace NValidate
{
    public class LinkedEnviron : Environ
    {
        readonly object _value;
        readonly Environ _previousEnviron;


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