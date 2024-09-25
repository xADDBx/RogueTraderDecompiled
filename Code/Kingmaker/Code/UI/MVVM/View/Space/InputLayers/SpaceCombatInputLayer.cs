using Kingmaker.UI.Pointer;
using Kingmaker.View;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Code.UI.MVVM.View.Space.InputLayers;

public class SpaceCombatInputLayer : SpaceMainInputLayer
{
	public SpaceCombatInputLayer()
	{
		base.CursorEnabled = true;
	}

	protected override void UpdateCursorMovement()
	{
		MoveCamera();
		ConsoleCursor.Instance.Or(null)?.SetToCenter();
		ConsoleCursor.Instance.Or(null)?.SnapToCurrentNode();
	}

	private void MoveCamera()
	{
		CameraRig.Instance.ScrollBy2D(m_LeftStickVector);
	}
}
