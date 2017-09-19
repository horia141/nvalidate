using NUnit.Framework.Constraints;
using NValidate.Internal;
using System.Collections.Generic;


namespace NValidate
{
    class RunnerCheckRecorder : CheckRecorder
    {
        GroupStatus _status;
        List<CheckResult> _results;
        bool _shouldRecordEveryCheck;

        public RunnerCheckRecorder(bool shouldRecordEveryCheck = true)
        {
            _status = GroupStatus.Success;
            _results = shouldRecordEveryCheck ? new List<CheckResult>() : null;
            _shouldRecordEveryCheck = shouldRecordEveryCheck;
        }

        public override void CriticalThat<T>(T del, IResolveConstraint expr, string description)
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

        public override void That<T>(T del, IResolveConstraint expr, string description)
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