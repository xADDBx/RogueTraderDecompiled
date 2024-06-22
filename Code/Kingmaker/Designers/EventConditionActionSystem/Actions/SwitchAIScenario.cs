using Kingmaker.AI.Scenarios;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.QA;
using Kingmaker.Utility;
using Kingmaker.Utility.Attributes;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Logging;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[TypeId("848b348a72fe1f045841a0d08892dcb7")]
public class SwitchAIScenario : GameAction
{
	[ValidateNotNull]
	[SerializeReference]
	public AbstractUnitEvaluator Unit;

	[SerializeField]
	private bool Deactivate;

	[SerializeField]
	[HideIf("Deactivate")]
	private AiScenarioType Scenario;

	[SerializeField]
	[ShowIf("IsHoldPosition")]
	private bool holdUnitPosition;

	[SerializeReference]
	[ShowIf("IsHoldStaticPosition")]
	public PositionEvaluator holdPosition;

	[SerializeReference]
	[ShowIf("IsHoldDynamicPosition")]
	public AbstractUnitEvaluator holdPositionNearUnit;

	[SerializeField]
	[ShowIf("IsHoldPosition")]
	private int range;

	[SerializeField]
	[ShowIf("IsHoldPosition")]
	private bool deactivateWhenPositionReached;

	[SerializeField]
	[ShowIf("IsHoldPosition")]
	private int idleRoundsThreshold;

	[SerializeField]
	[ShowIf("IsPriorityTarget")]
	private MechanicEntityEvaluator[] m_PriorityTargets;

	[SerializeField]
	[ShowIf("IsPriorityTarget")]
	private bool m_SimultaneousTargets;

	[SerializeField]
	[ShowIf("IsPriorityTarget")]
	private bool m_AttackPriorityTargetsOnly;

	private bool IsHoldPosition
	{
		get
		{
			if (!Deactivate)
			{
				return Scenario == AiScenarioType.HoldPosition;
			}
			return false;
		}
	}

	private bool IsHoldStaticPosition
	{
		get
		{
			if (IsHoldPosition)
			{
				return !holdUnitPosition;
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
				return holdUnitPosition;
			}
			return false;
		}
	}

	private bool IsBreach
	{
		get
		{
			if (!Deactivate)
			{
				return Scenario == AiScenarioType.Breach;
			}
			return false;
		}
	}

	private bool IsPriorityTarget
	{
		get
		{
			if (!Deactivate)
			{
				return Scenario == AiScenarioType.PriorityTarget;
			}
			return false;
		}
	}

	public override string GetCaption()
	{
		if (!Deactivate)
		{
			return $"Switch {Unit} scenario to {Scenario}";
		}
		return $"Deactivate {Unit} scenario";
	}

	protected override void RunAction()
	{
		if (!(Unit.GetValue() is BaseUnitEntity baseUnitEntity))
		{
			string message = $"[IS NOT BASE UNIT ENTITY] Game action {this}, {Unit} is not BaseUnitEntity";
			if (!QAModeExceptionReporter.MaybeShowError(message))
			{
				UberDebug.LogError(message);
			}
			return;
		}
		if (Deactivate)
		{
			baseUnitEntity.Brain.CurrentScenario?.Complete();
			return;
		}
		switch (Scenario)
		{
		case AiScenarioType.HoldPosition:
		{
			TargetWrapper target = (holdUnitPosition ? new TargetWrapper(holdPositionNearUnit.GetValue()) : new TargetWrapper(holdPosition.GetValue()));
			baseUnitEntity.Brain.SetHoldPosition(target, range, idleRoundsThreshold, deactivateWhenPositionReached);
			break;
		}
		case AiScenarioType.PriorityTarget:
			baseUnitEntity.Brain.SetPriorityTarget(m_PriorityTargets, m_SimultaneousTargets, m_AttackPriorityTargetsOnly, idleRoundsThreshold);
			break;
		case AiScenarioType.Breach:
			break;
		}
	}
}
