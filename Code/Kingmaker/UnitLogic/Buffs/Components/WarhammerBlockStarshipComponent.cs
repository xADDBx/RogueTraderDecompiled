using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules.Starships;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility.DotNetExtensions;
using StateHasher.Core;
using UnityEngine;
using Warhammer.SpaceCombat.StarshipLogic;
using Warhammer.SpaceCombat.StarshipLogic.Weapon;

namespace Kingmaker.UnitLogic.Buffs.Components;

[TypeId("c89848ca6085b104c92d7d4c4509b93d")]
public class WarhammerBlockStarshipComponent : UnitBuffComponentDelegate, IHashable
{
	public enum BlockingStrategyType
	{
		Random,
		FromAttackDirection
	}

	public BlockingStrategyType BlockingStrategy;

	protected override void OnActivate()
	{
		int starshipComponentIndex = GetStarshipComponentIndex();
		if (starshipComponentIndex >= 0)
		{
			base.Owner.GetOrCreate<StarshipPartBlockedComponents>().Block(base.Fact, starshipComponentIndex);
		}
	}

	private int GetStarshipComponentIndex()
	{
		switch (BlockingStrategy)
		{
		case BlockingStrategyType.Random:
		{
			List<WeaponSlot> list = base.Owner.GetHull()?.WeaponSlots;
			if (list != null)
			{
				return base.Owner.Random.Range(0, list.Count);
			}
			return -1;
		}
		case BlockingStrategyType.FromAttackDirection:
			return GetComponentIndexFromAttackDirection();
		default:
			throw new ArgumentOutOfRangeException();
		}
	}

	private int GetComponentIndexFromAttackDirection()
	{
		StarshipHitLocation starshipHitLocation = StarshipHitLocation.Undefined;
		StarshipEntity starshipEntity = base.Owner.Health.LastHandledDamage?.Initiator as StarshipEntity;
		StarshipEntity starshipEntity2 = base.Owner as StarshipEntity;
		if (starshipEntity != null && starshipEntity2 != null)
		{
			starshipHitLocation = Rulebook.Trigger(new RuleStarshipCalculateHitLocation(starshipEntity, starshipEntity2)).ResultHitLocation;
		}
		List<WeaponSlot> list;
		switch (starshipHitLocation)
		{
		case StarshipHitLocation.Undefined:
			list = base.Owner.GetHull()?.WeaponSlots;
			if (list != null)
			{
				return base.Owner.Random.Range(0, list.Count);
			}
			return -1;
		case StarshipHitLocation.Fore:
			list = base.Owner.GetHull()?.WeaponSlots.Where((WeaponSlot x) => x.FiringArc == RestrictedFiringArc.Fore).ToList();
			break;
		case StarshipHitLocation.Port:
			list = base.Owner.GetHull()?.WeaponSlots.Where((WeaponSlot x) => x.FiringArc == RestrictedFiringArc.Port).ToList();
			break;
		case StarshipHitLocation.Starboard:
			list = base.Owner.GetHull()?.WeaponSlots.Where((WeaponSlot x) => x.FiringArc == RestrictedFiringArc.Starboard).ToList();
			break;
		case StarshipHitLocation.Aft:
			list = base.Owner.GetHull()?.WeaponSlots.Where((WeaponSlot x) => x.FiringArc == RestrictedFiringArc.Aft).ToList();
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
		if (list == null)
		{
			return -1;
		}
		WeaponSlot item = list.Random(base.Owner.Random);
		return base.Owner.GetHull()?.WeaponSlots.IndexOf(item) ?? (-1);
	}

	protected override void OnDeactivate()
	{
		base.Owner.GetOrCreate<StarshipPartBlockedComponents>().Unblock(base.Fact);
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
