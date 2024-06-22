using System;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Controllers.Combat;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using UnityEngine;

namespace Kingmaker.EntitySystem.Properties.Getters;

[Serializable]
[TypeId("a591ed5aa7bf4a549a80ad6e6fec3f63")]
public class LastAttackPositionDistanceGetter : UnitPropertyGetter
{
	protected override int GetBaseValue()
	{
		Vector3? lastAttackPosition = base.CurrentEntity.GetLastAttackPosition();
		if (!lastAttackPosition.HasValue)
		{
			return 0;
		}
		return base.CurrentEntity.DistanceToInCells(lastAttackPosition.Value);
	}

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return "Distance from " + FormulaTargetScope.Current + " to its last attack position";
	}
}
