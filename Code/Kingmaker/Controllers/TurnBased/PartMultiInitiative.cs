using System.Collections.Generic;
using System.Linq;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Mechanics;
using Newtonsoft.Json;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.Controllers.TurnBased;

public class PartMultiInitiative : MechanicEntityPart, IUnitDeathHandler, ISubscriber, ITurnEndHandler, ISubscriber<IMechanicEntity>, IHashable
{
	public int AdditionalTurnsCount;

	public bool ByEnemiesCount;

	[JsonProperty]
	public MechanicEntity CorrespondingEnemy;

	[JsonProperty]
	public List<InitiativePlaceholderEntity> Placeholders;

	[JsonProperty]
	public bool SwitchCorrespondingEnemyOnTurnEnd;

	public void Setup(int additionalTurns)
	{
		AdditionalTurnsCount = additionalTurns;
		Placeholders = new List<InitiativePlaceholderEntity>();
		ByEnemiesCount = false;
	}

	public void SetupByEnemiesCount()
	{
		if (Placeholders == null)
		{
			Placeholders = new List<InitiativePlaceholderEntity>();
		}
		ByEnemiesCount = true;
	}

	public IEnumerable<InitiativePlaceholderEntity> EnsurePlaceholders()
	{
		return from i in Enumerable.Range(0, AdditionalTurnsCount)
			select InitiativePlaceholderEntity.Ensure(base.Owner, i);
	}

	public void HandleUnitDeath(AbstractUnitEntity unitEntity)
	{
		if (unitEntity == CorrespondingEnemy)
		{
			SwitchCorrespondingEnemyOnTurnEnd = true;
		}
	}

	public void HandleUnitEndTurn(bool isTurnBased)
	{
		if (!SwitchCorrespondingEnemyOnTurnEnd)
		{
			return;
		}
		SwitchCorrespondingEnemyOnTurnEnd = false;
		if (Placeholders.Any((InitiativePlaceholderEntity p) => !p.CorrespondingEnemy.IsDeadOrUnconscious))
		{
			InitiativePlaceholderEntity initiativePlaceholderEntity = Placeholders.First((InitiativePlaceholderEntity p) => !p.CorrespondingEnemy.IsDeadOrUnconscious);
			base.Owner.Initiative.ChangePlaces(initiativePlaceholderEntity.Initiative);
			CorrespondingEnemy = initiativePlaceholderEntity.CorrespondingEnemy;
			Placeholders.Remove(initiativePlaceholderEntity);
			initiativePlaceholderEntity.Dispose();
		}
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		Hash128 val2 = ClassHasher<MechanicEntity>.GetHash128(CorrespondingEnemy);
		result.Append(ref val2);
		List<InitiativePlaceholderEntity> placeholders = Placeholders;
		if (placeholders != null)
		{
			for (int i = 0; i < placeholders.Count; i++)
			{
				Hash128 val3 = ClassHasher<InitiativePlaceholderEntity>.GetHash128(placeholders[i]);
				result.Append(ref val3);
			}
		}
		result.Append(ref SwitchCorrespondingEnemyOnTurnEnd);
		return result;
	}
}
