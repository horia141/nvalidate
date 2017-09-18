using System;
using System.Collections.Generic;
using System.Threading;


namespace NValidate
{
    /// <summary>
    /// The overall result of a set of checks.
    /// </summary>
    public enum GroupStatus
    {
        /// <summary>
        /// All of the checks have been successfull.
        /// </summary>
        Success = 0,
        /// <summary>
        /// The set of checks have been skipped.
        /// </summary>
        Skipped = 1,
        /// <summary>
        /// At least one of the checks has failed, but none has errored.
        /// </summary>
        Failure = 2,
        /// <summary>
        /// At least one of the checks has errored, and possibly some have failed.
        /// </summary>
        Error = 3
    }


    /// <summary>
    /// The result of a single check.
    /// </summary>
    public enum CheckStatus
    {
        /// <summary>
        /// The checked condition was met.
        /// </summary>
        Success = 0,
        /// <summary>
        /// The checked condition was not met.
        /// </summary>
        Failure = 1,
        /// <summary>
        /// The checked condition was not met. Furthermore, the failure means later checks in the instance
        /// cannot be safely be made, so the whole process is stopped.
        /// </summary>
        CriticalFailure = 2,
        /// <summary>
        /// The check on the condition was not performed. Usually because of a previous check ending in a
        /// <see cref="CriticalFailure"/>.
        /// </summary>
        NotRan = 3
    }


    public class ResultSummary
    {
        int _totalCnt;
        int _successCnt;
        int _skippedCnt;
        int _failureCnt;
        int _errorCnt;

        public ResultSummary()
        {
            _totalCnt = 0;
            _successCnt = 0;
            _skippedCnt = 0;
            _failureCnt = 0;
            _errorCnt = 0;
        }


        public void Add(ResultSummary summary)
        {
            _totalCnt += summary._totalCnt;
            _successCnt += summary._successCnt;
            _skippedCnt += summary._skippedCnt;
            _failureCnt += summary._failureCnt;
            _errorCnt += summary._errorCnt;
        }


        public void AddSuccess()
        {
            Interlocked.Increment(ref _totalCnt);
            Interlocked.Increment(ref _successCnt);
        }


        public void AddSkipped()
        {
            Interlocked.Increment(ref _totalCnt);
            Interlocked.Increment(ref _skippedCnt);
        }


        public void AddFailure()
        {
            Interlocked.Increment(ref _totalCnt);
            Interlocked.Increment(ref _failureCnt);
        }

        public void AddError()
        {
            Interlocked.Increment(ref _totalCnt);
            Interlocked.Increment(ref _errorCnt);
        }


        public int Total => _totalCnt;
        public int Success => _successCnt;
        public int Skipped => _skippedCnt;
        public int Failure => _failureCnt;
        public int Error => _errorCnt;
    }


    /// <summary>
    /// Results for the whole validation process.
    /// </summary>
    public class ValidatorRunResult
    {
        /// <summary>
        /// The date at which the process was run.
        /// </summary>
        public DateTime RunDate { get; set; }

        /// <summary>
        /// The status of the run. Can't be Skipped, because you can't skip an entire process. Can  be Error only if something
        /// really bad happens, such as an uncought exception in an unexpected place.
        /// </summary>
        public GroupStatus Status { get; set; }

        /// <summary>
        /// Summary of the fixtures that were run and their reults.
        /// </summary>
        public ResultSummary SummaryAtFixtureLevel { get; set; }

        /// <summary>
        /// Summary of the templates that were run and their results.
        /// </summary>
        public ResultSummary SummaryAtTemplateLevel { get; set; }

        /// <summary>
        /// Summary of the instances that were run and their results.
        /// </summary>
        public ResultSummary SummaryAtInstanceLevel { get; set; }

        /// <summary>
        /// The set of all results for fixtures that run. Will be null if an error occurred (Status == Error).
        /// </summary>
        public List<ValidatorFixtureResult> FixtureResults { get; set; }

        /// <summary>
        /// The captured exception when Status == Error. Is null otherwise.
        /// </summary>
        public Exception Error { get; set; }
        
        public void AddFixtureResult(ValidatorFixtureResult fixtureResult)
        {
            switch (fixtureResult.Status)
            {
                case GroupStatus.Success:
                    SummaryAtFixtureLevel.AddSuccess();
                    break;
                case GroupStatus.Skipped:
                    SummaryAtFixtureLevel.AddSkipped();
                    break;
                case GroupStatus.Failure:
                    SummaryAtFixtureLevel.AddFailure();
                    break;
                case GroupStatus.Error:
                    SummaryAtFixtureLevel.AddError();
                    break;
            }

            SummaryAtTemplateLevel.Add(fixtureResult.SummaryAtTemplateLevel);
            SummaryAtInstanceLevel.Add(fixtureResult.SummaryAtInstanceLevel);
            FixtureResults.Add(fixtureResult);
        }


        public bool Skipped => Status == GroupStatus.Skipped;
    }


    /// <summary>
    /// Results for a validator fixture.
    /// </summary>
    public class ValidatorFixtureResult
    {
        /// <summary>
        /// The name of the fixture. Usually given as a / separated path.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The status of the fixture. Can be Error if something bad happens at the fixture level.
        /// </summary>
        public GroupStatus Status { get; set; }

        /// <summary>
        /// Summary of the templates that were run and their results.
        /// </summary>
        public ResultSummary SummaryAtTemplateLevel { get; set; }

        /// <summary>
        /// Summary of the instances that were run and their results.
        /// </summary>
        public ResultSummary SummaryAtInstanceLevel { get; set; }

        /// <summary>
        /// The set of all results for templates that were run. Will be null if an error ocurred (Status == Error).
        /// </summary>
        public List<ValidatorTemplateResult> TemplateResults { get; set; }

        /// <summary>
        /// The captured exception when Status == Error. Is null otherwise.
        /// </summary>
        public Exception Error { get; set; }


        public void AddTemplateResult(ValidatorTemplateResult templateResult)
        {
            switch (templateResult.Status)
            {
                case GroupStatus.Success:
                    SummaryAtTemplateLevel.AddSuccess();
                    break;
                case GroupStatus.Skipped:
                    SummaryAtTemplateLevel.AddSkipped();
                    break;
                case GroupStatus.Failure:
                    SummaryAtTemplateLevel.AddFailure();
                    break;
                case GroupStatus.Error:
                    SummaryAtTemplateLevel.AddError();
                    break;
            }

            SummaryAtInstanceLevel.Add(templateResult.SummaryAtInstanceLevel);
            TemplateResults.Add(templateResult);
        }


        public bool Skipped => Status == GroupStatus.Skipped;
    }


    /// <summary>
    /// Results for a validator template.
    /// </summary>
    public class ValidatorTemplateResult
    {
        public const int MAX_NUMBER_OF_INSTANCE_RESULTS = 25;

        object _lock = new object();
        Exception _error;

        /// <summary>
        /// The name of the template.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The status of the template. Can be Error if something bad happens at the template level.
        /// </summary>
        public GroupStatus Status { get; set; }

        /// <summary>
        /// Summary of the instances that were run and their results.
        /// </summary>
        public ResultSummary SummaryAtInstanceLevel { get; set; }

        /// <summary>
        /// A set of results for instances that were run. Only instances with results different from Success or Skipped are explicitly
        /// included. At most <see cref="MAX_NUMBER_OF_INSTANCE_RESULTS"/> are included, because some templates can have large cardinality
        /// when projected. Most issues will be repetitive in any case. It is null if Status == Error.
        /// </summary>
        public List<ValidatorInstanceResult> InstanceResults { get; set; }

        /// <summary>
        /// The captured exception when Status == Error. It is null otherwise
        /// </summary>
        public Exception Error => _error;


        public void AddInstanceResult(ValidatorInstanceResult instanceResult)
        {
            // Happy path in case of many error.
            if (!ShouldRecord)
                return;

            lock (_lock)
            {
                // Double check. Perhaps things have changed while waiting for the lock.
                if (!ShouldRecord)
                    return;

                InstanceResults.Add(instanceResult);
            }
        }


        public void SetError(Exception error)
        {
            Interlocked.CompareExchange<Exception>(ref _error, error, null);
        }


        public bool ShouldRecord => InstanceResults.Count <= MAX_NUMBER_OF_INSTANCE_RESULTS;


        public bool Skipped => Status == GroupStatus.Skipped;
    }


    /// <summary>
    /// Results for a validator instance from a template.
    /// </summary>
    public class ValidatorInstanceResult
    {
        /// <summary>
        /// The name of the instance.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The status of the instance. Can be Error if somethig bad happens during the evaluation.
        /// </summary>
        public GroupStatus Status { get; set; }

        /// <summary>
        /// The set of all checks performed in for the instance. Is null when Status == Error.
        /// </summary>
        public IEnumerable<CheckResult> CheckResults { get; set; }

        /// <summary>
        /// The captured exception when Status == Error. It is null otherwise.
        /// </summary>
        public Exception Error { get; set; } // Contains the error which was raised. Can be null.


        /// <summary>
        /// Whether the instasnce was skipped or not.
        /// </summary>
        public bool Skipped => Status == GroupStatus.Skipped;
    }


    /// <summary>
    /// Results for a single check in an instance.
    /// </summary>
    public class CheckResult
    {
        /// <summary>
        /// The name of the check.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The status of the check.
        /// </summary>
        public CheckStatus Status { get; set; }
    }
}