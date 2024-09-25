using System.Collections.Generic;
using System.Linq;
using Kingmaker.Code.UI.MVVM.VM.Settings.Menu;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.SelectionGroup;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.Settings.PC.Menu;

public class SettingsMenuSelectorPCView : ViewBase<SelectionGroupRadioVM<SettingsMenuEntityVM>>
{
	[SerializeField]
	private List<SettingsMenuEntityPCView> m_MenuEntities;

	public void Initialize()
	{
		if (m_MenuEntities.Empty())
		{
			m_MenuEntities = GetComponentsInChildren<SettingsMenuEntityPCView>().ToList();
		}
	}

	protected override void BindViewImplementation()
	{
		for (int i = 0; i < m_MenuEntities.Count; i++)
		{
			if (i >= base.ViewModel.EntitiesCollection.Count)
			{
				m_MenuEntities[i].gameObject.SetActive(value: false);
				continue;
			}
			m_MenuEntities[i].gameObject.SetActive(value: true);
			m_MenuEntities[i].Bind(base.ViewModel.EntitiesCollection[i]);
		}
	}

	protected override void DestroyViewImplementation()
	{
	}

	public void OnNext()
	{
		base.ViewModel.SelectNextValidEntity();
	}

	public void OnPrev()
	{
		base.ViewModel.SelectPrevValidEntity();
	}
}
