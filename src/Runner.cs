using NValidate.Attributes;
using NValidate.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;


namespace NValidate
{
    public class Runner
    {
        DateTime _dateToRun;
        Environ _environ;


        public Runner(DateTime dateToRun, Environ environ)
        {
            _dateToRun = dateToRun;
            _environ = environ;
        }

        public ValidatorRunResult Run(Assembly currentAssembly)
        {
            var validatorRunResult = new ValidatorRunResult();

            validatorRunResult.RunDate = _dateToRun;
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