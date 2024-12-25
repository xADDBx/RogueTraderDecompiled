using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Persistence.Versioning;
using Kingmaker.Utility.Attributes;
using Kingmaker.View.MapObjects;
using Owlcat.QA.Validation;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[ComponentName("Actions/ResetInteractionSkillCheck")]
[AllowMultipleComponents]
[TypeId("92f1b65b092f4e599036b28efc5bbeb0")]
[PlayerUpgraderAllowed(true)]
public class ResetInteractionSkillCheck : GameAction
{
	[ValidateNotNull]
	[SerializeReference]
	public MechanicEntityEvaluator MapObject;

	[SerializeField]
	private bool m_OverrideEnabled;

	[SerializeField]
	[ShowIf("m_OverrideEnabled")]
	private bool m_Enabled = true;

	public override string GetCaption()
	{
		return $"Reset InteractionSkillCheck on MapObject {MapObject} to default";
	}

	protected override void RunAction()
	{
		MechanicEntity value;
		InteractionSkillCheckPart interactionSkillCheckPart = ((!MapObject.TryGetValue(out value)) ? null : value?.GetOptional<InteractionSkillCheckPart>());
		if (interactionSkillCheckPart != null)
		{
			interactionSkillCheckPart.OnResetToDefault();
			if (m_OverrideEnabled)
			{
				value.IsInGame = m_Enabled;
			}
		}
	}
}
