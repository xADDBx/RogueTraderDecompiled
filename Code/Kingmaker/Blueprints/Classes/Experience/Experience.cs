using JetBrains.Annotations;
using Kingmaker.Blueprints.Area;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Blueprints.Quests;
using Kingmaker.Blueprints.Quests.Logic;
using Kingmaker.Blueprints.Root;
using Kingmaker.Designers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Parts;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Blueprints.Classes.Experience;

[AllowedOn(typeof(BlueprintUnit))]
[AllowedOn(typeof(BlueprintQuest))]
[AllowedOn(typeof(BlueprintTrap))]
[AllowedOn(typeof(BlueprintQuestObjective))]
[TypeId("011e862b513000f4bae31886f3489ace")]
public class Experience : EntityFactComponentDelegate, IQuestLogic, IQuestObjectiveLogic, IHashable
{
	public EncounterType Encounter;

	public int CR;

	public float Modifier = 1f;

	[CanBeNull]
	[SerializeReference]
	public IntEvaluator Count;

	[DrawExperienceFromCR]
	public bool Dummy;

	public static void Apply(Experience e, [CanBeNull] BaseUnitEntity targetUnit = null)
	{
		e.Apply(targetUnit);
	}

	public static void ApplyForSkillCheck(Experience e, [NotNull] BaseUnitEntity actor)
	{
		e.ApplyForSkillCheck(actor);
	}

	private void Apply([CanBeNull] BaseUnitEntity targetUnit = null)
	{
		float num = ((targetUnit?.GetSummonedMonsterOption() != null) ? Kingmaker.Blueprints.Root.Root.Common.Progression.SummonedUnitExperienceFactor : 1f);
		GameHelper.GainExperience(ExperienceHelper.GetXp(Encounter, CR, Modifier * num, Count), targetUnit);
	}

	private void ApplyForSkillCheck([NotNull] BaseUnitEntity actor)
	{
		GameHelper.GainExperienceForSkillCheck(ExperienceHelper.GetXp(Encounter, CR, Modifier, Count), actor);
	}

	void IQuestLogic.OnStarted()
	{
	}

	void IQuestObjectiveLogic.OnCompleted()
	{
		Apply();
	}

	void IQuestObjectiveLogic.OnFailed()
	{
	}

	void IQuestObjectiveLogic.OnBecameVisible()
	{
	}

	void IQuestObjectiveLogic.OnStarted()
	{
	}

	void IQuestLogic.OnCompleted()
	{
		Apply();
	}

	void IQuestLogic.OnFailed()
	{
	}

	public string GetDescription()
	{
		return $"XP (CR={CR}, {Encounter})";
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
