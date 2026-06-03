using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Mechanics;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.Parts;

public class PartDestroyOnCombatEnd : MechanicEntityPart, IPartyCombatHandler, ISubscriber, IHashable
{
	public void HandlePartyCombatStateChanged(bool inCombat)
	{
		if (!inCombat)
		{
			Game.Instance.EntityDestroyer.Destroy(base.Owner);
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
