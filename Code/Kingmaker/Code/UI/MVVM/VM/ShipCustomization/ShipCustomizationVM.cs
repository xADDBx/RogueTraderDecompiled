using System;
using Kingmaker.Code.UI.MVVM.VM.Slots;
using Kingmaker.Code.UI.MVVM.VM.Space;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.GameModes;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.MVVM.VM.ShipCustomization.Posts;
using Kingmaker.UnitLogic.Levelup;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.Utility;
using UniRx;

namespace Kingmaker.Code.UI.MVVM.VM.ShipCustomization;

public class ShipCustomizationVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable
{
	public readonly ShipVM SpaceShipVM;

	public readonly ShipStatsVM ShipStatsVM;

	public readonly ShipHealthAndRepairVM ShipHealthAndRepairVM;

	public readonly ShipTabsNavigationVM Navigation;

	public readonly LensSelectorVM Selector;

	private readonly ReactiveProperty<LevelUpManager> m_LevelUpManager = new ReactiveProperty<LevelUpManager>();

	private AutoDisposingList<ShipComponentSlotVM> m_AllSlots = new AutoDisposingList<ShipComponentSlotVM>();

	private readonly Action m_CloseAction;

	public readonly BoolReactiveProperty CanChangeEquipment = new BoolReactiveProperty();

	public readonly ReactiveProperty<ShipCustomizationTab> ActiveTab = new ReactiveProperty<ShipCustomizationTab>();

	private readonly ReactiveProperty<BaseUnitEntity> m_Unit = new ReactiveProperty<BaseUnitEntity>();

	public ShipUpgradeVm ShipUpgradeVm;

	public ShipSkillsVM ShipSkillsVM;

	public ShipPostsVM ShipPostsVM;

	public ShipCustomizationVM(Action closeAction, ShipCustomizationTab tab = ShipCustomizationTab.Upgrade)
	{
		m_CloseAction = closeAction;
		AddDisposable(EventBus.Subscribe(this));
		CanChangeEquipment.Value = Game.Instance.CurrentMode == GameModeType.SpaceCombat;
		AddDisposable(SpaceShipVM = new ShipVM(ActiveTab));
		AddDisposable(ShipStatsVM = new ShipStatsVM());
		AddDisposable(ShipHealthAndRepairVM = new ShipHealthAndRepairVM(CanChangeEquipment.Value));
		AddDisposable(Selector = new LensSelectorVM(needToResetPosition: false));
		AddDisposable(Navigation = new ShipTabsNavigationVM());
		AddDisposable(Navigation.ActiveTab.Subscribe(delegate(ShipCustomizationTab val)
		{
			SelectWindow(val);
		}));
		m_Unit.Value = Game.Instance.Player.PlayerShip;
		SetCurrentTab(tab);
		InitStats();
	}

	protected override void DisposeImplementation()
	{
		m_AllSlots.Clear();
		ShipUpgradeVm?.Dispose();
		ShipSkillsVM?.Dispose();
		ShipPostsVM?.Dispose();
	}

	private void SelectWindow(ShipCustomizationTab tab)
	{
		switch (tab)
		{
		case ShipCustomizationTab.Upgrade:
			ShipUpgradeVm?.Dispose();
			ShipUpgradeVm = new ShipUpgradeVm(CanChangeEquipment.Value);
			break;
		case ShipCustomizationTab.Skills:
			ShipSkillsVM?.Dispose();
			ShipSkillsVM = new ShipSkillsVM(m_Unit, m_LevelUpManager, CanChangeEquipment.Value);
			break;
		case ShipCustomizationTab.Posts:
			ShipPostsVM?.Dispose();
			ShipPostsVM = new ShipPostsVM(CanChangeEquipment.Value);
			break;
		}
		ActiveTab.Value = tab;
	}

	public void SetCurrentTab(ShipCustomizationTab pageType)
	{
		if (ActiveTab.Value != pageType)
		{
			Navigation.SetActiveTab(pageType);
		}
	}

	private void InitStats()
	{
		SpaceShipVM.ShouldShow.Value = true;
	}

	public void Close()
	{
		m_CloseAction?.Invoke();
	}
}
