using Kingmaker.Code.UI.MVVM.VM.DlcManager;
using Kingmaker.UI.Common;
using Owlcat.Runtime.UI.SelectionGroup.View;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.DlcManager.Base;

public class DlcManagerMenuEntityBaseView : SelectionGroupEntityView<DlcManagerMenuEntityVM>
{
	[SerializeField]
	private TextMeshProUGUI m_Label;

	private AccessibilityTextHelper m_AccessibilityTextHelper;

	public override void DoInitialize()
	{
		base.DoInitialize();
		base.gameObject.SetActive(value: false);
		AddDisposable(m_AccessibilityTextHelper = new AccessibilityTextHelper(m_Label));
	}

	protected override void BindViewImplementation()
	{
		base.gameObject.SetActive(value: true);
		m_Label.text = base.ViewModel.Title;
		base.BindViewImplementation();
		if (Game.Instance.IsControllerMouse)
		{
			m_AccessibilityTextHelper.UpdateTextSize();
		}
	}

	protected override void DestroyViewImplementation()
	{
		base.DestroyViewImplementation();
		base.gameObject.SetActive(value: false);
	}
}
