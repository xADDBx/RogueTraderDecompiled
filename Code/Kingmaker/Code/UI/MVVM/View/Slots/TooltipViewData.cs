using System;
using System.Collections.Generic;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.Slots;

[Serializable]
public class TooltipViewData
{
	[SerializeField]
	private RectTransform m_TooltipPlace;

	[SerializeField]
	private List<Vector2> m_Pivots = new List<Vector2>
	{
		new Vector2(0.5f, 0.5f)
	};

	public RectTransform TooltipPlace => m_TooltipPlace;

	public List<Vector2> Pivots => m_Pivots;

	public TooltipViewData()
	{
	}

	public TooltipViewData(RectTransform tooltipPlace, List<Vector2> pivots)
	{
		m_TooltipPlace = tooltipPlace;
		m_Pivots = pivots;
	}
}
