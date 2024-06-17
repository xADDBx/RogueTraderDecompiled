using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Cargo;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Cargo;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Templates;
using Kingmaker.UI.Common;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.Tooltips;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.VM.ServiceWindows.CargoManagement.Components;

public class CargoRewardSlotVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable
{
	public Sprite TypeIcon;

	public readonly ReactiveProperty<int> TotalFillValue = new ReactiveProperty<int>();

	public readonly ReactiveProperty<int> Count = new ReactiveProperty<int>(1);

	public readonly ReactiveProperty<TooltipBaseTemplate> Tooltip = new ReactiveProperty<TooltipBaseTemplate>();

	private ItemsItemOrigin m_Origin;

	public ItemsItemOrigin Origin => m_Origin;

	public CargoRewardSlotVM(BlueprintCargo blueprintCargo)
	{
		m_Origin = blueprintCargo?.OriginType ?? ItemsItemOrigin.None;
		TypeIcon = UIConfig.Instance.UIIcons.CargoIcons.GetIconByOrigin(m_Origin);
		CargoInventory component = blueprintCargo.GetComponent<CargoInventory>();
		TotalFillValue.Value = component?.UnusableVolumePercent ?? 0;
		Tooltip.Value = new TooltipTemplateSimple(UIStrings.Instance.CargoTexts.GetLabelByOrigin(m_Origin), blueprintCargo?.Description);
	}

	public CargoRewardSlotVM(CargoEntity cargo)
	{
		m_Origin = (cargo?.Blueprint?.OriginType).GetValueOrDefault();
		TypeIcon = UIConfig.Instance.UIIcons.CargoIcons.GetIconByOrigin(m_Origin);
		TotalFillValue.Value = cargo?.FilledVolumePercent ?? 0;
		Tooltip.Value = new TooltipTemplateSimple(UIStrings.Instance.CargoTexts.GetLabelByOrigin(m_Origin), cargo?.Blueprint?.Description);
	}

	protected override void DisposeImplementation()
	{
	}

	public void IncreaseCount()
	{
		Count.Value++;
	}
}
