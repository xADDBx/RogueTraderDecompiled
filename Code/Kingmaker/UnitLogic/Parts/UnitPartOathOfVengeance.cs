using System.Collections.Generic;
using System.Linq;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility.DotNetExtensions;
using Newtonsoft.Json;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.UnitLogic.Parts;

public class UnitPartOathOfVengeance : BaseUnitPart, IGlobalRulebookHandler<RuleDealDamage>, IRulebookHandler<RuleDealDamage>, ISubscriber, IGlobalRulebookSubscriber, ITurnBasedModeHandler, IHashable
{
	[JsonProperty]
	public List<OathOfVengeanceEntry> Entries = new List<OathOfVengeanceEntry>();

	public void AddEntry(UnitEntity ally, UnitEntity enemy)
	{
		OathOfVengeanceEntry oathOfVengeanceEntry = new OathOfVengeanceEntry();
		oathOfVengeanceEntry.Ally = ally;
		oathOfVengeanceEntry.Enemy = enemy;
		Entries.Add(oathOfVengeanceEntry);
	}

	public bool HasEntries(UnitEntity ally)
	{
		return !GetEntries(ally).Empty();
	}

	public IEnumerable<OathOfVengeanceEntry> GetEntries(UnitEntity ally)
	{
		return Entries.Where((OathOfVengeanceEntry p) => p.Ally == ally);
	}

	public void RemoveEntries()
	{
		Entries.Clear();
	}

	public void OnEventAboutToTrigger(RuleDealDamage evt)
	{
	}

	public void OnEventDidTrigger(RuleDealDamage evt)
	{
		UnitEntity unitEntity = evt.Initiator as UnitEntity;
		if (evt.Target is UnitEntity unitEntity2 && unitEntity != null && base.Owner.IsAlly(unitEntity2))
		{
			AddEntry(unitEntity2, unitEntity);
		}
	}

	public void HandleTurnBasedModeSwitched(bool isTurnBased)
	{
		if (!isTurnBased)
		{
			RemoveEntries();
		}
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		List<OathOfVengeanceEntry> entries = Entries;
		if (entries != null)
		{
			for (int i = 0; i < entries.Count; i++)
			{
				Hash128 val2 = ClassHasher<OathOfVengeanceEntry>.GetHash128(entries[i]);
				result.Append(ref val2);
			}
		}
		return result;
	}
}
