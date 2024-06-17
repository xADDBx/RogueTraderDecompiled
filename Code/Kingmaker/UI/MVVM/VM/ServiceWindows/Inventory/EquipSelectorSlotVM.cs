using Kingmaker.Code.UI.MVVM.VM.Tooltip.Templates;
using Kingmaker.Items;
using Kingmaker.PubSubSystem.Core;
using Owlcat.Runtime.UI.SelectionGroup;
using Owlcat.Runtime.UI.Tooltips;
using Owlcat.Runtime.UniRx;
using UniRx;
using UnityEngine;

namespace Kingmaker.UI.MVVM.VM.ServiceWindows.Inventory;

public class EquipSelectorSlotVM : SelectionGroupEntityVM
{
	public readonly Sprite Icon;

	public readonly string DisplayName;

	public readonly ReactiveProperty<TooltipBaseTemplate> Tooltip = new ReactiveProperty<TooltipBaseTemplate>();

	public readonly ReactiveProperty<int> UsableCount = new ReactiveProperty<int>(0);

	public readonly ItemEntity Item;

	private readonly BoolReactiveProperty m_IsSelected = new BoolReactiveProperty();

	public EquipSelectorSlotVM(ItemEntity item)
		: base(allowSwitchOff: false)
	{
		AddDisposable(EventBus.Subscribe(this));
		Item = item;
		Icon = item.Icon;
		UsableCount.Value = item.Charges;
		DisplayName = item.Name;
		AddDisposable(RefreshView.Subscribe(UnSelect));
		RefreshTooltip();
	}

	protected override void DoSelectMe()
	{
		m_IsSelected.Value = true;
	}

	private void UnSelect()
	{
		m_IsSelected.Value = false;
	}

	public void RefreshTooltip(bool forceUpdate = false)
	{
		Tooltip.Value = new TooltipTemplateItem(Item, null, forceUpdate);
	}
}
