using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.Designers.EventConditionActionSystem.ContextData;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Newtonsoft.Json;
using Owlcat.Runtime.Core.Utility.EditorAttributes;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Designers.Mechanics.Facts;

[ComponentName("Events/EvaluatedUnitTurnTrigger")]
[AllowMultipleComponents]
[TypeId("6944afb3caa12a445ba1b41c97d4eb1e")]
public class EvaluatedUnitTurnTrigger : EntityFactComponentDelegate, ITurnStartHandler, ISubscriber<IMechanicEntity>, ISubscriber, ITurnEndHandler, IHashable
{
	public class Data : IEntityFactComponentSavableData, IHashable
	{
		[JsonProperty]
		public int CurrentTurn;

		[JsonProperty]
		public bool IsOneActivated;

		public override Hash128 GetHash128()
		{
			Hash128 result = default(Hash128);
			Hash128 val = base.GetHash128();
			result.Append(ref val);
			result.Append(ref CurrentTurn);
			result.Append(ref IsOneActivated);
			return result;
		}
	}

	[SerializeReference]
	public AbstractUnitEvaluator Unit;

	[InfoBox(Text = "The number of the round in the battle when the trigger is triggered. The default value is '1', i.e. the trigger is triggered at the beginning of the mob's turn in the first round of the battle. '0'-triggered at the beginning of the unit's turn each round")]
	public int Round = 1;

	public ConditionsChecker Conditions;

	[InfoBox(Text = "A one-time trigger, if 'true'. It is used if the 'Round = 0' field, otherwise it is ignored. By default: 'false'.")]
	public bool Once;

	[InfoBox(Text = "It is triggered at the end of the unit's turn instead of the beginning. By default: 'false'.")]
	public bool TriggerOnEndTurn;

	public ActionList Actions;

	protected override void OnFactAttached()
	{
		Data data = RequestSavableData<Data>();
		data.CurrentTurn = 0;
		data.IsOneActivated = false;
	}

	void ITurnStartHandler.HandleUnitStartTurn(bool isTurnBased)
	{
		if (!TriggerOnEndTurn)
		{
			EventHandler(EventInvokerExtensions.MechanicEntity);
		}
	}

	void ITurnEndHandler.HandleUnitEndTurn(bool isTurnBased)
	{
		if (TriggerOnEndTurn)
		{
			EventHandler(EventInvokerExtensions.MechanicEntity);
		}
	}

	private void EventHandler(MechanicEntity entity)
	{
		if (!Game.Instance.Player.IsInCombat)
		{
			return;
		}
		using (ContextData<FactData>.Request().Setup(base.Fact))
		{
			if (Unit == null || !Unit.Is(entity) || ((BaseUnitEntity)entity).LifeState.IsDead || !Conditions.Check())
			{
				return;
			}
			Data data = RequestSavableData<Data>();
			data.CurrentTurn++;
			if (Round == 0 && Once)
			{
				if (!data.IsOneActivated)
				{
					Actions.Run();
					data.IsOneActivated = true;
				}
			}
			else if (Round == 0)
			{
				Actions.Run();
			}
			else if (data.CurrentTurn == Round)
			{
				Actions.Run();
			}
		}
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
