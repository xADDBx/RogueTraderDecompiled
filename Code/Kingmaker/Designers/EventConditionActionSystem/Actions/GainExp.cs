using JetBrains.Annotations;
using Kingmaker.Blueprints.Classes.Experience;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Persistence.Versioning;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[PlayerUpgraderAllowed(false)]
[TypeId("db6095c1906d85748b0a8506de7c9dd7")]
public class GainExp : GameAction
{
	public EncounterType Encounter;

	public int CR;

	public float Modifier = 1f;

	[CanBeNull]
	[SerializeReference]
	public IntEvaluator Count;

	[DrawExperienceFromCR]
	public bool Dummy;

	public override string GetDescription()
	{
		return "Выдает игроку опыт в соответствии с указанным типом энкаунтера";
	}

	public override string GetCaption()
	{
		return $"Gain XP (CR={CR}, {Encounter})";
	}

	protected override void RunAction()
	{
		GameHelper.GainExperience(ExperienceHelper.GetXp(Encounter, CR, Modifier, Count));
	}
}
