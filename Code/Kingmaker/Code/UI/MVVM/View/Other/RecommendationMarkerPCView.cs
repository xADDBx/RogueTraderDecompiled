using Kingmaker.Blueprints.Root;
using Kingmaker.Code.UI.MVVM.VM.Other;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Owlcat.Runtime.UI.MVVM;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View.Other;

public class RecommendationMarkerPCView : ViewBase<RecommendationMarkerVM>
{
	[SerializeField]
	private Image m_RecommendationImage;

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
		if (base.ViewModel.Recommendation == RecommendationType.Neutral)
		{
			base.gameObject.SetActive(value: false);
			return;
		}
		base.gameObject.SetActive(value: true);
		bool flag = base.ViewModel.Recommendation == RecommendationType.Recommended;
		m_RecommendationImage.sprite = (flag ? UIConfig.Instance.UIIcons.Recommended : UIConfig.Instance.UIIcons.NotRecommended);
		AddDisposable(m_RecommendationImage.SetGlossaryTooltip(flag ? "RecommendedFeature" : "NotRecommendedFeature"));
	}

	protected override void DestroyViewImplementation()
	{
	}
}
