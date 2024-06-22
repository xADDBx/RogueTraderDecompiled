using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Pathfinding;
using Kingmaker.SpaceCombat.StarshipLogic.Parts;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.Utility;
using Kingmaker.Utility.Attributes;
using Kingmaker.Utility.Random;
using UnityEngine;

namespace Kingmaker.UnitLogic.Abilities.Components.CasterCheckers;

[AllowedOn(typeof(BlueprintAbility))]
[TypeId("7d521ff336efba541a0cfa902d15a623")]
public class StarshipAIAbilityRestrictionLogic : BlueprintComponent, IAbilityCasterRestriction, IAbilityTargetRestriction
{
	public enum LogicMode
	{
		RestartShields,
		StareToAbyss,
		PlasmaDrivesKamikaze,
		DisableDoubleReinforce,
		PlayerStarshipOnly
	}

	public LogicMode logicMode;

	[ShowIf("IsKamikaze")]
	public int lowHP_Percent;

	[ShowIf("IsKamikaze")]
	public int highHP_Percent;

	[ShowIf("IsKamikaze")]
	public int lowHP_Chances;

	[ShowIf("IsKamikaze")]
	public int highHP_Chances;

	[ShowIf("IsReinforceSameSide")]
	public StarshipSectorShieldsType shieldsSector;

	public bool IsKamikaze => logicMode == LogicMode.PlasmaDrivesKamikaze;

	public bool IsReinforceSameSide => logicMode == LogicMode.DisableDoubleReinforce;

	public bool IsCasterRestrictionPassed(MechanicEntity caster)
	{
		if (!(caster is StarshipEntity starship))
		{
			return false;
		}
		return logicMode switch
		{
			LogicMode.RestartShields => RestartShieldsLogic(starship), 
			LogicMode.PlasmaDrivesKamikaze => PlasmaDrivesKamikazeLogic(starship), 
			LogicMode.DisableDoubleReinforce => CheckReinforceSameSide(starship), 
			_ => true, 
		};
	}

	public bool IsTargetRestrictionPassed(AbilityData ability, TargetWrapper target, Vector3 casterPosition)
	{
		if (!(ability.Caster is StarshipEntity starship))
		{
			return false;
		}
		return logicMode switch
		{
			LogicMode.StareToAbyss => StareToAbyssLogic(starship, casterPosition, target), 
			LogicMode.PlayerStarshipOnly => target.Entity == Game.Instance.Player.PlayerShip, 
			_ => true, 
		};
	}

	private bool PlasmaDrivesKamikazeLogic(StarshipEntity starship)
	{
		int num = starship.Health.HitPointsLeft * 100 / starship.Health.MaxHitPoints;
		int num2 = 0;
		if (num <= lowHP_Percent)
		{
			num2 = lowHP_Chances;
		}
		else if (num <= highHP_Percent)
		{
			num2 = highHP_Chances;
		}
		if (num2 > 0 && PFStatefulRandom.SpaceCombat.Range(0, 100) < num2)
		{
			return true;
		}
		return false;
	}

	private bool CheckReinforceSameSide(StarshipEntity starship)
	{
		PartStarshipShields shields = starship.Shields;
		if (shields == null)
		{
			return false;
		}
		return !shields.GetShields(shieldsSector).Reinforced;
	}

	private bool RestartShieldsLogic(StarshipEntity starship)
	{
		PartStarshipShields shields = starship.Shields;
		if (shields == null)
		{
			return false;
		}
		bool num = shields.ShieldsSum * 100 / shields.ShieldsMaxSum <= 50;
		StarshipSectorShields weakestSector = shields.WeakestSector;
		bool flag = weakestSector.Current * 100 / weakestSector.Max <= 20;
		return num || flag;
	}

	private bool StareToAbyssLogic(StarshipEntity starship, Vector3 casterPosition, TargetWrapper target)
	{
		if ((starship.Position - casterPosition).magnitude > GraphParamsMechanicsCache.GridCellSize / 2f)
		{
			return false;
		}
		StarshipEntity obj = target.Entity as StarshipEntity;
		if (obj == null || obj.Navigation.IsSoftUnit)
		{
			return false;
		}
		return true;
	}

	public string GetAbilityCasterRestrictionUIText(MechanicEntity caster)
	{
		return LocalizedTexts.Instance.Reasons.UnavailableGeneric;
	}

	public string GetAbilityTargetRestrictionUIText(AbilityData ability, TargetWrapper target, Vector3 casterPosition)
	{
		return LocalizedTexts.Instance.Reasons.UnavailableGeneric;
	}
}
