using System.Collections.Generic;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Mechanics.Entities;
using UnityEngine;

namespace Kingmaker.Controllers.Optimization;

public class AreaEffectTrigger : MonoBehaviour
{
	public readonly HashSet<UnitReference> Inside = new HashSet<UnitReference>();

	public readonly HashSet<UnitReference> Entered = new HashSet<UnitReference>();

	public readonly HashSet<UnitReference> Exited = new HashSet<UnitReference>();

	public AreaEffectEntity Unit { get; set; }

	public void ClearDelta()
	{
		Entered.Clear();
		Exited.Clear();
	}

	private void OnTriggerEnter2D(Collider2D other)
	{
		if (EntityDataLink.GetEntity(other) is BaseUnitEntity { IsExtra: false } baseUnitEntity)
		{
			if (!Inside.Add(baseUnitEntity.FromBaseUnitEntity()))
			{
				EntityBoundsController.Logger.Error("Cant add {0} to vision of {1} - already added", baseUnitEntity, Unit);
			}
			else
			{
				Entered.Add(baseUnitEntity.FromBaseUnitEntity());
			}
		}
	}

	private void OnTriggerExit2D(Collider2D other)
	{
		if (EntityDataLink.GetEntity(other) is BaseUnitEntity { IsExtra: false } baseUnitEntity)
		{
			if (!Inside.Remove(baseUnitEntity.FromBaseUnitEntity()))
			{
				EntityBoundsController.Logger.Error("Cant remove {0} to vision of {1} - never added", baseUnitEntity, Unit);
			}
			else
			{
				Exited.Add(baseUnitEntity.FromBaseUnitEntity());
			}
		}
	}
}
