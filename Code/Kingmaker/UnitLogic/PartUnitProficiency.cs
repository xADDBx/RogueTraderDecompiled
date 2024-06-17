using System.Collections.Generic;
using Kingmaker.Blueprints.Items.Armors;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.Enums;
using Kingmaker.Utility;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic;

public class PartUnitProficiency : EntityPart, IHashable
{
	public interface IOwner : IEntityPartOwner<PartUnitProficiency>, IEntityPartOwner
	{
		PartUnitProficiency Proficiencies { get; }
	}

	private readonly MultiSet<ArmorProficiencyGroup> m_ArmorProficiencies = new MultiSet<ArmorProficiencyGroup> { ArmorProficiencyGroup.None };

	private readonly MultiSet<WeaponProficiency> m_WeaponProficiencies = new MultiSet<WeaponProficiency>
	{
		new WeaponProficiency(WeaponCategory.None)
	};

	public IEnumerable<ArmorProficiencyGroup> ArmorProficiencies => m_ArmorProficiencies;

	public IEnumerable<WeaponProficiency> WeaponProficiencies => m_WeaponProficiencies;

	public void Add(ArmorProficiencyGroup proficiency)
	{
		m_ArmorProficiencies.Add(proficiency);
	}

	public void Add(in WeaponProficiency proficiency)
	{
		m_WeaponProficiencies.Add(proficiency);
	}

	public void Remove(ArmorProficiencyGroup proficiency)
	{
		m_ArmorProficiencies.Remove(proficiency);
	}

	public void Remove(in WeaponProficiency proficiency)
	{
		m_WeaponProficiencies.Remove(proficiency);
	}

	public void Clear()
	{
		m_ArmorProficiencies.Clear();
		m_WeaponProficiencies.Clear();
	}

	public bool Contains(ArmorProficiencyGroup proficiency)
	{
		return m_ArmorProficiencies.Contains(proficiency);
	}

	public bool Contains(WeaponProficiency proficiency)
	{
		return m_WeaponProficiencies.Contains(proficiency);
	}

	public bool Contains(WeaponCategory category, WeaponFamily family)
	{
		if (!m_WeaponProficiencies.Contains(new WeaponProficiency(category, family)))
		{
			return m_WeaponProficiencies.Contains(new WeaponProficiency(category));
		}
		return true;
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
