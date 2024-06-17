using Kingmaker.AreaLogic.Etudes;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Utility.EditorAttributes;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.Designers.EventConditionActionSystem.Conditions;

[TypeId("ee651e222f282454fbd2821c9ea18a72")]
public class AnotherEtudeOfGroupIsPlaying : Condition
{
	[InfoBox(Text = "Внутри этюда будет игнорировать сам этюд")]
	[ValidateNotNull]
	[SerializeField]
	[FormerlySerializedAs("Group")]
	private BlueprintEtudeConflictingGroupReference m_Group;

	public BlueprintEtudeConflictingGroup Group => m_Group?.Get();

	protected override string GetConditionCaption()
	{
		return $"Another etude of the group ({Group}) is playing";
	}

	protected override bool CheckCondition()
	{
		BlueprintEtude conflictingGroupTask = Game.Instance.Player.EtudesSystem.GetConflictingGroupTask(Group);
		if (conflictingGroupTask == null)
		{
			return false;
		}
		if (base.Owner as BlueprintEtude == conflictingGroupTask)
		{
			return false;
		}
		return true;
	}
}
