using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Blueprints.Root;
using Kingmaker.UI.Models.Log.GameLogCntxt;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.Utility;
using Kingmaker.Utility.Attributes;
using UnityEngine;

namespace Kingmaker.UnitLogic.Abilities.Components.TargetCheckers;

[TypeId("d7f95cddb4de45b47ad1c7e6b1accd65")]
public class WarhammerAbilityTargetHasBuffRank : BlueprintComponent, IAbilityTargetRestriction
{
	public bool Not;

	[SerializeField]
	private BlueprintBuffReference m_Buff;

	public bool maxRank = true;

	[HideIf("maxRank")]
	public int rank = 1;

	public BlueprintBuff Buff => m_Buff?.Get();

	public bool IsTargetRestrictionPassed(AbilityData ability, TargetWrapper target, Vector3 casterPosition)
	{
		if (target.Entity == null)
		{
			return false;
		}
		Buff buff = target.Entity.Buffs.GetBuff(Buff);
		int num = ((!maxRank) ? rank : (buff?.Blueprint.MaxRank ?? 0));
		bool flag = buff != null && (buff.Blueprint.Stacking != StackingType.Rank || buff.Rank >= num);
		return Not ^ flag;
	}

	public string GetAbilityTargetRestrictionUIText(AbilityData ability, TargetWrapper target, Vector3 casterPosition)
	{
		return (Not ? BlueprintRoot.Instance.LocalizedTexts.Reasons.TargetHasNoConditionAndBuff : BlueprintRoot.Instance.LocalizedTexts.Reasons.TargetHasConditionOrBuff).ToString(delegate
		{
			GameLogContext.Text = $"{Buff.name} ({rank})";
		});
	}
}
