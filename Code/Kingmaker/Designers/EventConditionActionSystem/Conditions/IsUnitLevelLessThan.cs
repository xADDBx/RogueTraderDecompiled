using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Levelup.Obsolete;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Conditions;

[TypeId("e3853cc4ac653674ea875f78fb93b8b9")]
public class IsUnitLevelLessThan : Condition
{
	[SerializeReference]
	public AbstractUnitEvaluator Unit;

	public int Level;

	public bool CheckExperience;

	protected override string GetConditionCaption()
	{
		return $"Is {Unit} level less than {Level}";
	}

	protected override bool CheckCondition()
	{
		PartUnitProgression progressionOptional = Unit.GetValue().GetProgressionOptional();
		if (progressionOptional == null)
		{
			return false;
		}
		if (!CheckExperience)
		{
			return progressionOptional.CharacterLevel < Level;
		}
		return LevelUpController.GetEffectiveLevel(progressionOptional.Owner) < Level;
	}
}
