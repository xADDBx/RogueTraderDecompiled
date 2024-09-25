using System.Collections.Generic;
using System.Diagnostics;
using Kingmaker.EntitySystem.Entities;
using UnityEngine;

namespace Kingmaker.Controllers.Optimization;

public class EntityViewTrigger : MonoBehaviour
{
	public readonly List<BaseUnitEntity> Entered = new List<BaseUnitEntity>();

	public readonly List<BaseUnitEntity> Exited = new List<BaseUnitEntity>();

	public BaseUnitEntity Unit { get; set; }

	[Conditional("REPLAY_LOG_ENABLED")]
	private void LogEntered(BaseUnitEntity unitInRange)
	{
		Entered.Add(unitInRange);
	}

	[Conditional("REPLAY_LOG_ENABLED")]
	private void LogExited(BaseUnitEntity unitInRange)
	{
		Exited.Add(unitInRange);
	}

	private void OnTriggerEnter2D(Collider2D other)
	{
		if (EntityDataLink.GetEntity(other) is BaseUnitEntity baseUnitEntity && baseUnitEntity != Unit && !Unit.Vision.CanBeInRange.Add(baseUnitEntity))
		{
			EntityBoundsController.Logger.Error("Cant add {0} to vision of {1} - already added", baseUnitEntity, Unit);
		}
	}

	private void OnTriggerExit2D(Collider2D other)
	{
		if (EntityDataLink.GetEntity(other) is BaseUnitEntity baseUnitEntity && baseUnitEntity != Unit && !Unit.Vision.CanBeInRange.Remove(baseUnitEntity))
		{
			EntityBoundsController.Logger.Error("Cant remove {0} to vision of {1} - never added", baseUnitEntity, Unit);
		}
	}
}
