using System.Linq;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.EntitySystem.Stats;

public class ModifiableValueSkill : ModifiableValueDependent<ModifiableValueAttributeStat>, IHashable
{
	public bool HasPenalties => base.Modifiers.Any((Modifier m) => !m.IsPermanent() && m.ModValue < 0);

	public bool HasBonuses => base.Modifiers.Any((Modifier m) => !m.IsPermanent() && m.ModValue > 0);

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
