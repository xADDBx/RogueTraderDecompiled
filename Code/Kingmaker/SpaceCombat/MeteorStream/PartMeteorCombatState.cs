using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.UnitLogic.Mechanics;
using Newtonsoft.Json;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.SpaceCombat.MeteorStream;

public class PartMeteorCombatState : MechanicEntityPart<MeteorStreamEntity>, IHashable
{
	public interface IOwner : IEntityPartOwner<PartMeteorCombatState>, IEntityPartOwner
	{
		PartMeteorCombatState CombatState { get; }
	}

	[JsonProperty]
	public bool IsInCombat { get; set; }

	public void JoinCombat()
	{
		IsInCombat = true;
	}

	public void LeaveCombat()
	{
		IsInCombat = false;
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		bool val2 = IsInCombat;
		result.Append(ref val2);
		return result;
	}
}
