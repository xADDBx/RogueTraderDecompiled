using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.UI.Models.Tooltip.Base;
using UniRx;

namespace Kingmaker.TurnBasedMode.Controllers;

public class CombatAction
{
	public enum ActionState
	{
		Unable,
		Available,
		WillBeUsed,
		WillBeLost,
		Used,
		Lost
	}

	public enum ActivityState
	{
		Available,
		WillBeUsed,
		WillBeLost,
		Unable,
		Lost,
		Used
	}

	public enum ActivityType
	{
		Move,
		Attack,
		Ability
	}

	public enum UsageType
	{
		None,
		Move,
		SingleAttack,
		FullAttack,
		SpellCombatAttack,
		TouchDeliver,
		UseAbility,
		UseItem,
		ChangeWeapon,
		InteractObject,
		Standup
	}

	private ActivityState? m_MovementActivityStatePredicted;

	private ActivityState? m_MovementActivityStateCurrent;

	private ActivityState? m_AttackActivityStatePredicted;

	private ActivityState? m_AttackActivityStateCurrent;

	private ActivityState? m_AbilityActivityStatePredicted;

	private ActivityState? m_AbilityActivityStateCurrent;

	public bool LockType;

	public bool HasMovePossibility;

	public float? MaxMoveDistance;

	public float? RemainingMoveDistance;

	public float? PredictedMoveDistance;

	public ReactiveCommand MoveDistanceUpdated = new ReactiveCommand();

	public bool CanUse => GetActionState() == ActionState.Available;

	public bool CanUseAbility
	{
		get
		{
			if (AbilityActivityStateCurrent.HasValue)
			{
				return AbilityActivityStateCurrent == ActivityState.Available;
			}
			return false;
		}
	}

	public ActionState PredictionState => GetActionState();

	public ActivityState? MovementActivityStatePredicted
	{
		get
		{
			if (!m_MovementActivityStateCurrent.HasValue)
			{
				return null;
			}
			if (!m_MovementActivityStatePredicted.HasValue || m_MovementActivityStatePredicted.Value < m_MovementActivityStateCurrent.Value)
			{
				return m_MovementActivityStateCurrent;
			}
			return m_MovementActivityStatePredicted;
		}
		set
		{
			m_MovementActivityStatePredicted = value;
			if (value == ActivityState.WillBeUsed)
			{
				m_AttackActivityStatePredicted = ActivityState.WillBeLost;
				m_AbilityActivityStatePredicted = ActivityState.WillBeLost;
			}
		}
	}

	public ActivityState? MovementActivityStateCurrent
	{
		get
		{
			return m_MovementActivityStateCurrent;
		}
		private set
		{
			if (!value.HasValue || (m_MovementActivityStateCurrent.HasValue && value.Value <= m_MovementActivityStateCurrent.Value))
			{
				return;
			}
			m_MovementActivityStateCurrent = value;
			switch (m_MovementActivityStateCurrent)
			{
			case ActivityState.WillBeUsed:
			case ActivityState.Used:
				if (AbilityActivityStateCurrent.HasValue)
				{
					AbilityActivityStateCurrent = ActivityState.Lost;
				}
				if (AttackActivityStateCurrent.HasValue)
				{
					AttackActivityStateCurrent = ActivityState.Lost;
				}
				break;
			}
			MovementActivityStatePredicted = null;
		}
	}

	public ActivityState? AttackActivityStatePredicted
	{
		get
		{
			if (!m_AttackActivityStateCurrent.HasValue)
			{
				return null;
			}
			if (!m_AttackActivityStatePredicted.HasValue || m_AttackActivityStatePredicted.Value < m_AttackActivityStateCurrent.Value)
			{
				return m_AttackActivityStateCurrent;
			}
			return m_AttackActivityStatePredicted;
		}
		set
		{
			m_AttackActivityStatePredicted = value;
			if (value == ActivityState.WillBeUsed)
			{
				m_MovementActivityStatePredicted = ActivityState.WillBeLost;
				m_AbilityActivityStatePredicted = ActivityState.WillBeLost;
			}
		}
	}

	public ActivityState? AttackActivityStateCurrent
	{
		get
		{
			return m_AttackActivityStateCurrent;
		}
		private set
		{
			if (!value.HasValue || (m_AttackActivityStateCurrent.HasValue && value.Value <= m_AttackActivityStateCurrent.Value))
			{
				return;
			}
			m_AttackActivityStateCurrent = value;
			switch (m_AttackActivityStateCurrent)
			{
			case ActivityState.WillBeUsed:
			case ActivityState.Used:
				if (MovementActivityStateCurrent.HasValue)
				{
					MovementActivityStateCurrent = ActivityState.Lost;
				}
				if (AbilityActivityStateCurrent.HasValue)
				{
					AbilityActivityStateCurrent = ActivityState.Lost;
				}
				break;
			}
			AttackActivityStatePredicted = null;
		}
	}

	public ActivityState? AbilityActivityStatePredicted
	{
		get
		{
			if (!m_AbilityActivityStateCurrent.HasValue)
			{
				return null;
			}
			if (!m_AbilityActivityStatePredicted.HasValue || m_AbilityActivityStatePredicted.Value < m_AbilityActivityStateCurrent.Value)
			{
				return m_AbilityActivityStateCurrent;
			}
			return m_AbilityActivityStatePredicted;
		}
		set
		{
			m_AbilityActivityStatePredicted = value;
			if (value == ActivityState.WillBeUsed)
			{
				m_MovementActivityStatePredicted = ActivityState.WillBeLost;
				m_AttackActivityStatePredicted = ActivityState.WillBeLost;
			}
		}
	}

	public ActivityState? AbilityActivityStateCurrent
	{
		get
		{
			return m_AbilityActivityStateCurrent;
		}
		private set
		{
			if (!value.HasValue || (m_AbilityActivityStateCurrent.HasValue && value.Value <= m_AbilityActivityStateCurrent.Value))
			{
				return;
			}
			m_AbilityActivityStateCurrent = value;
			switch (m_AbilityActivityStateCurrent)
			{
			case ActivityState.WillBeUsed:
			case ActivityState.Used:
				if (AttackActivityStateCurrent.HasValue)
				{
					AttackActivityStateCurrent = ActivityState.Lost;
				}
				if (MovementActivityStateCurrent.HasValue)
				{
					MovementActivityStateCurrent = ActivityState.Lost;
				}
				break;
			}
			AbilityActivityStatePredicted = null;
		}
	}

	public IUIDataProvider CurrentAbility { get; private set; }

	public IUIDataProvider PredictedAbility { get; private set; }

	public UsageType Type { get; private set; }

	public bool HasMoveDistance
	{
		get
		{
			if (RemainingMoveDistance.HasValue)
			{
				return RemainingMoveDistance > 0f;
			}
			return false;
		}
	}

	public bool HasFullMoveDistance
	{
		get
		{
			if (RemainingMoveDistance.HasValue)
			{
				return object.Equals(RemainingMoveDistance, MaxMoveDistance);
			}
			return false;
		}
	}

	public CombatAction(ActivityState? moveActivityStateDefault, ActivityState? attackActivityStateDefault, ActivityState? abilityActivityStateDefault, float? maxMoveDistance = null)
	{
		MaxMoveDistance = maxMoveDistance;
		ChangeDefaultStates(moveActivityStateDefault, attackActivityStateDefault, abilityActivityStateDefault);
	}

	public void SetPrediction(UsageType type, ActivityType activity, ActivityState state, IUIDataProvider ability = null, float? predictedMoveDistance = null)
	{
		switch (activity)
		{
		case ActivityType.Move:
			if (MovementActivityStateCurrent.HasValue && state >= MovementActivityStateCurrent.Value)
			{
				MovementActivityStatePredicted = state;
				PredictedMoveDistance = predictedMoveDistance;
				if (!LockType)
				{
					Type = type;
				}
			}
			break;
		case ActivityType.Attack:
			if (AttackActivityStateCurrent.HasValue && state >= AttackActivityStateCurrent.Value)
			{
				AttackActivityStatePredicted = state;
				PredictedMoveDistance = predictedMoveDistance;
				if (!LockType)
				{
					Type = type;
				}
				PredictedMoveDistance = null;
			}
			break;
		case ActivityType.Ability:
			if (!AbilityActivityStateCurrent.HasValue || state < AbilityActivityStateCurrent.Value)
			{
				break;
			}
			AbilityActivityStatePredicted = state;
			if (!LockType)
			{
				Type = type;
				if (ability != null)
				{
					PredictedAbility = ability;
				}
			}
			PredictedMoveDistance = null;
			break;
		default:
			throw new ArgumentOutOfRangeException("activity", activity, null);
		}
		MoveDistanceUpdated.Execute();
	}

	public void ChangeDefaultStates(ActivityState? moveActivityStateDefault, ActivityState? attackActivityStateDefault, ActivityState? abilityActivityStateDefault)
	{
		MovementActivityStateCurrent = moveActivityStateDefault;
		AttackActivityStateCurrent = attackActivityStateDefault;
		AbilityActivityStateCurrent = abilityActivityStateDefault;
		LockType = false;
	}

	public void Reset()
	{
		LockType = false;
		m_MovementActivityStateCurrent = ActivityState.Available;
		m_AttackActivityStateCurrent = ActivityState.Available;
		m_AbilityActivityStateCurrent = ActivityState.Available;
		m_MovementActivityStatePredicted = null;
		m_AttackActivityStatePredicted = null;
		m_AbilityActivityStatePredicted = null;
	}

	private bool IsLaterState(ActivityState? newState, ActivityState? oldState)
	{
		ActivityState num = (newState.HasValue ? newState.Value : ((ActivityState)(-1)));
		int num2 = (int)(oldState.HasValue ? oldState.Value : ((ActivityState)(-1)));
		return (int)num > num2;
	}

	public void UpdateCurrentStates(bool fromStartAction, bool success)
	{
		if (MovementActivityStateCurrent.HasValue && MovementActivityStatePredicted.HasValue)
		{
			if (MovementActivityStatePredicted.Value == ActivityState.WillBeUsed)
			{
				if (HasMoveDistance && HasMovePossibility)
				{
					MovementActivityStateCurrent = ActivityState.Available;
					if (!HasFullMoveDistance)
					{
						if (AttackActivityStateCurrent.HasValue)
						{
							AttackActivityStateCurrent = ActivityState.Lost;
						}
						if (AbilityActivityStateCurrent.HasValue)
						{
							AbilityActivityStateCurrent = ActivityState.Lost;
						}
						LockType = true;
					}
				}
				else
				{
					MovementActivityStateCurrent = ActivityState.Used;
					LockType = true;
				}
			}
			else if (MovementActivityStatePredicted.Value == ActivityState.WillBeLost && success)
			{
				MovementActivityStateCurrent = ActivityState.Lost;
			}
		}
		if (AttackActivityStateCurrent.HasValue && AttackActivityStatePredicted.HasValue && success)
		{
			if (AttackActivityStatePredicted.Value == ActivityState.WillBeUsed)
			{
				AttackActivityStateCurrent = ActivityState.Used;
				LockType = true;
			}
			else if (AttackActivityStatePredicted.Value == ActivityState.WillBeLost)
			{
				AttackActivityStateCurrent = ActivityState.Lost;
			}
		}
		if (AbilityActivityStateCurrent.HasValue && AbilityActivityStatePredicted.HasValue && success)
		{
			if (AbilityActivityStatePredicted.Value == ActivityState.WillBeUsed)
			{
				AbilityActivityStateCurrent = ActivityState.Used;
				LockType = true;
			}
			else if (AbilityActivityStatePredicted.Value == ActivityState.WillBeLost)
			{
				AbilityActivityStateCurrent = ActivityState.Lost;
			}
			if (PredictedAbility != null && CurrentAbility == null)
			{
				CurrentAbility = PredictedAbility;
			}
		}
	}

	public void UpdateRemainingDistance(float? remainingDistance, float maxDistance, bool hasMovePossibility)
	{
		HasMovePossibility = hasMovePossibility;
		RemainingMoveDistance = remainingDistance;
		MaxMoveDistance = maxDistance;
		if (PredictedMoveDistance > RemainingMoveDistance)
		{
			PredictedMoveDistance = RemainingMoveDistance;
		}
		MoveDistanceUpdated.Execute();
	}

	public void ClearPredictions()
	{
		MovementActivityStatePredicted = MovementActivityStateCurrent;
		AttackActivityStatePredicted = AttackActivityStateCurrent;
		AbilityActivityStatePredicted = AbilityActivityStateCurrent;
		PredictedAbility = null;
		if (!LockType)
		{
			Type = UsageType.None;
		}
	}

	private ActionState GetActionState(bool prediction = true)
	{
		List<ActivityState?> source = new List<ActivityState?>
		{
			(prediction && MovementActivityStatePredicted.HasValue) ? MovementActivityStatePredicted : MovementActivityStateCurrent,
			(prediction && AttackActivityStatePredicted.HasValue) ? AttackActivityStatePredicted : AttackActivityStateCurrent,
			(prediction && AbilityActivityStatePredicted.HasValue) ? AbilityActivityStatePredicted : AbilityActivityStateCurrent
		};
		if (source.Any((ActivityState? state) => state.HasValue && state.Value == ActivityState.WillBeUsed))
		{
			return ActionState.WillBeUsed;
		}
		if (source.All((ActivityState? state) => !state.HasValue || state.Value == ActivityState.WillBeLost))
		{
			return ActionState.WillBeLost;
		}
		if (source.All((ActivityState? state) => !state.HasValue || state.Value == ActivityState.Lost))
		{
			return ActionState.Lost;
		}
		if (source.Any((ActivityState? state) => state.HasValue && state.Value == ActivityState.Used))
		{
			return ActionState.Used;
		}
		if (source.Any((ActivityState? state) => state.HasValue && state.Value == ActivityState.Available))
		{
			return ActionState.Available;
		}
		return ActionState.Unable;
	}
}
