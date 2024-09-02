using System;
using Kingmaker.Blueprints;
using Kingmaker.Controllers.Combat;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.EntitySystem.Persistence.JsonUtility;
using Kingmaker.Items;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.SpaceCombat.StarshipLogic.Parts;
using Kingmaker.UI.Models.UnitSettings;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Alignments;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Groups;
using Kingmaker.UnitLogic.Parts;
using StateHasher.Core;
using UnityEngine;
using Warhammer.SpaceCombat.Blueprints;

namespace Kingmaker.EntitySystem.Entities;

public class StarshipEntity : BaseUnitEntity, PartStarship.IOwner, IEntityPartOwner<PartStarship>, IEntityPartOwner, PartStarshipHull.IOwner, IEntityPartOwner<PartStarshipHull>, PartStarshipPartCrew.IOwner, IEntityPartOwner<PartStarshipPartCrew>, PartStarshipShields.IOwner, IEntityPartOwner<PartStarshipShields>, PartStarshipNavigation.IOwner, IEntityPartOwner<PartStarshipNavigation>, PartStarshipProgression.IOwner, IEntityPartOwner<PartStarshipProgression>, PartStarshipEngine.IOwner, IEntityPartOwner<PartStarshipEngine>, PartStarshipMorale.IOwner, IEntityPartOwner<PartStarshipMorale>, IStarshipEntity, IBaseUnitEntity, IAbstractUnitEntity, IMechanicEntity, IEntity, IDisposable, IHashable
{
	public new BlueprintStarship OriginalBlueprint => (BlueprintStarship)base.OriginalBlueprint;

	public new BlueprintStarship Blueprint => (BlueprintStarship)base.Blueprint;

	public override bool IsDirectlyControllable
	{
		get
		{
			if (Game.Instance.CurrentlyLoadedArea != null && Game.Instance.CurrentlyLoadedArea.IsShipArea)
			{
				return base.IsDirectlyControllable;
			}
			return false;
		}
	}

	public override Type RequiredBlueprintType => typeof(BlueprintStarship);

	public override PartInventory Inventory => GetRequired<PartInventory>();

	public override PartFaction Faction => GetRequired<PartFaction>();

	public override PartUnitProficiency Proficiencies => GetRequired<PartUnitProficiency>();

	public override PartUnitBody Body => GetRequired<PartUnitBody>();

	public override PartAbilityResourceCollection AbilityResources => GetRequired<PartAbilityResourceCollection>();

	public override PartUnitProgression Progression => GetRequired<PartUnitProgression>();

	public override PartUnitState State => GetRequired<PartUnitState>();

	public override PartUnitBrain Brain => GetRequired<PartUnitBrain>();

	public override PartUnitAsks Asks => GetRequired<PartUnitAsks>();

	public override PartUnitViewSettings ViewSettings => GetRequired<PartUnitViewSettings>();

	public override PartUnitDescription Description => GetRequired<PartUnitDescription>();

	public override PartVision Vision => GetRequired<PartVision>();

	public override PartUnitStealth Stealth => GetRequired<PartUnitStealth>();

	public override PartUnitAlignment Alignment => GetRequired<PartUnitAlignment>();

	public override PartUnitCombatState CombatState => GetRequired<PartUnitCombatState>();

	public override PartCombatGroup CombatGroup => GetRequired<PartCombatGroup>();

	public override PartStatsAttributes Attributes => GetRequired<PartStatsAttributes>();

	public override PartStatsSkills Skills => GetRequired<PartStatsSkills>();

	public override PartStatsSaves Saves => GetRequired<PartStatsSaves>();

	public override PartHealth Health => GetRequired<PartHealth>();

	public PartStarship Starship => GetRequired<PartStarship>();

	public PartStarshipHull Hull => GetRequired<PartStarshipHull>();

	public PartStarshipPartCrew Crew => GetRequired<PartStarshipPartCrew>();

	public PartStarshipShields Shields => GetRequired<PartStarshipShields>();

	public PartStarshipNavigation Navigation => GetRequired<PartStarshipNavigation>();

	public PartStarshipProgression StarshipProgression => GetRequired<PartStarshipProgression>();

	public PartStarshipEngine Engine => GetRequired<PartStarshipEngine>();

	public PartStarshipMorale Morale => GetRequired<PartStarshipMorale>();

	public BlueprintItemAugerArray AugerArray => (BlueprintItemAugerArray)(Hull.HullSlots.AugerArray.MaybeItem?.Blueprint);

	public BlueprintItemArmorPlating ArmorPlatings => (BlueprintItemArmorPlating)(Hull.HullSlots.ArmorPlating.MaybeItem?.Blueprint);

	public int TeamUnitsAlive => Blueprint.GetComponent<StarshipTeamController>()?.UnitsAlive(this) ?? 1;

	public bool IsSoftUnit => Blueprint.IsSoftUnit;

	public StarshipEntity(string uniqueId, bool isInGame, BlueprintStarship blueprint)
		: base(uniqueId, isInGame, blueprint)
	{
	}

	protected StarshipEntity(JsonConstructorMark _)
		: base(_)
	{
	}

	protected override void OnCreateParts()
	{
		base.OnCreateParts();
		GetOrCreate<PartUnitViewSettings>();
		GetOrCreate<PartUnitCommands>();
		GetOrCreate<PartUnitCombatState>();
		GetOrCreate<PartFaction>();
		GetOrCreate<PartCombatGroup>();
		GetOrCreate<PartVision>();
		GetOrCreate<PartUnitStealth>();
		GetOrCreate<PartStatsAttributes>();
		GetOrCreate<PartStatsSkills>();
		GetOrCreate<PartStatsSaves>();
		GetOrCreate<PartUnitProgression>();
		GetOrCreate<PartUnitState>();
		GetOrCreate<PartHealth>();
		GetOrCreate<PartLifeState>();
		GetOrCreate<PartMovable>();
		GetOrCreate<PartUnitProficiency>();
		GetOrCreate<PartAbilityResourceCollection>();
		GetOrCreate<PartInventory>();
		GetOrCreate<PartUnitBody>();
		GetOrCreate<PartUnitBrain>();
		GetOrCreate<PartUnitAsks>();
		GetOrCreate<PartUnitDescription>();
		GetOrCreate<PartStarship>();
		GetOrCreate<PartStarshipHull>();
		GetOrCreate<PartStarshipPartCrew>();
		GetOrCreate<PartStarshipShields>();
		GetOrCreate<PartStarshipNavigation>();
		GetOrCreate<PartStarshipProgression>();
		GetOrCreate<PartStarshipEngine>();
		GetOrCreate<PartStarshipMorale>();
		GetOrCreate<PartUnitUISettings>();
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
