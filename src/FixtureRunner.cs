using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;


namespace NValidate
{
    class FixtureRunner
    {
        readonly Environ _environ;
        readonly TypeInfo _validatorFixtureInfo;


        public FixtureRunner(Environ environ, TypeInfo validatorFixtureInfo)
        {
            _environ = environ;
            _validatorFixtureInfo = validatorFixtureInfo;
        }


        public ValidatorFixtureResult Run()
        {
            var validatorFixtureResult = new ValidatorFixtureResult();

            validatorFixtureResult.Name = _validatorFixtureInfo.GetCustomAttribute<ValidatorFixtureAttribute>().Name;
            validatorFixtureResult.SummaryAtTemplateLevel = new ResultSummary();
            validatorFixtureResult.SummaryAtInstanceLevel = new ResultSummary();

            if (SkipAttribute.HasAttribute(_validatorFixtureInfo))
            {
                validatorFixtureResult.Status = GroupStatus.Skipped;
                return validatorFixtureResult;
            }

            validatorFixtureResult.TemplateResults = new List<ValidatorTemplateResult>();

            try
            {

                foreach (var validatorTemplateInfo in _validatorFixtureInfo.GetMethods().Where(ValidatorTemplateAttribute.HasAttribute))
                {
                    var templateRunner = new TemplateRunner(_environ, _validatorFixtureInfo, validatorTemplateInfo);
                    var validatorTemplateResult = templateRunner.Run();
                    validatorFixtureResult.AddTemplateResult(validatorTemplateResult);
                }
            }
            catch (Exception e)
            {
                validatorFixtureResult.Error = e;
                validatorFixtureResult.TemplateResults = null;
            }

            if (validatorFixtureResult.Error != null)
                validatorFixtureResult.Status = GroupStatus.Error;
            else if (validatorFixtureResult.SummaryAtTemplateLevel.Failure > 0)
                validatorFixtureResult.Status = GroupStatus.Failure;
            else
                validatorFixtureResult.Status = GroupStatus.Success;

            return validatorFixtureResult;
        }
    }
}