using System.Collections.Generic;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.ContextMenu;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Templates;
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

public class ShipUpgradeStructureSlotPCView : ShipUpgradeSlotPCView, IHasTooltipTemplate
{
	[SerializeField]
	protected TextMeshProUGUI m_CurrentInternalStructureLevel;

	private int m_CurrentLevel;

	private TooltipTemplateSimple m_StructureTooltip;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		TooltipConfig tooltipConfig = default(TooltipConfig);
		tooltipConfig.PriorityPivots = new List<Vector2>
		{
			new Vector2(0.5f, 0f)
		};
		TooltipConfig config = tooltipConfig;
		AddDisposable(base.ViewModel.CanUpgradeInternalStructure.Subscribe(m_CanUpgrade.gameObject.SetActive));
		AddDisposable(m_MultiButton.SetTooltip(base.ViewModel.ShipInternalStructureTooltip, config));
		AddDisposable(base.ViewModel.UpgradeCostValue.Subscribe(delegate
		{
			UpdateCostValue();
		}));
		AddDisposable(base.ViewModel.CurrentInternalStructureLevel.Subscribe(UpdateInternalStructureLevel));
	}

	protected override void SetupContextMenu()
	{
		base.SetupContextMenu();
		HeaderEntity = new ContextMenuCollectionEntity(SetupHeaderText(), SetupSubTitleText(), isHeader: true);
		UpgradeEntity = new ContextMenuCollectionEntity(SetupUpgradeText(), TryUpgrade, condition: true, base.ViewModel.CanUpgradeInternalStructure.Value, m_Upgrade, UISounds.ButtonSoundsEnum.NoSound);
		DowngradeEntity = new ContextMenuCollectionEntity(SetupDowngradeText(), TryDowngrade, condition: true, base.ViewModel.CanDowngradeInternalStructure, m_Downgrade, UISounds.ButtonSoundsEnum.NoSound);
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
		return ContextMenuText.UpgradeInternalStructure.Text;
	}

	private string SetupSubTitleText()
	{
		return UIStrings.Instance.ShipCustomization.CurrentUpgradeLevel.Text + "  " + UIUtility.ArabicToRoman(m_CurrentLevel);
	}

	private string SetupUpgradeText()
	{
		return ContextMenuText.UpgradeInternalStructure.Text + ": " + $"{Hull.InternalStructure.Blueprint.UpgradeCost[(Hull.InternalStructure.Blueprint.UpgradeCost.Length <= Hull.InternalStructure.UpgradeLevel + 1) ? Hull.InternalStructure.UpgradeLevel : (Hull.InternalStructure.UpgradeLevel + 1)]} " + UIStrings.Instance.ShipCustomization.Scrap.Text;
	}

	private string SetupDowngradeText()
	{
		return ContextMenuText.DowngradeInternalStructure.Text + ": " + $"{Hull.InternalStructure.Blueprint.UpgradeCost[Hull.InternalStructure.UpgradeLevel]} " + UIStrings.Instance.ShipCustomization.Scrap.Text;
	}

	protected override void TryUpgrade()
	{
		Game.Instance.GameCommandQueue.UpgradeSystemComponent(SystemComponent.SystemComponentType.InternalStructure);
	}

	protected override void TryDowngrade()
	{
		Game.Instance.GameCommandQueue.DowngradeSystemComponent(SystemComponent.SystemComponentType.InternalStructure);
	}

	private void UpdateCostValue()
	{
		HeaderEntity.SetNewTitleText(ContextMenuText.UpgradeInternalStructure.Text);
		HeaderEntity.SetSubtitleText(UIStrings.Instance.ShipCustomization.CurrentUpgradeLevel.Text + "  " + UIUtility.ArabicToRoman(m_CurrentLevel));
		UpgradeEntity.SetNewTitleText(ContextMenuText.UpgradeInternalStructure.Text + ": " + $"{Hull.InternalStructure.Blueprint.UpgradeCost[(Hull.InternalStructure.Blueprint.UpgradeCost.Length <= Hull.InternalStructure.UpgradeLevel + 1) ? Hull.InternalStructure.UpgradeLevel : (Hull.InternalStructure.UpgradeLevel + 1)]} " + UIStrings.Instance.ShipCustomization.Scrap.Text);
		UpgradeEntity.ForceUpdateInteractive(base.ViewModel.CanUpgradeInternalStructure.Value);
		DowngradeEntity.SetNewTitleText(ContextMenuText.DowngradeInternalStructure.Text + ": " + $"{Hull.InternalStructure.Blueprint.UpgradeCost[Hull.InternalStructure.UpgradeLevel]} " + UIStrings.Instance.ShipCustomization.Scrap.Text);
		DowngradeEntity.ForceUpdateInteractive(base.ViewModel.CanDowngradeInternalStructure);
	}

	private void UpdateInternalStructureLevel(int value)
	{
		m_CurrentInternalStructureLevel.text = UIUtility.ArabicToRoman(value);
		m_CurrentLevel = value;
	}

	public TooltipBaseTemplate TooltipTemplate()
	{
		return base.ViewModel.ShipInternalStructureTooltip;
	}
}
