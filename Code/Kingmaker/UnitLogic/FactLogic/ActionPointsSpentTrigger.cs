using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Enums;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility.Attributes;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.FactLogic;

[TypeId("e36bc5ac422a452cb2757717cbd3f5a4")]
public class ActionPointsSpentTrigger : ActionPointsChangedTrigger, IUnitSpentActionPoints<EntitySubscriber>, IUnitSpentActionPoints, ISubscriber<IMechanicEntity>, ISubscriber, IEventTag<IUnitSpentActionPoints, EntitySubscriber>, IUnitSpentMovementPoints<EntitySubscriber>, IUnitSpentMovementPoints, IEventTag<IUnitSpentMovementPoints, EntitySubscriber>, IHashable
{
	[SerializeField]
	[KDB("Надо ли записывать количество потраченных AP в контекстную переменную для использования в экшенах")]
	private bool m_SavePointsSpentAsContextValue;

	[SerializeField]
	[ShowIf("m_SavePointsSpentAsContextValue")]
	[KDB("Контекстная переменная для хранения количества потраченных AP.\nВажно: Movement AP будут округлены до ближайшего целого числа")]
	private ContextPropertyName m_PointsSpentContextValue;

	[SerializeField]
	[KDB("Должен ли компонент отрабатывать по достижению только триггерного значения")]
	private bool m_UseTriggeredValue;

	[SerializeField]
	[ShowIf("m_UseTriggeredValue")]
	private ContextValue m_TriggerValue;

	private int TriggerValue => m_TriggerValue.Calculate(base.Context);

	public void HandleUnitSpentActionPoints(int actionPointsSpent)
	{
		if (m_Type != 0 || !Restriction.IsPassed(base.Fact, base.Owner))
		{
			return;
		}
		int actionPointsYellow = base.Owner.CombatState.ActionPointsYellow;
		if ((!m_UseTriggeredValue || actionPointsYellow <= TriggerValue) && (actionPointsYellow + actionPointsSpent > TriggerValue || actionPointsSpent == -1))
		{
			if (m_SavePointsSpentAsContextValue)
			{
				base.Context[m_PointsSpentContextValue] = actionPointsSpent;
			}
			base.Fact.RunActionInContext(Actions);
		}
	}

	public void HandleUnitSpentMovementPoints(float movementPointsSpent)
	{
		if (m_Type != PointsType.Blue || !Restriction.IsPassed(base.Fact, base.Owner))
		{
			return;
		}
		float actionPointsBlue = base.Owner.CombatState.ActionPointsBlue;
		if ((!m_UseTriggeredValue || !(actionPointsBlue > (float)TriggerValue)) && (!(actionPointsBlue + movementPointsSpent <= (float)TriggerValue) || !(movementPointsSpent >= 0f)))
		{
			if (m_SavePointsSpentAsContextValue)
			{
				base.Context[m_PointsSpentContextValue] = (int)movementPointsSpent;
			}
			base.Fact.RunActionInContext(Actions);
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
