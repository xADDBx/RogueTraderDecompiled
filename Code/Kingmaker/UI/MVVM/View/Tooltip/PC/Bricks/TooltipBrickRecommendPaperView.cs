using Kingmaker.Code.UI.MVVM.View.Tooltip.Bricks;
using Kingmaker.UI.MVVM.VM.Tooltip.Bricks;
using TMPro;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.Tooltip.PC.Bricks;

public class TooltipBrickRecommendPaperView : TooltipBaseBrickView<TooltipBrickRecommendPaperVM>
{
	[SerializeField]
	private GameObject m_RecomendationIconTrue;

	[SerializeField]
	private GameObject m_RecomendationIconFalse;

	[SerializeField]
	private TextMeshProUGUI m_RecommendationName;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		m_RecomendationIconTrue.SetActive(!base.ViewModel.Value);
		m_RecomendationIconFalse.SetActive(base.ViewModel.Value);
		m_RecommendationName.text = base.ViewModel.FeatureName;
	}
}
