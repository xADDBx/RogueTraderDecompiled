using Kingmaker.Code.UI.MVVM.VM.Tooltip.Templates;
using Kingmaker.Items;
using Kingmaker.PubSubSystem.Core;
using Owlcat.Runtime.UI.SelectionGroup;
using Owlcat.Runtime.UI.Tooltips;
using Owlcat.Runtime.UniRx;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.VM.ShipCustomization;

public class ShipComponentItemSlotVM : SelectionGroupEntityVM
{
	public readonly Sprite Icon;

	public readonly string DisplayName;

	public readonly ReactiveProperty<TooltipBaseTemplate> Tooltip = new ReactiveProperty<TooltipBaseTemplate>();

	public readonly ItemEntity Item;

	public new readonly BoolReactiveProperty IsSelected = new BoolReactiveProperty();

	public ShipComponentItemSlotVM(ItemEntity item)
		: base(allowSwitchOff: false)
	{
		AddDisposable(EventBus.Subscribe(this));
		Item = item;
		Icon = item?.Icon;
		DisplayName = item?.Name;
		AddDisposable(RefreshView.Subscribe(UnSelect));
		Tooltip.Value = new TooltipTemplateItem(item, null, forceUpdateCache: false, replenishing: false, null, isScreenTooltip: true);
	}

	protected override void DoSelectMe()
	{
		IsSelected.Value = true;
	}

	private void UnSelect()
	{
		IsSelected.Value = false;
	}
}
