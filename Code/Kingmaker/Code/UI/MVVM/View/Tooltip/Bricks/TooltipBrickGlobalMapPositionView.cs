using Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.Tooltip.Bricks;

public class TooltipBrickGlobalMapPositionView : TooltipBaseBrickView<TooltipBrickGlobalMapPositionVM>
{
	[SerializeField]
	private Transform[] m_SystemPoints;

	[SerializeField]
	private Transform m_PositionSystemMarker;

	[SerializeField]
	private Transform m_PositionShipMarker;

	protected override void BindViewImplementation()
	{
		SetSystemMarkerPosition();
		SetShipPosition();
	}

	private void SetSystemMarkerPosition()
	{
		m_PositionSystemMarker.localPosition = m_SystemPoints[0].localPosition;
	}

	private void SetShipPosition()
	{
		m_PositionShipMarker.localPosition = m_SystemPoints[30].localPosition;
	}
}
