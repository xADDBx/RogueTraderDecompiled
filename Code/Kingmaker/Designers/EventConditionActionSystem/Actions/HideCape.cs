using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.QA;
using Kingmaker.Visual.CharacterSystem;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Physics.PositionBasedDynamics.Scene;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[ComponentName("Actions/HideCape")]
[TypeId("6fc6344b19b30e34bb65773bddd7980a")]
public class HideCape : GameAction
{
	[ValidateNotNull]
	[SerializeReference]
	public AbstractUnitEvaluator Target;

	public bool Hide = true;

	public override string GetDescription()
	{
		return string.Format("{0} плащ у юнита {1}", Hide ? "Скрывает" : "Показывает", Target);
	}

	public override string GetCaption()
	{
		return (Hide ? "Hide" : "Show") + " cape for " + (Target ? Target.GetCaption() : "???");
	}

	protected override void RunAction()
	{
		if (!(Target.GetValue() is BaseUnitEntity baseUnitEntity))
		{
			string text = $"[IS NOT BASE UNIT ENTITY] Game action {this}, {Target} is not BaseUnitEntity";
			if (!QAModeExceptionReporter.MaybeShowError(text))
			{
				PFLog.TechArt.Error(text);
			}
			return;
		}
		Character character = baseUnitEntity.View?.CharacterAvatar;
		if (character == null)
		{
			PFLog.TechArt.Error("Character avatar not found for unit " + baseUnitEntity.Name);
			return;
		}
		Transform transform = character.transform.FindChildRecursive("C_back_weapon_slot_08");
		if (transform != null)
		{
			PBDMeshBody componentInChildren = transform.GetComponentInChildren<PBDMeshBody>();
			if (componentInChildren != null)
			{
				MeshRenderer component = componentInChildren.GetComponent<MeshRenderer>();
				if (component != null)
				{
					component.enabled = !Hide;
					PFLog.TechArt.Log("Cape " + (Hide ? "hidden" : "shown") + " for unit " + baseUnitEntity.Name);
				}
				else
				{
					PFLog.TechArt.Log("No MeshRenderer found on PBDMeshBody for unit " + baseUnitEntity.Name);
				}
			}
			else
			{
				PFLog.TechArt.Log("No PBDMeshBody found in cape bone for unit " + baseUnitEntity.Name);
			}
		}
		else
		{
			PFLog.TechArt.Log("No cape bone (C_back_weapon_slot_08) found for unit " + baseUnitEntity.Name);
		}
	}
}
