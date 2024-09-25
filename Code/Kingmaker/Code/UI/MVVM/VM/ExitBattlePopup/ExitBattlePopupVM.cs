using System;
using System.Collections.Generic;
using Kingmaker.Blueprints;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.CargoManagement.Components;
using Kingmaker.Code.UI.MVVM.VM.Slots;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.GameCommands;
using Kingmaker.Items;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.MVVM.VM.ExitBattlePopup;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Levelup.Obsolete.Blueprints;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.Utility;
using UniRx;
using UnityEngine;
using Warhammer.SpaceCombat;

namespace Kingmaker.Code.UI.MVVM.VM.ExitBattlePopup;

public class ExitBattlePopupVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable, IEndSpaceCombatHandler, ISubscriber, IExitSpaceCombatHandler, ISpaceCombatRewardUIHandler, IStarshipExpToNextLevelHandler, ISubscriber<IStarshipEntity>
{
	public readonly ReactiveProperty<bool> IsActive = new ReactiveProperty<bool>();

	public readonly ReactiveProperty<int> CurrentLevel = new ReactiveProperty<int>();

	public readonly ReactiveProperty<int> ExpLevel = new ReactiveProperty<int>();

	public readonly ReactiveProperty<int> LevelDiff = new ReactiveProperty<int>();

	public readonly ReactiveProperty<int> GainedExpAmount = new ReactiveProperty<int>();

	public readonly ReactiveProperty<int> CurrentExp = new ReactiveProperty<int>(0);

	public readonly ReactiveProperty<int> PrevExp = new ReactiveProperty<int>(0);

	public readonly ReactiveProperty<int> NextLevelExp = new ReactiveProperty<int>(0);

	public readonly ReactiveProperty<float> ExpRatio = new ReactiveProperty<float>(0f);

	public readonly ReactiveProperty<bool> IsUpgradeAvailable = new ReactiveProperty<bool>();

	public readonly ReactiveProperty<bool> HasItems = new ReactiveProperty<bool>();

	public readonly ReactiveProperty<bool> HasCargo = new ReactiveProperty<bool>();

	public readonly ReactiveCommand UpdateRewards = new ReactiveCommand();

	public readonly ScrapRewardSlotVM ScrapVM;

	public SlotsGroupVM<ItemSlotVM> ItemsSlotsGroup;

	public readonly AutoDisposingReactiveCollection<CargoRewardSlotVM> CargoRewards = new AutoDisposingReactiveCollection<CargoRewardSlotVM>();

	private readonly List<BlueprintItemReference> m_Items = new List<BlueprintItemReference>();

	private readonly List<int> m_ItemCounts = new List<int>();

	private readonly List<BlueprintCargoReference> m_Cargoes = new List<BlueprintCargoReference>();

	public ExitBattlePopupVM()
	{
		AddDisposable(EventBus.Subscribe(this));
		AddDisposable(ScrapVM = new ScrapRewardSlotVM());
	}

	protected override void DisposeImplementation()
	{
		m_Items.Clear();
		m_ItemCounts.Clear();
		m_Cargoes.Clear();
	}

	public void HandleEndSpaceCombat()
	{
		IsActive.Value = true;
	}

	public void ExitBattle(bool forceOpenVoidshipUpgrade = false)
	{
		Game.Instance.GameCommandQueue.AddCommand(new ExitSpaceCombatGameCommand(forceOpenVoidshipUpgrade));
	}

	void IExitSpaceCombatHandler.HandleExitSpaceCombat()
	{
		IsActive.Value = false;
	}

	public void HandleSpaceCombatReward(List<BlueprintItemReference> items, List<int> itemCounts, List<BlueprintCargoReference> cargoes, int scrap)
	{
		m_Items.AddRange(items);
		m_ItemCounts.AddRange(itemCounts);
		m_Cargoes.AddRange(cargoes);
		ScrapVM.IncreaseAmount(scrap);
		SetItems();
		SetCargo();
		UpdateRewards.Execute();
	}

	public void HandleStarshipExpToNextLevel(int currentLevel, int expToNextLevel, int gainedExp)
	{
		StarshipEntity starshipEntity = EventInvokerExtensions.StarshipEntity;
		if (starshipEntity != null)
		{
			PartUnitProgression progression = starshipEntity.Progression;
			GainedExpAmount.Value = gainedExp;
			int experience = progression.Experience;
			BlueprintStatProgression experienceTable = starshipEntity.StarshipProgression.ExperienceTable;
			int level = Mathf.Min(experienceTable.Bonuses.Length - 1, currentLevel + 1);
			CurrentLevel.Value = progression.CharacterLevel;
			ExpLevel.Value = currentLevel;
			LevelDiff.Value = currentLevel - progression.CharacterLevel;
			NextLevelExp.Value = experienceTable.GetBonus(level);
			PrevExp.Value = experience - gainedExp;
			ExpRatio.Value = (float)experience / (float)(NextLevelExp.Value - PrevExp.Value);
			CurrentExp.Value = experience;
			IsUpgradeAvailable.Value = currentLevel > progression.CharacterLevel;
		}
	}

	private void SetItems()
	{
		ItemsCollection itemsCollection = new ItemsCollection(null);
		for (int i = 0; i < m_Items.Count; i++)
		{
			itemsCollection.Add(m_Items[i], m_ItemCounts[i]);
		}
		HasItems.Value = !itemsCollection.Empty();
		AddDisposable(ItemsSlotsGroup = new ItemSlotsGroupVM(itemsCollection, itemsCollection, 0, 0));
	}

	private void SetCargo()
	{
		CargoRewards.Clear();
		foreach (BlueprintCargoReference cargo in m_Cargoes)
		{
			CargoRewardSlotVM cargoRewardSlotVM = new CargoRewardSlotVM(cargo.Get());
			AddDisposable(cargoRewardSlotVM);
			CargoRewards.Add(cargoRewardSlotVM);
		}
		HasCargo.Value = !m_Cargoes.Empty();
	}
}
