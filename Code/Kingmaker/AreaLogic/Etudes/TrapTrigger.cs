using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.View.MapObjects.Traps;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.AreaLogic.Etudes;

[TypeId("451f931d6df745fe80a6934fe11eda36")]
public class TrapTrigger : EntityFactComponentDelegate, IDisarmTrapHandler, ISubscriber<IBaseUnitEntity>, ISubscriber, ITrapActivationHandler, IHashable
{
	[SerializeReference]
	public MapObjectEvaluator Trap;

	public ActionList OnActivation;

	public ActionList OnDisarm;

	public void HandleDisarmTrapSuccess(TrapObjectView trap)
	{
		if (EqualsToReferenced(trap))
		{
			OnDisarm.Run();
		}
	}

	public void HandleDisarmTrapFail(TrapObjectView trap)
	{
	}

	public void HandleDisarmTrapCriticalFail(TrapObjectView trap)
	{
	}

	public void HandleTrapActivation(TrapObjectView trap)
	{
		if (EqualsToReferenced(trap))
		{
			OnActivation.Run();
		}
	}

	private bool EqualsToReferenced(TrapObjectView trap)
	{
		if (Trap.TryGetValue(out var value))
		{
			return value.UniqueId == trap.Data.UniqueId;
		}
		return false;
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
