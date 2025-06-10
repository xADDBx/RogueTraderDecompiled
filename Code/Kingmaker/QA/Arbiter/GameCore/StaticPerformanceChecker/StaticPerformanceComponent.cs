using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Area;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.QA.Arbiter.StaticPerformanceChecker;
using Kingmaker.QA.Arbiter.Tasks;
using Owlcat.QA.Validation;

namespace Kingmaker.QA.Arbiter.GameCore.StaticPerformanceChecker;

[ComponentName("Arbiter/Static Performance Checker")]
[AllowedOn(typeof(BlueprintArbiterInstruction))]
[AllowMultipleComponents]
[TypeId("03988233b46a41d6a5e3572b3c444a84")]
public class StaticPerformanceComponent : BlueprintComponent, IArbiterCheckerComponent
{
	[ValidateNotNull]
	public BlueprintAreaReference Area;

	public BlueprintAreaPresetReference StartPreset;

	public int Step;

	public BlueprintAreaPreset Preset => StartPreset?.Get() ?? Area.Get().DefaultPreset;

	public ArbiterTask GetArbiterTask(ArbiterStartupParameters arguments)
	{
		return new StaticPerformanceCheckerTask(this, Step);
	}
}
