using System.Collections.Generic;
using System.Linq;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Levelup.Obsolete.Blueprints.Spells;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility;
using Kingmaker.Utility.DotNetExtensions;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.Parts;

public class UnitPartBuffSuppress : BaseUnitPart, IHashable
{
	private readonly MultiSet<SpellDescriptor> m_SpellDescriptors = new MultiSet<SpellDescriptor>();

	private readonly MultiSet<BlueprintBuff> m_Buffs = new MultiSet<BlueprintBuff>();

	private readonly MultiSet<SpellSchool> m_SpellSchools = new MultiSet<SpellSchool>();

	private static IEnumerable<SpellDescriptor> GetValues(SpellDescriptor spellDescriptor)
	{
		return from v in EnumUtils.GetValues<SpellDescriptor>()
			where v != SpellDescriptor.None && (spellDescriptor & v) != 0
			select v;
	}

	public void Suppress(SpellSchool[] spellSchools)
	{
		foreach (SpellSchool item in spellSchools)
		{
			m_SpellSchools.Add(item);
		}
	}

	public void Suppress(SpellDescriptor spellDescriptor)
	{
		foreach (SpellDescriptor value in GetValues(spellDescriptor))
		{
			m_SpellDescriptors.Add(value);
		}
		Update();
	}

	public void Suppress(BlueprintBuff buff)
	{
		m_Buffs.Add(buff);
		Update();
	}

	public void Release(SpellSchool[] spellSchools)
	{
		foreach (SpellSchool item in spellSchools)
		{
			m_SpellSchools.Remove(item);
		}
		Update();
		TryRemovePart();
	}

	public void Release(SpellDescriptor spellDescriptor)
	{
		foreach (SpellDescriptor value in GetValues(spellDescriptor))
		{
			m_SpellDescriptors.Remove(value);
		}
		Update();
		TryRemovePart();
	}

	public void Release(BlueprintBuff buff)
	{
		m_Buffs.Remove(buff);
		Update();
		TryRemovePart();
	}

	private void TryRemovePart()
	{
		if (!m_Buffs.Any() && !m_SpellDescriptors.Any() && !m_SpellSchools.Any())
		{
			base.Owner.Remove<UnitPartBuffSuppress>();
		}
	}

	public bool IsSuppressed(Buff buff)
	{
		if (!m_Buffs.Contains(buff.Blueprint) && !GetValues(buff.Context.SpellDescriptor).Any((SpellDescriptor d) => m_SpellDescriptors.Contains(d)))
		{
			return m_SpellSchools.Contains(buff.Context.SpellSchool);
		}
		return true;
	}

	private void Update()
	{
		foreach (Buff buff in base.Owner.Buffs)
		{
			bool flag = IsSuppressed(buff);
			if (buff.IsSuppressed != flag)
			{
				if (flag && buff.Active)
				{
					buff.Deactivate();
				}
				buff.IsSuppressed = flag;
				if (!flag && !buff.Active)
				{
					buff.Activate();
				}
			}
		}
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
