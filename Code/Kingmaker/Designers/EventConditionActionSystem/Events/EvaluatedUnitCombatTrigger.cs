using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Designers.EventConditionActionSystem.ContextData;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Events;

[ComponentName("Events/EvaluatedUnitCombatTrigger")]
[AllowMultipleComponents]
[TypeId("f77f7470b5b4ccf489fa052f95c399a1")]
public class EvaluatedUnitCombatTrigger : EntityFactComponentDelegate, IUnitCombatHandler, ISubscriber<IBaseUnitEntity>, ISubscriber, IHashable
{
	public class UnitData : SingleUnitData<UnitData>
	{
	}

	[SerializeReference]
	public AbstractUnitEvaluator Unit;

	public ActionList Actions;

	public bool TriggerOnExit;

	public void HandleUnitJoinCombat()
	{
		BaseUnitEntity baseUnitEntity = EventInvokerExtensions.BaseUnitEntity;
		using (ContextData<FactData>.Request().Setup(base.Fact))
		{
			if (Unit.Is(baseUnitEntity) && !TriggerOnExit)
			{
				using (ContextData<UnitData>.Request().Setup(baseUnitEntity))
				{
					Actions.Run();
					return;
				}
			}
		}
	}

	public void HandleUnitLeaveCombat()
	{
		BaseUnitEntity baseUnitEntity = EventInvokerExtensions.BaseUnitEntity;
		using (ContextData<FactData>.Request().Setup(base.Fact))
		{
			if (Unit.Is(baseUnitEntity) && TriggerOnExit)
			{
				using (ContextData<UnitData>.Request().Setup(baseUnitEntity))
				{
					Actions.Run();
					return;
				}
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
