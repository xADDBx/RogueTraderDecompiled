using Kingmaker.Utility.Attributes;
using Owlcat.Runtime.UI.SelectionGroup.View;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View.Credits;

public class CreditsMenuEntityPCView : SelectionGroupEntityView<CreditsMenuEntityVM>
{
	[SerializeField]
	private bool m_WithImage;

	[ShowIf("m_WithImage")]
	[SerializeField]
	private Image m_Image;

	[ShowIf("WithText")]
	[SerializeField]
	private TextMeshProUGUI m_Label;

	private bool WithText => !m_WithImage;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		SetImage();
		SetLabel();
	}

	private void SetImage()
	{
		m_Image.gameObject.SetActive(m_Image != null && m_WithImage);
		if (!(m_Image == null) && m_WithImage)
		{
			m_Image.sprite = base.ViewModel.Logo;
		}
	}

	private void SetLabel()
	{
		m_Label.gameObject.SetActive(m_Label != null && WithText);
		if (!(m_Label == null) && WithText)
		{
			m_Label.text = base.ViewModel.Label;
		}
	}
}
