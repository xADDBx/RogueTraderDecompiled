using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Templates;
using Kingmaker.Globalmap.Blueprints.Colonization;
using Kingmaker.Globalmap.Colonization.Rewards;
using Kingmaker.UI.MVVM.VM.Colonization.Rewards;
using Owlcat.Runtime.UI.Tooltips;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.VM.Colonization.Rewards;

public class RewardColonyTraitUI : RewardUI<RewardColonyTrait>
{
	public override string Name => string.Empty;

	public override string Description
	{
		get
		{
			if (base.Reward.Trait != null)
			{
				return base.Reward.Trait.Name;
			}
			return string.Empty;
		}
	}

	public override Sprite Icon => null;

	public override string NameForAcronym => null;

	public override string CountText => null;

	public override bool ApplyToAllColonies => base.Reward.ApplyToAllColonies;

	public RewardColonyTraitUI(RewardColonyTrait reward)
		: base(reward)
	{
	}

	public override TooltipBaseTemplate GetTooltip()
	{
		BlueprintColonyTrait trait = base.Reward.Trait;
		if (trait == null)
		{
			PFLog.UI.Error("RewardColonyTraitUI.GetTooltip - Trait is null!");
			return null;
		}
		return new TooltipTemplateColonyTrait(ApplyToAllColonies ? (trait.Name + " (" + UIStrings.Instance.ColonyProjectsRewards.ForAllColonies.Text + ")") : trait.Name, trait.MechanicString, trait.Description, trait.EfficiencyModifier, trait.ContentmentModifier, trait.SecurityModifier);
	}
}
