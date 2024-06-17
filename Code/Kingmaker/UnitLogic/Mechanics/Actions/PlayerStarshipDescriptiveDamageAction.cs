using System.Linq;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Controllers.Units;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic.Mechanics.Damage;
using UnityEngine;

namespace Kingmaker.UnitLogic.Mechanics.Actions;

[ComponentName("Actions/PlayerStarshipDescriptiveDamageAction")]
[AllowMultipleComponents]
[TypeId("3ed9f542a3b83084ab7fc16a9d94d9c2")]
public class PlayerStarshipDescriptiveDamageAction : GameAction
{
	private enum DamageValue
	{
		Tiny,
		VeryLow,
		Low,
		Average,
		High
	}

	[SerializeField]
	private DamageValue damageValue;

	[SerializeField]
	private bool IsWarp;

	public override void RunAction()
	{
		StarshipEntity playerShip = Game.Instance.Player.PlayerShip;
		int num = damageValue switch
		{
			DamageValue.Tiny => 4, 
			DamageValue.VeryLow => 7, 
			DamageValue.Low => 22, 
			DamageValue.Average => 39, 
			DamageValue.High => 55, 
			_ => 0, 
		};
		int num2 = playerShip.Health.MaxHitPoints * num / 100 / 3;
		int num3 = playerShip.Health.HitPointsLeft * num * 2 / 100 / 3;
		int num4 = num2 + num3;
		if (IsWarp)
		{
			int num5 = (from t in playerShip.Facts.GetComponents<StarshipScenarioDamageBonus>()
				select t.WarpDamagePct_Bonus).DefaultIfEmpty(0).Sum();
			num4 = (num4 * (100 + num5) + 50) / 100;
		}
		DamageDescription damageDescription = new DamageDescription
		{
			Dice = DiceFormula.Zero,
			Bonus = num4,
			TypeDescription = new DamageTypeDescription
			{
				Type = DamageType.Direct
			}
		};
		Rulebook.Trigger(new RuleDealDamage(playerShip, playerShip, damageDescription.CreateDamage())
		{
			DisableGameLog = false,
			DisableFxAndSound = false
		});
		UnitLifeController.ForceTickOnUnit(playerShip);
	}

	public override string GetCaption()
	{
		return $"Do \"{damageValue}\" damage to player starship";
	}
}
