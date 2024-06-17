using System.Collections.Generic;
using System.Linq;
using Kingmaker.Code.UI.MVVM.VM.Settings.Menu;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.SelectionGroup;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.Settings.Console.Menu;

public class SettingsMenuSelectorConsoleView : ViewBase<SelectionGroupRadioVM<SettingsMenuEntityVM>>
{
	[SerializeField]
	public List<SettingsMenuEntityConsoleView> m_MenuEntities;

	public void Initialize()
	{
		if (m_MenuEntities.Empty())
		{
			m_MenuEntities = GetComponentsInChildren<SettingsMenuEntityConsoleView>().ToList();
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

	public void OnNext()
	{
		base.ViewModel.SelectNextValidEntity();
	}

	public void OnPrev()
	{
		base.ViewModel.SelectPrevValidEntity();
	}

	protected override void DestroyViewImplementation()
	{
	}
}
