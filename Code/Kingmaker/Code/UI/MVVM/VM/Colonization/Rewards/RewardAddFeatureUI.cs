using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Templates;
using Kingmaker.Globalmap.Colonization.Rewards;
using Kingmaker.UI.MVVM.VM.Colonization.Rewards;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Progression.Features;
using Owlcat.Runtime.UI.Tooltips;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.VM.Colonization.Rewards;

public class RewardAddFeatureUI : RewardUI<RewardAddFeature>
{
	public BlueprintFeature Fact => base.Reward.Fact as BlueprintFeature;

	public override string Name => base.Reward.Fact.Name;

	public override string Description => GetDescription();

	public override Sprite Icon => base.Reward.Fact.Icon;

	public override string NameForAcronym => null;

	public override string CountText => null;

	public RewardAddFeatureUI(RewardAddFeature reward)
		: base(reward)
	{
	}

	public override TooltipBaseTemplate GetTooltip()
	{
		if (base.Reward.Fact == null)
		{
			PFLog.System.Error("RewardAddFeature.GetUITooltip - Fact is null!");
			return null;
		}
		if (base.Reward.Fact is BlueprintFeature feature)
		{
			return new TooltipTemplateFeature(feature);
		}
		if (base.Reward.Fact is BlueprintBuff blueprintBuff)
		{
			return new TooltipTemplateSimple(blueprintBuff.Name, blueprintBuff.Description);
		}
		return null;
	}

	private string GetDescription()
	{
		if (base.Reward.Fact == null)
		{
			PFLog.System.Error("RewardAddFeature.GetDescription - Fact is null!");
			return string.Empty;
		}
		if (base.Reward.AddToParty)
		{
			return string.Format(UIStrings.Instance.ColonyProjectsRewards.RewardAddFeatureParty, base.Reward.Fact.Name);
		}
		if (base.Reward.UnitEvaluator != null && base.Reward.UnitEvaluator.Is(Game.Instance.Player.PlayerShip))
		{
			return string.Format(UIStrings.Instance.ColonyProjectsRewards.RewardAddFeatureShip, base.Reward.Fact.Name);
		}
		if (base.Reward.UnitEvaluator != null)
		{
			return string.Format(UIStrings.Instance.ColonyProjectsRewards.RewardAddFeature, base.Reward.UnitEvaluator, base.Reward.Fact.Name);
		}
		PFLog.System.Error("RewardAddFeature.GetDescription - m_Unit is null!");
		return string.Empty;
	}
}
