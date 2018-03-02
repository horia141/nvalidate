using NValidate.Attributes;
using NValidate.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;


namespace NValidate
{
    /// <summary>
    /// The "entry point" for the whole validation process. You provide an <see cref="Environ"/>
    /// which provides the various data needed by validators, and an assembly where validators can be
    /// found and it does the rest. That is, it'll run all the validators in the ways they're meant,
    /// record the results and build a <see cref="ValidatorRunResult"/> describing the run's status.
    /// </summary>
    /// <remarks>
    /// Currently there is an assumption that this thing will be run once a day. It's not baked in,
    /// and perhaps it would make better sense to provide a date in the <see cref="Run(Assembly)"/>
    /// method, but that's the way it is for now.
    /// </remarks>
    public class ValidationRunner
    {
        DateTime _dateToRun;
        Environ _environ;


        /// <summary>
        /// Build an instance of this for a day and <see cref="Environ"/>.
        /// </summary>
        /// <param name="dateToRun">The day this validation run is for</param>
        /// <param name="environ">A source of data to validate</param>
        public ValidationRunner(DateTime dateToRun, Environ environ)
        {
            _dateToRun = dateToRun;
            _environ = environ;
        }

        /// <summary>
        /// Perform the validation run, mark of any issues and return issues as well as analytics.
        /// </summary>
        /// <param name="currentAssembly">An assembly to pickup fixtures and templates from</param>
        /// <returns>
        /// The details of each validator fixture, template and instance which were checked
        /// </returns>
        public ValidatorRunResult Run(Assembly currentAssembly)
        {
            var validatorRunResult = new ValidatorRunResult();

            validatorRunResult.RunDate = _dateToRun;
            validatorRunResult.ShouldReport = true;
            validatorRunResult.SummaryAtFixtureLevel = new ResultSummary();
            validatorRunResult.SummaryAtTemplateLevel = new ResultSummary();
            validatorRunResult.SummaryAtInstanceLevel = new ResultSummary();
            validatorRunResult.FixtureResults = new List<ValidatorFixtureResult>();

            try
            {
                foreach (var validatorFixtureInfo in currentAssembly.DefinedTypes.Where(ValidatorFixtureAttribute.HasAttribute))
                {
                    var validatorFixtureRunner = new FixtureRunner(_environ, validatorFixtureInfo);
                    var validatorFixtureResult = validatorFixtureRunner.Run();
                    validatorRunResult.AddFixtureResult(validatorFixtureResult);
                }
            }
            catch (Exception e)
            {
                validatorRunResult.Error = e;
            }

            if (validatorRunResult.Error != null)
                validatorRunResult.Status = GroupStatus.Error;
            else if (validatorRunResult.SummaryAtFixtureLevel.Failure > 0)
                validatorRunResult.Status = GroupStatus.Failure;
            else
                validatorRunResult.Status = GroupStatus.Success;

            return validatorRunResult;
        }
    }
}