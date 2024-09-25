using JetBrains.Annotations;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using UnityEngine;

namespace Kingmaker.Blueprints.Root;

public class StatusBuffsRoot : ScriptableObject
{
	public StatDamageEntry[] Entries = new StatDamageEntry[0];

	[CanBeNull]
	public BlueprintBuff GetDamageBuff(StatType attribute, bool drain)
	{
		StatDamageEntry[] entries = Entries;
		foreach (StatDamageEntry statDamageEntry in entries)
		{
			if (statDamageEntry.Attribute == attribute)
			{
				if (!drain)
				{
					return statDamageEntry.DamageBuff;
				}
				return statDamageEntry.DrainBuff;
			}
		}
		return null;
	}
}
