using System;


namespace NValidate
{
    public interface Projector
    {
        // string GetName(... object[] args);
        // void Project(... object[] args);
    }


    public class DefaultProjector : Projector
    {
        public string GetName() => "";

        public void Project(Environ environ, Action<Environ> filterAndValidate)
        {
            filterAndValidate(environ);
        }
    }
}