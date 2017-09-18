using NUnit.Framework.Constraints;
using NValidate.Internal;
using System.Collections.Generic;


namespace NValidate
{
    public class CheckRecorder
    {
        GroupStatus _status;
        List<CheckResult> _results;
        bool _shouldRecordEveryCheck;


        public CheckRecorder(bool shouldRecordEveryCheck = true)
        {
            _status = GroupStatus.Success;
            _results = shouldRecordEveryCheck ? new List<CheckResult>() : null;
            _shouldRecordEveryCheck = shouldRecordEveryCheck;
        }


        public void CriticalThat<T>(T del, IResolveConstraint expr) => That(del, expr, null);


        /// <summary>
        /// Record the result of a applying the expr constraint to del. If the constraint cannot be satisfied, all further checks in the current test
        /// are abandoned. This will be the last result recorded. Is thread safe.
        /// </summary>
        public void CriticalThat<T>(T del, IResolveConstraint expr, string description)
        {
            var constraint = expr.Resolve();
            var result = constraint.ApplyTo(del);

            if (result.IsSuccess)
            {
                if (_shouldRecordEveryCheck)
                    _results.Add(new CheckResult()
                    {
                        Name = description ?? "",
                        Status = CheckStatus.Success
                    });
            }
            else
            {
                _status = GroupStatus.Failure;
                if (_shouldRecordEveryCheck)
                    _results.Add(new CheckResult()
                    {
                        Name = description ?? "",
                        Status = CheckStatus.CriticalFailure
                    });

                throw new FailedToValidateCriticalCheckException();
            }
        }


        public void That<T>(T del, IResolveConstraint expr) => That(del, expr, null);


        /// <summary>
        /// Record the result of applying the expr constraint to del. Does not stop later checks in the test from running. Is thread safe.
        /// </summary>
        public void That<T>(T del, IResolveConstraint expr, string description)
        {
            var constraint = expr.Resolve();
            var result = constraint.ApplyTo(del);

            if (result.IsSuccess)
            {
                if (_shouldRecordEveryCheck)
                    _results.Add(new CheckResult()
                    {
                        Name = description ?? "",
                        Status = CheckStatus.Success
                    });
            }
            else
            {
                _status = GroupStatus.Failure;
                if (_shouldRecordEveryCheck)
                    _results.Add(new CheckResult()
                    {
                        Name = description ?? "",
                        Status = CheckStatus.Failure
                    });
            }
        }


        public bool IsSuccess => _status == GroupStatus.Success;
        public bool IsFailure => _status == GroupStatus.Failure;
        public IEnumerable<CheckResult> Results => _results;
    }
}