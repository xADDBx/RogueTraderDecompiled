using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Blueprints.Root;
using Kingmaker.Controllers;
using Kingmaker.Designers.WarhammerSurfaceCombatPrototype;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Utility.Random;

namespace Kingmaker.UnitLogic.Mechanics.Actions;

[TypeId("bf4bdb1263684ce08f9f864df4836ac7")]
public class ContextActionRunPsychicPhenomena : ContextAction
{
	public bool UsePerilsEffect;

	private BlueprintPsychicPhenomenaRoot PsychicPhenomenaRoot => BlueprintRoot.Instance.WarhammerRoot.PsychicPhenomenaRoot;

	public override string GetCaption()
	{
		return "Run psychic phenomena";
	}

	protected override void RunAction()
	{
		if (UsePerilsEffect)
		{
			BlueprintAbilityReference blueprintAbilityReference = PsychicPhenomenaRoot.GetAllowedPerils().Random(PFStatefulRandom.Mechanics);
			if (blueprintAbilityReference != null)
			{
				PsychicPhenomenaController.TriggerFakePsychicPhenomenaEffectOnTarget(base.TargetEntity, base.Context, blueprintAbilityReference, null);
				EventBus.RaiseEvent(delegate(IFakePsychicPhenomenaTrigger h)
				{
					h.HandleFakePsychicPhenomena(isPsychicPhenomena: false, isPerilsOfTheWarp: true);
				});
			}
		}
		else
		{
			PsychicPhenomenaController.TriggerFakePsychicPhenomenaEffectOnTarget(base.TargetEntity, base.Context, null, PsychicPhenomenaRoot.PsychicPhenomena.Random(PFStatefulRandom.Mechanics));
			EventBus.RaiseEvent(delegate(IFakePsychicPhenomenaTrigger h)
			{
				h.HandleFakePsychicPhenomena(isPsychicPhenomena: true, isPerilsOfTheWarp: false);
			});
		}
	}
}
