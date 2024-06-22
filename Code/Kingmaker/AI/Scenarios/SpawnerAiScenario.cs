using JetBrains.Annotations;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic;
using Kingmaker.Utility;
using Kingmaker.Utility.Attributes;
using Kingmaker.View.MapObjects;
using Kingmaker.View.Spawners;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.AI.Scenarios;

[RequireComponent(typeof(UnitSpawnerBase))]
[KnowledgeDatabaseID("0bf4f33922f76254cb6cf08686361e27")]
public class SpawnerAiScenario : EntityPartComponent<SpawnerAiScenario.Part>
{
	public class Part : ViewBasedPart, IUnitCombatHandler, ISubscriber<IBaseUnitEntity>, ISubscriber, IHashable
	{
		public new SpawnerAiScenario Source => (SpawnerAiScenario)base.Source;

		private AbstractUnitEntity SpawnedUnit => ((UnitSpawnerBase.MyData)base.Owner).SpawnedUnit.Entity ?? null;

		public void HandleUnitJoinCombat()
		{
			if (EventInvokerExtensions.BaseUnitEntity == SpawnedUnit && (Source.ScenarioActivationConditions?.Get()?.Check() ?? true))
			{
				switch (Source.Scenario)
				{
				case AiScenarioType.HoldPosition:
				{
					TargetWrapper target = (Source.HoldUnitPosition ? new TargetWrapper(Source.HoldPositionNearUnit.GetValue()) : (Source.HoldPosition ? new TargetWrapper(Source.HoldPosition.position) : ((TargetWrapper)base.Owner.Position)));
					SpawnedUnit.GetRequired<PartUnitBrain>().SetHoldPosition(target, Source.Range, Source.IdleRoundsThreshold, Source.DeactivateWhenPositionReached);
					break;
				}
				case AiScenarioType.PriorityTarget:
					SpawnedUnit.GetRequired<PartUnitBrain>().SetPriorityTarget(Source.m_PriorityTargets, Source.SimultaneousTargets, Source.AttackPriorityTargetsOnly, Source.IdleRoundsThreshold);
					break;
				case AiScenarioType.Breach:
					break;
				}
			}
		}

		public void HandleUnitLeaveCombat()
		{
		}

		public override Hash128 GetHash128()
		{
			Hash128 result = default(Hash128);
			Hash128 val = base.GetHash128();
			result.Append(ref val);
			return result;
		}
	}

	[CanBeNull]
	public ConditionsReference ScenarioActivationConditions;

	[SerializeField]
	private AiScenarioType Scenario;

	[SerializeField]
	[ShowIf("IsHoldPosition")]
	private bool HoldUnitPosition;

	[SerializeReference]
	[ShowIf("IsHoldStaticPosition")]
	public Transform HoldPosition;

	[SerializeReference]
	[ShowIf("IsHoldDynamicPosition")]
	public AbstractUnitEvaluator HoldPositionNearUnit;

	[SerializeField]
	[ShowIf("IsHoldPosition")]
	private int Range;

	[SerializeField]
	[ShowIf("IsHoldPosition")]
	private bool DeactivateWhenPositionReached;

	[SerializeReference]
	[ShowIf("IsHoldPosition")]
	private int IdleRoundsThreshold;

	[SerializeReference]
	[ShowIf("IsPriorityTarget")]
	private MechanicEntityEvaluator[] m_PriorityTargets;

	[SerializeField]
	[ShowIf("IsPriorityTarget")]
	private bool SimultaneousTargets;

	[SerializeField]
	[ShowIf("IsPriorityTarget")]
	private bool AttackPriorityTargetsOnly;

	private bool IsHoldPosition => Scenario == AiScenarioType.HoldPosition;

	private bool IsHoldStaticPosition
	{
		get
		{
			if (IsHoldPosition)
			{
				return !HoldUnitPosition;
			}
			return false;
		}
	}

	private bool IsHoldDynamicPosition
	{
		get
		{
			if (IsHoldPosition)
			{
				return HoldUnitPosition;
			}
			return false;
		}
	}

	private bool IsBreach => Scenario == AiScenarioType.Breach;

	private bool IsPriorityTarget => Scenario == AiScenarioType.PriorityTarget;
}
