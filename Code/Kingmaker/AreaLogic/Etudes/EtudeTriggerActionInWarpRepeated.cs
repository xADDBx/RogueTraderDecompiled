using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.Utility.Attributes;
using Newtonsoft.Json;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.AreaLogic.Etudes;

[TypeId("42be2d47b4b0428192d4a1bb2cd0a438")]
public class EtudeTriggerActionInWarpRepeated : EtudeBracketTrigger, ISectorMapWarpTravelRepeatedEventHandler, ISubscriber, IHashable
{
	public class SavableData : IEntityFactComponentSavableData, IHashable
	{
		[JsonProperty]
		public int CurrentJumpNumber;

		[JsonProperty]
		public int CurrentCallActionNumber;

		[JsonProperty]
		public bool IsCompleted;

		public override Hash128 GetHash128()
		{
			Hash128 result = default(Hash128);
			Hash128 val = base.GetHash128();
			result.Append(ref val);
			result.Append(ref CurrentJumpNumber);
			result.Append(ref CurrentCallActionNumber);
			result.Append(ref IsCompleted);
			return result;
		}
	}

	public enum RepeatCountTypes
	{
		Count,
		CountEvaluator,
		IfRunConditionTrue,
		Infinity
	}

	[Tooltip("Интервал вызова экшенов, раз в N прыжков, если 0 будем вызывать постоянно")]
	[SerializeField]
	private int m_CallActionWarpInterval;

	[Tooltip("Экшен лист который будет выполняться раз в CallActionWarpInterval прыжков")]
	[SerializeField]
	private ActionList m_Actions;

	[SerializeField]
	private RepeatCountTypes m_RepeatCountType = RepeatCountTypes.Infinity;

	[ShowIf("IsRepeatCountTypeCount")]
	[Tooltip("Количество повторений")]
	[SerializeField]
	private int m_RepeatCount;

	[ShowIf("IsRepeatCountTypeCountEvaluator")]
	[Tooltip("Количество повторений через IntEvaluator")]
	[SerializeReference]
	private IntEvaluator m_RepeatCountEvaluator;

	[ShowIf("IsRepeatCountTypeIfRunConditionTrue")]
	[Tooltip("Пока выполняется условие")]
	[SerializeField]
	private ConditionsChecker m_RunRestrictions;

	[SerializeField]
	private ConditionsChecker m_ForceEndRestrictions;

	[SerializeField]
	private bool m_CallActionsOnForceEnd;

	private bool IsRepeatCountTypeCount => m_RepeatCountType == RepeatCountTypes.Count;

	private bool IsRepeatCountTypeCountEvaluator => m_RepeatCountType == RepeatCountTypes.CountEvaluator;

	private bool IsRepeatCountTypeIfRunConditionTrue => m_RepeatCountType == RepeatCountTypes.IfRunConditionTrue;

	public void HandleStartJumpEvent()
	{
		SavableData savableData = RequestSavableData<SavableData>();
		if (savableData.IsCompleted)
		{
			return;
		}
		if (m_ForceEndRestrictions.HasConditions && m_ForceEndRestrictions.Check())
		{
			savableData.IsCompleted = true;
			if (m_CallActionsOnForceEnd)
			{
				CallActions();
			}
			return;
		}
		if (m_RepeatCountType == RepeatCountTypes.IfRunConditionTrue && m_RunRestrictions.HasConditions && !m_RunRestrictions.Check())
		{
			savableData.IsCompleted = true;
			return;
		}
		savableData.CurrentJumpNumber++;
		bool flag = savableData.CurrentJumpNumber % Mathf.Max(m_CallActionWarpInterval, 1) == 0;
		int num = int.MaxValue;
		switch (m_RepeatCountType)
		{
		case RepeatCountTypes.Count:
			num = m_RepeatCount;
			break;
		case RepeatCountTypes.CountEvaluator:
			num = m_RepeatCountEvaluator.GetValue();
			break;
		}
		if (flag)
		{
			CallActions();
			savableData.CurrentCallActionNumber++;
		}
		if (savableData.CurrentCallActionNumber >= num)
		{
			savableData.IsCompleted = true;
		}
	}

	private void CallActions()
	{
		Game.Instance.SectorMapTravelController.PauseTravel();
		m_Actions.Run();
		Game.Instance.SectorMapTravelController.UnpauseManual();
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
