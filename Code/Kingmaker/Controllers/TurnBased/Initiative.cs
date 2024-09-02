using System.Linq;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.QA;
using Kingmaker.UnitLogic.Squads;
using Newtonsoft.Json;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Controllers.TurnBased;

public class Initiative : IHashable
{
	public enum Event
	{
		RoundStart,
		RoundEnd,
		TurnStart
	}

	public const float Min = 1f;

	[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
	public float Roll { get; set; }

	[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
	public float Value { get; set; }

	[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
	public int Order { get; set; }

	[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
	public int InterruptingOrder { get; set; }

	[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
	public int WasPreparedForRound { get; set; }

	[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
	public bool PreparationInterrupted { get; set; }

	[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
	public int LastTurn { get; set; }

	public double TurnOrderPriority => (double)Value + 0.001 - (double)Order * 0.0001;

	public bool ActedThisRound => LastTurn == Game.Instance.TurnController.GameRound;

	public bool Empty => Value == 0f;

	public bool RollEmpty => Roll < 0.001f;

	public void Clear()
	{
		Value = 0f;
		Order = 0;
		InterruptingOrder = 0;
		Roll = 0f;
		WasPreparedForRound = 0;
	}

	public bool ShouldActNow(bool isTurnBased, Event @event, out int actRound)
	{
		if (Game.Instance.TurnController.IsPreparationTurn)
		{
			actRound = 0;
			return false;
		}
		int gameRound = Game.Instance.TurnController.GameRound;
		if (!isTurnBased)
		{
			actRound = ((@event == Event.RoundStart) ? gameRound : 0);
		}
		else if (LastTurn >= gameRound)
		{
			actRound = 0;
		}
		else
		{
			switch (@event)
			{
			case Event.RoundStart:
				actRound = 0;
				break;
			case Event.RoundEnd:
				actRound = gameRound;
				break;
			default:
			{
				if (Game.Instance.TurnController.CurrentUnit is UnitSquad unitSquad)
				{
					double num = unitSquad.Units.Min((UnitReference u) => u.ToBaseUnitEntity().Initiative.TurnOrderPriority);
					actRound = ((TurnOrderPriority >= num) ? gameRound : 0);
					break;
				}
				MechanicEntity currentUnit = Game.Instance.TurnController.CurrentUnit;
				if (currentUnit != null)
				{
					actRound = ((TurnOrderPriority >= currentUnit.Initiative.TurnOrderPriority) ? gameRound : 0);
					break;
				}
				actRound = 0;
				TurnController turnController = Game.Instance.TurnController;
				if ((turnController == null || (!turnController.IsPreparationTurn && !turnController.IsManualCombatTurn)) ? true : false)
				{
					PFLog.Default.ErrorWithReport("Something wrong with initiative of buff or area effect");
				}
				break;
			}
			}
		}
		return actRound > 0;
	}

	public static int OrderAscending(Initiative i1, Initiative i2)
	{
		return i1.TurnOrderPriority.CompareTo(i2.TurnOrderPriority);
	}

	public static int OrderDescending(Initiative i1, Initiative i2)
	{
		return -OrderAscending(i1, i2);
	}

	public static int OrderAscending(IInitiativeHolder i1, IInitiativeHolder i2)
	{
		return OrderAscending(i1.Initiative, i2.Initiative);
	}

	public static int OrderDescending(IInitiativeHolder i1, IInitiativeHolder i2)
	{
		return OrderDescending(i1.Initiative, i2.Initiative);
	}

	public void ChangePlaces(Initiative initiative)
	{
		if (initiative != this)
		{
			float value = initiative.Value;
			Value = initiative.Value;
			initiative.Value = value;
			int order = initiative.Order;
			Order = initiative.Order;
			initiative.Order = order;
			EventBus.RaiseEvent(delegate(IInitiativeChangeHandler h)
			{
				h.HandleInitiativeChanged();
			});
		}
	}

	public void CopyFrom(Initiative initiative)
	{
		Roll = initiative.Roll;
		Value = initiative.Value;
		Order = initiative.Order;
	}

	public virtual Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		float val = Roll;
		result.Append(ref val);
		float val2 = Value;
		result.Append(ref val2);
		int val3 = Order;
		result.Append(ref val3);
		int val4 = InterruptingOrder;
		result.Append(ref val4);
		int val5 = WasPreparedForRound;
		result.Append(ref val5);
		bool val6 = PreparationInterrupted;
		result.Append(ref val6);
		int val7 = LastTurn;
		result.Append(ref val7);
		return result;
	}
}
