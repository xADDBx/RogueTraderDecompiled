using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Interaction;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.View.MapObjects.InteractionRestrictions;

public class DemolitionMeltaChargeRestrictionPart : SkillUseRestrictionPart<DemolitionMeltaChargeRestrictionSettings>, IHashable
{
	public override bool ShouldCheckSourceComponent => false;

	public override bool ShowInteractFx => true;

	public override StatType Skill => base.Settings.GetSkill();

	public override InteractionActorType Type => InteractionActorType.MeltaCharge;

	public override string GetInteractionName()
	{
		return LocalizedTexts.Instance.Stats.GetText(base.Settings.GetSkill());
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
