using System.Collections.Generic;
using System.Linq;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.Inventory;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.SelectionGroup;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.ServiceWindows.Inventory;

public class WeaponSetSelectorPCView : ViewBase<SelectionGroupRadioVM<WeaponSetVM>>
{
	[SerializeField]
	protected List<WeaponSetBaseView> m_WeaponSetViews;

	protected override void BindViewImplementation()
	{
		int i;
		for (i = 0; i < m_WeaponSetViews.Count; i++)
		{
			m_WeaponSetViews[i].Bind(base.ViewModel.EntitiesCollection.FirstOrDefault((WeaponSetVM e) => e.Index == i));
		}
	}

	protected override void DestroyViewImplementation()
	{
	}
}
