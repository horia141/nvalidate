using System;


namespace NValidate.Internal
{
    /// <summary>
    /// Exception thrown when a critical condition is not satisfied.
    /// </summary>
    /// <remarks>
    /// When a runner for an validator instnce checks a constraint defined via a 
    /// <see cref="RunnerCheckRecorder.CriticalThat{T}(T, NUnit.Framework.Constraints.IResolveConstraint)"/> and it fails,
    /// this exception is thrown, so the execution of the whole instance is stopped.
    /// </remarks>
    class FailedToValidateCriticalCheckException : Exception
    {
    }
}