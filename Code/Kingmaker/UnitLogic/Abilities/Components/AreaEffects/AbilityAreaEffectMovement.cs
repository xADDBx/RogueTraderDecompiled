using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility;
using Owlcat.Runtime.Visual.FogOfWar;
using UnityEngine;

namespace Kingmaker.UnitLogic.Abilities.Components.AreaEffects;

[TypeId("0e1f492ac13c4af9add252666e481277")]
public class AbilityAreaEffectMovement : AbilityAreaEffectLogic
{
	public Feet DistancePerRound;

	public float SpeedMps => DistancePerRound.Meters / 5f;

	protected override void OnTick(MechanicsContext context, AreaEffectEntity areaEffect)
	{
		Vector3 normalized = (Quaternion.Euler(0f, context.MainTarget.Orientation, 0f) * Vector3.forward).normalized;
		Vector3 vector = areaEffect.View.ViewTransform.position + normalized * SpeedMps * Game.Instance.TimeController.DeltaTime;
		if (Physics.Raycast(new Ray(vector + new Vector3(0f, 10f, 0f), Vector3.down), out var hitInfo, 20f, 2359553))
		{
			vector = hitInfo.point;
		}
		if (!LineOfSightGeometry.Instance.HasObstacle(areaEffect.View.ViewTransform.position, vector))
		{
			areaEffect.View.ViewTransform.position = vector;
		}
	}
}
