using JetBrains.Annotations;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Mechanics;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.Parts;

public class PartPetPolymorphed : BaseUnitPart, IHashable
{
	public PetPolymorphRTBuff Component { get; private set; }

	public GameObject ViewReplacement { get; set; }

	public void Setup([NotNull] PetPolymorphRTBuff component)
	{
		Component = component;
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
