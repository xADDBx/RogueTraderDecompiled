using System.Collections.Generic;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Pathfinding;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility.DotNetExtensions;
using Newtonsoft.Json;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.UnitLogic.Parts;

public class UnitPartVigilBeyondTime : BaseUnitPart, IHashable
{
	[JsonProperty]
	public List<VigilEntry> Entries = new List<VigilEntry>();

	public void AddEntry(Buff buff, BlueprintAbility teleportAbility)
	{
		VigilEntry vigilEntry = new VigilEntry();
		vigilEntry.OldPosition = buff.Owner.Position.GetNearestNodeXZUnwalkable();
		vigilEntry.OldDamage = buff.Owner.LifeState.Health.Damage;
		vigilEntry.Buff = buff;
		vigilEntry.TeleportAbility = teleportAbility;
		Entries.Add(vigilEntry);
	}

	public bool HasEntries()
	{
		return !Entries.Empty();
	}

	public void RemoveEntry(Buff buff)
	{
		Entries.RemoveAll((VigilEntry p) => p.Buff == buff || p.Buff == null);
	}

	public void ActivateVigil(UnitEntity unit)
	{
		VigilEntry entry = Entries.FirstOrDefault((VigilEntry p) => p.Buff?.Owner == unit);
		if (entry == null)
		{
			return;
		}
		AbilityData spell = new AbilityData(entry.TeleportAbility, unit);
		Vector3 vector = entry.OldPosition?.Vector3Position ?? unit.Position;
		int distanceToInCells = unit.DistanceToInCells(vector);
		if (distanceToInCells > 0)
		{
			unit.LifeState.Health.SetDamage(entry.OldDamage);
			if (unit.IsEnemy(base.Owner))
			{
				EventBus.RaiseEvent(delegate(IUnitAbilityNonPushForceMoveHandler h)
				{
					h.HandleUnitNonPushForceMove(distanceToInCells, entry.Buff.Context, unit);
				});
			}
			Rulebook.Trigger(new RulePerformAbility(spell, vector)
			{
				IgnoreCooldown = true,
				ForceFreeAction = true
			});
			RemoveEntry(entry.Buff);
		}
		else
		{
			unit.LifeState.Health.SetDamage(entry.OldDamage);
			entry.Buff.Remove();
			RemoveEntry(entry.Buff);
		}
	}

	protected override void OnPostLoad()
	{
		base.OnPostLoad();
		foreach (VigilEntry entry in Entries)
		{
			entry.OnPostLoad();
		}
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		List<VigilEntry> entries = Entries;
		if (entries != null)
		{
			for (int i = 0; i < entries.Count; i++)
			{
				Hash128 val2 = ClassHasher<VigilEntry>.GetHash128(entries[i]);
				result.Append(ref val2);
			}
		}
		return result;
	}
}
