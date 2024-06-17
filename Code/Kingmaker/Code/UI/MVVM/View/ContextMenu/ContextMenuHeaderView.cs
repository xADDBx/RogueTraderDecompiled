using Kingmaker.Code.UI.MVVM.VM.ContextMenu;
using Owlcat.Runtime.UI.MVVM;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.ContextMenu;

public class ContextMenuHeaderView : ViewBase<ContextMenuEntityVM>
{
	[SerializeField]
	protected TextMeshProUGUI m_Label;

	[SerializeField]
	protected TextMeshProUGUI m_SubText;

	private bool m_IsInit;

	public void Initialize()
	{
		if (!m_IsInit)
		{
			m_IsInit = true;
		}
	}

	protected override void BindViewImplementation()
	{
		m_Label.text = base.ViewModel.Title.Value;
		m_SubText.text = base.ViewModel.SubTitle.Value;
		m_SubText.gameObject.SetActive(base.ViewModel.SubTitle.Value != null);
	}

	protected override void DestroyViewImplementation()
	{
	}
}
