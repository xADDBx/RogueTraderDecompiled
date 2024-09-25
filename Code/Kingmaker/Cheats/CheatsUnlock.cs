using System;
using System.Collections.Generic;
using System.Linq;
using Core.Cheats;
using Kingmaker.AreaLogic.Etudes;
using Kingmaker.AreaLogic.QuestSystem;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.Quests;
using Kingmaker.Designers;
using Kingmaker.Designers.EventConditionActionSystem.Actions;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.Common;
using Kingmaker.UI.InputSystems;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility.BuildModeUtils;
using Kingmaker.Utility.UnityExtensions;
using Owlcat.Runtime.Visual.FogOfWar;
using UnityEngine;

namespace Kingmaker.Cheats;

public class CheatsUnlock
{
	private static Quest s_Quest;

	private static QuestObjective s_QuestObjective;

	public static void RegisterCommands(KeyboardAccess keyboard)
	{
		if (BuildModeUtility.CheatsEnabled)
		{
			keyboard.Bind("DebugQuestNext", DebugQuestNext);
			keyboard.Bind("DebugQuestPrevious", DebugQuestPrevious);
			keyboard.Bind("DebugObjectiveNext", DebugObjectiveNext);
			keyboard.Bind("DebugObjectivePrevious", DebugObjectivePrevious);
			keyboard.Bind("DebugObjectiveComplete", DebugObjectiveComplete);
			SmartConsole.RegisterCommand("lock_flag", LockFlag);
			SmartConsole.RegisterCommand("lock_flag_all", LockAll);
			SmartConsole.RegisterCommand("list_flags", ListFlags);
			SmartConsole.RegisterCommand("objective_give", GiveObjective);
			SmartConsole.RegisterCommand("objective_fail", FailObjective);
			SmartConsole.RegisterCommand("unlock_all_stories", UnlockCompanionAllStories);
			SmartConsole.RegisterCommand("create_all_items", CreateAllItems);
			SmartConsole.RegisterCommand("fill_preset", "string: fill_preset <bool: select dialog; default=false> <bool: cuesseen; default=false>", FillPreset);
			SmartConsole.RegisterCommand("toggle_fow", "Switch Fog Of War Enabled flag", ToggleFow);
			SmartConsole.RegisterCommand("complete_quests", CompleteQuests);
			SmartConsole.RegisterCommand("recruit_companion", RecruitCompanion);
			SmartConsole.RegisterCommand("list_all_quests", ListAllQuest);
			SmartConsole.RegisterCommand("add_feature", CheatAddFeature);
			SmartConsole.RegisterCommand("etude_start", StartEtude);
			SmartConsole.RegisterCommand("etude_complete", CompleteEtude);
			SmartConsole.RegisterCommand("etude_check", CheckEtude);
			SmartConsole.RegisterCommand("etude_update", UpdateEtudes);
			SmartConsole.RegisterCommand("etude_uncomplete", UncompleteEtude);
		}
	}

	private static void UncompleteEtude(string parameters)
	{
		string paramString = Utilities.GetParamString(parameters, 1, "Can't parse etude name from given parameters");
		BlueprintEtude blueprint = Utilities.GetBlueprint<BlueprintEtude>(paramString);
		if (blueprint == null)
		{
			PFLog.SmartConsole.Log("Can't find Etude with name: " + paramString);
		}
		else
		{
			Game.Instance.Player.EtudesSystem.UnstartEtude(blueprint);
		}
	}

	private static void UpdateEtudes(string parameters)
	{
		Game.Instance.Player.EtudesSystem.MarkConditionsDirty();
	}

	private static void SetEtudeActive(string parameters)
	{
		string paramString = Utilities.GetParamString(parameters, 1, "Can't parse etude name from given parameters");
		bool? paramBool = Utilities.GetParamBool(parameters, 2, "Can't parse etude active state from given parameters");
		if (!string.IsNullOrEmpty(paramString) && paramBool.HasValue)
		{
			Utilities.GetBlueprint<BlueprintEtude>(paramString);
		}
	}

	private static void StartEtude(string parameters)
	{
		string paramString = Utilities.GetParamString(parameters, 1, "Can't parse etude name from given parameters");
		BlueprintEtude blueprint = Utilities.GetBlueprint<BlueprintEtude>(paramString);
		if (blueprint == null)
		{
			PFLog.SmartConsole.Log("Can't find Etude with name: " + paramString);
		}
		else
		{
			Game.Instance.Player.EtudesSystem.StartEtude(blueprint, "cheats");
		}
	}

	private static void CompleteEtude(string parameters)
	{
		string paramString = Utilities.GetParamString(parameters, 1, "Can't parse etude name from given parameters");
		BlueprintEtude blueprint = Utilities.GetBlueprint<BlueprintEtude>(paramString);
		if (blueprint == null)
		{
			PFLog.SmartConsole.Log("Can't find Etude with name: " + paramString);
		}
		else
		{
			Game.Instance.Player.EtudesSystem.MarkEtudeCompleted(blueprint, "cheat");
		}
	}

	private static void CheckEtude(string parameters)
	{
		string paramString = Utilities.GetParamString(parameters, 1, "Can't parse etude name from given parameters");
		BlueprintEtude blueprint = Utilities.GetBlueprint<BlueprintEtude>(paramString);
		if (blueprint == null)
		{
			PFLog.SmartConsole.Log("Can't find Etude with name: " + paramString);
		}
		else
		{
			PFLog.SmartConsole.Log(Game.Instance.Player.EtudesSystem.GetDebugInfo(blueprint));
		}
	}

	private static void UnlockCompanionAllStories(string parameters)
	{
		foreach (BlueprintCompanionStory scriptableObject in Utilities.GetScriptableObjects<BlueprintCompanionStory>())
		{
			UnlockCompanionStory unlockCompanionStory = new UnlockCompanionStory();
			unlockCompanionStory.Story = scriptableObject;
			unlockCompanionStory.Run();
		}
	}

	public static void DebugObjectiveComplete()
	{
		if (s_QuestObjective == null)
		{
			UIUtility.SendWarning("");
		}
		else
		{
			CheatsHelper.Run("objective_complete " + s_QuestObjective.Blueprint.AssetGuid);
		}
	}

	public static void DebugObjectivePrevious()
	{
		if (s_Quest == null)
		{
			UIUtility.SendWarning("Quest is null");
			return;
		}
		List<QuestObjective> list = new List<QuestObjective>(s_Quest.Objectives);
		int i;
		for (i = 0; i < list.Count; i++)
		{
			if (s_QuestObjective == list[0])
			{
				break;
			}
		}
		if (i-- == -1)
		{
			i = 0;
		}
		s_QuestObjective = list[i];
		UIUtility.SendWarning($"QuestObjective: {Utilities.GetBlueprintName(s_QuestObjective.Blueprint)} ({s_QuestObjective.Blueprint.Title} {s_QuestObjective.State})");
	}

	public static void DebugObjectiveNext()
	{
		if (s_Quest == null)
		{
			UIUtility.SendWarning("Quest is null");
			return;
		}
		List<QuestObjective> list = new List<QuestObjective>(s_Quest.Objectives);
		int i;
		for (i = 0; i < list.Count; i++)
		{
			if (s_QuestObjective == list[0])
			{
				break;
			}
		}
		if (i++ == list.Count)
		{
			i = 0;
		}
		s_QuestObjective = list[i];
		UIUtility.SendWarning($"QuestObjective: {Utilities.GetBlueprintName(s_QuestObjective.Blueprint)} ({s_QuestObjective.Blueprint.Title} {s_QuestObjective.State})");
	}

	public static void DebugQuestPrevious()
	{
		List<Quest> list = new List<Quest>(Game.Instance.Player.QuestBook.Quests);
		if (list.Count == 0)
		{
			UIUtility.SendWarning("Quest count is 0");
			return;
		}
		int i;
		for (i = 0; i < list.Count && list[i] != s_Quest; i++)
		{
		}
		if (i-- == -1)
		{
			i = list.Count - 1;
		}
		s_Quest = list[i];
		UIUtility.SendWarning($"Quest: {Utilities.GetBlueprintName(s_Quest.Blueprint)} ({s_Quest.Blueprint.Title})");
	}

	public static void DebugQuestNext()
	{
		List<Quest> list = new List<Quest>(Game.Instance.Player.QuestBook.Quests);
		if (list.Count == 0)
		{
			UIUtility.SendWarning("Quest count is 0");
			return;
		}
		int i;
		for (i = 0; i < list.Count && list[i] != s_Quest; i++)
		{
		}
		if (i++ == list.Count)
		{
			i = 0;
		}
		s_Quest = list[i];
		UIUtility.SendWarning($"Quest: {Utilities.GetBlueprintName(s_Quest.Blueprint)} ({s_Quest.Blueprint.Title})");
	}

	private static void ToggleFow(string parameters)
	{
		if (FogOfWarArea.Active != null)
		{
			FogOfWarArea.Active.enabled = !FogOfWarArea.Active.enabled;
		}
	}

	private static void ListFlags(string parameters)
	{
		PFLog.SmartConsole.Log("Flags on player are:");
		foreach (KeyValuePair<BlueprintUnlockableFlag, int> unlockedFlag in Game.Instance.Player.UnlockableFlags.UnlockedFlags)
		{
			PFLog.SmartConsole.Log($"Flag: {unlockedFlag.Key}:{unlockedFlag.Value}");
		}
	}

	[Cheat(Name = "check_flag")]
	public static void CheckFlag(string flag = null)
	{
		if (flag.IsNullOrEmpty())
		{
			PFLog.SmartConsole.Log("Flag name is not specified");
			return;
		}
		BlueprintUnlockableFlag blueprint = Utilities.GetBlueprint<BlueprintUnlockableFlag>(flag);
		if (blueprint == null)
		{
			PFLog.SmartConsole.Log("Can't find Flag with name: " + flag);
			return;
		}
		string text = (blueprint.IsLocked ? "Locked" : "Unlocked");
		PFLog.SmartConsole.Log("Flag " + flag + " status: " + text + ", value: " + blueprint.Value + ".");
	}

	private static void LockFlag(string parameters)
	{
		string paramString = Utilities.GetParamString(parameters, 1, "Can't parse flag name from given parameters");
		BlueprintUnlockableFlag blueprint = Utilities.GetBlueprint<BlueprintUnlockableFlag>(paramString);
		if (blueprint == null)
		{
			PFLog.SmartConsole.Log("Can't find Flag with name: " + paramString);
		}
		else
		{
			Game.Instance.Player.UnlockableFlags.Lock(blueprint);
		}
	}

	private static void LockAll(string parameters)
	{
		foreach (BlueprintUnlockableFlag item in new List<BlueprintUnlockableFlag>(Game.Instance.Player.UnlockableFlags.UnlockedFlags.Keys))
		{
			Game.Instance.Player.UnlockableFlags.Lock(item);
		}
	}

	private static void GiveObjective(string parameters)
	{
		string paramString = Utilities.GetParamString(parameters, 1, "Can't parse objective name from given parameters");
		BlueprintQuestObjective blueprint = Utilities.GetBlueprint<BlueprintQuestObjective>(paramString);
		if (blueprint == null)
		{
			PFLog.SmartConsole.Log("Can't find Objective with name: '" + paramString + "'");
			return;
		}
		Game.Instance.Player.QuestBook.GiveObjective(blueprint);
		PFLog.SmartConsole.Log("Quest '" + Utilities.GetBlueprintPath(blueprint) + "' given");
	}

	[Cheat(Name = "objective_complete", ExecutionPolicy = ExecutionPolicy.PlayMode)]
	public static void CompleteObjective(BlueprintQuestObjective targetObjective)
	{
		foreach (Quest quest in Game.Instance.Player.QuestBook.Quests)
		{
			foreach (QuestObjective objective in quest.Objectives)
			{
				BlueprintQuestObjective blueprint = objective.Blueprint;
				if (targetObjective == blueprint)
				{
					Game.Instance.Player.QuestBook.CompleteObjective(blueprint);
					PFLog.SmartConsole.Log("Objective '" + Utilities.GetBlueprintPath(blueprint) + " completed");
					return;
				}
			}
		}
		PFLog.SmartConsole.Log("Can't find Objective with name in character: '" + targetObjective?.ToString() + "'");
	}

	private static void FailObjective(string parameters)
	{
		string paramString = Utilities.GetParamString(parameters, 1, "Can't parse objective name from given parameters");
		foreach (Quest quest in Game.Instance.Player.QuestBook.Quests)
		{
			foreach (QuestObjective objective in quest.Objectives)
			{
				BlueprintQuestObjective blueprint = objective.Blueprint;
				if (Utilities.GetBlueprintName(blueprint).Equals(paramString) || Utilities.GetBlueprintPath(blueprint).Contains(paramString) || blueprint.AssetGuid.Equals(paramString))
				{
					Game.Instance.Player.QuestBook.FailObjective(objective.Blueprint);
					PFLog.SmartConsole.Log("Objective '" + Utilities.GetBlueprintPath(objective.Blueprint) + "' failed");
					return;
				}
			}
		}
		PFLog.SmartConsole.Log("Can't find Objective with name in character: '" + paramString + "'");
	}

	public static void FillPreset(string parameters)
	{
	}

	[Cheat(Name = "create_item", ExecutionPolicy = ExecutionPolicy.PlayMode)]
	public static void CreateItem(BlueprintItem blueprint, int quantity = 1)
	{
		GameHelper.GetPlayerCharacter().Inventory.Add(blueprint, quantity);
	}

	private static void CheatAddFeature(string parameters)
	{
		BlueprintUnitFact blueprint = Utilities.GetBlueprint<BlueprintUnitFact>(Utilities.GetParamString(parameters, 1, "Can't parse item name from given parameters"));
		if (blueprint != null)
		{
			(Utilities.GetUnitUnderMouse() ?? GameHelper.GetPlayerCharacter()).AddFact(blueprint);
		}
	}

	private static void CreateAllItems(string parameters)
	{
		foreach (BlueprintItem scriptableObject in Utilities.GetScriptableObjects<BlueprintItem>())
		{
			try
			{
				GameHelper.GetPlayerCharacter().Inventory.Add(scriptableObject, 1);
			}
			catch (Exception ex)
			{
				PFLog.Default.Exception(ex);
			}
		}
	}

	private static void RecruitCompanion(string parameters)
	{
		string paramString = Utilities.GetParamString(parameters, 1, null);
		bool? paramBool = Utilities.GetParamBool(parameters, 2, null);
		BlueprintUnit blueprint = Utilities.GetBlueprint<BlueprintUnit>(paramString);
		if (blueprint == null)
		{
			PFLog.SmartConsole.Log("Cant get companion with name '" + paramString + "'");
			return;
		}
		SceneEntitiesState crossSceneState = Game.Instance.State.PlayerState.CrossSceneState;
		if (!paramBool.HasValue || paramBool.Value)
		{
			BaseUnitEntity baseUnitEntity = blueprint.CreateEntity(null, isInGame: false);
			Game.Instance.EntitySpawner.SpawnEntityImmediately(baseUnitEntity, crossSceneState);
			baseUnitEntity.GetOrCreate<UnitPartCompanion>().SetState(CompanionState.ExCompanion);
			return;
		}
		Vector3 position = Game.Instance.Player.MainCharacter.Entity.Position;
		BaseUnitEntity baseUnitEntity2 = Game.Instance.EntitySpawner.SpawnUnit(blueprint, position, Quaternion.identity, crossSceneState);
		Game.Instance.Player.AddCompanion(baseUnitEntity2);
		EventBus.RaiseEvent((IBaseUnitEntity)baseUnitEntity2, (Action<IPartyHandler>)delegate(IPartyHandler h)
		{
			h.HandleAddCompanion();
		}, isCheckRuntime: true);
	}

	[Cheat(Name = "recruit_main_character", ExecutionPolicy = ExecutionPolicy.PlayMode)]
	public static void RecruitMainCharacter(BlueprintUnit unit)
	{
		if (unit == null)
		{
			PFLog.SmartConsole.Log($"Cant get blueprint with name '{unit}'");
			return;
		}
		BaseUnitEntity baseUnitEntity = Game.Instance.Player.AllCharacters.FirstOrDefault((BaseUnitEntity c) => c.Blueprint == unit);
		if (baseUnitEntity != null)
		{
			Game.Instance.Player.SetMainCharacter(baseUnitEntity);
			return;
		}
		SceneEntitiesState crossSceneState = Game.Instance.State.PlayerState.CrossSceneState;
		Vector3 position = Game.Instance.Player.MainCharacter.Entity.Position;
		BaseUnitEntity mainCharacter = Game.Instance.EntitySpawner.SpawnUnit(unit, position, Quaternion.identity, crossSceneState);
		Game.Instance.Player.SetMainCharacter(mainCharacter);
	}

	[Cheat(Name = "remove_companion_from_roster", ExecutionPolicy = ExecutionPolicy.PlayMode)]
	public static void RemoveCompanionFromRoster(BlueprintUnit unit)
	{
		BaseUnitEntity baseUnitEntity = Game.Instance.Player.AllCharacters.FirstOrDefault((BaseUnitEntity c) => c.Blueprint == unit);
		UnitPartCompanion unitPartCompanion = baseUnitEntity?.GetOptional<UnitPartCompanion>();
		if (baseUnitEntity == null || unitPartCompanion == null)
		{
			PFLog.SmartConsole.Log($"Cant get companion with name '{unit}'");
			return;
		}
		baseUnitEntity.IsInGame = false;
		unitPartCompanion.SetState(CompanionState.ExCompanion);
	}

	[Cheat(Name = "unrecruit_companion", ExecutionPolicy = ExecutionPolicy.PlayMode)]
	public static void UnrecruitCompanion(BlueprintUnit unit)
	{
		BaseUnitEntity baseUnitEntity = Game.Instance.Player.AllCharacters.FirstOrDefault((BaseUnitEntity c) => c.Blueprint == unit);
		UnitPartCompanion unitPartCompanion = baseUnitEntity?.GetOptional<UnitPartCompanion>();
		if (baseUnitEntity == null || unitPartCompanion == null)
		{
			PFLog.SmartConsole.Log($"Cant get companion with name '{unit}'");
		}
		else
		{
			Game.Instance.Player.DetachPartyMember(baseUnitEntity);
		}
	}

	private static void ListAllQuest(string parameters)
	{
		string text = "";
		foreach (BlueprintQuest scriptableObject in Utilities.GetScriptableObjects<BlueprintQuest>())
		{
			text += $"{scriptableObject.Title} {scriptableObject.AssetGuid}\n";
			foreach (BlueprintQuestObjective objective in scriptableObject.Objectives)
			{
				text += $"    {objective.Title} {objective.AssetGuid}\n";
			}
		}
		GUIUtility.systemCopyBuffer = text;
	}

	private static void CompleteQuests(string parameters)
	{
		foreach (Quest quest in Game.Instance.Player.QuestBook.Quests)
		{
			foreach (BlueprintQuestObjective allObjective in quest.Blueprint.AllObjectives)
			{
				QuestObjectiveState objectiveState = Game.Instance.Player.QuestBook.GetObjectiveState(allObjective);
				if (objectiveState != QuestObjectiveState.Completed && objectiveState != QuestObjectiveState.Failed)
				{
					Game.Instance.Player.QuestBook.GiveObjective(allObjective);
					Game.Instance.Player.QuestBook.CompleteObjective(allObjective);
					if (allObjective.IsFinishParent)
					{
						break;
					}
				}
			}
		}
	}
}
