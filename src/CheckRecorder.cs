using NUnit.Framework.Constraints;


namespace NValidate
{
    /// <summary>
    /// Records the results of checks performed inside validators. Checks are expressed as constraints on supplied values.
    /// </summary>
    public abstract class CheckRecorder
    {
        /// <summary>
        /// Applies the constraint expression <paramref name="expression"/> to the supplied value <paramref name="value"/>.
        /// If the constraint is met, the success is recorded. Otherwise, the execution of the whole validator instance is stopped.
        /// </summary>
        /// <param name="value">A value which is checked by the constraint expression <paramref name="expression"/></param>
        /// <param name="expression">A <see cref="IResolveConstraint"/> expression which is applied to the value <paramref name="value"/></param>
        public void CriticalThat<T>(T value, IResolveConstraint expression) => CriticalThat(value, expression, null);

        /// <summary>
        /// Applies the constraint expression <paramref name="expression"/> to the supplied value <paramref name="value"/>.
        /// If the constraint is met, the success is recorded. Otherwise, the execution of the whole validator instance is stopped.
        /// </summary>
        /// <param name="value">A value which is checked by the constraint expression <paramref name="expression"/></param>
        /// <param name="expression">A <see cref="IResolveConstraint"/> expression which is applied to the value <paramref name="value"/></param>
        /// <param name="description">A human-readable description of the check that is performed</param>
        public abstract void CriticalThat<T>(T value, IResolveConstraint expression, string description);

        /// <summary>
        /// Applies the constraint expression <paramref name="expression"/> to the supplied value <paramref name="value"/>.
        /// If the constraint is met, the success is recorded. Otherwise, a failure is recorded.
        /// </summary>
        /// <param name="value">A value which is checked by the constraint expression <paramref name="expression"/></param>
        /// <param name="expression">A <see cref="IResolveConstraint"/> expression which is applied to the value <paramref name="value"/></param>
        public void That<T>(T value, IResolveConstraint expression) => That(value, expression, null);

        /// <summary>
        /// Applies the constraint expression <paramref name="expression"/> to the supplied value <paramref name="value"/>.
        /// If the constraint is met, the success is recorded. Otherwise, a failure is recorded.
        /// </summary>
        /// <param name="value">A value which is checked by the constraint expression <paramref name="expression"/></param>
        /// <param name="expression">A <see cref="IResolveConstraint"/> expression which is applied to the value <paramref name="value"/></param>
        /// <param name="description">A human-readable description of the check that is performed</param>
        public abstract void That<T>(T value, IResolveConstraint expression, string description);
    }
}