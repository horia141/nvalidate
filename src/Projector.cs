using System;
using System.Collections.Generic;


namespace NValidate
{
    public interface Projector
    {
        // string GetName(... object[] args);
	// bool IsException(ISet exceptionSet, ... object[] args);
        // void Project(... object[] args);
    }


    public class DefaultProjector : Projector
    {
        public string GetName() => "";

	public bool IsException(ISet<object> exceptionSet) => false;

        public void Project(Environ environ, Action<Environ> filterAndValidate)
        {
            filterAndValidate(environ);
        }
    }
}