using Kingmaker.Code.UI.MVVM.VM.Other;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Owlcat.Runtime.UI.MVVM;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View.Other;

public class EntityPCView : ViewBase<EntityVM>
{
	[SerializeField]
	private Image m_Icon;

	[SerializeField]
	private TextMeshProUGUI m_Label;

	protected override void BindViewImplementation()
	{
		SetIcon();
		SetLabel();
		SetTooltip();
	}

	private void SetIcon()
	{
		m_Icon.sprite = base.ViewModel.Icon;
		m_Icon.preserveAspect = true;
		m_Icon.gameObject.SetActive(base.ViewModel.Icon);
	}

	private void SetLabel()
	{
		m_Label.text = base.ViewModel.Name;
	}

	private void SetTooltip()
	{
		AddDisposable(this.SetTooltip(base.ViewModel.Tooltip));
	}

	protected override void DestroyViewImplementation()
	{
	}
}
