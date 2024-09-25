using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Code.Enums.Helper;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Mechanics.Entities;
using Kingmaker.Pathfinding;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.Utility;
using Kingmaker.View;
using Pathfinding;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.UnitLogic.Mechanics.Actions;

[TypeId("c7a3b2de9c37f154797b063a5730e307")]
public class ContextActionSpawnMonster : ContextAction
{
	[SerializeField]
	[FormerlySerializedAs("Blueprint")]
	private BlueprintUnitReference m_Blueprint;

	public ActionList AfterSpawn;

	[SerializeField]
	[FormerlySerializedAs("SummonPool")]
	private BlueprintSummonPoolReference m_SummonPool;

	public ContextDurationValue DurationValue;

	public ContextDiceValue CountValue;

	public ContextValue LevelValue = new ContextValue();

	public bool DoNotLinkToCaster;

	public bool IsDirectlyControllable;

	public BlueprintUnit Blueprint => m_Blueprint?.Get();

	public BlueprintSummonPool SummonPool => m_SummonPool?.Get();

	public override string GetCaption()
	{
		return $"Summon {Blueprint.name} x {CountValue} for {DurationValue}";
	}

	protected override void RunAction()
	{
		MechanicEntity maybeCaster = base.Context.MaybeCaster;
		if (maybeCaster == null)
		{
			Element.LogError(this, "Caster is missing");
			return;
		}
		Rounds duration = DurationValue.Calculate(base.Context);
		int num = CountValue.Calculate(base.Context);
		int level = LevelValue.Calculate(base.Context);
		Vector3 aroundPoint = base.Target.Point;
		bool flag = false;
		IntRect rectForSize = SizePathfindingHelper.GetRectForSize(Blueprint.Size);
		WarhammerSingleNodeBlocker exceptBlocker = ((maybeCaster is BaseUnitEntity baseUnitEntity) ? baseUnitEntity.View.MovementAgent.Blocker : null);
		CustomGridNode nearestNodeXZUnwalkable = ObstacleAnalyzer.GetNearestNodeXZUnwalkable(base.Target.Point);
		if (nearestNodeXZUnwalkable != null && WarhammerBlockManager.Instance.CanUnitStandOnNode(rectForSize, nearestNodeXZUnwalkable, exceptBlocker))
		{
			aroundPoint = nearestNodeXZUnwalkable.Vector3Position;
			flag = true;
		}
		else
		{
			foreach (CustomGridNodeBase item in GridAreaHelper.GetNodesSpiralAround(nearestNodeXZUnwalkable, rectForSize, 2))
			{
				if (WarhammerBlockManager.Instance.CanUnitStandOnNode(rectForSize, item, exceptBlocker))
				{
					aroundPoint = item.Vector3Position;
					flag = true;
					break;
				}
			}
		}
		if (!flag)
		{
			return;
		}
		UnitEntityView unitEntityView = Blueprint.Prefab.Load();
		float radius = ((unitEntityView != null) ? unitEntityView.Corpulence : 0.5f);
		FreePlaceSelector.PlaceSpawnPlaces(num, radius, aroundPoint);
		for (int i = 0; i < num; i++)
		{
			aroundPoint = FreePlaceSelector.GetRelaxedPosition(i, projectOnGround: true);
			RulePerformSummonUnit rule = new RulePerformSummonUnit(maybeCaster, Blueprint, aroundPoint, duration, level)
			{
				Context = base.Context,
				DoNotLinkToCaster = DoNotLinkToCaster
			};
			BaseUnitEntity summonedUnit = base.Context.TriggerRule(rule).SummonedUnit;
			if (base.Context is AbilityExecutionContext { ExecutionFromPsychicPhenomena: not false })
			{
				summonedUnit.MarkSpawnFromPsychicPhenomena();
			}
			if (SummonPool != null)
			{
				Game.Instance.SummonPools.Register(SummonPool, summonedUnit);
			}
			using (base.Context.GetDataScope(summonedUnit.ToITargetWrapper()))
			{
				AfterSpawn.Run();
			}
		}
	}
}
