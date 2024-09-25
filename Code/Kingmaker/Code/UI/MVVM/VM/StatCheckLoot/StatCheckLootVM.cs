using System;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Globalmap.Exploration;
using Kingmaker.Items;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.View.MapObjects;
using Owlcat.Runtime.UI.MVVM;
using UniRx;

namespace Kingmaker.Code.UI.MVVM.VM.StatCheckLoot;

public abstract class StatCheckLootVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable
{
	public readonly ReactiveProperty<bool> ShouldShow = new ReactiveProperty<bool>(initialValue: false);

	public readonly ReactiveProperty<StatCheckLootPageType> CurrentPageType = new ReactiveProperty<StatCheckLootPageType>(StatCheckLootPageType.None);

	public readonly StatCheckLootMainPageVM StatCheckLootMainPageVM;

	public readonly StatCheckLootUnitsPageVM StatCheckLootUnitsPageVM;

	private ICheckForLoot m_CheckForLoot;

	protected StatCheckLootVM()
	{
		AddDisposable(EventBus.Subscribe(this));
		AddDisposable(StatCheckLootMainPageVM = new StatCheckLootMainPageVM(HandleUnitsPageOpened, HandleStatCheckLootDoCheck, HandleCloseDialog));
		AddDisposable(StatCheckLootUnitsPageVM = new StatCheckLootUnitsPageVM(HandleConfirmSelectedUnit));
	}

	protected override void DisposeImplementation()
	{
	}

	public void HandleStatCheckLootStartCheck(ICheckForLoot checkForLoot)
	{
		ShouldShow.Value = true;
		m_CheckForLoot = checkForLoot;
		CurrentPageType.Value = StatCheckLootPageType.Main;
		StatCheckLootMainPageVM.HandlePageOpened(checkForLoot);
	}

	public void HandleStatCheckLootChecked()
	{
		OpenLoot();
		Hide();
	}

	private void HandleStatCheckLootDoCheck(BaseUnitEntity unitEntity, StatType statType)
	{
		m_CheckForLoot.Check(statType, unitEntity);
	}

	private void HandleCloseDialog()
	{
		Hide();
	}

	private void HandleUnitsPageOpened(BaseUnitEntity baseUnitEntity, StatType statType)
	{
		CurrentPageType.Value = StatCheckLootPageType.Units;
		StatCheckLootUnitsPageVM.HandlePageOpened(baseUnitEntity, statType);
	}

	private void HandleConfirmSelectedUnit(BaseUnitEntity unitEntity, StatType statType)
	{
		CurrentPageType.Value = StatCheckLootPageType.Main;
		StatCheckLootMainPageVM.HandleConfirmSelectedUnit(unitEntity, statType);
	}

	private void OpenLoot()
	{
		ILootable[] lootable = new ILootable[1] { m_CheckForLoot.GetLoot() };
		EventBus.RaiseEvent(Game.Instance.Player.MainCharacter.ToIBaseUnitEntity(), delegate(ILootInteractionHandler l)
		{
			l.HandleSpaceLootInteraction(lootable, LootContainerType.StarSystemObject, null, m_CheckForLoot.GetCheckResult());
		});
	}

	private void Hide()
	{
		ShouldShow.Value = false;
		CurrentPageType.Value = StatCheckLootPageType.None;
	}
}
