using System.Collections.Generic;
using Kingmaker.Controllers;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.StateHasher.Hashers;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility;
using Kingmaker.Utility.DotNetExtensions;
using Newtonsoft.Json;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.UnitLogic.Parts;

public class UnitPartSpringAttack : BaseUnitPart, ITurnBasedModeHandler, ISubscriber, IUnitJumpHandler, ITurnStartHandler, ISubscriber<IMechanicEntity>, ITurnEndHandler, IInterruptTurnStartHandler, IInterruptTurnEndHandler, IHashable
{
	[JsonProperty]
	public int LastIndex;

	[JsonProperty]
	public EntityRef<AreaEffectEntity> TurnStartMark;

	[JsonProperty]
	public List<SpringAttackEntry> Entries = new List<SpringAttackEntry>();

	[JsonProperty]
	public BlueprintAbility DeathWaltzBlueprint { get; set; }

	[JsonProperty]
	public BlueprintAbility DeathWaltzUltimateBlueprint { get; set; }

	[JsonProperty]
	public UnitFact SpringAttackFeature { get; set; }

	[JsonProperty]
	public Vector3 TurnStartPosition { get; set; }

	[JsonProperty]
	public BlueprintAbilityAreaEffect AreaMark { get; set; }

	public void AddEntry(Vector3 oldPosition, Vector3 newPosition)
	{
		SpringAttackEntry springAttackEntry = new SpringAttackEntry();
		springAttackEntry.OldPosition = oldPosition;
		springAttackEntry.NewPosition = newPosition;
		springAttackEntry.AreaMark = AreaEffectsController.Spawn(SpringAttackFeature.MaybeContext, AreaMark, oldPosition, 1.Rounds().Seconds);
		LastIndex++;
		springAttackEntry.Index = LastIndex;
		if (Entries == null)
		{
			Entries = new List<SpringAttackEntry>();
		}
		Entries.Add(springAttackEntry);
	}

	public bool HasEntries()
	{
		return !Entries.Empty();
	}

	public void RemoveEntries()
	{
		foreach (SpringAttackEntry entry in Entries)
		{
			if (entry.AreaMark.Entity != null)
			{
				entry.AreaMark.Entity.ForceEnded = true;
			}
		}
		Entries.Clear();
		LastIndex = 0;
	}

	public void HandleTurnBasedModeSwitched(bool isTurnBased)
	{
		if (!isTurnBased)
		{
			RemoveEntries();
			if (TurnStartMark.Entity != null)
			{
				TurnStartMark.Entity.ForceEnded = true;
			}
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
		if (isTurnBased && EventInvokerExtensions.MechanicEntity == base.Owner && base.Owner != null)
		{
			RemoveEntries();
			TurnStartPosition = base.Owner.Position;
			TurnStartMark = AreaEffectsController.Spawn(SpringAttackFeature.MaybeContext, AreaMark, TurnStartPosition, 1.Rounds().Seconds);
		}
	}

	public void HandleUnitStartInterruptTurn(InterruptionData interruptionData)
	{
		if (EventInvokerExtensions.MechanicEntity == base.Owner && base.Owner != null)
		{
			RemoveEntries();
			TurnStartPosition = base.Owner.Position;
			TurnStartMark = AreaEffectsController.Spawn(SpringAttackFeature.MaybeContext, AreaMark, TurnStartPosition, 1.Rounds().Seconds);
		}
	}

	public void HandleUnitEndTurn(bool isTurnBased)
	{
		if (EventInvokerExtensions.MechanicEntity == base.Owner && base.Owner != null)
		{
			RemoveEntries();
			if (TurnStartMark.Entity != null)
			{
				TurnStartMark.Entity.ForceEnded = true;
			}
		}
	}

	public void HandleUnitEndInterruptTurn()
	{
		if (EventInvokerExtensions.MechanicEntity == base.Owner && base.Owner != null)
		{
			RemoveEntries();
			if (TurnStartMark.Entity != null)
			{
				TurnStartMark.Entity.ForceEnded = true;
			}
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
		Hash128 val4 = ClassHasher<UnitFact>.GetHash128(SpringAttackFeature);
		result.Append(ref val4);
		Vector3 val5 = TurnStartPosition;
		result.Append(ref val5);
		result.Append(ref LastIndex);
		Hash128 val6 = Kingmaker.StateHasher.Hashers.SimpleBlueprintHasher.GetHash128(AreaMark);
		result.Append(ref val6);
		EntityRef<AreaEffectEntity> obj = TurnStartMark;
		Hash128 val7 = StructHasher<EntityRef<AreaEffectEntity>>.GetHash128(ref obj);
		result.Append(ref val7);
		List<SpringAttackEntry> entries = Entries;
		if (entries != null)
		{
			for (int i = 0; i < entries.Count; i++)
			{
				Hash128 val8 = ClassHasher<SpringAttackEntry>.GetHash128(entries[i]);
				result.Append(ref val8);
			}
		}
		return result;
	}
}
