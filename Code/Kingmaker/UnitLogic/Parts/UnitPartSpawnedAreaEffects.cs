using System.Collections.Generic;
using Kingmaker.Blueprints;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.QA;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility.DotNetExtensions;
using Newtonsoft.Json;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.UnitLogic.Parts;

public class UnitPartSpawnedAreaEffects : BaseUnitPart, IHashable
{
	public class Entry : IHashable
	{
		[JsonProperty]
		public EntityFactSource Source;

		[JsonProperty]
		public EntityRef<AreaEffectEntity> EntityRef;

		public virtual Hash128 GetHash128()
		{
			Hash128 result = default(Hash128);
			Hash128 val = ClassHasher<EntityFactSource>.GetHash128(Source);
			result.Append(ref val);
			EntityRef<AreaEffectEntity> obj = EntityRef;
			Hash128 val2 = StructHasher<EntityRef<AreaEffectEntity>>.GetHash128(ref obj);
			result.Append(ref val2);
			return result;
		}
	}

	[JsonProperty]
	private readonly List<Entry> m_Entries = new List<Entry>();

	public void Add(EntityFact sourceFact, BlueprintComponent sourceComponent, AreaEffectEntity areaEffect)
	{
		Entry entry = m_Entries.FirstItem((Entry i) => i.Source.IsFrom(sourceFact, sourceComponent));
		if (entry != null)
		{
			PFLog.Default.ErrorWithReport($"AreaEffect from {sourceFact}.{sourceComponent} already exists");
			m_Entries.Remove(entry);
		}
		entry = new Entry
		{
			Source = new EntityFactSource(sourceFact, sourceComponent),
			EntityRef = areaEffect
		};
		m_Entries.Add(entry);
	}

	public void RemoveAndEnd(EntityFact sourceFact, BlueprintComponent sourceComponent)
	{
		Entry entry = m_Entries.FirstItem((Entry i) => i.Source.IsFrom(sourceFact, sourceComponent));
		entry?.EntityRef.Entity?.ForceEnd();
		m_Entries.Remove(entry);
		if (m_Entries.Empty())
		{
			RemoveSelf();
		}
	}

	public AreaEffectEntity Get(EntityFact sourceFact, BlueprintComponent sourceComponent)
	{
		return m_Entries.FirstItem((Entry i) => i.Source.IsFrom(sourceFact, sourceComponent))?.EntityRef.Entity;
	}

	public bool Contains(BlueprintAbilityAreaEffect areaEffect)
	{
		return m_Entries.Any((Entry x) => x.EntityRef.Entity?.Blueprint == areaEffect);
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		List<Entry> entries = m_Entries;
		if (entries != null)
		{
			for (int i = 0; i < entries.Count; i++)
			{
				Hash128 val2 = ClassHasher<Entry>.GetHash128(entries[i]);
				result.Append(ref val2);
			}
		}
		return result;
	}
}
