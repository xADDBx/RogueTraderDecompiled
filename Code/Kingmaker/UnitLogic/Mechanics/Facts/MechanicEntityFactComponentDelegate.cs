using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.Mechanics.Facts;

public class MechanicEntityFactComponentDelegate : EntityFactComponentDelegate<MechanicEntity>, IHashable
{
	public new MechanicEntityFact Fact => (MechanicEntityFact)base.Fact;

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
