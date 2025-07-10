using System;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.QA;
using Kingmaker.Visual.CharacterSystem;
using Owlcat.Runtime.Core.Physics.PositionBasedDynamics.Scene;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.AreaLogic.Cutscenes.Commands;

[Serializable]
[TypeId("dae204bd0ffada24bb60f340e3649c41")]
public class CommandHideCape : CommandBase
{
	private class Data
	{
		public GameObject CloakGameObject;

		public bool OriginalState;
	}

	[SerializeReference]
	public AbstractUnitEvaluator Unit;

	public bool Hide = true;

	protected override void OnRun(CutscenePlayerData player, bool skipping)
	{
		if (!(Unit.GetValue() is BaseUnitEntity baseUnitEntity))
		{
			string text = $"[IS NOT BASE UNIT ENTITY] Cutscene command {this}, {Unit} is not BaseUnitEntity";
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
		Data commandData = player.GetCommandData<Data>(this);
		Transform transform = character.transform.FindChildRecursive("C_back_weapon_slot_08");
		if (transform != null)
		{
			PBDMeshBody componentInChildren = transform.GetComponentInChildren<PBDMeshBody>();
			if (componentInChildren != null)
			{
				commandData.CloakGameObject = componentInChildren.gameObject;
				commandData.OriginalState = componentInChildren.gameObject.activeSelf;
				PFLog.TechArt.Log("Finded:" + componentInChildren.gameObject.name);
				componentInChildren.GetComponent<MeshRenderer>().enabled = !Hide;
				PFLog.TechArt.Log("Cape " + (Hide ? "hidden" : "shown") + " for unit " + baseUnitEntity.Name);
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

	protected override void OnSetTime(double time, CutscenePlayerData player)
	{
	}

	public override bool IsFinished(CutscenePlayerData player)
	{
		return true;
	}

	protected override void OnStop(CutscenePlayerData player)
	{
		Data commandData = player.GetCommandData<Data>(this);
		if (commandData.CloakGameObject != null)
		{
			commandData.CloakGameObject.SetActive(commandData.OriginalState);
		}
	}

	public override string GetCaption()
	{
		return (Hide ? "Hide" : "Show") + " cape for " + (Unit ? Unit.GetCaptionShort() : "???");
	}

	public override string GetWarning()
	{
		if (!Unit || !Unit.CanEvaluate())
		{
			return "No unit specified";
		}
		return null;
	}
}
