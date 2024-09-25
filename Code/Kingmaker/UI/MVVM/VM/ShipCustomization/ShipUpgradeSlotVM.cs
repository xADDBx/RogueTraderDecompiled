using System;
using System.Collections.Generic;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.ContextMenu;
using Kingmaker.Code.UI.MVVM.VM.WarningNotification;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.SpaceCombat.StarshipLogic.Parts;
using Kingmaker.UI.MVVM.VM.Tooltip.Templates;
using Kingmaker.UI.Sound;
using Owlcat.Runtime.UI.MVVM;
using UniRx;
using Warhammer.SpaceCombat;
using Warhammer.SpaceCombat.Blueprints;
using Warhammer.SpaceCombat.StarshipLogic.Equipment;

namespace Kingmaker.UI.MVVM.VM.ShipCustomization;

public class ShipUpgradeSlotVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable, IUpgradeSystemComponentHandler, ISubscriber<StarshipEntity>, ISubscriber
{
	public readonly BoolReactiveProperty IsLocked = new BoolReactiveProperty();

	public readonly ReactiveProperty<List<ContextMenuCollectionEntity>> ContextMenu = new ReactiveProperty<List<ContextMenuCollectionEntity>>();

	public readonly ReactiveCommand UpgradeCostValue = new ReactiveCommand();

	public readonly ReactiveProperty<int> CurrentProwRamLevel = new ReactiveProperty<int>();

	public readonly ReactiveProperty<int> CurrentInternalStructureLevel = new ReactiveProperty<int>();

	public readonly TooltipTemplateShipProwRamUpgrade ShipUpgradeTooltip;

	public readonly TooltipTemplateShipInternalStructureUpgrade ShipInternalStructureTooltip;

	public readonly PlayerShipType ShipType;

	private readonly PartStarshipHull m_Hull;

	public readonly BoolReactiveProperty CanUpgradeProwRam = new BoolReactiveProperty();

	public readonly BoolReactiveProperty CanUpgradeInternalStructure = new BoolReactiveProperty();

	public bool CanDowngradeRam;

	public bool CanDowngradeInternalStructure;

	public ShipUpgradeSlotVM(bool isLocked = false)
	{
		AddDisposable(EventBus.Subscribe(this));
		m_Hull = Game.Instance.Player.PlayerShip.Hull;
		IsLocked.Value = isLocked;
		AddDisposable(UpgradeCostValue.Subscribe(delegate
		{
			UpdateValues();
		}));
		ShipType = Game.Instance.Player.PlayerShip.Blueprint.ShipType;
		ShipUpgradeTooltip = new TooltipTemplateShipProwRamUpgrade(UIStrings.Instance.ShipCustomization.UpgradeProwRam, UIStrings.Instance.ShipCustomization.UpgradeProwRamDescription);
		ShipInternalStructureTooltip = new TooltipTemplateShipInternalStructureUpgrade(UIStrings.Instance.ShipCustomization.UpgradeInternalStructure, UIStrings.Instance.ShipCustomization.UpgradeInternalStructureDescription);
		UpdateValues();
	}

	protected override void DisposeImplementation()
	{
	}

	private void UpdateValues()
	{
		CurrentProwRamLevel.Value = Game.Instance.Player.PlayerShip.Hull.ProwRam.UpgradeLevel;
		CurrentInternalStructureLevel.Value = Game.Instance.Player.PlayerShip.Hull.InternalStructure.UpgradeLevel;
		CanUpgradeProwRam.Value = m_Hull.ProwRam.IsEnoughScrap && !m_Hull.ProwRam.IsMaxLevel;
		CanUpgradeInternalStructure.Value = m_Hull.InternalStructure.IsEnoughScrap && !m_Hull.InternalStructure.IsMaxLevel;
		CanDowngradeRam = Game.Instance.Player.PlayerShip.Hull.ProwRam.UpgradeLevel > 0;
		CanDowngradeInternalStructure = Game.Instance.Player.PlayerShip.Hull.InternalStructure.UpgradeLevel > 0;
	}

	public void HandleSystemComponentUpgrade(SystemComponent.SystemComponentType componentType, SystemComponent.UpgradeResult result)
	{
		UpgradeCostValue?.Execute();
		switch (result)
		{
		case SystemComponent.UpgradeResult.Error:
			EventBus.RaiseEvent(delegate(IWarningNotificationUIHandler h)
			{
				h.HandleWarning(UIStrings.Instance.ShipCustomization.CantUpgrade.Text, addToLog: false, WarningNotificationFormat.Attention);
			});
			break;
		case SystemComponent.UpgradeResult.NotEnoughScrap:
			EventBus.RaiseEvent(delegate(IWarningNotificationUIHandler h)
			{
				h.HandleWarning(UIStrings.Instance.ShipCustomization.NotEnoughScrap.Text, addToLog: false, WarningNotificationFormat.Attention);
			});
			break;
		case SystemComponent.UpgradeResult.MaxUpgrade:
			EventBus.RaiseEvent(delegate(IWarningNotificationUIHandler h)
			{
				h.HandleWarning(UIStrings.Instance.ShipCustomization.AlreadyMaximum.Text, addToLog: false);
			});
			break;
		case SystemComponent.UpgradeResult.Successful:
			EventBus.RaiseEvent(delegate(IWarningNotificationUIHandler h)
			{
				h.HandleWarning(UIStrings.Instance.ShipCustomization.Upgraded.Text, addToLog: false);
			});
			break;
		}
		switch (result)
		{
		case SystemComponent.UpgradeResult.MaxUpgrade:
		case SystemComponent.UpgradeResult.NotEnoughScrap:
		case SystemComponent.UpgradeResult.Error:
			UISounds.Instance.Sounds.Combat.CombatGridCantPerformActionClick.Play();
			break;
		case SystemComponent.UpgradeResult.Successful:
			UISounds.Instance.Sounds.Ship.ShipUpgrade.Play();
			break;
		}
	}

	public void HandleSystemComponentDowngrade(SystemComponent.SystemComponentType componentType, SystemComponent.DowngradeResult result)
	{
		UpgradeCostValue?.Execute();
		switch (result)
		{
		case SystemComponent.DowngradeResult.Error:
			EventBus.RaiseEvent(delegate(IWarningNotificationUIHandler h)
			{
				h.HandleWarning(UIStrings.Instance.ShipCustomization.CantDowngrade.Text, addToLog: false, WarningNotificationFormat.Attention);
			});
			break;
		case SystemComponent.DowngradeResult.MinUpgrade:
			EventBus.RaiseEvent(delegate(IWarningNotificationUIHandler h)
			{
				h.HandleWarning(UIStrings.Instance.ShipCustomization.MinLevel.Text, addToLog: false, WarningNotificationFormat.Attention);
			});
			break;
		case SystemComponent.DowngradeResult.Successful:
			EventBus.RaiseEvent(delegate(IWarningNotificationUIHandler h)
			{
				h.HandleWarning(UIStrings.Instance.ShipCustomization.Downgraded.Text, addToLog: false);
			});
			break;
		}
		switch (result)
		{
		case SystemComponent.DowngradeResult.MinUpgrade:
		case SystemComponent.DowngradeResult.Error:
			UISounds.Instance.Sounds.Combat.CombatGridCantPerformActionClick.Play();
			break;
		case SystemComponent.DowngradeResult.Successful:
			UISounds.Instance.Sounds.Ship.ShipDowngrade.Play();
			break;
		}
	}
}
