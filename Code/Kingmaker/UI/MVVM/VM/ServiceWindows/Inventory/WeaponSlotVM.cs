using System;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Templates;
using Kingmaker.Items.Slots;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.Tooltips;
using UnityEngine;

namespace Kingmaker.UI.MVVM.VM.ServiceWindows.Inventory;

public class WeaponSlotVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable
{
	public readonly Sprite Icon;

	public readonly TooltipBaseTemplate Tooltip;

	private readonly WeaponSlot m_WeaponSlot;

	public WeaponSlotVM(WeaponSlot weaponSlot)
	{
		m_WeaponSlot = weaponSlot;
		Icon = weaponSlot.Item.Icon.Or(Game.Instance.BlueprintRoot.UIConfig.UIIcons.DefaultItemIcon);
		Tooltip = new TooltipTemplateItem(weaponSlot.Item);
	}

	protected override void DisposeImplementation()
	{
	}
}
