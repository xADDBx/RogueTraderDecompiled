using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Designers.Mechanics.Buffs;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Persistence.JsonUtility;
using Kingmaker.UnitLogic.Buffs;
using Newtonsoft.Json;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.SpaceCombat.StarshipLogic.Parts;

[Serializable]
public class StarshipSectorShields : IHashable
{
	[JsonProperty]
	private int m_Damage;

	[JsonProperty]
	private bool m_Reinforced;

	[JsonProperty]
	private bool m_WasHitLastTurn;

	private PartStarshipShields m_Owner;

	[JsonProperty]
	public StarshipSectorShieldsType Sector { get; private set; }

	public int Max => GetMax();

	public int Current => Max - Damage;

	public int Damage
	{
		get
		{
			return m_Damage;
		}
		set
		{
			m_Damage = Math.Clamp(value, 0, Max);
		}
	}

	public bool Reinforced
	{
		get
		{
			return m_Reinforced;
		}
		set
		{
			if (!value)
			{
				foreach (Buff item in new List<Buff>(m_Owner.Owner.Buffs.RawFacts))
				{
					EntityFactComponent entityFactComponent = item.Components.Find((EntityFactComponent c) => c.SourceBlueprintComponent is StarshipShieldEnhancement);
					if (entityFactComponent != null && entityFactComponent.SourceBlueprintComponent is StarshipShieldEnhancement { isReinforcement: not false } starshipShieldEnhancement && starshipShieldEnhancement.ValidFor(Sector))
					{
						m_Owner.Owner.Buffs.Remove(item);
					}
				}
			}
			m_Reinforced = value;
		}
	}

	public bool WasHitLastTurn
	{
		get
		{
			return m_WasHitLastTurn;
		}
		set
		{
			m_WasHitLastTurn = value;
		}
	}

	public int RamAbsorbMod
	{
		get
		{
			if (!m_Reinforced)
			{
				return 6;
			}
			return 4;
		}
	}

	private int GetMax()
	{
		int num = Sector switch
		{
			StarshipSectorShieldsType.Fore => m_Owner.VoidShieldGenerator?.Fore ?? 0, 
			StarshipSectorShieldsType.Port => m_Owner.VoidShieldGenerator?.Port ?? 0, 
			StarshipSectorShieldsType.Starboard => m_Owner.VoidShieldGenerator?.Starboard ?? 0, 
			StarshipSectorShieldsType.Aft => m_Owner.VoidShieldGenerator?.Aft ?? 0, 
			_ => 0, 
		};
		IEnumerable<StarshipShieldEnhancement> source = from ench in m_Owner.Owner.Facts.GetComponents<StarshipShieldEnhancement>()
			where ench.ValidFor(Sector)
			select ench;
		int num2 = source.Select((StarshipShieldEnhancement ench) => ench.bonusFlat).DefaultIfEmpty(0).Sum();
		int num3 = source.Select((StarshipShieldEnhancement ench) => ench.bonusPct).DefaultIfEmpty(0).Sum();
		return (num + num2) * (100 + num3) / 100;
	}

	public StarshipSectorShields(PartStarshipShields owner, StarshipSectorShieldsType sector)
	{
		m_Owner = owner;
		Sector = sector;
	}

	[JsonConstructor]
	public StarshipSectorShields(JsonConstructorMark _)
	{
	}

	public void PrePostLoad(PartStarshipShields owner)
	{
		m_Owner = owner;
	}

	public override string ToString()
	{
		return $"{Sector} shields: {Current}/{Max}";
	}

	public virtual Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		result.Append(ref m_Damage);
		result.Append(ref m_Reinforced);
		result.Append(ref m_WasHitLastTurn);
		StarshipSectorShieldsType val = Sector;
		result.Append(ref val);
		return result;
	}
}
