using System.Collections.Generic;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Cargo;
using Kingmaker.Blueprints.Root;
using Kingmaker.Code.UI.MVVM.VM.Colonization.Rewards;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.CargoManagement.Components;
using Kingmaker.Code.UI.MVVM.VM.Slots;
using Kingmaker.GameCommands;
using Kingmaker.Globalmap.Blueprints.Colonization;
using Kingmaker.Globalmap.Colonization;
using Kingmaker.Globalmap.Colonization.Rewards;
using Kingmaker.Items;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.MVVM.VM.Colonization;
using Kingmaker.UI.MVVM.VM.Colonization.Rewards;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.UI.Utility;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.VM.Colonization;

public class ColonyRewardsVM : ColonyUIComponentVM, IColonyRewardsUIHandler, ISubscriber
{
	public readonly ReactiveProperty<bool> ShouldShow = new ReactiveProperty<bool>(initialValue: false);

	public readonly ReactiveProperty<bool> HasFinishedProject = new ReactiveProperty<bool>(initialValue: false);

	public readonly ReactiveProperty<string> FinishedProjectName = new ReactiveProperty<string>();

	public readonly ReactiveProperty<Sprite> FinishedProjectIcon = new ReactiveProperty<Sprite>();

	public readonly ReactiveProperty<bool> HasStats = new ReactiveProperty<bool>();

	public readonly ReactiveProperty<string> ColonyName = new ReactiveProperty<string>();

	public readonly ReactiveProperty<string> ContentmentStatText = new ReactiveProperty<string>();

	public readonly ReactiveProperty<string> EfficiencyStatText = new ReactiveProperty<string>();

	public readonly ReactiveProperty<string> SecurityStatText = new ReactiveProperty<string>();

	public readonly ReactiveProperty<bool> HasStatsAllColonies = new ReactiveProperty<bool>();

	public readonly ReactiveProperty<string> ContentmentStatAllColoniesText = new ReactiveProperty<string>();

	public readonly ReactiveProperty<string> EfficiencyStatAllColoniesText = new ReactiveProperty<string>();

	public readonly ReactiveProperty<string> SecurityStatAllColoniesText = new ReactiveProperty<string>();

	public readonly ReactiveProperty<bool> HasItems = new ReactiveProperty<bool>();

	public readonly ReactiveProperty<bool> HasCargo = new ReactiveProperty<bool>();

	public readonly ReactiveProperty<bool> HasOtherRewards = new ReactiveProperty<bool>();

	public SlotsGroupVM<ItemSlotVM> SlotsGroup;

	public readonly AutoDisposingReactiveCollection<CargoRewardSlotVM> CargoRewards = new AutoDisposingReactiveCollection<CargoRewardSlotVM>();

	public readonly AutoDisposingReactiveCollection<ColonyRewardsOtherRewardVM> OtherRewards = new AutoDisposingReactiveCollection<ColonyRewardsOtherRewardVM>();

	public readonly ReactiveCommand UpdateRewards = new ReactiveCommand();

	public ColonyRewardsVM()
	{
		AddDisposable(EventBus.Subscribe(this));
	}

	protected override void SetColonyImpl(Colony colony)
	{
		if (colony == null)
		{
			return;
		}
		ColonyName.Value = colony.Blueprint.ColonyName;
		ColonyProject colonyProject = colony.FinishedProjectsSinceLastVisit.FirstOrDefault();
		if (colonyProject != null)
		{
			HasFinishedProject.Value = true;
			BlueprintColonyProject blueprint = colonyProject.Blueprint;
			FinishedProjectName.Value = blueprint.Name;
			FinishedProjectIcon.Value = (blueprint.Icon ? blueprint.Icon : UIConfig.Instance.UIIcons.DefaultColonyProjectIcon);
			OtherRewards.Clear();
			foreach (Reward component in blueprint.GetComponents<Reward>())
			{
				RewardUI reward = RewardUIFactory.GetReward(component);
				SetRewards(reward);
			}
		}
		SetItems(colony.LootToReceive.Items);
		SetCargo(colony.LootToReceive.Cargo);
		UpdateRewards.Execute();
	}

	public void HandleHide()
	{
		ClearFinishedProjects();
		AddRewards();
		ShouldShow.Value = false;
		HasStats.Value = false;
		HasStatsAllColonies.Value = false;
		HasItems.Value = false;
		HasCargo.Value = false;
		CargoRewards.Clear();
		HasOtherRewards.Value = false;
		OtherRewards.Clear();
	}

	void IColonyRewardsUIHandler.HandleColonyRewardsShow(Colony colony)
	{
		if (m_Colony == colony)
		{
			ShowRewards();
		}
	}

	private void ShowRewards()
	{
		if (HasStats.Value || HasItems.Value || HasCargo.Value || HasOtherRewards.Value)
		{
			ShouldShow.Value = true;
		}
	}

	private void SetRewards(RewardUI rewardUI)
	{
		bool applyToAllColonies = rewardUI.ApplyToAllColonies;
		if (!(rewardUI is RewardChangeStatContentmentUI))
		{
			if (!(rewardUI is RewardChangeStatEfficiencyUI))
			{
				if (!(rewardUI is RewardChangeStatSecurityUI))
				{
					if (rewardUI is RewardProfitFactorUI || rewardUI is RewardResourceProjectUI || rewardUI is RewardScrapUI || rewardUI is RewardStartContractUI || rewardUI is RewardSoulMarkUI)
					{
						SetOtherReward(rewardUI);
					}
				}
				else
				{
					SetStatReward(applyToAllColonies ? SecurityStatAllColoniesText : SecurityStatText, rewardUI, applyToAllColonies);
				}
			}
			else
			{
				SetStatReward(applyToAllColonies ? EfficiencyStatAllColoniesText : EfficiencyStatText, rewardUI, applyToAllColonies);
			}
		}
		else
		{
			SetStatReward(applyToAllColonies ? ContentmentStatAllColoniesText : ContentmentStatText, rewardUI, applyToAllColonies);
		}
	}

	private void SetStatReward(ReactiveProperty<string> stat, RewardUI rewardUI, bool allColonies)
	{
		stat.Value = rewardUI.Description + " " + rewardUI.CountText;
		if (allColonies)
		{
			HasStatsAllColonies.Value = true;
		}
		else
		{
			HasStats.Value = true;
		}
	}

	private void SetOtherReward(RewardUI rewardUI)
	{
		ColonyRewardsOtherRewardVM colonyRewardsOtherRewardVM = new ColonyRewardsOtherRewardVM(rewardUI);
		AddDisposable(colonyRewardsOtherRewardVM);
		OtherRewards.Add(colonyRewardsOtherRewardVM);
		HasOtherRewards.Value = true;
	}

	private void SetItems(ItemsCollection items)
	{
		HasItems.Value = items != null && !items.Empty();
		AddDisposable(SlotsGroup = new ItemSlotsGroupVM(items, items, 0, 0));
	}

	private void SetCargo(List<BlueprintCargoReference> cargoes)
	{
		CargoRewards.Clear();
		foreach (BlueprintCargoReference cargo in cargoes)
		{
			BlueprintCargo blueprintCargo = cargo.Get();
			CargoRewardSlotVM cargoRewardSlotVM = CargoRewards.FirstOrDefault((CargoRewardSlotVM c) => c.Origin == blueprintCargo.OriginType);
			if (cargoRewardSlotVM != null)
			{
				cargoRewardSlotVM.IncreaseCount();
				continue;
			}
			CargoRewardSlotVM cargoRewardSlotVM2 = new CargoRewardSlotVM(blueprintCargo);
			AddDisposable(cargoRewardSlotVM2);
			CargoRewards.Add(cargoRewardSlotVM2);
		}
		HasCargo.Value = !cargoes.Empty();
	}

	private void AddRewards()
	{
		if (m_Colony != null)
		{
			Game.Instance.GameCommandQueue.ReceiveLootFromColony((ColonyRef)m_Colony);
		}
	}

	private void ClearFinishedProjects()
	{
		if (m_Colony != null)
		{
			Game.Instance.GameCommandQueue.ClearFinishedProjectsSinceLastVisit((ColonyRef)m_Colony);
		}
	}
}
