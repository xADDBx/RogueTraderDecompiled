using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Controllers;
using Kingmaker.Controllers.Clicks.Handlers;
using Kingmaker.Controllers.Projectiles;
using Kingmaker.Designers.EventConditionActionSystem.ContextData;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.Pathfinding;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Abilities.Components.Patterns;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Utility.Random;
using Kingmaker.View;
using Pathfinding;
using UnityEngine;

namespace Kingmaker.UnitLogic.Abilities.Components;

[KDB("Вешается на абилку и позволяет выбрать две точки между которыми будет проложен указанный Ареа эффект. Паттерн указанной зоны не используется и будет перезаписан на луч между двумя выбранными точками.")]
[TypeId("c71ac31a363bb1746a29153ebb240395")]
public class AbilitySpawnAreaBetweenTwoPoints : AbilityCustomLogic, IAbilityMultiTarget, IAbilityAoEPatternProvider
{
	private readonly struct CollectLineNodesCallback : Linecast.ICanTransitionBetweenCells
	{
		private readonly List<CustomGridNodeBase> m_Nodes;

		private readonly int m_MaxSize;

		public CollectLineNodesCallback(List<CustomGridNodeBase> nodes, int maxSize)
		{
			m_Nodes = nodes;
			m_MaxSize = maxSize;
		}

		public bool CanTransitionBetweenCells(CustomGridNodeBase nodeFrom, CustomGridNodeBase nodeTo, Vector3 transitionPosition, float distanceFactor)
		{
			if (m_MaxSize > 0 && m_Nodes.Count >= m_MaxSize)
			{
				return false;
			}
			m_Nodes.Add(nodeFrom);
			return true;
		}
	}

	[KDB("Паттерн указанной зоны будет перезаписан на линию между двумя точками.")]
	[SerializeField]
	private BlueprintAbilityAreaEffectReference m_AreaEffect;

	public ContextDurationValue DurationValue;

	[KDB("Ограничивает длину линии.")]
	[SerializeField]
	private ContextValue m_MaxRange;

	[KDB("Фиксирует линию в горизонтальном или вертикальном положении, запрещая строить ее по диагонали.")]
	[SerializeField]
	private bool m_ForceCardinal;

	[SerializeField]
	private bool m_IgnoreLos;

	public BlueprintAbilityAreaEffect AreaEffect => m_AreaEffect?.Get();

	public bool IsIgnoreLos => m_IgnoreLos;

	public bool UseMeleeLos => false;

	public bool IsIgnoreLevelDifference => false;

	public int PatternAngle => 0;

	public bool CalculateAttackFromPatternCentre => false;

	public bool ExcludeUnwalkable => false;

	public TargetType Targets => TargetType.Any;

	public AoEPattern Pattern => null;

	public int TargetAbilityCount => 2;

	public override void Cleanup(AbilityExecutionContext context)
	{
	}

	public override IEnumerator<AbilityDeliveryTarget> Deliver(AbilityExecutionContext context, TargetWrapper target)
	{
		List<TargetWrapper> allTargets = context.AllTargets;
		if (allTargets.Count < 2)
		{
			PFLog.Default.Error(context.AbilityBlueprint, "Two target points are required.");
			yield break;
		}
		Vector3 point = allTargets[0].Point;
		Vector3 point2 = allTargets[1].Point;
		SpawnAreaEffect(context, allTargets[0], allTargets[0].NearestNode, allTargets[1].NearestNode);
		HashSet<TargetWrapper> targetsBetween = GetTargetsBetween(context, point, point2);
		foreach (TargetWrapper item in targetsBetween)
		{
			yield return new AbilityDeliveryTarget(item);
		}
	}

	private HashSet<TargetWrapper> GetTargetsBetween(AbilityExecutionContext context, Vector3 startPoint, Vector3 endPoint)
	{
		HashSet<TargetWrapper> hashSet = new HashSet<TargetWrapper>();
		CustomGridNodeBase startNode = (CustomGridNodeBase)ObstacleAnalyzer.GetNearestNode(startPoint).node;
		CustomGridNodeBase endNode = (CustomGridNodeBase)ObstacleAnalyzer.GetNearestNode(endPoint).node;
		foreach (CustomGridNodeBase item in GetNodesAlongLine(startNode, endNode, m_ForceCardinal, context))
		{
			BaseUnitEntity unit;
			TargetWrapper targetWrapper = ((!item.TryGetUnit(out unit)) ? new TargetWrapper(item.Vector3Position) : new TargetWrapper(unit));
			if (context.Ability.IsValid(targetWrapper))
			{
				hashSet.Add(targetWrapper);
			}
		}
		return hashSet;
	}

	private List<CustomGridNodeBase> GetNodesAlongLine(CustomGridNodeBase startNode, CustomGridNodeBase endNode, bool forceCardinal, MechanicsContext context)
	{
		Vector3 vector = endNode.Vector3Position;
		CustomGridNodeBase item = endNode;
		if (forceCardinal)
		{
			Vector3 vector2 = endNode.Vector3Position - startNode.Vector3Position;
			if (Mathf.Abs(vector2.x) > Mathf.Abs(vector2.z))
			{
				vector2.z = 0f;
			}
			else
			{
				vector2.x = 0f;
			}
			vector = startNode.Vector3Position + vector2;
			item = (CustomGridNodeBase)ObstacleAnalyzer.GetNearestNode(vector).node;
		}
		List<CustomGridNodeBase> list = new List<CustomGridNodeBase>();
		Vector3 vector3Position = startNode.Vector3Position;
		int num = m_MaxRange.Calculate(context);
		CollectLineNodesCallback condition = new CollectLineNodesCallback(list, num);
		NNConstraint constraint = (m_IgnoreLos ? NNConstraint.None : NNConstraint.Default);
		Linecast.LinecastGrid2(startNode.Graph, vector3Position, vector, startNode, out var _, constraint, ref condition);
		if (list.Count < num)
		{
			list.Add(item);
		}
		return list;
	}

	public bool TryGetNextTargetAbility(AbilityData rootAbility, int targetIndex, out AbilityData ability)
	{
		ability = rootAbility;
		return targetIndex < TargetAbilityCount;
	}

	public void OverridePattern(AoEPattern pattern)
	{
	}

	public OrientedPatternData GetOrientedPattern(IAbilityDataProviderForPattern ability, CustomGridNodeBase startNode, CustomGridNodeBase endNode, bool coveredTargetsOnly = false)
	{
		ClickWithSelectedAbilityHandler selectedAbilityHandler = Game.Instance.SelectedAbilityHandler;
		startNode = ((selectedAbilityHandler == null || selectedAbilityHandler.MultiTargetHandler.Targets.Count <= 0) ? endNode : Game.Instance.SelectedAbilityHandler?.MultiTargetHandler.Targets.FirstOrDefault()?.NearestNode);
		AbilityExecutionContext context = ability.Data.CreateExecutionContext(new TargetWrapper(endNode.Vector3Position));
		return new OrientedPatternData(GetNodesAlongLine(startNode, endNode, m_ForceCardinal, context), startNode);
	}

	private void SpawnAreaEffect(AbilityExecutionContext context, TargetWrapper target, CustomGridNodeBase startNode, CustomGridNodeBase endNode)
	{
		TimeSpan seconds = DurationValue.Calculate(context).Seconds;
		List<CustomGridNodeBase> nodesAlongLine = GetNodesAlongLine(startNode, endNode, m_ForceCardinal, context);
		OrientedPatternData pattern = new OrientedPatternData(nodesAlongLine, startNode);
		AreaEffectEntity areaEffectEntity = AreaEffectsController.Spawn(overridenPattern: new OverrideAreaEffectPatternData(pattern, overridePatternWithAttackPattern: true), parentContext: context, blueprint: AreaEffect, target: target, duration: seconds);
		if (areaEffectEntity != null && ContextData<FactData>.Current?.Fact is UnitFact { SourceFact: { } sourceFact } unitFact && sourceFact.Owner is Entity)
		{
			areaEffectEntity.SourceFact = new EntityFactRef(unitFact.SourceFact);
		}
		if (areaEffectEntity != null && context != null)
		{
			foreach (BaseUnitEntity u in Game.Instance.State.AllBaseUnits)
			{
				if (!u.LifeState.IsDead && u.IsInGame && !areaEffectEntity.Blueprint.IsAllArea && areaEffectEntity.Contains(u) && (areaEffectEntity.AffectEnemies || !context.Caster.IsEnemy(u)))
				{
					EventBus.RaiseEvent(delegate(IApplyAbilityEffectHandler h)
					{
						h.OnTryToApplyAbilityEffect(context, new AbilityDeliveryTarget(u));
					});
				}
			}
		}
		LaunchLineProjectile(context, nodesAlongLine);
	}

	private static void LaunchLineProjectile(AbilityExecutionContext context, List<CustomGridNodeBase> nodes)
	{
		if (!(context?.Ability == null) && nodes != null && nodes.Count >= 2)
		{
			BlueprintProjectile blueprintProjectile = context.Ability.ProjectileVariants.Random(PFStatefulRandom.UnitLogic.Abilities);
			if (blueprintProjectile != null)
			{
				Vector3 vector3Position = nodes[0].Vector3Position;
				Vector3 vector3Position2 = nodes[nodes.Count - 1].Vector3Position;
				new ProjectileLauncher(blueprintProjectile, new TargetWrapper(vector3Position), new TargetWrapper(vector3Position2)).Ability(context.Ability).Launch();
			}
		}
	}
}
