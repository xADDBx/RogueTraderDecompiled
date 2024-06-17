using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Mechanics.Entities;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic.Mechanics.Damage;
using Kingmaker.Utility.Attributes;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[ComponentName("Actions/DamageParty")]
[AllowMultipleComponents]
[TypeId("2379c20c25ff18a49ac676292ec98e7a")]
public class DamageParty : GameAction
{
	[HideIf("NoSource")]
	[SerializeReference]
	public AbstractUnitEvaluator DamageSource;

	public bool NoSource;

	public DamageDescription Damage;

	public bool DisableBattleLog;

	public override void RunAction()
	{
		AbstractUnitEntity abstractUnitEntity = (NoSource ? null : DamageSource.GetValue());
		foreach (BaseUnitEntity partyAndPet in Game.Instance.Player.PartyAndPets)
		{
			Rulebook.Trigger(new RuleDealDamage(abstractUnitEntity ?? partyAndPet, partyAndPet, Damage.CreateDamage())
			{
				DisableGameLog = true
			});
		}
	}

	public override string GetCaption()
	{
		return "Damage Party";
	}
}
