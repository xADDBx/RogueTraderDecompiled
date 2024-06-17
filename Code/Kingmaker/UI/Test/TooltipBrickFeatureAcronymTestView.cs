using Kingmaker.Code.UI.MVVM.View.Tooltip.Bricks;
using Kingmaker.Utility.DotNetExtensions;
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
		MutableFeatureElements mutableFeatureElements = m_MutableElementsList.FirstOrDefault();
		Image component = m_Background.GetComponent<Image>();
		if (mutableFeatureElements != null && !(component == null))
		{
			m_DefaultTextColor = mutableFeatureElements.Label.color;
			m_DefaultBgrColor = component.color;
			base.BindViewImplementation();
			if (base.ViewModel.Feature != null && string.IsNullOrEmpty(base.ViewModel.Name))
			{
				m_IconBlock.SetActive(value: false);
				component.color = Color.red;
				mutableFeatureElements.Label.text = base.ViewModel.Feature.name;
				mutableFeatureElements.Label.color = Color.red;
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
		MutableFeatureElements mutableFeatureElements = m_MutableElementsList.FirstOrDefault();
		Image component = m_Background.GetComponent<Image>();
		if (mutableFeatureElements != null && !(component == null))
		{
			mutableFeatureElements.Label.color = m_DefaultTextColor;
			component.color = m_DefaultBgrColor;
		}
	}

	private void ShowInfo()
	{
	}
}
