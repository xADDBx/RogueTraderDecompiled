using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Enums;
using Kingmaker.UnitLogic.Mechanics;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.Parts;

public class UnitPronePart : UnitPart, IUnitConditionsChanged<EntitySubscriber>, IUnitConditionsChanged, ISubscriber<IAbstractUnitEntity>, ISubscriber, IEventTag<IUnitLifeStateChanged, EntitySubscriber>, IUnitLifeStateChanged<EntitySubscriber>, IUnitLifeStateChanged, IHashable
{
	private void Update()
	{
		if ((bool)base.Owner.View)
		{
			if (base.Owner.State.IsProne)
			{
				base.Owner.View.EnterProneState();
			}
			else
			{
				base.Owner.View.LeaveProneState();
			}
		}
		if (base.Owner.State.IsProne)
		{
			base.Owner.Commands.InterruptAllInterruptible();
		}
		base.Owner.Wake(1f);
	}

	public void HandleUnitConditionsChanged(UnitCondition condition)
	{
		Update();
	}

	public void HandleUnitLifeStateChanged(UnitLifeState prevLifeState)
	{
		Update();
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
