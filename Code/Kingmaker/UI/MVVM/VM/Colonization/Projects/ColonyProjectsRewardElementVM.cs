using System;
using Kingmaker.Code.UI.MVVM.VM.Colonization.Rewards;
using Kingmaker.Globalmap.Colonization.Rewards;
using Kingmaker.UI.Common;
using Kingmaker.UI.MVVM.VM.Colonization.Rewards;
using Owlcat.Runtime.UI.MVVM;
using UniRx;
using UnityEngine;

namespace Kingmaker.UI.MVVM.VM.Colonization.Projects;

public class ColonyProjectsRewardElementVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable
{
	public readonly ReactiveProperty<Sprite> Icon = new ReactiveProperty<Sprite>();

	public readonly ReactiveProperty<Color32> IconColor = new ReactiveProperty<Color32>();

	public readonly ReactiveProperty<string> Acronym = new ReactiveProperty<string>();

	public readonly ReactiveProperty<string> Description = new ReactiveProperty<string>();

	public readonly ReactiveProperty<int> Count = new ReactiveProperty<int>();

	public readonly ReactiveProperty<string> CountText = new ReactiveProperty<string>();

	public readonly ReactiveProperty<bool> ApplyToAllColonies = new ReactiveProperty<bool>();

	public Reward Reward { get; }

	public RewardElementVisualType VisualType { get; private set; }

	public ColonyProjectsRewardElementVM(Reward reward)
	{
		Reward = reward;
		RewardUI reward2 = RewardUIFactory.GetReward(reward);
		SetVisualType(reward2);
		SetIcon(reward2);
		Description.Value = reward2.Description;
		Count.Value = reward2.Count;
		CountText.Value = reward2.CountText;
		ApplyToAllColonies.Value = reward2.ApplyToAllColonies;
	}

	protected override void DisposeImplementation()
	{
	}

	private void SetIcon(RewardUI rewardUI)
	{
		if (rewardUI is RewardAddFeatureUI featureIcon)
		{
			SetFeatureIcon(featureIcon);
			return;
		}
		Icon.Value = rewardUI.Icon;
		IconColor.Value = Color.white;
	}

	private void SetFeatureIcon(RewardAddFeatureUI rewardAddFeatureUI)
	{
		if (rewardAddFeatureUI.Icon != null)
		{
			Icon.Value = rewardAddFeatureUI.Icon;
			IconColor.Value = Color.white;
		}
		else if (rewardAddFeatureUI.Fact != null)
		{
			Icon.Value = UIUtility.GetIconByText(rewardAddFeatureUI.Name);
			IconColor.Value = UIUtility.GetColorByText(rewardAddFeatureUI.Name);
			Acronym.Value = UIUtility.GetAbilityAcronym(rewardAddFeatureUI.Fact);
		}
	}

	private void SetVisualType(RewardUI rewardUI)
	{
		if (!(rewardUI is RewardItemUI))
		{
			if (!(rewardUI is RewardConsumableUI))
			{
				if (!(rewardUI is RewardAddFeatureUI))
				{
					if (!(rewardUI is RewardStartContractUI))
					{
						if (rewardUI is RewardCargoUI || rewardUI is RewardChangeStatContentmentUI || rewardUI is RewardChangeStatEfficiencyUI || rewardUI is RewardChangeStatSecurityUI || rewardUI is RewardColonyTraitUI || rewardUI is RewardModifyOldWoundsDelayRoundsUI || rewardUI is RewardModifyWoundDamagePerTurnThresholdHPFractionUI || rewardUI is RewardModifyWoundsStackForTraumaUI || rewardUI is RewardProfitFactorUI || rewardUI is RewardReputationUI || rewardUI is RewardResourceNotFromColonyUI || rewardUI is RewardResourceProjectUI || rewardUI is RewardSoulMarkUI || rewardUI is RewardUnhideUnitsOnSceneREUI || rewardUI is RewardAllRoutesNotDeadlyUI || rewardUI is RewardChangeNewPassageCostUI || rewardUI is RewardScrapUI || rewardUI is RewardActivateSpawnersUI || rewardUI is RewardVendorDiscountUI || rewardUI is RewardNavigatorResourceUI)
						{
							VisualType = RewardElementVisualType.Default;
						}
						else
						{
							VisualType = RewardElementVisualType.Default;
						}
					}
					else
					{
						VisualType = RewardElementVisualType.Contract;
					}
				}
				else
				{
					VisualType = RewardElementVisualType.Feature;
				}
			}
			else
			{
				VisualType = RewardElementVisualType.Consumable;
			}
		}
		else
		{
			VisualType = RewardElementVisualType.Item;
		}
	}
}
