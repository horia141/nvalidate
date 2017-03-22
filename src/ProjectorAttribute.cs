using System;
using System.Reflection;
using System.Linq;

namespace NValidate
{
    /// <summary>
    /// Project according to a given projector.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple=false)]
    class ProjectorAttribute : Attribute
    {
        Type _projectorInfo;
        MethodInfo _getNameMethodInfo;
        MethodInfo _projectMethodInfo;
        Projector _projectorObject;


        public ProjectorAttribute(Type projectorInfo)
        {
            _projectorInfo = projectorInfo;

            if (!_projectorInfo.GetInterfaces().Contains(typeof(Projector)))
                throw new ArgumentException("ProjectorAttribute only accepts a projector as argument");

            _getNameMethodInfo = _projectorInfo.GetMethod("GetName", BindingFlags.Public | BindingFlags.Instance);

            if (_getNameMethodInfo == null)
                throw new ArgumentException("Projector must have a GetName method");
            if (_getNameMethodInfo.ReturnType != typeof(string))
                throw new ArgumentException("Project method must return string");

            _projectMethodInfo = _projectorInfo.GetMethod("Project", BindingFlags.Public | BindingFlags.Instance);

            if (_projectMethodInfo == null)
                throw new ArgumentException("Projector must have a Project method");
            if (_projectMethodInfo.ReturnType != typeof(void))
                throw new ArgumentException("Project method must return void");

            var constructors = _projectorInfo.GetConstructors();

            if (constructors.Length != 1)
                throw new ArgumentException("Projector type is supposed to have a single constructor");

            var constructor = constructors.First();

            if (constructor.GetParameters().Count() != 0)
                throw new ArgumentException("Projector object is not supposed to have arguments");

            _projectorObject = (Projector)constructor.Invoke(new object[0]);
        }


        public string GetName(Environ environ)
        {
            return (string)_getNameMethodInfo.Invoke(_projectorObject, environ.ResolveParameters(_getNameMethodInfo.GetParameters()));
        }


        public void Project(Environ environ)
        {
            _projectMethodInfo.Invoke(_projectorObject, environ.ResolveParameters(_projectMethodInfo.GetParameters()));
        }
    }
}