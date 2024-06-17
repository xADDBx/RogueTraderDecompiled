using Kingmaker.TurnBasedMode.Controllers;
using UnityEngine;

namespace Kingmaker.TurnBasedMode;

public class ActionsState
{
	public struct Actions
	{
		public CombatAction FiveFootStep;

		public CombatAction Move;

		public CombatAction Standard;

		public CombatAction Swift;

		public CombatAction Free;
	}

	public Actions ActionsStates;

	public bool Overused;

	public float ApproachRadius;

	public Vector3 ApproachPoint;

	public bool NeedLOS;

	public int IgnoreBlockerId;

	private bool m_IsOutOfRange;

	public bool IsSurprise;

	public CombatAction FiveFootStep => ActionsStates.FiveFootStep;

	public CombatAction Move => ActionsStates.Move;

	public CombatAction Standard => ActionsStates.Standard;

	public CombatAction Swift => ActionsStates.Swift;

	public CombatAction Free => ActionsStates.Free;

	public bool IsOutOfRange => m_IsOutOfRange;

	public ActionsState(Actions currentStates, bool isSurprise)
	{
		ActionsStates = currentStates;
		IsSurprise = isSurprise;
	}

	public bool IsMove()
	{
		bool num = FiveFootStep.MovementActivityStatePredicted.GetValueOrDefault() == CombatAction.ActivityState.WillBeUsed;
		bool flag = Move.MovementActivityStatePredicted.GetValueOrDefault() == CombatAction.ActivityState.WillBeUsed;
		bool flag2 = Standard.MovementActivityStatePredicted.GetValueOrDefault() == CombatAction.ActivityState.WillBeUsed;
		return num || flag || flag2;
	}

	public bool IsAttack()
	{
		bool num = Move.AttackActivityStatePredicted.GetValueOrDefault() == CombatAction.ActivityState.WillBeUsed;
		bool flag = Standard.AttackActivityStatePredicted.GetValueOrDefault() == CombatAction.ActivityState.WillBeUsed;
		return num || flag;
	}

	public void Clear()
	{
		Overused = false;
		FiveFootStep.ClearPredictions();
		Move.ClearPredictions();
		Standard.ClearPredictions();
		Swift.ClearPredictions();
		Free.ClearPredictions();
		ApproachPoint = Vector3.zero;
		ApproachRadius = 0f;
		NeedLOS = false;
		IgnoreBlockerId = 0;
	}

	public void UpdateOutOfRange(bool isOutOfRange)
	{
		m_IsOutOfRange = isOutOfRange;
	}

	public void UpdateMoveDistance()
	{
	}
}
