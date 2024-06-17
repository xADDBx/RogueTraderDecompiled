using System.Collections.Generic;
using System.Linq;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.SelectionGroup;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.Credits;

public class CreditsMenuSelectorBaseView : ViewBase<SelectionGroupRadioVM<CreditsMenuEntityVM>>
{
	[SerializeField]
	private List<CreditsMenuEntityPCView> m_MenuEntities;

	public void Initialize()
	{
		if (m_MenuEntities.Empty())
		{
			m_MenuEntities = GetComponentsInChildren<CreditsMenuEntityPCView>().ToList();
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
