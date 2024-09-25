using Owlcat.Runtime.UI.Tooltips;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.Slots;

public class TooltipPlaces : MonoBehaviour
{
	[SerializeField]
	private TooltipViewData m_MainTooltipData;

	[SerializeField]
	private TooltipViewData m_ComparativeTooltipData;

	public TooltipConfig GetMainTooltipConfig(TooltipConfig baseConfig = default(TooltipConfig))
	{
		if (m_MainTooltipData.TooltipPlace == null)
		{
			return baseConfig;
		}
		baseConfig.TooltipPlace = m_MainTooltipData.TooltipPlace;
		baseConfig.PriorityPivots = m_MainTooltipData.Pivots;
		return baseConfig;
	}

	public TooltipConfig GetCompareTooltipConfig(TooltipConfig baseConfig = default(TooltipConfig))
	{
		if (m_ComparativeTooltipData.TooltipPlace == null)
		{
			return baseConfig;
		}
		baseConfig.TooltipPlace = m_ComparativeTooltipData.TooltipPlace;
		baseConfig.PriorityPivots = m_ComparativeTooltipData.Pivots;
		baseConfig.InfoCallConsoleMethod = InfoCallConsoleMethod.None;
		return baseConfig;
	}
}
