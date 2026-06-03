using System.Collections.Generic;
using Kingmaker.Code.UI.MVVM.VM.ActionBar;
using Kingmaker.Code.UI.MVVM.VM.ActionBar.Surface;
using Kingmaker.UI.Models.UnitSettings;
using Owlcat.Runtime.UI.MVVM;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.ActionBar;

public abstract class SurfaceActionBarPartAbilitiesBaseView : ViewBase<SurfaceActionBarPartAbilitiesVM>
{
	[SerializeField]
	protected RectTransform m_TooltipPlace;

	protected int SlotsInRow => 10;

	public abstract void Initialize();

	public virtual void UpdateGrayscale()
	{
	}

	protected List<ActionBarSlotVM> GetGridSlots(out MechanicActionBarSlot overdriveAbility)
	{
		overdriveAbility = ((base.ViewModel.Unit.Entity != null) ? base.ViewModel.Unit.Entity.UISettings.GetAugmentsOverdriveSlotSlot() : new MechanicActionBarSlotEmpty());
		bool flag = !(overdriveAbility is MechanicActionBarSlotEmpty);
		List<ActionBarSlotVM> list = new List<ActionBarSlotVM>(base.ViewModel.Slots.Count);
		foreach (ActionBarSlotVM slot in base.ViewModel.Slots)
		{
			if (!flag || !(slot.MechanicActionBarSlot.KeyName == overdriveAbility.KeyName))
			{
				list.Add(slot);
			}
		}
		return list;
	}
}
