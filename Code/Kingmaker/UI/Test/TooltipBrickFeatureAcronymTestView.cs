using Kingmaker.Code.UI.MVVM.View.Tooltip.Bricks;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.Controls.Other;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.UI.Test;

public class TooltipBrickFeatureAcronymTestView : TooltipBrickFeatureView
{
	[SerializeField]
	private OwlcatMultiButton m_Button;

	private Color m_DefaultTextColor;

	private Color m_DefaultBgrColor;

	protected override void BindViewImplementation()
	{
		Image component = m_Background.GetComponent<Image>();
		if (!(m_Label == null) && !(component == null))
		{
			m_DefaultTextColor = m_Label.color;
			m_DefaultBgrColor = component.color;
			base.BindViewImplementation();
			if (base.ViewModel.Feature != null && string.IsNullOrEmpty(base.ViewModel.Name))
			{
				m_IconBlock.gameObject.SetActive(value: false);
				component.color = Color.red;
				m_Label.text = base.ViewModel.Feature.name;
				m_Label.color = Color.red;
			}
			m_Background.SetActive(value: true);
			AddDisposable(m_Button.OnLeftClickAsObservable().Subscribe(delegate
			{
				ShowInfo();
			}));
		}
	}

	protected override void DestroyViewImplementation()
	{
		base.DestroyViewImplementation();
		Image component = m_Background.GetComponent<Image>();
		if (!(m_Label == null) && !(component == null))
		{
			m_Label.color = m_DefaultTextColor;
			component.color = m_DefaultBgrColor;
		}
	}

	private void ShowInfo()
	{
	}
}
