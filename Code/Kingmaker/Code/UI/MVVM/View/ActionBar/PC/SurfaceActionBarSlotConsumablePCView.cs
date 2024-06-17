using System.Collections.Generic;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.ActionBar.PC;

public class SurfaceActionBarSlotConsumablePCView : SurfaceActionBarSlotConsumableView
{
	[Header("PCSlot")]
	[SerializeField]
	private ActionBarSlotPCView m_SlotPCView;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		m_SlotPCView.Bind(base.ViewModel);
	}

	public void SetKeyBinding(int index)
	{
		m_SlotPCView.SetKeyBinding(index);
	}

	public override void SetTooltipCustomPosition(RectTransform rectTransform, List<Vector2> pivots = null)
	{
		m_SlotPCView.SetTooltipCustomPosition(rectTransform, pivots);
	}
}
