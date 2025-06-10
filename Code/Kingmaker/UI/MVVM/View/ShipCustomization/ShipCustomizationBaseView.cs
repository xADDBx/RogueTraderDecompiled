using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM;
using Kingmaker.Code.UI.MVVM.View.Slots;
using Kingmaker.Code.UI.MVVM.View.Space.PC;
using Kingmaker.Code.UI.MVVM.VM.ShipCustomization;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.Common.Animations;
using Kingmaker.UI.InputSystems;
using Kingmaker.UI.Models;
using Kingmaker.UI.MVVM.View.ShipCustomization.ShipPosts;
using Kingmaker.UI.Sound;
using Kingmaker.UnitLogic.Levelup.Obsolete;
using Owlcat.Runtime.UI.MVVM;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.UI.MVVM.View.ShipCustomization;

public class ShipCustomizationBaseView<TShipUpgradeView, TShipSkills, TShipHealthAndRepair, TShipAbilitiesView> : ViewBase<ShipCustomizationVM>, IInitializable
{
	[Header("Common Block")]
	[SerializeField]
	protected FadeAnimator m_FadeAnimator;

	[Header("Navigation")]
	[SerializeField]
	protected ShipTabsNavigationPCView m_ShipTabsNavigationPCView;

	[SerializeField]
	private GameObject m_HasShipLevel;

	[Header("Ship Info")]
	[SerializeField]
	protected GameObject m_ShipInfo;

	[SerializeField]
	protected ShipPCView m_SpaceShipPCView;

	[SerializeField]
	protected ShipStatsPCView m_ShipStatsPCView;

	[SerializeField]
	protected TShipHealthAndRepair m_ShipHealthAndRepairView;

	[SerializeField]
	protected FlexibleLensSelectorView m_SelectorView;

	[FormerlySerializedAs("m_ShipUpgradePCView")]
	[SerializeField]
	protected TShipUpgradeView m_ShipUpgradeView;

	[Header("Skills And Posts")]
	[SerializeField]
	protected FadeAnimator m_SkillsAndPostsFadeAnimator;

	[SerializeField]
	protected TShipSkills m_ShipSkillsPCView;

	[SerializeField]
	protected PostsBaseView m_ShipPostsView;

	[Header("Abilities")]
	[SerializeField]
	protected TShipAbilitiesView m_ShipAbilitiesView;

	[Header("Lock state")]
	[SerializeField]
	protected GameObject LockState;

	[SerializeField]
	protected TextMeshProUGUI LockStateText;

	private readonly BoolReactiveProperty m_NeedShowShipLevel = new BoolReactiveProperty();

	public virtual void Initialize()
	{
		base.gameObject.SetActive(value: false);
	}

	protected override void BindViewImplementation()
	{
		ShowWindow();
		m_ShipTabsNavigationPCView.Bind(base.ViewModel.Navigation);
		AddDisposable(MainThreadDispatcher.InfrequentUpdateAsObservable().Subscribe(delegate
		{
			OnUpdateHandler();
		}));
		m_SpaceShipPCView.Bind(base.ViewModel.SpaceShipVM);
		m_ShipStatsPCView.Bind(base.ViewModel.ShipStatsVM);
		AddDisposable(EscHotkeyManager.Instance.Subscribe(Close));
		AddDisposable(base.ViewModel.CanChangeEquipment.Subscribe(LockState.SetActive));
		BindShip();
		AddDisposable(base.ViewModel.ActiveTab.Subscribe(BindSelectedView));
		m_SelectorView.Bind(base.ViewModel.Selector);
		AddDisposable(m_NeedShowShipLevel.Subscribe(m_HasShipLevel.SetActive));
		m_SelectorView.ChangeTab((int)base.ViewModel.ActiveTab.Value);
		LockStateText.text = UIStrings.Instance.ExplorationTexts.ExploNotInteractable.Text;
	}

	protected override void DestroyViewImplementation()
	{
		HideWindow();
	}

	private void OnUpdateHandler()
	{
		m_NeedShowShipLevel.Value = LevelUpController.CanLevelUp(Game.Instance.Player.PlayerShip);
	}

	protected virtual void BindShip()
	{
	}

	protected virtual void BindSelectedView(ShipCustomizationTab tab)
	{
		SetupAdditionalWindows(tab == ShipCustomizationTab.Upgrade);
	}

	private void SetupAdditionalWindows(bool value)
	{
		m_SpaceShipPCView.gameObject.SetActive(value);
		m_ShipStatsPCView.gameObject.SetActive(value);
	}

	protected void SetPrevTab()
	{
		TooltipHelper.HideTooltip();
		m_SelectorView.SetPrevTab();
		m_ShipTabsNavigationPCView.SetPrevTab();
	}

	protected void SetNextTab()
	{
		TooltipHelper.HideTooltip();
		m_SelectorView.SetNextTab();
		m_ShipTabsNavigationPCView.SetNextTab();
	}

	private void ShowWindow()
	{
		base.gameObject.SetActive(value: true);
		m_FadeAnimator.AppearAnimation(OnAppearEnd);
		UISounds.Instance.Sounds.Inventory.InventoryOpen.Play();
		EventBus.RaiseEvent(delegate(IShipCustomizationUIHandler h)
		{
			h.HandleOpenShipCustomization();
		});
		EventBus.RaiseEvent(delegate(IFullScreenUIHandler h)
		{
			h.HandleFullScreenUiChanged(state: true, FullScreenUIType.ShipCustomization);
		});
	}

	private void HideWindow()
	{
		m_FadeAnimator.DisappearAnimation(OnDisappearEnd);
		UISounds.Instance.Sounds.Inventory.InventoryClose.Play();
		EventBus.RaiseEvent(delegate(IShipCustomizationUIHandler h)
		{
			h.HandleCloseAllComponentsMenu();
		});
		EventBus.RaiseEvent(delegate(IFullScreenUIHandler h)
		{
			h.HandleFullScreenUiChanged(state: false, FullScreenUIType.ShipCustomization);
		});
	}

	private void OnAppearEnd()
	{
	}

	private void OnDisappearEnd()
	{
		base.gameObject.SetActive(value: false);
	}

	protected virtual void Close()
	{
		EventBus.RaiseEvent(delegate(IShipCustomizationForceUIHandler h)
		{
			h.HandleForceCloseAllComponentsMenu();
		});
	}
}
