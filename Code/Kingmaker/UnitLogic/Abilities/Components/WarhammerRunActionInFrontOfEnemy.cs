using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.Pathfinding;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.Utility;
using Kingmaker.Utility.Random;
using Kingmaker.View;
using UnityEngine;

namespace Kingmaker.UnitLogic.Abilities.Components;

[TypeId("3755ee60f6c4a114489442845b9433c1")]
public class WarhammerRunActionInFrontOfEnemy : AbilityApplyEffect
{
	public int frontConeAngle;

	public int frontConeRotation;

	public int frontConeRangeMin;

	public int frontConeRangeMax;

	public bool randomRotationOn180;

	public int repeats;

	public int addRangeEachRepeat;

	public ActionList Actions;

	public override void Apply(AbilityExecutionContext context, TargetWrapper target)
	{
		if (target.Entity == null)
		{
			PFLog.Default.Error(context.AbilityBlueprint, "No target.Entity");
			return;
		}
		for (int i = 0; i <= repeats; i++)
		{
			using (context.GetDataScope(new TargetWrapper(GetActionPosition(target, i))))
			{
				Actions.Run();
			}
		}
	}

	public Vector3 GetActionPosition(TargetWrapper target, int repeat)
	{
		Vector3 vector = new Vector3(0f, 0f, PFStatefulRandom.SpaceCombat.Range(frontConeRangeMin, frontConeRangeMax) + repeat * addRangeEachRepeat) * GraphParamsMechanicsCache.GridCellSize;
		Quaternion quaternion = Quaternion.Euler(0f, PFStatefulRandom.SpaceCombat.Range(-frontConeAngle, frontConeAngle) + frontConeRotation, 0f);
		if (randomRotationOn180 && PFStatefulRandom.SpaceCombat.YesOrNo)
		{
			quaternion *= Quaternion.Euler(0f, 180f, 0f);
		}
		return (Vector3)ObstacleAnalyzer.GetNearestNode(target.Entity.View.ViewTransform.position + target.Entity.View.ViewTransform.rotation * quaternion * vector).node.position;
	}
}
