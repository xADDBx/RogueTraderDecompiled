using Kingmaker.Code.UI.MVVM.VM.SaveLoad;
using Owlcat.Runtime.UI.MVVM;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.SaveLoad.Base;

public class SaveLoadMenuBaseView : ViewBase<SaveLoadMenuVM>
{
	[SerializeField]
	private SaveLoadMenuSelectorBaseView m_Selector;

	private bool m_IsInit;

	public void Initialize()
	{
		if (!m_IsInit)
		{
			m_Selector.Initialize();
			m_IsInit = true;
		}
	}

	protected override void BindViewImplementation()
	{
		m_Selector.Bind(base.ViewModel.SelectionGroup);
	}

	protected override void DestroyViewImplementation()
	{
	}
}
