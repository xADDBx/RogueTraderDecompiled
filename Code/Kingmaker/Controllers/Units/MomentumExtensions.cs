using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Properties;
using Kingmaker.UnitLogic.Mechanics;
using UnityEngine;

namespace Kingmaker.Controllers.Units;

public static class MomentumExtensions
{
	public static int GetDesperateMeasureThreshold(this MechanicEntity mechanicEntity)
	{
		MechanicsContext mechanicsContext = ((mechanicEntity is BaseUnitEntity baseUnitEntity) ? baseUnitEntity.Context : mechanicEntity.MainFact.MaybeContext);
		if (mechanicsContext == null)
		{
			mechanicsContext = new MechanicsContext(mechanicEntity, mechanicEntity, mechanicEntity.Blueprint);
		}
		PropertyContext context = new PropertyContext(mechanicEntity, null, null, mechanicsContext);
		int value = Game.Instance.BlueprintRoot.WarhammerRoot.MomentumRoot.DesperateMeasureThreshold.GetValue(context);
		float num = 1f;
		return Mathf.RoundToInt((float)value * num);
	}
}
