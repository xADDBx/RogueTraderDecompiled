using System;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.UnitLogic.Abilities;
using UnityEngine;

namespace Kingmaker.Blueprints.Root.Fx;

[TypeId("7571b15930bb480d812ed4476db15a1e")]
public class CastsGroup : BlueprintScriptableObject
{
	[SerializeField]
	private CastGroupForSpellSource m_ArcaneCasts;

	[SerializeField]
	private CastGroupForSpellSource m_DivineCasts;

	public GameObject GetPreCast(SpellSource spellSource)
	{
		return GetSpellSourceGroup(spellSource).PreCast;
	}

	public GameObject GetPreCastGround(SpellSource spellSource)
	{
		return GetSpellSourceGroup(spellSource).PreCastGround;
	}

	public GameObject GetCast(SpellSource spellSource)
	{
		return GetSpellSourceGroup(spellSource).Cast;
	}

	public GameObject GetCastGround(SpellSource spellSource)
	{
		return GetSpellSourceGroup(spellSource).CastGround;
	}

	public GameObject GetCastFail(SpellSource spellSource)
	{
		return GetSpellSourceGroup(spellSource).CastFail;
	}

	public GameObject GetCastFailGround(SpellSource spellSource)
	{
		return GetSpellSourceGroup(spellSource).CastFailGround;
	}

	private CastGroupForSpellSource GetSpellSourceGroup(SpellSource spellSource)
	{
		return spellSource switch
		{
			SpellSource.Arcane => m_ArcaneCasts, 
			SpellSource.Divine => m_DivineCasts, 
			_ => throw new ArgumentOutOfRangeException("spellSource", spellSource, null), 
		};
	}
}
