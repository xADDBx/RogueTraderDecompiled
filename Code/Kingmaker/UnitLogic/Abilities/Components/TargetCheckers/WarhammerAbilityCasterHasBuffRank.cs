using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Blueprints.Root;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Localization;
using Kingmaker.UI.Models.Log.GameLogCntxt;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility.Attributes;
using UnityEngine;

namespace Kingmaker.UnitLogic.Abilities.Components.TargetCheckers;

[TypeId("0f42f9eaf21652b489581d013b09d016")]
public class WarhammerAbilityCasterHasBuffRank : BlueprintComponent, IAbilityCasterRestriction
{
	public bool Not;

	[SerializeField]
	private BlueprintBuffReference m_Buff;

	public bool maxRank = true;

	[HideIf("maxRank")]
	public ContextValue rank = 1;

	public BlueprintBuff Buff => m_Buff?.Get();

	public bool IsCasterRestrictionPassed(MechanicEntity caster)
	{
		if (caster == null)
		{
			return false;
		}
		Buff buff = caster.Buffs.GetBuff(Buff);
		int requiredRank = GetRequiredRank(caster, buff?.Blueprint);
		bool flag = buff != null && (buff.Blueprint.Stacking != StackingType.Rank || buff.Rank >= requiredRank);
		return Not ^ flag;
	}

	public string GetAbilityCasterRestrictionUIText(MechanicEntity caster)
	{
		LocalizedString obj = (Not ? BlueprintRoot.Instance.LocalizedTexts.Reasons.TargetHasNoConditionAndBuff : BlueprintRoot.Instance.LocalizedTexts.Reasons.TargetHasConditionOrBuff);
		int displayedRank = GetRequiredRank(caster, Buff);
		return obj.ToString(delegate
		{
			GameLogContext.Text = $"{Buff.name} ({displayedRank})";
		});
	}

	private int GetRequiredRank(MechanicEntity caster, BlueprintBuff buff)
	{
		if (maxRank)
		{
			return buff?.MaxRank ?? 0;
		}
		MechanicsContext context = ContextData<MechanicsContext.Data>.Current?.Context ?? new MechanicsContext(caster, caster, base.OwnerBlueprint);
		return rank.Calculate(context);
	}
}
