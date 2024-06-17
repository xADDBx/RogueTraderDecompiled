using System;
using System.Collections.Generic;
using System.Linq;
using Core.Cheats;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.GameCommands;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Levelup;
using Kingmaker.UnitLogic.Levelup.Obsolete.Blueprints;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Progression.Paths;
using Kingmaker.Utility.DotNetExtensions;
using StateHasher.Core;
using UnityEngine;
using Warhammer.SpaceCombat;
using Warhammer.SpaceCombat.Blueprints.Progression;

namespace Kingmaker.SpaceCombat.StarshipLogic.Parts;

public class PartStarshipProgression : StarshipPart, IHashable
{
	public interface IOwner : IEntityPartOwner<PartStarshipProgression>, IEntityPartOwner
	{
		PartStarshipProgression StarshipProgression { get; }
	}

	public PartUnitProgression Progression => base.Owner.Progression;

	public static BlueprintCareerPath CareerPath => Game.Instance.BlueprintRoot.Progression.ShipPath;

	public BlueprintStatProgression ExperienceTable => Game.Instance.BlueprintRoot.Progression.StarshipXPTable;

	public BlueprintShipComponentsUnlockTable ComponentsUnlockTable => Game.Instance.BlueprintRoot.Progression.StarshipComponentsUnlockTable;

	public bool CanLevelUp
	{
		get
		{
			if (Progression.CharacterLevel > 0)
			{
				return Progression.Experience >= ExperienceTable.GetBonus(Progression.CharacterLevel + 1);
			}
			return false;
		}
	}

	public int ExpToNextLevel => Mathf.Max(ExperienceTable.GetBonus(Progression.ExperienceLevel + 1) - Progression.Experience, 0);

	public void AddStarshipLevel(LevelUpManager manager)
	{
		Game.Instance.GameCommandQueue.CommitLvlUp(manager);
		ComponentsUnlockTable?.UpdateShipEquipment(Progression.CharacterLevel);
		EventBus.RaiseEvent((IStarshipEntity)base.Owner, (Action<IStarshipLevelUpHandler>)delegate(IStarshipLevelUpHandler h)
		{
			h.HandleStarshipLevelUp(Progression.CharacterLevel, manager);
		}, isCheckRuntime: true);
	}

	public IEnumerable<Ability> GetUnlockedAbilities(IEnumerable<BlueprintAbility> blueprints)
	{
		List<Ability> list = new List<Ability>();
		foreach (Ability ability in base.Owner.Abilities)
		{
			if (blueprints.Contains(ability.Blueprint))
			{
				list.Add(ability);
			}
		}
		return list;
	}

	public IEnumerable<Ability> GetUnlockedAbilities()
	{
		List<Ability> list = new List<Ability>();
		foreach (Ability ability in base.Owner.Abilities)
		{
			list.Add(ability);
		}
		return list;
	}

	[Cheat(Name = "try_to_fix_ship_progression")]
	public static string TryToFixShipProgression()
	{
		using PooledStringBuilder pooledStringBuilder = ContextData<PooledStringBuilder>.Request();
		foreach (BaseUnitEntity item in Game.Instance.Player.AllStarships.Where((BaseUnitEntity ship) => ship.GetOptional<PartStarshipProgression>() == null))
		{
			item.GetOrCreate<PartStarshipProgression>();
			pooledStringBuilder.Builder.AppendFormat("Fixed {0}", item);
		}
		return (pooledStringBuilder.Builder.Length == 0) ? "Nothing to fix" : pooledStringBuilder.Builder.ToString();
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
