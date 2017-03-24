using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;


namespace NValidate.Internal
{
    class TemplateRunner
    {
        static readonly ProjectorAttribute _s_defaultProjector = new ProjectorAttribute(typeof(DefaultProjector));

        readonly Environ _environ;
        readonly TypeInfo _validatorFixtureInfo;
        readonly MethodInfo _validatorTemplateInfo;


        public TemplateRunner(Environ environ, TypeInfo validatorFixtureInfo, MethodInfo validatorTemplateInfo)
        {
            _environ = environ;
            _validatorFixtureInfo = validatorFixtureInfo;
            _validatorTemplateInfo = validatorTemplateInfo;
        }


        public ValidatorTemplateResult Run()
        {
            var validatorTemplateResult = new ValidatorTemplateResult();
            validatorTemplateResult.Name = _validatorTemplateInfo.Name;
            validatorTemplateResult.SummaryAtInstanceLevel = new ResultSummary();

            if (SkipAttribute.HasAttribute(_validatorTemplateInfo))
            {
                validatorTemplateResult.Status = GroupStatus.Skipped;
                return validatorTemplateResult;
            }

            validatorTemplateResult.InstanceResults = new List<ValidatorInstanceResult>();

            var validatorFixture = _validatorFixtureInfo.GetConstructor(Type.EmptyTypes).Invoke(new object[0]);

            ProjectorAttribute projector = _validatorTemplateInfo.GetCustomAttribute<ProjectorAttribute>() ?? _s_defaultProjector;
            IEnumerable<FilterAttribute> filters = _validatorTemplateInfo.GetCustomAttributes<FilterAttribute>();

            CancellationTokenSource cts = new CancellationTokenSource();

            var environ = _environ
                .Add(cts)
                .Add((Action<Environ>)(env =>
            {
                try
                {
                    if (!filters.All(fa => fa.IsAllowed(env)))
                        return;

                    var checkRecorder = new CheckRecorder(validatorTemplateResult.ShouldRecord);

                    try
                    {
                        _validatorTemplateInfo.Invoke(validatorFixture, env.Add(checkRecorder).ResolveParameters(_validatorTemplateInfo.GetParameters()));

                        if (checkRecorder.IsSuccess)
                        {
                            validatorTemplateResult.SummaryAtInstanceLevel.AddSuccess();
                        }
                        else if (checkRecorder.IsFailure)
                        {
                            validatorTemplateResult.SummaryAtInstanceLevel.AddFailure();
                            // Prevent allocation if we aren't going to explicitly record this.
                            if (!validatorTemplateResult.ShouldRecord)
                                return;
                            validatorTemplateResult.AddInstanceResult(new ValidatorInstanceResult()
                            {
                                Name = projector.GetName(env),
                                Status = GroupStatus.Failure,
                                CheckResults = checkRecorder.Results,
                                Error = null
                            });
                        }
                    }
                    catch (TargetInvocationException e) when (e.InnerException is FailedToValidateCriticalCheckException)
                    {
                        validatorTemplateResult.SummaryAtInstanceLevel.AddFailure();
                        validatorTemplateResult.AddInstanceResult(new ValidatorInstanceResult()
                        {
                            Name = projector.GetName(env),
                            Status = GroupStatus.Failure,
                            CheckResults = checkRecorder.Results,
                            Error = null
                        });
                    }
                    catch (TargetInvocationException e)
                    {
                        validatorTemplateResult.SummaryAtInstanceLevel.AddError();
                        validatorTemplateResult.AddInstanceResult(new ValidatorInstanceResult()
                        {
                            Name = projector.GetName(env),
                            Status = GroupStatus.Error,
                            CheckResults = null,
                            Error = e.InnerException
                        });
                    }
                }
                catch (Exception e)
                {
                    validatorTemplateResult.SetError(e);
                    validatorTemplateResult.InstanceResults = null;
                    cts.Cancel();
                }
            }));
            environ = environ.Add(environ); // This is the base environment. It should be accessible downstream.

            projector.Project(environ);

            if (validatorTemplateResult.InstanceResults.Count == 0 && validatorTemplateResult.Error == null)
                validatorTemplateResult.Status = GroupStatus.Success;
            else if (validatorTemplateResult.Error != null)
                validatorTemplateResult.Status = GroupStatus.Error;
            else
                validatorTemplateResult.Status = GroupStatus.Failure;

            return validatorTemplateResult;
        }
    }
}