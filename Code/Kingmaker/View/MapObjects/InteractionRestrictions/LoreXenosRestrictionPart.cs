using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Interaction;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.View.MapObjects.InteractionRestrictions;

public class LoreXenosRestrictionPart : SkillUseWithoutItemRestrictionPart<LoreXenosRestrictionSettings>, ISkillUseRestrictionWithoutItem, IHashable
{
	public override InteractionActorType Type => InteractionActorType.LoreXenos;

	public override StatType Skill => base.Settings.GetSkill();

	public int DCOverrideValue => DCOverride;

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
