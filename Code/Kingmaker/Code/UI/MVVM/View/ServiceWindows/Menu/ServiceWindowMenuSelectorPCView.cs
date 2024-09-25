using System.Collections.Generic;
using System.Linq;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.Menu;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.SelectionGroup;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.ServiceWindows.Menu;

public class ServiceWindowMenuSelectorPCView : ViewBase<SelectionGroupRadioVM<ServiceWindowsMenuEntityVM>>
{
	[SerializeField]
	private List<ServiceWindowsMenuEntityPCView> m_MenuEntities;

	public void Initialize()
	{
		if (m_MenuEntities.Empty())
		{
			m_MenuEntities = GetComponentsInChildren<ServiceWindowsMenuEntityPCView>().ToList();
		}
	}

	protected override void BindViewImplementation()
	{
		for (int i = 0; i < m_MenuEntities.Count; i++)
		{
			if (i >= base.ViewModel.EntitiesCollection.Count)
			{
				m_MenuEntities[i].gameObject.SetActive(value: false);
			}
			else
			{
				m_MenuEntities[i].Bind(base.ViewModel.EntitiesCollection[i]);
			}
		}
	}

	protected override void DestroyViewImplementation()
	{
	}
}
