using System.Collections.Generic;
using System.Linq;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Blueprints.Root;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Mechanics.Entities;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility;
using UnityEngine;
using Warhammer.SpaceCombat.Blueprints;

namespace Warhammer.SpaceCombat.StarshipLogic.Abilities;

[AllowedOn(typeof(BlueprintAbility))]
[TypeId("439c9659d1354f74eb5eaeb2d7fa037a")]
public class AbilityCustomStarshipLaunchCraft : AbilityCustomLogic, IAbilityCasterRestriction
{
	[SerializeField]
	private BlueprintStarshipReference m_CraftBlueprint;

	[SerializeField]
	private float launchTime;

	[SerializeField]
	private int unitsLimit;

	[SerializeField]
	private ActionList ActionsOnUnit;

	public BlueprintStarship CraftBlueprint => m_CraftBlueprint?.Get();

	public override void Cleanup(AbilityExecutionContext context)
	{
	}

	public override IEnumerator<AbilityDeliveryTarget> Deliver(AbilityExecutionContext context, TargetWrapper target)
	{
		if (context.Caster is StarshipEntity starshipEntity)
		{
			Vector3 position = (target.HasEntity ? GetUnitSpawnNode(starshipEntity, target) : target.Point);
			BaseUnitEntity unit = WarhammerContextActionSpawnChildStarship.SpawnStarship(CraftBlueprint, position, null, starshipEntity, actBeforeSummoner: false);
			double startTime = Game.Instance.TimeController.GameTime.TotalMilliseconds;
			while (!((float)(Game.Instance.TimeController.GameTime.TotalMilliseconds - startTime) / 1000f >= launchTime))
			{
				yield return null;
			}
			using (context.GetDataScope(unit.ToITargetWrapper()))
			{
				ActionsOnUnit.Run();
			}
			yield return new AbilityDeliveryTarget(target);
		}
	}

	private Vector3 GetUnitSpawnNode(StarshipEntity starship, TargetWrapper target)
	{
		return AbilityCustomStarshipNPCTorpedoLaunch.GetTorpedoSpawnNode(starship, target, 180, 3)?.Vector3Position ?? target.Point;
	}

	public bool IsCasterRestrictionPassed(MechanicEntity caster)
	{
		StarshipEntity starship = caster as StarshipEntity;
		if (starship == null)
		{
			return false;
		}
		return Game.Instance.State.AllBaseAwakeUnits.Where(delegate(BaseUnitEntity unit)
		{
			if (unit.Blueprint == CraftBlueprint)
			{
				UnitPartSummonedMonster summonedMonsterOption = unit.GetSummonedMonsterOption();
				if (summonedMonsterOption != null)
				{
					return summonedMonsterOption.Summoner == starship;
				}
			}
			return false;
		}).ToList().Count < unitsLimit;
	}

	public string GetAbilityCasterRestrictionUIText(MechanicEntity caster)
	{
		return LocalizedTexts.Instance.Reasons.CantLaunchMoreWings;
	}
}
