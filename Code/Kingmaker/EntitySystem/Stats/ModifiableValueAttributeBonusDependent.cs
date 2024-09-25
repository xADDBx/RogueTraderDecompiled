using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.EntitySystem.Stats;

public class ModifiableValueAttributeBonusDependent : ModifiableValueDependent<ModifiableValueAttributeStat>, IHashable
{
	public override int BaseStatBonus => base.BaseStat.Bonus;

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
