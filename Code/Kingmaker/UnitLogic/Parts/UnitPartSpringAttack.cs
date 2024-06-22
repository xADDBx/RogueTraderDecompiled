using System.Collections.Generic;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.StateHasher.Hashers;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility.DotNetExtensions;
using Newtonsoft.Json;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.UnitLogic.Parts;

public class UnitPartSpringAttack : BaseUnitPart, ITurnBasedModeHandler, ISubscriber, IUnitJumpHandler, ITurnStartHandler, ISubscriber<IMechanicEntity>, IInterruptTurnStartHandler, IHashable
{
	[JsonProperty]
	public int LastIndex;

	[JsonProperty]
	public List<SpringAttackEntry> Entries = new List<SpringAttackEntry>();

	[JsonProperty]
	public BlueprintAbility DeathWaltzBlueprint { get; set; }

	[JsonProperty]
	public BlueprintAbility DeathWaltzUltimateBlueprint { get; set; }

	[JsonProperty]
	public Vector3 TurnStartPosition { get; set; }

	public void AddEntry(Vector3 oldPosition, Vector3 newPosition)
	{
		SpringAttackEntry springAttackEntry = new SpringAttackEntry();
		springAttackEntry.OldPosition = oldPosition;
		springAttackEntry.NewPosition = newPosition;
		LastIndex++;
		springAttackEntry.Index = LastIndex;
		Entries.Add(springAttackEntry);
	}

	public bool HasEntries()
	{
		return !Entries.Empty();
	}

	public void RemoveEntries()
	{
		Entries.Clear();
		LastIndex = 0;
	}

	public void HandleTurnBasedModeSwitched(bool isTurnBased)
	{
		if (!isTurnBased)
		{
			RemoveEntries();
		}
	}

	public void HandleUnitJump(int distanceInCells, Vector3 startPoint, Vector3 targetPoint, MechanicEntity caster, BlueprintAbility ability)
	{
		if (caster == base.Owner && (ability == DeathWaltzBlueprint || ability == DeathWaltzUltimateBlueprint))
		{
			AddEntry(startPoint, targetPoint);
		}
	}

	public void HandleUnitStartTurn(bool isTurnBased)
	{
		if (EventInvokerExtensions.MechanicEntity == base.Owner && base.Owner != null)
		{
			RemoveEntries();
			TurnStartPosition = base.Owner.Position;
		}
	}

	public void HandleUnitStartInterruptTurn(InterruptionData interruptionData)
	{
		if (EventInvokerExtensions.MechanicEntity == base.Owner && base.Owner != null)
		{
			RemoveEntries();
			TurnStartPosition = base.Owner.Position;
		}
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		Hash128 val2 = Kingmaker.StateHasher.Hashers.SimpleBlueprintHasher.GetHash128(DeathWaltzBlueprint);
		result.Append(ref val2);
		Hash128 val3 = Kingmaker.StateHasher.Hashers.SimpleBlueprintHasher.GetHash128(DeathWaltzUltimateBlueprint);
		result.Append(ref val3);
		Vector3 val4 = TurnStartPosition;
		result.Append(ref val4);
		result.Append(ref LastIndex);
		List<SpringAttackEntry> entries = Entries;
		if (entries != null)
		{
			for (int i = 0; i < entries.Count; i++)
			{
				Hash128 val5 = ClassHasher<SpringAttackEntry>.GetHash128(entries[i]);
				result.Append(ref val5);
			}
		}
		return result;
	}
}
