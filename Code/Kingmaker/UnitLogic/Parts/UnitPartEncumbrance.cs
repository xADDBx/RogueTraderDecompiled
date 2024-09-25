using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Mechanics;
using Newtonsoft.Json;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.Parts;

public class UnitPartEncumbrance : BaseUnitPart, IHashable
{
	[JsonProperty]
	public Encumbrance Value { get; private set; }

	public static int GetArmorCheckPenalty(BaseUnitEntity owner, Encumbrance encumbrance)
	{
		switch (encumbrance)
		{
		case Encumbrance.Medium:
			return -3;
		case Encumbrance.Heavy:
		case Encumbrance.Overload:
			return -6;
		default:
			return 0;
		}
	}

	public static int? GetMaxDexterityBonus(BaseUnitEntity owner, Encumbrance encumbrance)
	{
		switch (encumbrance)
		{
		case Encumbrance.Medium:
			return 3;
		case Encumbrance.Heavy:
		case Encumbrance.Overload:
			return 1;
		default:
			return null;
		}
	}

	public void Init(Encumbrance encumbrance)
	{
		if (Value != encumbrance)
		{
			Clean();
			Value = encumbrance;
		}
	}

	protected override void OnDetach()
	{
		Clean();
	}

	private void Clean()
	{
	}

	public void Set(Encumbrance value)
	{
		Value = value;
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		Encumbrance val2 = Value;
		result.Append(ref val2);
		return result;
	}
}
