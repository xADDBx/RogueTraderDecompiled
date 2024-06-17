using Kingmaker.Code.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Newtonsoft.Json;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.UI.Models.UnitSettings;

public class MechanicActionBarSlotMemorizedSpell : MechanicActionBarSlotSpell, IHashable
{
	[JsonProperty]
	public SpellSlot SpellSlot { get; }

	public override AbilityData Spell => SpellSlot?.Spell;

	[JsonConstructor]
	public MechanicActionBarSlotMemorizedSpell(SpellSlot spellSlot)
	{
		SpellSlot = spellSlot;
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		Hash128 val2 = ClassHasher<SpellSlot>.GetHash128(SpellSlot);
		result.Append(ref val2);
		return result;
	}
}
