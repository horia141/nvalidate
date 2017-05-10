using System;
using System.Collections.Generic;


namespace NValidate
{
    public interface Projector
    {
        // string GetName(... object[] args);
	// bool IsException(HashSet exceptionSet, ... object[] args);
        // void Project(... object[] args);
    }


    public class DefaultProjector : Projector
    {
        public string GetName() => "";

	public bool IsException(HashSet<object> exceptionSet) => false;

        public void Project(Environ environ, Action<Environ> filterAndValidate)
        {
            filterAndValidate(environ);
        }
    }
}