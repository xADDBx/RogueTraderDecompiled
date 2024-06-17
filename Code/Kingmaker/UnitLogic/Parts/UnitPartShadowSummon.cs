using Kingmaker.UnitLogic.Mechanics;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.Parts;

public class UnitPartShadowSummon : BaseUnitPart, IHashable
{
	public MechanicsContext Context { get; private set; }

	public void Setup(MechanicsContext parentContext)
	{
		Context = parentContext.CloneFor(base.Owner.Blueprint, base.Owner);
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
