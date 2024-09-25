using Kingmaker.Code.UI.MVVM.VM.Colonization.Rewards;
using Kingmaker.Globalmap.Colonization.Rewards;

namespace Kingmaker.UI.MVVM.VM.Colonization.Rewards;

public class RewardUIFactory
{
	public static RewardUI GetReward(Reward reward)
	{
		return TryCreateRewardActivateSpawnersUI(reward) ?? TryCreateRewardAddFeatureUI(reward) ?? TryCreateRewardCargoUI(reward) ?? TryCreateRewardChangeStatContentmentUI(reward) ?? TryCreateRewardChangeStatEfficiencyUI(reward) ?? TryCreateRewardChangeStatSecurityUI(reward) ?? TryCreateRewardColonyTraitUI(reward) ?? TryCreateRewardConsumableUI(reward) ?? TryCreateRewardItemUI(reward) ?? TryCreateRewardModifyOldWoundsDelayRoundsUI(reward) ?? TryCreateRewardModifyWoundDamagePerTurnThresholdHPFractionUI(reward) ?? TryCreateRewardModifyWoundsStackForTraumaUI(reward) ?? TryCreateRewardProfitFactorUI(reward) ?? TryCreateRewardReputationUI(reward) ?? TryCreateRewardResourceNotFromColonyUI(reward) ?? TryCreateRewardResourceProjectUI(reward) ?? TryCreateRewardStartContractUI(reward) ?? TryCreateRewardUnhideUnitsOnSceneREUI(reward) ?? TryCreateRewardScrapUI(reward) ?? TryCreateRewardSoulMarkUI(reward) ?? TryCreateRewardVendorDiscountUI(reward) ?? TryCreateRewardAllRotesNotDeadlyUI(reward) ?? TryCreateRewardChangeNewPassageCostUI(reward) ?? new RewardUI(reward);
	}

	private static RewardUI TryCreateRewardActivateSpawnersUI(Reward reward)
	{
		if (!(reward is RewardActivateSpawners reward2))
		{
			return null;
		}
		return new RewardActivateSpawnersUI(reward2);
	}

	private static RewardUI TryCreateRewardAddFeatureUI(Reward reward)
	{
		if (!(reward is RewardAddFeature reward2))
		{
			return null;
		}
		return new RewardAddFeatureUI(reward2);
	}

	private static RewardUI TryCreateRewardCargoUI(Reward reward)
	{
		if (!(reward is RewardCargo reward2))
		{
			return null;
		}
		return new RewardCargoUI(reward2);
	}

	private static RewardUI TryCreateRewardChangeStatContentmentUI(Reward reward)
	{
		if (!(reward is RewardChangeStatContentment reward2))
		{
			return null;
		}
		return new RewardChangeStatContentmentUI(reward2);
	}

	private static RewardUI TryCreateRewardChangeStatEfficiencyUI(Reward reward)
	{
		if (!(reward is RewardChangeStatEfficiency reward2))
		{
			return null;
		}
		return new RewardChangeStatEfficiencyUI(reward2);
	}

	private static RewardUI TryCreateRewardChangeStatSecurityUI(Reward reward)
	{
		if (!(reward is RewardChangeStatSecurity reward2))
		{
			return null;
		}
		return new RewardChangeStatSecurityUI(reward2);
	}

	private static RewardUI TryCreateRewardColonyTraitUI(Reward reward)
	{
		if (!(reward is RewardColonyTrait reward2))
		{
			return null;
		}
		return new RewardColonyTraitUI(reward2);
	}

	private static RewardUI TryCreateRewardModifyWoundsStackForTraumaUI(Reward reward)
	{
		if (!(reward is RewardModifyWoundsStackForTrauma reward2))
		{
			return null;
		}
		return new RewardModifyWoundsStackForTraumaUI(reward2);
	}

	private static RewardUI TryCreateRewardModifyOldWoundsDelayRoundsUI(Reward reward)
	{
		if (!(reward is RewardModifyOldWoundsDelayRounds reward2))
		{
			return null;
		}
		return new RewardModifyOldWoundsDelayRoundsUI(reward2);
	}

	private static RewardUI TryCreateRewardModifyWoundDamagePerTurnThresholdHPFractionUI(Reward reward)
	{
		if (!(reward is RewardModifyWoundDamagePerTurnThresholdHPFraction reward2))
		{
			return null;
		}
		return new RewardModifyWoundDamagePerTurnThresholdHPFractionUI(reward2);
	}

	private static RewardUI TryCreateRewardConsumableUI(Reward reward)
	{
		if (!(reward is RewardConsumable reward2))
		{
			return null;
		}
		return new RewardConsumableUI(reward2);
	}

	private static RewardUI TryCreateRewardItemUI(Reward reward)
	{
		if (!(reward is RewardItem reward2))
		{
			return null;
		}
		return new RewardItemUI(reward2);
	}

	private static RewardUI TryCreateRewardProfitFactorUI(Reward reward)
	{
		if (!(reward is RewardProfitFactor reward2))
		{
			return null;
		}
		return new RewardProfitFactorUI(reward2);
	}

	private static RewardUI TryCreateRewardReputationUI(Reward reward)
	{
		if (!(reward is RewardReputation reward2))
		{
			return null;
		}
		return new RewardReputationUI(reward2);
	}

	private static RewardUI TryCreateRewardResourceNotFromColonyUI(Reward reward)
	{
		if (!(reward is RewardResourceNotFromColony reward2))
		{
			return null;
		}
		return new RewardResourceNotFromColonyUI(reward2);
	}

	private static RewardUI TryCreateRewardResourceProjectUI(Reward reward)
	{
		if (!(reward is RewardResourceProject reward2))
		{
			return null;
		}
		return new RewardResourceProjectUI(reward2);
	}

	private static RewardUI TryCreateRewardStartContractUI(Reward reward)
	{
		if (!(reward is RewardStartContract reward2))
		{
			return null;
		}
		return new RewardStartContractUI(reward2);
	}

	private static RewardUI TryCreateRewardUnhideUnitsOnSceneREUI(Reward reward)
	{
		if (!(reward is RewardUnhideUnitsOnSceneRE reward2))
		{
			return null;
		}
		return new RewardUnhideUnitsOnSceneREUI(reward2);
	}

	private static RewardUI TryCreateRewardScrapUI(Reward reward)
	{
		if (!(reward is RewardScrap reward2))
		{
			return null;
		}
		return new RewardScrapUI(reward2);
	}

	private static RewardUI TryCreateRewardSoulMarkUI(Reward reward)
	{
		if (!(reward is RewardSoulMark reward2))
		{
			return null;
		}
		return new RewardSoulMarkUI(reward2);
	}

	private static RewardUI TryCreateRewardVendorDiscountUI(Reward reward)
	{
		if (!(reward is RewardVendorDiscount reward2))
		{
			return null;
		}
		return new RewardVendorDiscountUI(reward2);
	}

	private static RewardUI TryCreateRewardAllRotesNotDeadlyUI(Reward reward)
	{
		if (!(reward is RewardAllRoutesNotDeadly reward2))
		{
			return null;
		}
		return new RewardAllRoutesNotDeadlyUI(reward2);
	}

	private static RewardUI TryCreateRewardChangeNewPassageCostUI(Reward reward)
	{
		if (!(reward is RewardChangeNewPassageCost reward2))
		{
			return null;
		}
		return new RewardChangeNewPassageCostUI(reward2);
	}
}
