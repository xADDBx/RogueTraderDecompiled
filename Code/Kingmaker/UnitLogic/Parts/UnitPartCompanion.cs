using System;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.StateHasher.Hashers;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility.GuidUtility;
using Kingmaker.View.Spawners;
using Newtonsoft.Json;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.Parts;

public class UnitPartCompanion : BaseUnitPart, IHashable
{
	[JsonProperty]
	private EntityRef m_Spawner;

	[JsonProperty]
	public CompanionState State { get; private set; }

	public void SetState(CompanionState s)
	{
		State = s;
		if (s == CompanionState.ExCompanion)
		{
			base.Owner.CombatGroup.Id = Uuid.Instance.CreateString();
		}
		base.Owner.GetOrCreate<UnitPartNonStackBonuses>();
		Game.Instance.Player.InvalidateCharacterLists();
		EventBus.RaiseEvent((IMechanicEntity)base.Owner, (Action<ICompanionStateChanged>)delegate(ICompanionStateChanged h)
		{
			h.HandleCompanionStateChanged();
		}, isCheckRuntime: true);
	}

	public bool IsControllableInParty()
	{
		if (!base.Owner.IsMainCharacter)
		{
			if (State == CompanionState.InParty)
			{
				return !Game.Instance.LoadedAreaState.Settings.CapitalPartyMode;
			}
			return false;
		}
		return true;
	}

	public static BaseUnitEntity FindCompanion(BlueprintUnit bp, CompanionState state)
	{
		return Game.Instance.Player.AllCrossSceneUnits.FirstOrDefault(delegate(BaseUnitEntity u)
		{
			if (u.Blueprint == bp)
			{
				UnitPartCompanion optional = u.GetOptional<UnitPartCompanion>();
				if (optional == null)
				{
					return false;
				}
				return optional.State == state;
			}
			return false;
		});
	}

	public void SetSpawner([CanBeNull] CompanionSpawner spawner)
	{
		CompanionSpawner currentSpawner = GetCurrentSpawner();
		if (!(currentSpawner == spawner))
		{
			currentSpawner?.ReleaseCompanion();
			m_Spawner = new EntityRef(spawner?.UniqueId);
		}
	}

	public CompanionSpawner GetCurrentSpawner()
	{
		return m_Spawner.Entity?.View as CompanionSpawner;
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		CompanionState val2 = State;
		result.Append(ref val2);
		EntityRef obj = m_Spawner;
		Hash128 val3 = EntityRefHasher.GetHash128(ref obj);
		result.Append(ref val3);
		return result;
	}
}
