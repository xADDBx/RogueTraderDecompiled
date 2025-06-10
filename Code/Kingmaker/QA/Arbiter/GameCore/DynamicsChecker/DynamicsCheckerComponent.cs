using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.QA.Arbiter.DynamicsChecker;
using Kingmaker.QA.Arbiter.Tasks;
using Kingmaker.QA.Clockwork;
using Owlcat.QA.Validation;

namespace Kingmaker.QA.Arbiter.GameCore.DynamicsChecker;

[ComponentName("Arbiter/Dynamics Checker")]
[AllowedOn(typeof(BlueprintArbiterInstruction))]
[AllowMultipleComponents]
[TypeId("e8e44b7be8f746cb9cca89aca5148be5")]
public class DynamicsCheckerComponent : BlueprintComponent, IArbiterCheckerComponent
{
	public bool DebugStart;

	[ValidateNotNull]
	public BlueprintAreaPresetReference StartPreset;

	public ClockworkCommandList CommandList;

	public ArbiterTask GetArbiterTask(ArbiterStartupParameters arguments)
	{
		return new DynamicsReportingTask(this, arguments);
	}
}
