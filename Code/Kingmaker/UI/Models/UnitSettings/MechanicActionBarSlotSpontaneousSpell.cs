using Kingmaker.UnitLogic.Abilities;
using Newtonsoft.Json;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.UI.Models.UnitSettings;

public class MechanicActionBarSlotSpontaneousSpell : MechanicActionBarSlotSpell, IHashable
{
	[JsonProperty]
	private AbilityData m_Spell;

	public override AbilityData Spell => m_Spell;

	[JsonConstructor]
	public MechanicActionBarSlotSpontaneousSpell(AbilityData spell)
	{
		m_Spell = spell;
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		Hash128 val2 = ClassHasher<AbilityData>.GetHash128(m_Spell);
		result.Append(ref val2);
		return result;
	}
}
