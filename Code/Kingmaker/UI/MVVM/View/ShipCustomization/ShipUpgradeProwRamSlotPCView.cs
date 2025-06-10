using System.Collections.Generic;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.ContextMenu;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Kingmaker.GameCommands;
using Kingmaker.UI.Common;
using Kingmaker.UI.Sound;
using Owlcat.Runtime.UI.Tooltips;
using TMPro;
using UniRx;
using UnityEngine;
using Warhammer.SpaceCombat.StarshipLogic.Equipment;

namespace Kingmaker.UI.MVVM.View.ShipCustomization;

public class ShipUpgradeProwRamSlotPCView : ShipUpgradeSlotPCView, IHasTooltipTemplate
{
	[SerializeField]
	protected TextMeshProUGUI m_CurrentProwRamLevel;

	private int m_CurrentLevel;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		TooltipConfig tooltipConfig = default(TooltipConfig);
		tooltipConfig.PriorityPivots = new List<Vector2>
		{
			new Vector2(0.5f, 0f)
		};
		TooltipConfig config = tooltipConfig;
		AddDisposable(base.ViewModel.CanUpgradeProwRam.Subscribe(m_CanUpgrade.gameObject.SetActive));
		AddDisposable(m_MultiButton.SetTooltip(base.ViewModel.ShipUpgradeTooltip, config));
		AddDisposable(base.ViewModel.UpgradeCostValue.Subscribe(delegate
		{
			UpdateCostValue();
		}));
		AddDisposable(base.ViewModel.CurrentProwRamLevel.Subscribe(UpdateProwRamLevel));
	}

	protected override void SetupContextMenu()
	{
		base.SetupContextMenu();
		HeaderEntity = new ContextMenuCollectionEntity(SetupHeaderText(), SetupSubTitleText(), isHeader: true);
		UpgradeEntity = new ContextMenuCollectionEntity(SetupUpgradeText(), TryUpgrade, condition: true, base.ViewModel.CanUpgradeProwRam.Value, m_Upgrade, UISounds.ButtonSoundsEnum.NoSound);
		DowngradeEntity = new ContextMenuCollectionEntity(SetupDowngradeText(), TryDowngrade, condition: true, base.ViewModel.CanDowngradeRam, m_Downgrade, UISounds.ButtonSoundsEnum.NoSound);
		base.ViewModel.ContextMenu.Value = new List<ContextMenuCollectionEntity>
		{
			HeaderEntity,
			new ContextMenuCollectionEntity(null, null),
			UpgradeEntity,
			DowngradeEntity
		};
	}

	private string SetupHeaderText()
	{
		return ContextMenuText.UpgradeProwRaw.Text;
	}

	private string SetupSubTitleText()
	{
		return UIStrings.Instance.ShipCustomization.CurrentUpgradeLevel.Text + "  " + UIUtility.ArabicToRoman(m_CurrentLevel);
	}

	private string SetupUpgradeText()
	{
		return ContextMenuText.UpgradeProwRaw.Text + ": " + $"{Hull.ProwRam.Blueprint.UpgradeCost[(Hull.ProwRam.Blueprint.UpgradeCost.Length <= Hull.ProwRam.UpgradeLevel + 1) ? Hull.ProwRam.UpgradeLevel : (Hull.ProwRam.UpgradeLevel + 1)]} " + UIStrings.Instance.ShipCustomization.Scrap.Text;
	}

	private string SetupDowngradeText()
	{
		return ContextMenuText.DowngradeProwRaw.Text + ": " + $"{Hull.ProwRam.Blueprint.UpgradeCost[Hull.ProwRam.UpgradeLevel]} " + UIStrings.Instance.ShipCustomization.Scrap.Text;
	}

	protected override void TryUpgrade()
	{
		Game.Instance.GameCommandQueue.UpgradeSystemComponent(SystemComponent.SystemComponentType.ProwRam);
	}

	protected override void TryDowngrade()
	{
		Game.Instance.GameCommandQueue.DowngradeSystemComponent(SystemComponent.SystemComponentType.ProwRam);
	}

	private void UpdateCostValue()
	{
		HeaderEntity.SetNewTitleText(ContextMenuText.UpgradeProwRaw.Text);
		HeaderEntity.SetSubtitleText(UIStrings.Instance.ShipCustomization.CurrentUpgradeLevel.Text + "  " + UIUtility.ArabicToRoman(m_CurrentLevel));
		UpgradeEntity.SetNewTitleText(ContextMenuText.UpgradeProwRaw.Text + ": " + $"{Hull.ProwRam.Blueprint.UpgradeCost[(Hull.ProwRam.Blueprint.UpgradeCost.Length <= Hull.ProwRam.UpgradeLevel + 1) ? Hull.ProwRam.UpgradeLevel : (Hull.ProwRam.UpgradeLevel + 1)]} " + UIStrings.Instance.ShipCustomization.Scrap.Text);
		UpgradeEntity.ForceUpdateInteractive(base.ViewModel.CanUpgradeProwRam.Value);
		DowngradeEntity.SetNewTitleText(ContextMenuText.DowngradeProwRaw.Text + ": " + $"{Hull.ProwRam.Blueprint.UpgradeCost[Hull.ProwRam.UpgradeLevel]} " + UIStrings.Instance.ShipCustomization.Scrap.Text);
		DowngradeEntity.ForceUpdateInteractive(base.ViewModel.CanDowngradeRam);
	}

	private void UpdateProwRamLevel(int value)
	{
		m_CurrentProwRamLevel.text = UIUtility.ArabicToRoman(value);
		m_CurrentLevel = value;
	}

	public TooltipBaseTemplate TooltipTemplate()
	{
		return base.ViewModel.ShipUpgradeTooltip;
	}
}
