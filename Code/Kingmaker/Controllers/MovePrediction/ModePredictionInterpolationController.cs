using Kingmaker.Controllers.Interfaces;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.View.Mechanics.Entities;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Controllers.MovePrediction;

public class ModePredictionInterpolationController : IControllerTick, IController
{
	TickType IControllerTick.GetTickType()
	{
		return TickType.EndOfFrame;
	}

	void IControllerTick.Tick()
	{
		if (Game.Instance.MovePredictionController.IsActive && GamepadInputController.CanProcessInput && Game.Instance.MovePredictionController.GetData(out var unit, out var oldOutput, out var newOutput) && unit != null && !(unit.Commands.Current is UnitMoveTo) && !(unit.View == null) && !(unit.View.MovementAgent.Or(null)?.NodeLinkTraverser?.IsTraverseNow).GetValueOrDefault())
		{
			Vector3 viewPosition = unit.View.GetViewPosition(oldOutput.Position);
			Vector3 viewPosition2 = unit.View.GetViewPosition(newOutput.Position);
			float interpolationProgress = Game.Instance.RealTimeController.InterpolationProgress;
			Vector3 position = Vector3.LerpUnclamped(viewPosition, viewPosition2, interpolationProgress);
			unit.View.transform.position = position;
			float y = Quaternion.LookRotation(oldOutput.Direction.To3D()).eulerAngles.y;
			float y2 = Quaternion.LookRotation(newOutput.Direction.To3D()).eulerAngles.y;
			float y3 = Mathf.LerpAngle(y, y2, interpolationProgress);
			unit.View.transform.rotation = Quaternion.Euler(0f, y3, 0f);
		}
	}
}
