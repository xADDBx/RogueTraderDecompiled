using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Core.Cheats;
using Kingmaker;
using Kingmaker.Achievements;
using Kingmaker.AI;
using Kingmaker.AreaLogic.Cutscenes;
using Kingmaker.AreaLogic.Etudes;
using Kingmaker.BarkBanters;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Area;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.Quests;
using Kingmaker.Cargo;
using Kingmaker.Cheats;
using Kingmaker.Code.UI.MVVM.VM.Common;
using Kingmaker.Code.UI.MVVM.VM.FirstLaunchSettings;
using Kingmaker.Controllers;
using Kingmaker.Controllers.Clicks;
using Kingmaker.Controllers.Dialog;
using Kingmaker.Controllers.MapObjects;
using Kingmaker.Controllers.MovePrediction;
using Kingmaker.Controllers.Net;
using Kingmaker.Controllers.StarSystem;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.Controllers.Units;
using Kingmaker.Designers.EventConditionActionSystem.Evaluators.BarkBanters;
using Kingmaker.Designers.WarhammerSurfaceCombatPrototype.PsychicPowers;
using Kingmaker.DialogSystem.Blueprints;
using Kingmaker.DLC;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Persistence;
using Kingmaker.Enums;
using Kingmaker.Formations;
using Kingmaker.Globalmap.Blueprints.Colonization;
using Kingmaker.Globalmap.Blueprints.SystemMap;
using Kingmaker.Localization.Enums;
using Kingmaker.Networking;
using Kingmaker.QA;
using Kingmaker.QA.Analytics;
using Kingmaker.QA.Arbiter;
using Kingmaker.Replay;
using Kingmaker.Settings;
using Kingmaker.Settings.Graphics;
using Kingmaker.SpaceCombat.StarshipLogic.Parts;
using Kingmaker.Tutorial;
using Kingmaker.Twitch;
using Kingmaker.UI.InputSystems;
using Kingmaker.UI.MVVM.VM.NetLobby;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Groups;
using Kingmaker.UnitLogic.Levelup.Obsolete.Blueprints.Prerequisites;
using Kingmaker.Utility;
using Kingmaker.View;
using Kingmaker.View.MapObjects;
using Kingmaker.Visual.Particles.ForcedCulling;
using Kingmaker.Visual.Sound;
using Owlcat.Runtime.Visual.Effects.WeatherSystem;
using UnityEngine;

namespace CheatsCodeGen;

public static class AllCheats
{
	public static readonly List<CheatMethodInfoInternal> Methods = new List<CheatMethodInfoInternal>
	{
		new CheatMethodInfoInternal(new Action<string>(AchievementsManager.ListAchievements), "void ListAchievements(string param = \\\"\\\")", "list_achievements", "", "", ExecutionPolicy.All, new CheatParameter[1]
		{
			new CheatParameter
			{
				Name = "param",
				Type = "System.String",
				HasDefaultValue = true
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action(CutsceneCheats.SkipCutscene), "void SkipCutscene()", "skip_cutscene", "", "", ExecutionPolicy.All, new CheatParameter[0], "void"),
		new CheatMethodInfoInternal(new Action(CutsceneCheats.SkipBark), "void SkipBark()", "skip_bark", "", "", ExecutionPolicy.All, new CheatParameter[0], "void"),
		new CheatMethodInfoInternal(new Action(EtudeBracketForbidColonization.ChangeForbidColonization), "void ChangeForbidColonization()", "change_colonization_access", "", "", ExecutionPolicy.All, new CheatParameter[0], "void"),
		new CheatMethodInfoInternal(new Action(EtudeBracketForbidOpenShipInventory.ChangeStarshipAccess), "void ChangeStarshipAccess()", "change_ship_access", "", "", ExecutionPolicy.All, new CheatParameter[0], "void"),
		new CheatMethodInfoInternal(new Action<string>(CargoHelper.AddCargo), "void AddCargo(string blueprintName)", "add_cargo", "", "", ExecutionPolicy.PlayMode, new CheatParameter[1]
		{
			new CheatParameter
			{
				Name = "blueprintName",
				Type = "System.String",
				HasDefaultValue = false
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action<string>(CargoHelper.RemoveCargo), "void RemoveCargo(string blueprintName)", "remove_cargo", "", "", ExecutionPolicy.PlayMode, new CheatParameter[1]
		{
			new CheatParameter
			{
				Name = "blueprintName",
				Type = "System.String",
				HasDefaultValue = false
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action<string>(CargoHelper.LootToCargo), "void LootToCargo(string blueprintName)", "move_open_loot_to_cargo", "", "", ExecutionPolicy.PlayMode, new CheatParameter[1]
		{
			new CheatParameter
			{
				Name = "blueprintName",
				Type = "System.String",
				HasDefaultValue = false
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action<string, FactionType>(CargoHelper.SellCargo), "void SellCargo(string blueprintName, FactionType faction)", "sell_cargo", "", "", ExecutionPolicy.PlayMode, new CheatParameter[2]
		{
			new CheatParameter
			{
				Name = "blueprintName",
				Type = "System.String",
				HasDefaultValue = false
			},
			new CheatParameter
			{
				Name = "faction",
				Type = "Kingmaker.Enums.FactionType",
				HasDefaultValue = false
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action(CargoHelper.AddRandomCargoToSell), "void AddRandomCargoToSell()", "add_random_cargo_to_sell", "", "", ExecutionPolicy.PlayMode, new CheatParameter[0], "void"),
		new CheatMethodInfoInternal(new Action(CargoHelper.RemoveRandomCargoFromSell), "void RemoveRandomCargoFromSell()", "remove_random_cargo_from_sell", "", "", ExecutionPolicy.PlayMode, new CheatParameter[0], "void"),
		new CheatMethodInfoInternal(new Action(CargoHelper.DealSellCargoes), "void DealSellCargoes()", "deal_sell_cargoes", "", "", ExecutionPolicy.PlayMode, new CheatParameter[0], "void"),
		new CheatMethodInfoInternal(new Action(CargoHelper.CancelSellCargoesDeal), "void CancelSellCargoesDeal()", "cancel_sell_cargoes", "", "", ExecutionPolicy.PlayMode, new CheatParameter[0], "void"),
		new CheatMethodInfoInternal(new Action<int, string>(CargoHelper.FillRandomCargo), "void FillRandomCargo(int targetVolume = 100, string blueprintName = null)", "fill_random_cargo", "", "", ExecutionPolicy.PlayMode, new CheatParameter[2]
		{
			new CheatParameter
			{
				Name = "targetVolume",
				Type = "System.Int32",
				HasDefaultValue = true
			},
			new CheatParameter
			{
				Name = "blueprintName",
				Type = "System.String",
				HasDefaultValue = true
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action<int, string>(CargoHelper.FillAllCargoes), "void FillAllCargoes(int targetVolume = 100, string blueprintName = null)", "fill_all_cargoes", "", "", ExecutionPolicy.PlayMode, new CheatParameter[2]
		{
			new CheatParameter
			{
				Name = "targetVolume",
				Type = "System.Int32",
				HasDefaultValue = true
			},
			new CheatParameter
			{
				Name = "blueprintName",
				Type = "System.String",
				HasDefaultValue = true
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action(CargoHelper.AddRandomCargo), "void AddRandomCargo()", "add_random_cargo", "", "", ExecutionPolicy.PlayMode, new CheatParameter[0], "void"),
		new CheatMethodInfoInternal(new Func<string, bool, Task>(CheatsHelper.Exec), "Task Exec(string path, bool silent = false)", "exec", "", "", ExecutionPolicy.All, new CheatParameter[2]
		{
			new CheatParameter
			{
				Name = "path",
				Type = "System.String",
				HasDefaultValue = false
			},
			new CheatParameter
			{
				Name = "silent",
				Type = "System.Boolean",
				HasDefaultValue = true
			}
		}, "Task"),
		new CheatMethodInfoInternal(new Action<string>(CheatsBots.ConsoleClockworkStart), "void ConsoleClockworkStart(string scenario)", "clockwork_start", "", "", ExecutionPolicy.All, new CheatParameter[1]
		{
			new CheatParameter
			{
				Name = "scenario",
				Type = "System.String",
				HasDefaultValue = false
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action(CheatsBots.ConsoleClockworkStop), "void ConsoleClockworkStop()", "clockwork_stop", "", "", ExecutionPolicy.All, new CheatParameter[0], "void"),
		new CheatMethodInfoInternal(new Func<string>(CheatsBots.ConsoleClockworkScenarioList), "string ConsoleClockworkScenarioList()", "clockwork_list_scenarios", "", "", ExecutionPolicy.All, new CheatParameter[0], "string"),
		new CheatMethodInfoInternal(new Func<string>(CheatsBots.ConsoleClockworkStatus), "string ConsoleClockworkStatus()", "clockwork_status", "", "", ExecutionPolicy.All, new CheatParameter[0], "string"),
		new CheatMethodInfoInternal(new Action<string>(CheatsBots.ConsoleArbiterStart), "void ConsoleArbiterStart(string instruction)", "arbiter_start", "", "", ExecutionPolicy.All, new CheatParameter[1]
		{
			new CheatParameter
			{
				Name = "instruction",
				Type = "System.String",
				HasDefaultValue = false
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action(CheatsBots.ConsoleArbiterStop), "void ConsoleArbiterStop()", "arbiter_stop", "", "", ExecutionPolicy.All, new CheatParameter[0], "void"),
		new CheatMethodInfoInternal(new Func<string>(CheatsBots.ConsoleArbiterInstructionList), "string ConsoleArbiterInstructionList()", "arbiter_list_instructions", "", "", ExecutionPolicy.All, new CheatParameter[0], "string"),
		new CheatMethodInfoInternal(new Action<string, int>(CheatsBots.MakeScreenshot), "void MakeScreenshot(string instructionName, int pointId)", "arbiter_screenshot", "", "", ExecutionPolicy.All, new CheatParameter[2]
		{
			new CheatParameter
			{
				Name = "instructionName",
				Type = "System.String",
				HasDefaultValue = false
			},
			new CheatParameter
			{
				Name = "pointId",
				Type = "System.Int32",
				HasDefaultValue = false
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action<string, int>(CheatsBots.ArbiterTeleportCamera), "void ArbiterTeleportCamera(string instructionName, int pointId)", "arbiter_camera_teleport", "", "", ExecutionPolicy.All, new CheatParameter[2]
		{
			new CheatParameter
			{
				Name = "instructionName",
				Type = "System.String",
				HasDefaultValue = false
			},
			new CheatParameter
			{
				Name = "pointId",
				Type = "System.Int32",
				HasDefaultValue = false
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action<BlueprintPlanet>(CheatsColonization.ColonizePlanet), "void ColonizePlanet(BlueprintPlanet blueprintPlanet)", "colonize", "", "", ExecutionPolicy.PlayMode, new CheatParameter[1]
		{
			new CheatParameter
			{
				Name = "blueprintPlanet",
				Type = "Kingmaker.Globalmap.Blueprints.SystemMap.BlueprintPlanet",
				HasDefaultValue = false
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action(CheatsColonization.EnableColonization), "void EnableColonization()", "enable_colonization", "", "", ExecutionPolicy.PlayMode, new CheatParameter[0], "void"),
		new CheatMethodInfoInternal(new Action(CheatsColonization.FinishAllCurrentProjects), "void FinishAllCurrentProjects()", "finish_projects", "", "", ExecutionPolicy.PlayMode, new CheatParameter[0], "void"),
		new CheatMethodInfoInternal(new Action<BlueprintResource, int>(CheatsColonization.AddResourceToColoniesCheat), "void AddResourceToColoniesCheat(BlueprintResource resource, int resourceCount = 1)", "add_colony_resource", "", "", ExecutionPolicy.PlayMode, new CheatParameter[2]
		{
			new CheatParameter
			{
				Name = "resource",
				Type = "Kingmaker.Globalmap.Blueprints.Colonization.BlueprintResource",
				HasDefaultValue = false
			},
			new CheatParameter
			{
				Name = "resourceCount",
				Type = "System.Int32",
				HasDefaultValue = true
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action<BlueprintColony, BlueprintColonyProject>(CheatsColonization.StartProject), "void StartProject(BlueprintColony colonyBlueprint, BlueprintColonyProject project)", "start_project", "", "", ExecutionPolicy.PlayMode, new CheatParameter[2]
		{
			new CheatParameter
			{
				Name = "colonyBlueprint",
				Type = "Kingmaker.Globalmap.Blueprints.Colonization.BlueprintColony",
				HasDefaultValue = false
			},
			new CheatParameter
			{
				Name = "project",
				Type = "Kingmaker.Globalmap.Blueprints.Colonization.BlueprintColonyProject",
				HasDefaultValue = false
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action<BlueprintResource>(CheatsColonization.UseMiner), "void UseMiner(BlueprintResource resource)", "use_miner", "", "", ExecutionPolicy.PlayMode, new CheatParameter[1]
		{
			new CheatParameter
			{
				Name = "resource",
				Type = "Kingmaker.Globalmap.Blueprints.Colonization.BlueprintResource",
				HasDefaultValue = false
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action<BlueprintResource>(CheatsColonization.RemoveMiner), "void RemoveMiner(BlueprintResource resource)", "remove_miner", "", "", ExecutionPolicy.PlayMode, new CheatParameter[1]
		{
			new CheatParameter
			{
				Name = "resource",
				Type = "Kingmaker.Globalmap.Blueprints.Colonization.BlueprintResource",
				HasDefaultValue = false
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action<int>(CheatsColonization.AddPF), "void AddPF(int value)", "add_pf", "", "", ExecutionPolicy.PlayMode, new CheatParameter[1]
		{
			new CheatParameter
			{
				Name = "value",
				Type = "System.Int32",
				HasDefaultValue = false
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action<BlueprintColony, string, int>(CheatsColonization.AddColonyStat), "void AddColonyStat(BlueprintColony colonyBlueprint, string stat, int value)", "add_colony_stat", "", "", ExecutionPolicy.PlayMode, new CheatParameter[3]
		{
			new CheatParameter
			{
				Name = "colonyBlueprint",
				Type = "Kingmaker.Globalmap.Blueprints.Colonization.BlueprintColony",
				HasDefaultValue = false
			},
			new CheatParameter
			{
				Name = "stat",
				Type = "System.String",
				HasDefaultValue = false
			},
			new CheatParameter
			{
				Name = "value",
				Type = "System.Int32",
				HasDefaultValue = false
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action<BlueprintColonyEvent, BlueprintPlanet, bool>(CheatsColonization.CheatAddEventToColony), "void CheatAddEventToColony(BlueprintColonyEvent colonyEvent, BlueprintPlanet planet, bool ignoreColonyEventRequirements = false)", "add_event_to_colony", "", "", ExecutionPolicy.PlayMode, new CheatParameter[3]
		{
			new CheatParameter
			{
				Name = "colonyEvent",
				Type = "Kingmaker.Globalmap.Blueprints.Colonization.BlueprintColonyEvent",
				HasDefaultValue = false
			},
			new CheatParameter
			{
				Name = "planet",
				Type = "Kingmaker.Globalmap.Blueprints.SystemMap.BlueprintPlanet",
				HasDefaultValue = false
			},
			new CheatParameter
			{
				Name = "ignoreColonyEventRequirements",
				Type = "System.Boolean",
				HasDefaultValue = true
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action<BlueprintColonyEvent, bool, bool, bool, bool>(CheatsColonization.CheatRemoveEventFromColony), "void CheatRemoveEventFromColony(BlueprintColonyEvent colonyEvent, bool replaceWithOtherEvent = false, bool addToExclusivePlanet = false, bool ignoreColonyEventRequirements = false, bool removeFromAllowedPlanet = false)", "remove_event_from_colony", "", "", ExecutionPolicy.PlayMode, new CheatParameter[5]
		{
			new CheatParameter
			{
				Name = "colonyEvent",
				Type = "Kingmaker.Globalmap.Blueprints.Colonization.BlueprintColonyEvent",
				HasDefaultValue = false
			},
			new CheatParameter
			{
				Name = "replaceWithOtherEvent",
				Type = "System.Boolean",
				HasDefaultValue = true
			},
			new CheatParameter
			{
				Name = "addToExclusivePlanet",
				Type = "System.Boolean",
				HasDefaultValue = true
			},
			new CheatParameter
			{
				Name = "ignoreColonyEventRequirements",
				Type = "System.Boolean",
				HasDefaultValue = true
			},
			new CheatParameter
			{
				Name = "removeFromAllowedPlanet",
				Type = "System.Boolean",
				HasDefaultValue = true
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action(CheatsColonization.CheatLogColoniesEvents), "void CheatLogColoniesEvents()", "log_colonies_events", "", "", ExecutionPolicy.PlayMode, new CheatParameter[0], "void"),
		new CheatMethodInfoInternal(new Action(CheatsCombat.KillAll), "void KillAll()", "kill_all", "", "", ExecutionPolicy.PlayMode, new CheatParameter[0], "void"),
		new CheatMethodInfoInternal(new Action<BaseUnitEntity>(CheatsCombat.Kill), "void Kill(BaseUnitEntity unit)", "kill", "", "", ExecutionPolicy.PlayMode, new CheatParameter[1]
		{
			new CheatParameter
			{
				Name = "unit",
				Type = "Kingmaker.EntitySystem.Entities.BaseUnitEntity",
				HasDefaultValue = false
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action<MechanicEntity, bool>(CheatsCombat.Damage), "void Damage(MechanicEntity entity, bool tryToKill = false)", "damage", "", "", ExecutionPolicy.PlayMode, new CheatParameter[2]
		{
			new CheatParameter
			{
				Name = "entity",
				Type = "Kingmaker.EntitySystem.Entities.MechanicEntity",
				HasDefaultValue = false
			},
			new CheatParameter
			{
				Name = "tryToKill",
				Type = "System.Boolean",
				HasDefaultValue = true
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action<MechanicEntity>(CheatsCombat.Heal), "void Heal(MechanicEntity entity)", "heal", "", "", ExecutionPolicy.PlayMode, new CheatParameter[1]
		{
			new CheatParameter
			{
				Name = "entity",
				Type = "Kingmaker.EntitySystem.Entities.MechanicEntity",
				HasDefaultValue = false
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action<BaseUnitEntity>(CheatsCombat.ListBuffs), "void ListBuffs(BaseUnitEntity targetUnit)", "list_buffs", "", "", ExecutionPolicy.PlayMode, new CheatParameter[1]
		{
			new CheatParameter
			{
				Name = "targetUnit",
				Type = "Kingmaker.EntitySystem.Entities.BaseUnitEntity",
				HasDefaultValue = false
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action<BaseUnitEntity>(CheatsCombat.DetachAllBuffs), "void DetachAllBuffs(BaseUnitEntity unit)", "detach_all_buffs", "", "", ExecutionPolicy.PlayMode, new CheatParameter[1]
		{
			new CheatParameter
			{
				Name = "unit",
				Type = "Kingmaker.EntitySystem.Entities.BaseUnitEntity",
				HasDefaultValue = false
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action(CheatsCombat.RestAll), "void RestAll()", "rest_all", "", "", ExecutionPolicy.PlayMode, new CheatParameter[0], "void"),
		new CheatMethodInfoInternal(new Action(CheatsCombat.SetMP100), "void SetMP100()", "setmp100", "", "", ExecutionPolicy.PlayMode, new CheatParameter[0], "void"),
		new CheatMethodInfoInternal(new Action<BlueprintUnitFact>(CheatsCombat.DetachFact), "void DetachFact(BlueprintUnitFact fact)", "detach_fact", "", "", ExecutionPolicy.PlayMode, new CheatParameter[1]
		{
			new CheatParameter
			{
				Name = "fact",
				Type = "Kingmaker.Blueprints.Facts.BlueprintUnitFact",
				HasDefaultValue = false
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action<BlueprintUnitFact>(CheatsCombat.AttachFact), "void AttachFact(BlueprintUnitFact fact)", "attach_fact", "", "", ExecutionPolicy.PlayMode, new CheatParameter[1]
		{
			new CheatParameter
			{
				Name = "fact",
				Type = "Kingmaker.Blueprints.Facts.BlueprintUnitFact",
				HasDefaultValue = false
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action<BlueprintUnit, BlueprintFaction, Vector3>(CheatsCombat.SpawnEnemyUnderCursor), "void SpawnEnemyUnderCursor(BlueprintUnit bp = null, BlueprintFaction factionBp = null, Vector3 position = default(Vector3))", "summon", "", "", ExecutionPolicy.PlayMode, new CheatParameter[3]
		{
			new CheatParameter
			{
				Name = "bp",
				Type = "Kingmaker.Blueprints.BlueprintUnit",
				HasDefaultValue = true
			},
			new CheatParameter
			{
				Name = "factionBp",
				Type = "Kingmaker.Blueprints.BlueprintFaction",
				HasDefaultValue = true
			},
			new CheatParameter
			{
				Name = "position",
				Type = "UnityEngine.Vector3",
				HasDefaultValue = true
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action<int, bool>(CheatsCombat.SpawnUnitsDense), "void SpawnUnitsDense(int number, bool roaming = false)", "spawn_units_dense", "", "", ExecutionPolicy.PlayMode, new CheatParameter[2]
		{
			new CheatParameter
			{
				Name = "number",
				Type = "System.Int32",
				HasDefaultValue = false
			},
			new CheatParameter
			{
				Name = "roaming",
				Type = "System.Boolean",
				HasDefaultValue = true
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action<int, bool>(CheatsCombat.SpawnUnitsSparse), "void SpawnUnitsSparse(int number, bool roaming = false)", "spawn_units_sparse", "", "", ExecutionPolicy.PlayMode, new CheatParameter[2]
		{
			new CheatParameter
			{
				Name = "number",
				Type = "System.Int32",
				HasDefaultValue = false
			},
			new CheatParameter
			{
				Name = "roaming",
				Type = "System.Boolean",
				HasDefaultValue = true
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action<int, bool>(CheatsCombat.SpawnExtraDense), "void SpawnExtraDense(int number, bool roaming = false)", "spawn_extra_dense", "", "", ExecutionPolicy.PlayMode, new CheatParameter[2]
		{
			new CheatParameter
			{
				Name = "number",
				Type = "System.Int32",
				HasDefaultValue = false
			},
			new CheatParameter
			{
				Name = "roaming",
				Type = "System.Boolean",
				HasDefaultValue = true
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action<int, bool>(CheatsCombat.SpawnExtraSparse), "void SpawnExtraSparse(int number, bool roaming = false)", "spawn_extra_sparse", "", "", ExecutionPolicy.PlayMode, new CheatParameter[2]
		{
			new CheatParameter
			{
				Name = "number",
				Type = "System.Int32",
				HasDefaultValue = false
			},
			new CheatParameter
			{
				Name = "roaming",
				Type = "System.Boolean",
				HasDefaultValue = true
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action<int>(CheatsCombat.SpawnEnemies), "void SpawnEnemies(int number)", "spawn_enemies", "", "", ExecutionPolicy.PlayMode, new CheatParameter[1]
		{
			new CheatParameter
			{
				Name = "number",
				Type = "System.Int32",
				HasDefaultValue = false
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action(CheatsCombat.SpawnTest), "void SpawnTest()", "spawn_test", "", "", ExecutionPolicy.PlayMode, new CheatParameter[0], "void"),
		new CheatMethodInfoInternal(new Action<int>(CheatsCombat.SetActionPointsYellow), "void SetActionPointsYellow(int yellow)", "set_action_points_yellow", "", "", ExecutionPolicy.PlayMode, new CheatParameter[1]
		{
			new CheatParameter
			{
				Name = "yellow",
				Type = "System.Int32",
				HasDefaultValue = false
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action<float>(CheatsCombat.SetActionPointsBlue), "void SetActionPointsBlue(float blue)", "set_action_points_blue", "", "", ExecutionPolicy.PlayMode, new CheatParameter[1]
		{
			new CheatParameter
			{
				Name = "blue",
				Type = "System.Single",
				HasDefaultValue = false
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action<int, int, RestrictionsHolder>(CheatsCombat.AddBonusAbilityUsage), "void AddBonusAbilityUsage(int count = 1, int costBonus = -5, RestrictionsHolder restrictions = null)", "add_bonus_ability_usage", "", "", ExecutionPolicy.PlayMode, new CheatParameter[3]
		{
			new CheatParameter
			{
				Name = "count",
				Type = "System.Int32",
				HasDefaultValue = true
			},
			new CheatParameter
			{
				Name = "costBonus",
				Type = "System.Int32",
				HasDefaultValue = true
			},
			new CheatParameter
			{
				Name = "restrictions",
				Type = "Kingmaker.Blueprints.RestrictionsHolder",
				HasDefaultValue = true
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action<MechanicEntity>(CheatsCommon.ExecuteAction), "void ExecuteAction(MechanicEntity target)", "action", "", "", ExecutionPolicy.PlayMode, new CheatParameter[1]
		{
			new CheatParameter
			{
				Name = "target",
				Type = "Kingmaker.EntitySystem.Entities.MechanicEntity",
				HasDefaultValue = false
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action(CheatsCommon.RandomEncounterStatusSwitch), "void RandomEncounterStatusSwitch()", "random_encounter_status_switch", "", "", ExecutionPolicy.PlayMode, new CheatParameter[0], "void"),
		new CheatMethodInfoInternal(new Action(CheatsCommon.ChecksFail), "void ChecksFail()", "checks_fail", "", "", ExecutionPolicy.PlayMode, new CheatParameter[0], "void"),
		new CheatMethodInfoInternal(new Action(CheatsCommon.ChecksSuccess), "void ChecksSuccess()", "checks_success", "", "", ExecutionPolicy.PlayMode, new CheatParameter[0], "void"),
		new CheatMethodInfoInternal(new Action<int>(CheatsCommon.SetPlayerDice), "void SetPlayerDice(int value)", "set_dice", "", "", ExecutionPolicy.PlayMode, new CheatParameter[1]
		{
			new CheatParameter
			{
				Name = "value",
				Type = "System.Int32",
				HasDefaultValue = false
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action(CheatsCommon.ReleasePlayerDice), "void ReleasePlayerDice()", "release_dice", "", "", ExecutionPolicy.PlayMode, new CheatParameter[0], "void"),
		new CheatMethodInfoInternal(new Func<string>(CheatsCommon.ShowSoulMarks), "string ShowSoulMarks()", "show_soul_marks", "", "", ExecutionPolicy.PlayMode, new CheatParameter[0], "string"),
		new CheatMethodInfoInternal(new Action<string, int>(CheatsCommon.ShiftSoulMark), "void ShiftSoulMark(string soulMark, int value)", "shift_soul_mark", "", "", ExecutionPolicy.PlayMode, new CheatParameter[2]
		{
			new CheatParameter
			{
				Name = "soulMark",
				Type = "System.String",
				HasDefaultValue = false
			},
			new CheatParameter
			{
				Name = "value",
				Type = "System.Int32",
				HasDefaultValue = false
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action<Locale>(CheatsCommon.ChangeLocalization), "void ChangeLocalization(Locale locale)", "change_localization", "", "", ExecutionPolicy.PlayMode, new CheatParameter[1]
		{
			new CheatParameter
			{
				Name = "locale",
				Type = "Kingmaker.Localization.Enums.Locale",
				HasDefaultValue = false
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action(CheatsCommon.ShowDebugBubble), "void ShowDebugBubble()", "emperor_open_my_eyes", "", "", ExecutionPolicy.PlayMode, new CheatParameter[0], "void"),
		new CheatMethodInfoInternal(new Action(CheatsCommon.ShowDebugMessageBox), "void ShowDebugMessageBox()", "show_message_box", "", "", ExecutionPolicy.PlayMode, new CheatParameter[0], "void"),
		new CheatMethodInfoInternal(new Action(CheatsCommon.StatCoercion), "void StatCoercion()", "stat_coercion", "", "", ExecutionPolicy.PlayMode, new CheatParameter[0], "void"),
		new CheatMethodInfoInternal(new Action(CheatsCommon.CleanSpace), "void CleanSpace()", "clean_space", "", "", ExecutionPolicy.PlayMode, new CheatParameter[0], "void"),
		new CheatMethodInfoInternal(new Action(CheatsContextData.RandomEncounterStatusSwitch), "void RandomEncounterStatusSwitch()", "break_context_data", "", "", ExecutionPolicy.PlayMode, new CheatParameter[0], "void"),
		new CheatMethodInfoInternal(new Action<Cutscene>(CheatsCutscenes.StopCutscenes), "void StopCutscenes(Cutscene cutscene)", "stop_cutscene", "", "", ExecutionPolicy.PlayMode, new CheatParameter[1]
		{
			new CheatParameter
			{
				Name = "cutscene",
				Type = "Kingmaker.AreaLogic.Cutscenes.Cutscene",
				HasDefaultValue = false
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action(CheatsDebug.DoRunOutOfMemory), "void DoRunOutOfMemory()", "alloc_crash", "Allocate memory until Unity crashes", "", ExecutionPolicy.All, new CheatParameter[0], "void"),
		new CheatMethodInfoInternal(new Action(CheatsDebug.DoGC), "void DoGC()", "gc", "Call GC.Collect()", "", ExecutionPolicy.All, new CheatParameter[0], "void"),
		new CheatMethodInfoInternal(new Action(CheatsDebug.DoGCUUA), "void DoGCUUA()", "gc_uua", "Call GC.Collect() and UnloadUnusedAssets", "", ExecutionPolicy.All, new CheatParameter[0], "void"),
		new CheatMethodInfoInternal(new Action(CheatsDebug.MemStats), "void MemStats()", "memstats", "Show allocated memory amounts", "", ExecutionPolicy.All, new CheatParameter[0], "void"),
		new CheatMethodInfoInternal(new Action(CheatsDebug.Quit), "void Quit()", "quit", "Call SystemUtil.ApplicationQuit()", "", ExecutionPolicy.All, new CheatParameter[0], "void"),
		new CheatMethodInfoInternal(new Action(CheatsDebug.QuitForce), "void QuitForce()", "quit_force", "Call Application.Quit(1)", "", ExecutionPolicy.All, new CheatParameter[0], "void"),
		new CheatMethodInfoInternal(new Action(CheatsDebug.WwiseProfilerCapture), "void WwiseProfilerCapture()", "wwise_profile", "Launch Wwise profiler session", "", ExecutionPolicy.All, new CheatParameter[0], "void"),
		new CheatMethodInfoInternal(new Action(CheatsDebug.ReloadUI), "void ReloadUI()", "reload_ui", "", "", ExecutionPolicy.PlayMode, new CheatParameter[0], "void"),
		new CheatMethodInfoInternal(new Action(CheatsDebug.ResetWidgetStash), "void ResetWidgetStash()", "reset_widget_stash", "", "", ExecutionPolicy.PlayMode, new CheatParameter[0], "void"),
		new CheatMethodInfoInternal(new Action(CheatsDebug.ChangeUINextPlatform), "void ChangeUINextPlatform()", "change_ui_next_platform", "", "", ExecutionPolicy.PlayMode, new CheatParameter[0], "void"),
		new CheatMethodInfoInternal(new Action(CheatsDebug.ChangeUIPrevPlatform), "void ChangeUIPrevPlatform()", "change_ui_prev_platform", "", "", ExecutionPolicy.PlayMode, new CheatParameter[0], "void"),
		new CheatMethodInfoInternal(new Action(CheatsDebug.CrashGame), "void CrashGame()", "debug_crash", "", "", ExecutionPolicy.All, new CheatParameter[0], "void"),
		new CheatMethodInfoInternal(new Action(CheatsDebug.ExceptionGame), "void ExceptionGame()", "debug_exception", "", "", ExecutionPolicy.All, new CheatParameter[0], "void"),
		new CheatMethodInfoInternal(new Action(CheatsDebug.ExceptionDBZ), "void ExceptionDBZ()", "test_exception_dbz", "", "", ExecutionPolicy.All, new CheatParameter[0], "void"),
		new CheatMethodInfoInternal(new Action(CheatsDebug.ExceptionNRE), "void ExceptionNRE()", "test_exception_nre", "", "", ExecutionPolicy.All, new CheatParameter[0], "void"),
		new CheatMethodInfoInternal(new Action(CheatsDebug.ExceptionAIOB), "void ExceptionAIOB()", "test_exception_aiob", "", "", ExecutionPolicy.All, new CheatParameter[0], "void"),
		new CheatMethodInfoInternal(new Action(CheatsDebug.ReturnToMainMenu), "void ReturnToMainMenu()", "return_to_main_menu", "", "", ExecutionPolicy.All, new CheatParameter[0], "void"),
		new CheatMethodInfoInternal(new Action(CheatsDebug.LogDisposables), "void LogDisposables()", "log_disposables", "", "", ExecutionPolicy.All, new CheatParameter[0], "void"),
		new CheatMethodInfoInternal(new Action<string>(CheatsDebug.TakeSnapshot), "void TakeSnapshot(string name = null)", "snapshot", "", "", ExecutionPolicy.All, new CheatParameter[1]
		{
			new CheatParameter
			{
				Name = "name",
				Type = "System.String",
				HasDefaultValue = true
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action<string>(CheatsDebug.TakeSnapshotFull), "void TakeSnapshotFull(string name = null)", "snapshot_full", "", "", ExecutionPolicy.All, new CheatParameter[1]
		{
			new CheatParameter
			{
				Name = "name",
				Type = "System.String",
				HasDefaultValue = true
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action<string>(CheatsDebug.TakeSnapshotNativeOnly), "void TakeSnapshotNativeOnly(string name = null)", "snapshot_objects", "", "", ExecutionPolicy.All, new CheatParameter[1]
		{
			new CheatParameter
			{
				Name = "name",
				Type = "System.String",
				HasDefaultValue = true
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action<string, int, int>(CheatsDebug.DebugSpamStart), "void DebugSpamStart(string spamType = \\\"exceptions\\\", int intervalMs = 10, int depth = 10)", "debug_spam_start", "", "", ExecutionPolicy.PlayMode, new CheatParameter[3]
		{
			new CheatParameter
			{
				Name = "spamType",
				Type = "System.String",
				HasDefaultValue = true
			},
			new CheatParameter
			{
				Name = "intervalMs",
				Type = "System.Int32",
				HasDefaultValue = true
			},
			new CheatParameter
			{
				Name = "depth",
				Type = "System.Int32",
				HasDefaultValue = true
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action<int, int, int>(CheatsDebug.DebugExceptionSpam), "void DebugExceptionSpam(int count = 5, int depth = 10, int interval = 10)", "debug_spam_exceptions", "", "", ExecutionPolicy.PlayMode, new CheatParameter[3]
		{
			new CheatParameter
			{
				Name = "count",
				Type = "System.Int32",
				HasDefaultValue = true
			},
			new CheatParameter
			{
				Name = "depth",
				Type = "System.Int32",
				HasDefaultValue = true
			},
			new CheatParameter
			{
				Name = "interval",
				Type = "System.Int32",
				HasDefaultValue = true
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action(CheatsDebug.DebugOffThread), "void DebugOffThread()", "debug_spam_start_in_outer_thread", "", "", ExecutionPolicy.PlayMode, new CheatParameter[0], "void"),
		new CheatMethodInfoInternal(new Action<string, int, int>(CheatsDebug.DebugStartSpam), "void DebugStartSpam(string spamType = \\\"exceptions\\\", int intervalMs = 10, int depth = 10)", "debug_start_spam", "", "", ExecutionPolicy.PlayMode, new CheatParameter[3]
		{
			new CheatParameter
			{
				Name = "spamType",
				Type = "System.String",
				HasDefaultValue = true
			},
			new CheatParameter
			{
				Name = "intervalMs",
				Type = "System.Int32",
				HasDefaultValue = true
			},
			new CheatParameter
			{
				Name = "depth",
				Type = "System.Int32",
				HasDefaultValue = true
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action(CheatsDebug.DebugStopSpam), "void DebugStopSpam()", "debug_stop_spam", "", "", ExecutionPolicy.PlayMode, new CheatParameter[0], "void"),
		new CheatMethodInfoInternal(new Action(CheatsDebug.DisableLogging), "void DisableLogging()", "disable_logging", "", "", ExecutionPolicy.PlayMode, new CheatParameter[0], "void"),
		new CheatMethodInfoInternal(new Action(CheatsDebug.EnableLogging), "void EnableLogging()", "enable_logging", "", "", ExecutionPolicy.PlayMode, new CheatParameter[0], "void"),
		new CheatMethodInfoInternal(new Action(CheatsDebug.ClearBugReportPrefs), "void ClearBugReportPrefs()", "reset_bugreport_prefs", "to make a call BugReport`s tutor", "", ExecutionPolicy.All, new CheatParameter[0], "void"),
		new CheatMethodInfoInternal(new Action<string>(CheatsExportCharacter.ExportCharacter), "void ExportCharacter(string preset)", "export_character", "", "", ExecutionPolicy.PlayMode, new CheatParameter[1]
		{
			new CheatParameter
			{
				Name = "preset",
				Type = "System.String",
				HasDefaultValue = false
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action(CheatsFinalArenaPlague.FinalArenaPlagueOff), "void FinalArenaPlagueOff()", "off_finalarenaplagueoff", "Disable final arena plague", "", ExecutionPolicy.All, new CheatParameter[0], "void"),
		new CheatMethodInfoInternal(new Action(CheatsGlobalMap.RevealGlobalMap), "void RevealGlobalMap()", "reveal_map", "", "", ExecutionPolicy.PlayMode, new CheatParameter[0], "void"),
		new CheatMethodInfoInternal(new Action<string>(CheatsGlobalMap.MoveToSystemOnGlobalMap), "void MoveToSystemOnGlobalMap(string systemPointBlueprintName)", "move_to_system", "", "", ExecutionPolicy.PlayMode, new CheatParameter[1]
		{
			new CheatParameter
			{
				Name = "systemPointBlueprintName",
				Type = "System.String",
				HasDefaultValue = false
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action<string, int>(CheatsGlobalMap.LowerPassageDifficulty), "void LowerPassageDifficulty(string systemPointBlueprintName, int difficulty)", "lower_passage_difficulty", "", "", ExecutionPolicy.PlayMode, new CheatParameter[2]
		{
			new CheatParameter
			{
				Name = "systemPointBlueprintName",
				Type = "System.String",
				HasDefaultValue = false
			},
			new CheatParameter
			{
				Name = "difficulty",
				Type = "System.Int32",
				HasDefaultValue = false
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action<string>(CheatsGlobalMap.CreateNewPassage), "void CreateNewPassage(string systemPointBlueprintName)", "create_new_passage", "", "", ExecutionPolicy.PlayMode, new CheatParameter[1]
		{
			new CheatParameter
			{
				Name = "systemPointBlueprintName",
				Type = "System.String",
				HasDefaultValue = false
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action<int>(CheatsGlobalMap.SetScanRadius), "void SetScanRadius(int value)", "set_scan_radius", "", "", ExecutionPolicy.PlayMode, new CheatParameter[1]
		{
			new CheatParameter
			{
				Name = "value",
				Type = "System.Int32",
				HasDefaultValue = false
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action<int>(CheatsGlobalMap.AddNavigatorResource), "void AddNavigatorResource(int value)", "add_navigator_resource", "", "", ExecutionPolicy.PlayMode, new CheatParameter[1]
		{
			new CheatParameter
			{
				Name = "value",
				Type = "System.Int32",
				HasDefaultValue = false
			}
		}, "void"),
		new CheatMethodInfoInternal(new Func<string>(CheatsGraphics.ToggleStochasticSSR), "string ToggleStochasticSSR()", "gl_togglestochasticssr", "Toggle stochastic SSR algorithm in PostProcessing_Global volume", "", ExecutionPolicy.All, new CheatParameter[0], "string"),
		new CheatMethodInfoInternal(new Func<string, string>(CheatsGraphics.DisableKeyword), "string DisableKeyword(string keyword)", "gl_disablekeyword", "Disables shader keyword in all materials in all loaded scenes", "", ExecutionPolicy.All, new CheatParameter[1]
		{
			new CheatParameter
			{
				Name = "keyword",
				Type = "System.String",
				HasDefaultValue = false
			}
		}, "string"),
		new CheatMethodInfoInternal(new Func<string, float, string>(CheatsGraphics.SetMaterialsFloat), "string SetMaterialsFloat(string name = \\\"\\\", float value = -1)", "gl_setmaterialsfloat", "Sets float value in all materials in all loaded scenes", "", ExecutionPolicy.All, new CheatParameter[2]
		{
			new CheatParameter
			{
				Name = "name",
				Type = "System.String",
				HasDefaultValue = true
			},
			new CheatParameter
			{
				Name = "value",
				Type = "System.Single",
				HasDefaultValue = true
			}
		}, "string"),
		new CheatMethodInfoInternal(new Func<string>(CheatsGraphics.ToggleSRPBatching), "string ToggleSRPBatching()", "gl_togglesrpbatching", "", "", ExecutionPolicy.All, new CheatParameter[0], "string"),
		new CheatMethodInfoInternal(new Func<string>(CheatsGraphics.ResetPBD), "string ResetPBD()", "gl_pbdresetmemory", "", "", ExecutionPolicy.All, new CheatParameter[0], "string"),
		new CheatMethodInfoInternal(new Func<string>(CheatsGraphics.TogglePBDEnabled), "string TogglePBDEnabled()", "gl_togglepbdenabled", "", "", ExecutionPolicy.All, new CheatParameter[0], "string"),
		new CheatMethodInfoInternal(new Func<string>(CheatsGraphics.TogglePBDCameraCullingEnabled), "string TogglePBDCameraCullingEnabled()", "gl_togglepbdcameracullingenabled", "", "", ExecutionPolicy.All, new CheatParameter[0], "string"),
		new CheatMethodInfoInternal(new Action(CheatsItems.AoeLoot), "void AoeLoot()", "aoe_loot", "", "", ExecutionPolicy.PlayMode, new CheatParameter[0], "void"),
		new CheatMethodInfoInternal(new Action<string, string, string>(CheatsRE.StartRE), "void StartRE(string areaBlueprintName, string areaEnterPointBlueprintName = null, string enemyGroupBlueprintName = null)", "start_re", "", "", ExecutionPolicy.PlayMode, new CheatParameter[3]
		{
			new CheatParameter
			{
				Name = "areaBlueprintName",
				Type = "System.String",
				HasDefaultValue = false
			},
			new CheatParameter
			{
				Name = "areaEnterPointBlueprintName",
				Type = "System.String",
				HasDefaultValue = true
			},
			new CheatParameter
			{
				Name = "enemyGroupBlueprintName",
				Type = "System.String",
				HasDefaultValue = true
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action(CheatsRE.TurnOffRandomEncounters), "void TurnOffRandomEncounters()", "turn_off_re", "", "", ExecutionPolicy.PlayMode, new CheatParameter[0], "void"),
		new CheatMethodInfoInternal(new Action(CheatsRE.TurnOnRandomEncounters), "void TurnOnRandomEncounters()", "turn_on_re", "", "", ExecutionPolicy.PlayMode, new CheatParameter[0], "void"),
		new CheatMethodInfoInternal(new Action<int>(CheatsRE.SetRandomSeed), "void SetRandomSeed(int seed)", "set_random_seed", "", "", ExecutionPolicy.PlayMode, new CheatParameter[1]
		{
			new CheatParameter
			{
				Name = "seed",
				Type = "System.Int32",
				HasDefaultValue = false
			}
		}, "void"),
		new CheatMethodInfoInternal(new Func<string>(CheatsSaves.ListSaves), "string ListSaves()", "list_saves", "", "", ExecutionPolicy.All, new CheatParameter[0], "string"),
		new CheatMethodInfoInternal(new Action<string>(CheatsSaves.RemoteLoadGame), "void RemoteLoadGame(string path)", "load_remote", "", "", ExecutionPolicy.All, new CheatParameter[1]
		{
			new CheatParameter
			{
				Name = "path",
				Type = "System.String",
				HasDefaultValue = false
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action<string>(CheatsSaves.SaveGame), "void SaveGame(string name)", "save", "", "", ExecutionPolicy.All, new CheatParameter[1]
		{
			new CheatParameter
			{
				Name = "name",
				Type = "System.String",
				HasDefaultValue = false
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action(CheatsSaves.SaveGameAuto), "void SaveGameAuto()", "save_auto", "", "", ExecutionPolicy.All, new CheatParameter[0], "void"),
		new CheatMethodInfoInternal(new Action<string>(CheatsSaves.LoadGame), "void LoadGame(string name)", "load", "", "", ExecutionPolicy.All, new CheatParameter[1]
		{
			new CheatParameter
			{
				Name = "name",
				Type = "System.String",
				HasDefaultValue = false
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action<string>(CheatsSaves.DeleteSaveGame), "void DeleteSaveGame(string name)", "delsave", "", "", ExecutionPolicy.All, new CheatParameter[1]
		{
			new CheatParameter
			{
				Name = "name",
				Type = "System.String",
				HasDefaultValue = false
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action<string>(CheatsSaves.CopySaves), "void CopySaves(string filter)", "copy_saves", "", "", ExecutionPolicy.All, new CheatParameter[1]
		{
			new CheatParameter
			{
				Name = "filter",
				Type = "System.String",
				HasDefaultValue = false
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action<string>(CheatsSaves.ImportSaves), "void ImportSaves(string folder = null)", "import_saves", "", "", ExecutionPolicy.All, new CheatParameter[1]
		{
			new CheatParameter
			{
				Name = "folder",
				Type = "System.String",
				HasDefaultValue = true
			}
		}, "void"),
		new CheatMethodInfoInternal(new Func<int>(CheatsScrap.Scrap), "int Scrap()", "scrap", "Show amount of scrap that player has", "", ExecutionPolicy.PlayMode, new CheatParameter[0], "int"),
		new CheatMethodInfoInternal(new Action<int>(CheatsScrap.Receive), "void Receive(int scrap)", "scrap_receive", "Give scrap to player", "Any positive integer: 20; 32, 265", ExecutionPolicy.PlayMode, new CheatParameter[1]
		{
			new CheatParameter
			{
				Name = "scrap",
				Type = "System.Int32",
				HasDefaultValue = false
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action<int>(CheatsScrap.Spend), "void Spend(int scrap)", "scrap_spend", "Take scrap from player", "Any positive integer: 20; 32, 265", ExecutionPolicy.PlayMode, new CheatParameter[1]
		{
			new CheatParameter
			{
				Name = "scrap",
				Type = "System.Int32",
				HasDefaultValue = false
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action(CheatsScrap.RepairShipFull), "void RepairShipFull()", "scrap_repair_full", "Repair players ship for scrap on max HP", "", ExecutionPolicy.PlayMode, new CheatParameter[0], "void"),
		new CheatMethodInfoInternal(new Action<int>(CheatsScrap.RepairShip), "void RepairShip(int restoreHealth)", "scrap_repair", "Repair players ship for scrap", "Amount of health to restore in integer: 7, 12, 30", ExecutionPolicy.PlayMode, new CheatParameter[1]
		{
			new CheatParameter
			{
				Name = "restoreHealth",
				Type = "System.Int32",
				HasDefaultValue = false
			}
		}, "void"),
		new CheatMethodInfoInternal(new Func<int, string>(CheatsScrap.DisassemblePart), "string DisassemblePart(int index)", "scrap_disassemble_component", "Disassemble component of ship into scrap", "Index in inventory(from 0): 0, 2, 12", ExecutionPolicy.PlayMode, new CheatParameter[1]
		{
			new CheatParameter
			{
				Name = "index",
				Type = "System.Int32",
				HasDefaultValue = false
			}
		}, "string"),
		new CheatMethodInfoInternal(new Func<int, string>(CheatsScrap.AssemblePart), "string AssemblePart(int index)", "scrap_assemble_component", "Tries to assemble component of ship into scrap", "Index in inventory(from 0): 0, 2, 12", ExecutionPolicy.PlayMode, new CheatParameter[1]
		{
			new CheatParameter
			{
				Name = "index",
				Type = "System.Int32",
				HasDefaultValue = false
			}
		}, "string"),
		new CheatMethodInfoInternal(new Action<int>(CheatsTime.SkipHours), "void SkipHours(int hours)", "skip_hours", "", "", ExecutionPolicy.PlayMode, new CheatParameter[1]
		{
			new CheatParameter
			{
				Name = "hours",
				Type = "System.Int32",
				HasDefaultValue = false
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action(CheatsTime.RerollWeather), "void RerollWeather()", "reroll_weather", "Force weather change", "", ExecutionPolicy.PlayMode, new CheatParameter[0], "void"),
		new CheatMethodInfoInternal(new Action(CheatsTime.TimeScaleUp), "void TimeScaleUp()", "time_scale_up", "", "", ExecutionPolicy.PlayMode, new CheatParameter[0], "void"),
		new CheatMethodInfoInternal(new Action(CheatsTime.TimeScaleDown), "void TimeScaleDown()", "time_scale_down", "", "", ExecutionPolicy.PlayMode, new CheatParameter[0], "void"),
		new CheatMethodInfoInternal(new Func<string>(CheatsTime.GetTime), "string GetTime()", "get_time", "", "", ExecutionPolicy.PlayMode, new CheatParameter[0], "string"),
		new CheatMethodInfoInternal(new Action<BlueprintAreaEnterPoint>(CheatsTransfer.Tp2Loc), "void Tp2Loc(BlueprintAreaEnterPoint enterPoint)", "tp2loc", "", "", ExecutionPolicy.PlayMode, new CheatParameter[1]
		{
			new CheatParameter
			{
				Name = "enterPoint",
				Type = "Kingmaker.Blueprints.Area.BlueprintAreaEnterPoint",
				HasDefaultValue = false
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action<string>(CheatsTransfer.ListLocs), "void ListLocs(string nameSubstring)", "list_locs", "", "", ExecutionPolicy.All, new CheatParameter[1]
		{
			new CheatParameter
			{
				Name = "nameSubstring",
				Type = "System.String",
				HasDefaultValue = false
			}
		}, "void"),
		new CheatMethodInfoInternal(new Func<string, string>(CheatsTransfer.EnableLocsAlias), "string EnableLocsAlias(string arg)", "locs_alias", "Enable/disable commands tp2loc_*location*", "True, true, false", ExecutionPolicy.All, new CheatParameter[1]
		{
			new CheatParameter
			{
				Name = "arg",
				Type = "System.String",
				HasDefaultValue = false
			}
		}, "string"),
		new CheatMethodInfoInternal(new Action<Vector3, string>(CheatsTransfer.LocalTeleport), "void LocalTeleport(Vector3 tpPosition, string selectedUnits)", "local_teleport", "", "", ExecutionPolicy.PlayMode, new CheatParameter[2]
		{
			new CheatParameter
			{
				Name = "tpPosition",
				Type = "UnityEngine.Vector3",
				HasDefaultValue = false
			},
			new CheatParameter
			{
				Name = "selectedUnits",
				Type = "System.String",
				HasDefaultValue = false
			}
		}, "void"),
		new CheatMethodInfoInternal(new Func<string>(CheatsTransfer.GetPosition), "string GetPosition()", "get_position", "", "", ExecutionPolicy.PlayMode, new CheatParameter[0], "string"),
		new CheatMethodInfoInternal(new Action(CheatsTransfer.ListGamePresets), "void ListGamePresets()", "list_game_presets", "", "", ExecutionPolicy.All, new CheatParameter[0], "void"),
		new CheatMethodInfoInternal(new Action<BlueprintAreaPreset>(CheatsTransfer.StartNewGame), "void StartNewGame(BlueprintAreaPreset preset)", "new_game", "", "", ExecutionPolicy.PlayMode, new CheatParameter[1]
		{
			new CheatParameter
			{
				Name = "preset",
				Type = "Kingmaker.Blueprints.Area.BlueprintAreaPreset",
				HasDefaultValue = false
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action<string>(CheatsUnlock.CheckFlag), "void CheckFlag(string flag = null)", "check_flag", "", "", ExecutionPolicy.All, new CheatParameter[1]
		{
			new CheatParameter
			{
				Name = "flag",
				Type = "System.String",
				HasDefaultValue = true
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action<BlueprintQuestObjective>(CheatsUnlock.CompleteObjective), "void CompleteObjective(BlueprintQuestObjective targetObjective)", "objective_complete", "", "", ExecutionPolicy.PlayMode, new CheatParameter[1]
		{
			new CheatParameter
			{
				Name = "targetObjective",
				Type = "Kingmaker.Blueprints.Quests.BlueprintQuestObjective",
				HasDefaultValue = false
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action<BlueprintItem, int>(CheatsUnlock.CreateItem), "void CreateItem(BlueprintItem blueprint, int quantity = 1)", "create_item", "", "", ExecutionPolicy.PlayMode, new CheatParameter[2]
		{
			new CheatParameter
			{
				Name = "blueprint",
				Type = "Kingmaker.Blueprints.Items.BlueprintItem",
				HasDefaultValue = false
			},
			new CheatParameter
			{
				Name = "quantity",
				Type = "System.Int32",
				HasDefaultValue = true
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action<BlueprintUnit>(CheatsUnlock.RecruitMainCharacter), "void RecruitMainCharacter(BlueprintUnit unit)", "recruit_main_character", "", "", ExecutionPolicy.PlayMode, new CheatParameter[1]
		{
			new CheatParameter
			{
				Name = "unit",
				Type = "Kingmaker.Blueprints.BlueprintUnit",
				HasDefaultValue = false
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action<BlueprintUnit>(CheatsUnlock.RemoveCompanionFromRoster), "void RemoveCompanionFromRoster(BlueprintUnit unit)", "remove_companion_from_roster", "", "", ExecutionPolicy.PlayMode, new CheatParameter[1]
		{
			new CheatParameter
			{
				Name = "unit",
				Type = "Kingmaker.Blueprints.BlueprintUnit",
				HasDefaultValue = false
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action<BlueprintUnit>(CheatsUnlock.UnrecruitCompanion), "void UnrecruitCompanion(BlueprintUnit unit)", "unrecruit_companion", "", "", ExecutionPolicy.PlayMode, new CheatParameter[1]
		{
			new CheatParameter
			{
				Name = "unit",
				Type = "Kingmaker.Blueprints.BlueprintUnit",
				HasDefaultValue = false
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action(DlcCheats.RefreshAllDLCStatuses), "void RefreshAllDLCStatuses()", "refresh_all_dlc_statuses", "", "", ExecutionPolicy.PlayMode, new CheatParameter[0], "void"),
		new CheatMethodInfoInternal(new Action<BlueprintDlc>(DlcCheats.SetDlcAvailable), "void SetDlcAvailable(BlueprintDlc dlc)", "set_dlc_available", "", "", ExecutionPolicy.PlayMode, new CheatParameter[1]
		{
			new CheatParameter
			{
				Name = "dlc",
				Type = "Kingmaker.DLC.BlueprintDlc",
				HasDefaultValue = false
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action<BlueprintDlc>(DlcCheats.SetDlcUnAvailable), "void SetDlcUnAvailable(BlueprintDlc dlc)", "set_dlc_unavailable", "", "", ExecutionPolicy.PlayMode, new CheatParameter[1]
		{
			new CheatParameter
			{
				Name = "dlc",
				Type = "Kingmaker.DLC.BlueprintDlc",
				HasDefaultValue = false
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action<BlueprintDlc>(DlcCheats.SetDlcEnabled), "void SetDlcEnabled(BlueprintDlc dlc)", "set_dlc_enabled", "", "", ExecutionPolicy.PlayMode, new CheatParameter[1]
		{
			new CheatParameter
			{
				Name = "dlc",
				Type = "Kingmaker.DLC.BlueprintDlc",
				HasDefaultValue = false
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action<BlueprintDlc>(DlcCheats.SetDlcDisabled), "void SetDlcDisabled(BlueprintDlc dlc)", "set_dlc_disabled", "", "", ExecutionPolicy.PlayMode, new CheatParameter[1]
		{
			new CheatParameter
			{
				Name = "dlc",
				Type = "Kingmaker.DLC.BlueprintDlc",
				HasDefaultValue = false
			}
		}, "void"),
		new CheatMethodInfoInternal(new Func<BlueprintDlc, string>(DlcCheats.CheckDlcStatus), "string CheckDlcStatus(BlueprintDlc dlc)", "check_dlc_status", "", "", ExecutionPolicy.PlayMode, new CheatParameter[1]
		{
			new CheatParameter
			{
				Name = "dlc",
				Type = "Kingmaker.DLC.BlueprintDlc",
				HasDefaultValue = false
			}
		}, "string"),
		new CheatMethodInfoInternal(new Action(DlcCheats.CheckAllDlcStatuses), "void CheckAllDlcStatuses()", "check_all_dlc_statuses", "", "", ExecutionPolicy.PlayMode, new CheatParameter[0], "void"),
		new CheatMethodInfoInternal(new Action(DlcCheats.SetAllDlcEnabled), "void SetAllDlcEnabled()", "set_all_dlc_enabled", "", "", ExecutionPolicy.PlayMode, new CheatParameter[0], "void"),
		new CheatMethodInfoInternal(new Action(DlcCheats.SetAllDlcDisabled), "void SetAllDlcDisabled()", "set_all_dlc_disabled", "", "", ExecutionPolicy.PlayMode, new CheatParameter[0], "void"),
		new CheatMethodInfoInternal(new Func<string, object>(StateExplorer.GetObject), "object GetObject(string path)", "game_data", "", "", ExecutionPolicy.All, new CheatParameter[1]
		{
			new CheatParameter
			{
				Name = "path",
				Type = "System.String",
				HasDefaultValue = false
			}
		}, "object"),
		new CheatMethodInfoInternal(new Action<BlueprintDialog>(DialogController.StartDialogCheat), "void StartDialogCheat(BlueprintDialog dialog)", "dialog_force", "", "", ExecutionPolicy.PlayMode, new CheatParameter[1]
		{
			new CheatParameter
			{
				Name = "dialog",
				Type = "Kingmaker.DialogSystem.Blueprints.BlueprintDialog",
				HasDefaultValue = false
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action(InteractionHighlightController.SwitchHighlightCovers), "void SwitchHighlightCovers()", "switch_highlight_covers", "", "", ExecutionPolicy.PlayMode, new CheatParameter[0], "void"),
		new CheatMethodInfoInternal(new Action<bool>(MovePredictionController.SetActive), "void SetActive(bool value)", "net_move", "", "", ExecutionPolicy.PlayMode, new CheatParameter[1]
		{
			new CheatParameter
			{
				Name = "value",
				Type = "System.Boolean",
				HasDefaultValue = false
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action(StateSerializationController.Collect_State), "void Collect_State()", "collect_state", "", "", ExecutionPolicy.All, new CheatParameter[0], "void"),
		new CheatMethodInfoInternal(new Action(StateSerializationController.Save_State), "void Save_State()", "save_state", "", "", ExecutionPolicy.All, new CheatParameter[0], "void"),
		new CheatMethodInfoInternal(new Action(StateSerializationController.Apply_State), "void Apply_State()", "apply_state", "", "", ExecutionPolicy.All, new CheatParameter[0], "void"),
		new CheatMethodInfoInternal(new Action<FactionType>(ReputationHelper.AddReputationToNextLevel), "void AddReputationToNextLevel(FactionType faction)", "add_reputation_to_next_level", "", "", ExecutionPolicy.PlayMode, new CheatParameter[1]
		{
			new CheatParameter
			{
				Name = "faction",
				Type = "Kingmaker.Enums.FactionType",
				HasDefaultValue = false
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action(ReputationHelper.UnlockAllVendors), "void UnlockAllVendors()", "unlock_all_vendors", "", "", ExecutionPolicy.PlayMode, new CheatParameter[0], "void"),
		new CheatMethodInfoInternal(new Action<FactionType>(ReputationHelper.UnlockVendor), "void UnlockVendor(FactionType faction)", "unlock_vendor", "", "", ExecutionPolicy.PlayMode, new CheatParameter[1]
		{
			new CheatParameter
			{
				Name = "faction",
				Type = "Kingmaker.Enums.FactionType",
				HasDefaultValue = false
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action(SlowMoController.Debug_Dodge), "void Debug_Dodge()", "debug_dodge", "", "", ExecutionPolicy.All, new CheatParameter[0], "void"),
		new CheatMethodInfoInternal(new Action<int>(TimeSpeedController.ForceSpeedMode), "void ForceSpeedMode(int speedMode = -1)", "net_speed_mode", "", "", ExecutionPolicy.PlayMode, new CheatParameter[1]
		{
			new CheatParameter
			{
				Name = "speedMode",
				Type = "System.Int32",
				HasDefaultValue = true
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action<bool>(TimeSpeedController.AutoSpeed.AutoSpeedCheat), "void AutoSpeedCheat(bool auto = false)", "net_auto_speed", "", "", ExecutionPolicy.PlayMode, new CheatParameter[1]
		{
			new CheatParameter
			{
				Name = "auto",
				Type = "System.Boolean",
				HasDefaultValue = true
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action<float>(TimeSpeedController.AutoSpeed.SlowCheat), "void SlowCheat(float value = 0.99)", "net_speed_slow", "", "", ExecutionPolicy.PlayMode, new CheatParameter[1]
		{
			new CheatParameter
			{
				Name = "value",
				Type = "System.Single",
				HasDefaultValue = true
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action(TurnController.TryEndPlayerTurnStatic), "void TryEndPlayerTurnStatic()", "end_turn", "", "", ExecutionPolicy.All, new CheatParameter[0], "void"),
		new CheatMethodInfoInternal(new Action<int, BaseUnitEntity>(MomentumController.ChangeMomentumBy), "void ChangeMomentumBy(int delta = 100, BaseUnitEntity unit = null)", "change_momentum_by", "", "", ExecutionPolicy.PlayMode, new CheatParameter[2]
		{
			new CheatParameter
			{
				Name = "delta",
				Type = "System.Int32",
				HasDefaultValue = true
			},
			new CheatParameter
			{
				Name = "unit",
				Type = "Kingmaker.EntitySystem.Entities.BaseUnitEntity",
				HasDefaultValue = true
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action(UnitForceMoveController.Debug_Force_Move), "void Debug_Force_Move()", "debug_force_move", "", "", ExecutionPolicy.All, new CheatParameter[0], "void"),
		new CheatMethodInfoInternal(new Action<BlueprintBarkBanterList>(ListBarkBanterEvaluator.Debug_Show_Banter_List), "void Debug_Show_Banter_List(BlueprintBarkBanterList list)", "debug_show_banter_list", "", "", ExecutionPolicy.All, new CheatParameter[1]
		{
			new CheatParameter
			{
				Name = "list",
				Type = "Kingmaker.BarkBanters.BlueprintBarkBanterList",
				HasDefaultValue = false
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action<int>(VeilThicknessCounter.DecreaseVeil), "void DecreaseVeil(int value = 1)", "veil_remove", "Decreases veil thickness by specified value (default = 1)", "", ExecutionPolicy.All, new CheatParameter[1]
		{
			new CheatParameter
			{
				Name = "value",
				Type = "System.Int32",
				HasDefaultValue = true
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action<int>(VeilThicknessCounter.IncreaseVeil), "void IncreaseVeil(int value = 1)", "veil_add", "Increases veil thickness by specified value (default = 1)", "", ExecutionPolicy.All, new CheatParameter[1]
		{
			new CheatParameter
			{
				Name = "value",
				Type = "System.Int32",
				HasDefaultValue = true
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action(VeilThicknessCounter.ClearVeil), "void ClearVeil()", "veil_clear", "Clears all veil thickness", "", ExecutionPolicy.All, new CheatParameter[0], "void"),
		new CheatMethodInfoInternal(new Action(PartyAutoFormationHelper.UpdateAutoFormation), "void UpdateAutoFormation()", "update_auto_formation", "", "", ExecutionPolicy.PlayMode, new CheatParameter[0], "void"),
		new CheatMethodInfoInternal(new Action<long>(GameRamInsufficiencyDetector.DebugRamInsufficiency), "void DebugRamInsufficiency(long sizeInMbs)", "debug_ram_insufficiency", "", "", ExecutionPolicy.All, new CheatParameter[1]
		{
			new CheatParameter
			{
				Name = "sizeInMbs",
				Type = "System.Int64",
				HasDefaultValue = false
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action(CheatNetManager.CheatNetInit), "void CheatNetInit()", "net_init", "", "", ExecutionPolicy.PlayMode, new CheatParameter[0], "void"),
		new CheatMethodInfoInternal(new Action(CheatNetManager.CheatNetHash), "void CheatNetHash()", "net_hash", "", "", ExecutionPolicy.PlayMode, new CheatParameter[0], "void"),
		new CheatMethodInfoInternal(new Action<string>(CheatNetManager.CheatJoinNet), "void CheatJoinNet(string roomName = null)", "net_join", "", "", ExecutionPolicy.PlayMode, new CheatParameter[1]
		{
			new CheatParameter
			{
				Name = "roomName",
				Type = "System.String",
				HasDefaultValue = true
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action(CheatNetManager.CheatStopNet), "void CheatStopNet()", "net_stop", "", "", ExecutionPolicy.PlayMode, new CheatParameter[0], "void"),
		new CheatMethodInfoInternal(new Action(CheatNetManager.CheatPrintPlayers), "void CheatPrintPlayers()", "net_players", "", "", ExecutionPolicy.PlayMode, new CheatParameter[0], "void"),
		new CheatMethodInfoInternal(new Action(CheatNetManager.CheatPrintRegions), "void CheatPrintRegions()", "net_regions", "", "", ExecutionPolicy.PlayMode, new CheatParameter[0], "void"),
		new CheatMethodInfoInternal(new Action<string>(CheatNetManager.CheatSetRegion), "void CheatSetRegion(string region)", "net_set_region", "", "", ExecutionPolicy.PlayMode, new CheatParameter[1]
		{
			new CheatParameter
			{
				Name = "region",
				Type = "System.String",
				HasDefaultValue = false
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action(CheatNetManager.CheatInvite), "void CheatInvite()", "net_invite", "", "", ExecutionPolicy.PlayMode, new CheatParameter[0], "void"),
		new CheatMethodInfoInternal(new Action<int>(CheatNetManager.CheatDesync), "void CheatDesync(int playerIndex = -1)", "net_desync", "", "", ExecutionPolicy.PlayMode, new CheatParameter[1]
		{
			new CheatParameter
			{
				Name = "playerIndex",
				Type = "System.Int32",
				HasDefaultValue = true
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action(CheatNetManager.CheatState), "void CheatState()", "net_state", "", "", ExecutionPolicy.PlayMode, new CheatParameter[0], "void"),
		new CheatMethodInfoInternal(new Action<int>(CheatNetManager.CheatThreadSleep), "void CheatThreadSleep(int timeMs = 1)", "thread_sleep", "", "", ExecutionPolicy.PlayMode, new CheatParameter[1]
		{
			new CheatParameter
			{
				Name = "timeMs",
				Type = "System.Int32",
				HasDefaultValue = true
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action(CheatNetManager.CheatCheckMath), "void CheatCheckMath()", "net_check_math", "", "", ExecutionPolicy.All, new CheatParameter[0], "void"),
		new CheatMethodInfoInternal(new Action<bool>(CheatNetManager.CheatSetRoomOpen), "void CheatSetRoomOpen(bool isOpen = true)", "net_set_open", "", "", ExecutionPolicy.PlayMode, new CheatParameter[1]
		{
			new CheatParameter
			{
				Name = "isOpen",
				Type = "System.Boolean",
				HasDefaultValue = true
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action<bool>(CheatNetManager.CheatIsRoomOpen), "void CheatIsRoomOpen(bool isOpen = true)", "net_is_open", "", "", ExecutionPolicy.PlayMode, new CheatParameter[1]
		{
			new CheatParameter
			{
				Name = "isOpen",
				Type = "System.Boolean",
				HasDefaultValue = true
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action(CheatNetManager.CheatPath), "void CheatPath()", "net_path", "", "", ExecutionPolicy.PlayMode, new CheatParameter[0], "void"),
		new CheatMethodInfoInternal(new Action<string>(CheatNetManager.CheatRunFsmTrigger), "void CheatRunFsmTrigger(string triggerName)", "net_fsm", "", "", ExecutionPolicy.PlayMode, new CheatParameter[1]
		{
			new CheatParameter
			{
				Name = "triggerName",
				Type = "System.String",
				HasDefaultValue = false
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action<int>(CheatNetManager.CheatTestLoadingProcessCommandsLogic), "void CheatTestLoadingProcessCommandsLogic(int count = 100)", "net_test_cmd", "", "", ExecutionPolicy.PlayMode, new CheatParameter[1]
		{
			new CheatParameter
			{
				Name = "count",
				Type = "System.Int32",
				HasDefaultValue = true
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action(CheatNetManager.CheatSendAvatarAlwaysViaPhoton), "void CheatSendAvatarAlwaysViaPhoton()", "net_avatar", "", "", ExecutionPolicy.PlayMode, new CheatParameter[0], "void"),
		new CheatMethodInfoInternal(new Action(CheatNetManager.CheatClearAvatarCache), "void CheatClearAvatarCache()", "net_av_clear", "", "", ExecutionPolicy.PlayMode, new CheatParameter[0], "void"),
		new CheatMethodInfoInternal(new Action(CheatNetManager.CheatAllowRunWithOnePlayer), "void CheatAllowRunWithOnePlayer()", "net_allow_one", "", "", ExecutionPolicy.PlayMode, new CheatParameter[0], "void"),
		new CheatMethodInfoInternal(new Action<int>(CheatNetManager.CheatSetMaxPacketSize), "void CheatSetMaxPacketSize(int packetSizeKb)", "net_packet", "", "", ExecutionPolicy.PlayMode, new CheatParameter[1]
		{
			new CheatParameter
			{
				Name = "packetSizeKb",
				Type = "System.Int32",
				HasDefaultValue = false
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action<int>(CheatNetManager.CheatSetStreamsCount), "void CheatSetStreamsCount(int cnt)", "net_streams", "", "", ExecutionPolicy.PlayMode, new CheatParameter[1]
		{
			new CheatParameter
			{
				Name = "cnt",
				Type = "System.Int32",
				HasDefaultValue = false
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action<bool>(CheatNetManager.CheatSlow), "void CheatSlow(bool activate = true)", "net_slow", "", "", ExecutionPolicy.PlayMode, new CheatParameter[1]
		{
			new CheatParameter
			{
				Name = "activate",
				Type = "System.Boolean",
				HasDefaultValue = true
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action(CheatNetManager.CheatPortraitManagerClearGuidMapping), "void CheatPortraitManagerClearGuidMapping()", "net_clear_portraits", "", "", ExecutionPolicy.PlayMode, new CheatParameter[0], "void"),
		new CheatMethodInfoInternal(new Action(CheatNetManager.CheatPauseDataSending), "void CheatPauseDataSending()", "net_pause_send", "", "", ExecutionPolicy.PlayMode, new CheatParameter[0], "void"),
		new CheatMethodInfoInternal(new Action<bool, bool>(CheatNetManager.CheatNetSimulationSetIncomingLag), "void CheatNetSimulationSetIncomingLag(bool enabled, bool reset = false)", "net_sim", "", "", ExecutionPolicy.PlayMode, new CheatParameter[2]
		{
			new CheatParameter
			{
				Name = "enabled",
				Type = "System.Boolean",
				HasDefaultValue = false
			},
			new CheatParameter
			{
				Name = "reset",
				Type = "System.Boolean",
				HasDefaultValue = true
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action<int>(CheatNetManager.CheatNetSimulationSetLag), "void CheatNetSimulationSetLag(int lag)", "net_sim_lag", "", "", ExecutionPolicy.PlayMode, new CheatParameter[1]
		{
			new CheatParameter
			{
				Name = "lag",
				Type = "System.Int32",
				HasDefaultValue = false
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action<int>(CheatNetManager.CheatNetSimulationSetIncomingLag), "void CheatNetSimulationSetIncomingLag(int lag)", "net_sim_inlag", "", "", ExecutionPolicy.PlayMode, new CheatParameter[1]
		{
			new CheatParameter
			{
				Name = "lag",
				Type = "System.Int32",
				HasDefaultValue = false
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action<int>(CheatNetManager.CheatNetSimulationSetOutgoingLag), "void CheatNetSimulationSetOutgoingLag(int lag)", "net_sim_outlag", "", "", ExecutionPolicy.PlayMode, new CheatParameter[1]
		{
			new CheatParameter
			{
				Name = "lag",
				Type = "System.Int32",
				HasDefaultValue = false
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action<int>(CheatNetManager.CheatNetSimulationSetJitter), "void CheatNetSimulationSetJitter(int jit)", "net_sim_jit", "", "", ExecutionPolicy.PlayMode, new CheatParameter[1]
		{
			new CheatParameter
			{
				Name = "jit",
				Type = "System.Int32",
				HasDefaultValue = false
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action<int>(CheatNetManager.CheatNetSimulationSetIncomingJitter), "void CheatNetSimulationSetIncomingJitter(int jit)", "net_sim_injit", "", "", ExecutionPolicy.PlayMode, new CheatParameter[1]
		{
			new CheatParameter
			{
				Name = "jit",
				Type = "System.Int32",
				HasDefaultValue = false
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action<int>(CheatNetManager.CheatNetSimulationSetOutgoingJitter), "void CheatNetSimulationSetOutgoingJitter(int jit)", "net_sim_outjit", "", "", ExecutionPolicy.PlayMode, new CheatParameter[1]
		{
			new CheatParameter
			{
				Name = "jit",
				Type = "System.Int32",
				HasDefaultValue = false
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action<int>(CheatNetManager.CheatNetSimulationSetLossPercentage), "void CheatNetSimulationSetLossPercentage(int loss)", "net_sim_loss", "", "", ExecutionPolicy.PlayMode, new CheatParameter[1]
		{
			new CheatParameter
			{
				Name = "loss",
				Type = "System.Int32",
				HasDefaultValue = false
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action<int>(CheatNetManager.CheatNetSimulationSetIncomingLossPercentage), "void CheatNetSimulationSetIncomingLossPercentage(int loss)", "net_sim_inloss", "", "", ExecutionPolicy.PlayMode, new CheatParameter[1]
		{
			new CheatParameter
			{
				Name = "loss",
				Type = "System.Int32",
				HasDefaultValue = false
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action<int>(CheatNetManager.CheatNetSimulationSetOutgoingLossPercentage), "void CheatNetSimulationSetOutgoingLossPercentage(int loss)", "net_sim_outloss", "", "", ExecutionPolicy.PlayMode, new CheatParameter[1]
		{
			new CheatParameter
			{
				Name = "loss",
				Type = "System.Int32",
				HasDefaultValue = false
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action<string, int>(Kingmaker.Networking.OwlcatAnalyticsExtensions.SendCoopStart), "void SendCoopStart(string sessionId, int numberOfPlayers)", "send_coop_start", "", "", ExecutionPolicy.All, new CheatParameter[2]
		{
			new CheatParameter
			{
				Name = "sessionId",
				Type = "System.String",
				HasDefaultValue = false
			},
			new CheatParameter
			{
				Name = "numberOfPlayers",
				Type = "System.Int32",
				HasDefaultValue = false
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action<string, int>(Kingmaker.Networking.OwlcatAnalyticsExtensions.SendCoopEnd), "void SendCoopEnd(string sessionId, int numberOfPlayers)", "send_coop_end", "", "", ExecutionPolicy.All, new CheatParameter[2]
		{
			new CheatParameter
			{
				Name = "sessionId",
				Type = "System.String",
				HasDefaultValue = false
			},
			new CheatParameter
			{
				Name = "numberOfPlayers",
				Type = "System.Int32",
				HasDefaultValue = false
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action<int>(PhotonManager.MaxPlayersCheat), "void MaxPlayersCheat(int value = 6)", "net_max_players", "", "", ExecutionPolicy.PlayMode, new CheatParameter[1]
		{
			new CheatParameter
			{
				Name = "value",
				Type = "System.Int32",
				HasDefaultValue = true
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action(SyncNetManager.ForceDefaultDesyncDetectionStrategy), "void ForceDefaultDesyncDetectionStrategy()", "net_desync_default", "", "", ExecutionPolicy.PlayMode, new CheatParameter[0], "void"),
		new CheatMethodInfoInternal(new Action<string>(Kingmaker.OwlcatAnalyticsExtensions.SendFatalError), "void SendFatalError(string message)", "send_fatal_error", "", "", ExecutionPolicy.All, new CheatParameter[1]
		{
			new CheatParameter
			{
				Name = "message",
				Type = "System.String",
				HasDefaultValue = false
			}
		}, "void"),
		new CheatMethodInfoInternal(new Func<int, int, Task>(OwlcatAnalyticsDebug.DebugCreateEvents), "Task DebugCreateEvents(int count, int interval)", "debug_analytics_create_events", "Создать кучу эвентов в аналитику", "", ExecutionPolicy.PlayMode, new CheatParameter[2]
		{
			new CheatParameter
			{
				Name = "count",
				Type = "System.Int32",
				HasDefaultValue = false
			},
			new CheatParameter
			{
				Name = "interval",
				Type = "System.Int32",
				HasDefaultValue = false
			}
		}, "Task"),
		new CheatMethodInfoInternal(new Func<InclemencyType, InclemencyType>(ArbiterClientIntegration.SetWeather), "InclemencyType SetWeather(InclemencyType type = Clear)", "weather_set", "Сменить погоду", "", ExecutionPolicy.All, new CheatParameter[1]
		{
			new CheatParameter
			{
				Name = "type",
				Type = "Owlcat.Runtime.Visual.Effects.WeatherSystem.InclemencyType",
				HasDefaultValue = true
			}
		}, "InclemencyType"),
		new CheatMethodInfoInternal(new Action(ArbiterClientIntegration.DisableClouds), "void DisableClouds()", "clouds_disable", "Отключить облака", "", ExecutionPolicy.All, new CheatParameter[0], "void"),
		new CheatMethodInfoInternal(new Action(ArbiterClientIntegration.EnableClouds), "void EnableClouds()", "clouds_enable", "Включить облака", "", ExecutionPolicy.All, new CheatParameter[0], "void"),
		new CheatMethodInfoInternal(new Action<bool>(ArbiterClientIntegration.DisableFog), "void DisableFog(bool disable = true)", "fog_disable", "Отключить туманных объемы", "", ExecutionPolicy.All, new CheatParameter[1]
		{
			new CheatParameter
			{
				Name = "disable",
				Type = "System.Boolean",
				HasDefaultValue = true
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action<bool>(ArbiterClientIntegration.EnableFog), "void EnableFog(bool disable = true)", "fog_enable", "Включить туманных объемы", "", ExecutionPolicy.All, new CheatParameter[1]
		{
			new CheatParameter
			{
				Name = "disable",
				Type = "System.Boolean",
				HasDefaultValue = true
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action<bool>(ArbiterClientIntegration.DisableWind), "void DisableWind(bool disable = true)", "wind_disable", "Отключить ветер", "", ExecutionPolicy.All, new CheatParameter[1]
		{
			new CheatParameter
			{
				Name = "disable",
				Type = "System.Boolean",
				HasDefaultValue = true
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action<bool>(ArbiterClientIntegration.EnableWind), "void EnableWind(bool disable = true)", "wind_enable", "Включить ветер", "", ExecutionPolicy.All, new CheatParameter[1]
		{
			new CheatParameter
			{
				Name = "disable",
				Type = "System.Boolean",
				HasDefaultValue = true
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action(ArbiterClientIntegration.DisableFow), "void DisableFow()", "fow_disable", "Отключить туман войны", "", ExecutionPolicy.All, new CheatParameter[0], "void"),
		new CheatMethodInfoInternal(new Action(ArbiterClientIntegration.EnableFow), "void EnableFow()", "fow_enable", "Включить туман войны", "", ExecutionPolicy.All, new CheatParameter[0], "void"),
		new CheatMethodInfoInternal(new Func<int, int>(ArbiterClientIntegration.SetVSync), "int SetVSync(int value)", "vsync_set", "Установить количество v-синков между кадрами", "", ExecutionPolicy.All, new CheatParameter[1]
		{
			new CheatParameter
			{
				Name = "value",
				Type = "System.Int32",
				HasDefaultValue = false
			}
		}, "int"),
		new CheatMethodInfoInternal(new Action(ArbiterClientIntegration.DisableFx), "void DisableFx()", "fx_disable", "Отключить fx", "", ExecutionPolicy.All, new CheatParameter[0], "void"),
		new CheatMethodInfoInternal(new Action(ArbiterClientIntegration.EnableFx), "void EnableFx()", "fx_enable", "Включить fx", "", ExecutionPolicy.All, new CheatParameter[0], "void"),
		new CheatMethodInfoInternal(new Action(ArbiterClientIntegration.HideUnits), "void HideUnits()", "units_hide", "Скрыть юниты", "", ExecutionPolicy.All, new CheatParameter[0], "void"),
		new CheatMethodInfoInternal(new Action(ArbiterClientIntegration.ShowUnits), "void ShowUnits()", "units_show", "Показать юниты", "", ExecutionPolicy.All, new CheatParameter[0], "void"),
		new CheatMethodInfoInternal(new Action(ArbiterClientIntegration.HideUi), "void HideUi()", "ui_hide", "Скрыть UI", "", ExecutionPolicy.All, new CheatParameter[0], "void"),
		new CheatMethodInfoInternal(new Action(ArbiterClientIntegration.ShowUi), "void ShowUi()", "ui_show", "Показать UI", "", ExecutionPolicy.All, new CheatParameter[0], "void"),
		new CheatMethodInfoInternal(new Action<string>(OwlcatProtocol.OwlcatProtocolHandler), "void OwlcatProtocolHandler(string message)", "owlcat_protocol", "", "", ExecutionPolicy.All, new CheatParameter[1]
		{
			new CheatParameter
			{
				Name = "message",
				Type = "System.String",
				HasDefaultValue = false
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action<int, int, int>(StackTraceSpamDetector.StackTraceSpamDetectionEnable), "void StackTraceSpamDetectionEnable(int frameSize, int count = 5, int cooldown = 30000)", "spam_stacktrace_detection", "", "", ExecutionPolicy.All, new CheatParameter[3]
		{
			new CheatParameter
			{
				Name = "frameSize",
				Type = "System.Int32",
				HasDefaultValue = false
			},
			new CheatParameter
			{
				Name = "count",
				Type = "System.Int32",
				HasDefaultValue = true
			},
			new CheatParameter
			{
				Name = "cooldown",
				Type = "System.Int32",
				HasDefaultValue = true
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action<int>(StackTraceSpamDetector.StackTraceSpamDetectionEnable), "void StackTraceSpamDetectionEnable(int cooldownMs = 30000)", "debug_spam_cooldown", "", "", ExecutionPolicy.All, new CheatParameter[1]
		{
			new CheatParameter
			{
				Name = "cooldownMs",
				Type = "System.Int32",
				HasDefaultValue = true
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action<int, int>(StackTraceSpamDetector.SpamDetectionConfigFast), "void SpamDetectionConfigFast(int count, int intervalMs)", "debug_spam_detection_fast", "", "", ExecutionPolicy.All, new CheatParameter[2]
		{
			new CheatParameter
			{
				Name = "count",
				Type = "System.Int32",
				HasDefaultValue = false
			},
			new CheatParameter
			{
				Name = "intervalMs",
				Type = "System.Int32",
				HasDefaultValue = false
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action<int, int, int>(StackTraceSpamDetector.SpamDetectionConfigLongTermFast), "void SpamDetectionConfigLongTermFast(int count, int intervalMs, int times)", "debug_spam_detection_longterm_fast", "", "", ExecutionPolicy.All, new CheatParameter[3]
		{
			new CheatParameter
			{
				Name = "count",
				Type = "System.Int32",
				HasDefaultValue = false
			},
			new CheatParameter
			{
				Name = "intervalMs",
				Type = "System.Int32",
				HasDefaultValue = false
			},
			new CheatParameter
			{
				Name = "times",
				Type = "System.Int32",
				HasDefaultValue = false
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action<int, int, int>(StackTraceSpamDetector.SpamDetectionConfigLongTermNormal), "void SpamDetectionConfigLongTermNormal(int count, int intervalMs, int times)", "debug_spam_detection_longterm_normal", "", "", ExecutionPolicy.All, new CheatParameter[3]
		{
			new CheatParameter
			{
				Name = "count",
				Type = "System.Int32",
				HasDefaultValue = false
			},
			new CheatParameter
			{
				Name = "intervalMs",
				Type = "System.Int32",
				HasDefaultValue = false
			},
			new CheatParameter
			{
				Name = "times",
				Type = "System.Int32",
				HasDefaultValue = false
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action<int, int>(StackTraceSpamDetector.SpamDetectionConfigLongTermNormal), "void SpamDetectionConfigLongTermNormal(int count, int intervalMs)", "debug_spam_detection_normal", "", "", ExecutionPolicy.All, new CheatParameter[2]
		{
			new CheatParameter
			{
				Name = "count",
				Type = "System.Int32",
				HasDefaultValue = false
			},
			new CheatParameter
			{
				Name = "intervalMs",
				Type = "System.Int32",
				HasDefaultValue = false
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action<int>(Replay.SetStateSkipFrames), "void SetStateSkipFrames(int n)", "replay_skip", "", "", ExecutionPolicy.All, new CheatParameter[1]
		{
			new CheatParameter
			{
				Name = "n",
				Type = "System.Int32",
				HasDefaultValue = false
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action(Replay.GetStateSkipFrames), "void GetStateSkipFrames()", "replay_skip_get", "", "", ExecutionPolicy.All, new CheatParameter[0], "void"),
		new CheatMethodInfoInternal(new Func<string, string, string>(Replay.CreateReplayStart), "string CreateReplayStart(string replayName, string saveName = null)", "replay_create_start", "", "", ExecutionPolicy.PlayMode, new CheatParameter[2]
		{
			new CheatParameter
			{
				Name = "replayName",
				Type = "System.String",
				HasDefaultValue = false
			},
			new CheatParameter
			{
				Name = "saveName",
				Type = "System.String",
				HasDefaultValue = true
			}
		}, "string"),
		new CheatMethodInfoInternal(new Action(Replay.CreateReplayCancel), "void CreateReplayCancel()", "replay_create_cancel", "", "", ExecutionPolicy.PlayMode, new CheatParameter[0], "void"),
		new CheatMethodInfoInternal(new Action(Replay.CreateReplayStop), "void CreateReplayStop()", "replay_create_stop", "", "", ExecutionPolicy.PlayMode, new CheatParameter[0], "void"),
		new CheatMethodInfoInternal(new Action<string, Action, bool>(Replay.PlayReplay), "void PlayReplay(string replayName, Action callbackAfterStart = null, bool popupOnEnd = false)", "replay_play", "", "", ExecutionPolicy.PlayMode, new CheatParameter[3]
		{
			new CheatParameter
			{
				Name = "replayName",
				Type = "System.String",
				HasDefaultValue = false
			},
			new CheatParameter
			{
				Name = "callbackAfterStart",
				Type = "System.Action",
				HasDefaultValue = true
			},
			new CheatParameter
			{
				Name = "popupOnEnd",
				Type = "System.Boolean",
				HasDefaultValue = true
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action(Replay.CancelReplay), "void CancelReplay()", "replay_cancel", "", "", ExecutionPolicy.PlayMode, new CheatParameter[0], "void"),
		new CheatMethodInfoInternal(new Action(Replay.NeedToSaveState.TurnOn), "void TurnOn()", "replay_log_on", "", "", ExecutionPolicy.All, new CheatParameter[0], "void"),
		new CheatMethodInfoInternal(new Action(Replay.NeedToSaveState.TurnOff), "void TurnOff()", "replay_log_off", "", "", ExecutionPolicy.All, new CheatParameter[0], "void"),
		new CheatMethodInfoInternal(new Action(TexturesQualityController.EnableTextureQualityLoweringToReduceMemoryUsage), "void EnableTextureQualityLoweringToReduceMemoryUsage()", "enable_texture_quality_lowering_to_reduce_memory_usage", "", "", ExecutionPolicy.All, new CheatParameter[0], "void"),
		new CheatMethodInfoInternal(new Action(TexturesQualityController.DisableTextureQualityLoweringToReduceMemoryUsage), "void DisableTextureQualityLoweringToReduceMemoryUsage()", "disable_texture_quality_lowering_to_reduce_memory_usage", "", "", ExecutionPolicy.All, new CheatParameter[0], "void"),
		new CheatMethodInfoInternal(new Action<int, string>(TexturesQualityController.TexturesMipmapLevelController.CheatSetMipMapLevel), "void CheatSetMipMapLevel(int level, string groupName = null)", "set_mipmap_level", "", "", ExecutionPolicy.PlayMode, new CheatParameter[2]
		{
			new CheatParameter
			{
				Name = "level",
				Type = "System.Int32",
				HasDefaultValue = false
			},
			new CheatParameter
			{
				Name = "groupName",
				Type = "System.String",
				HasDefaultValue = true
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action(SettingsRoot.DeletePlayerPrefs), "void DeletePlayerPrefs()", "clear_prefs", "", "", ExecutionPolicy.All, new CheatParameter[0], "void"),
		new CheatMethodInfoInternal(new Action<string>(PartStarshipNavigation.SetSpeedMode), "void SetSpeedMode(string cond)", "set_starship_speed_mode", "", "", ExecutionPolicy.All, new CheatParameter[1]
		{
			new CheatParameter
			{
				Name = "cond",
				Type = "System.String",
				HasDefaultValue = false
			}
		}, "void"),
		new CheatMethodInfoInternal(new Func<string>(PartStarshipProgression.TryToFixShipProgression), "string TryToFixShipProgression()", "try_to_fix_ship_progression", "", "", ExecutionPolicy.All, new CheatParameter[0], "string"),
		new CheatMethodInfoInternal(new Func<string, string>(TutorialCheats.ShowTutorial), "string ShowTutorial(string blueprint)", "tutorial_start", "", "", ExecutionPolicy.All, new CheatParameter[1]
		{
			new CheatParameter
			{
				Name = "blueprint",
				Type = "System.String",
				HasDefaultValue = false
			}
		}, "string"),
		new CheatMethodInfoInternal(new Action(TutorialSystem.UnBanAll), "void UnBanAll()", "tutorial_unban", "", "", ExecutionPolicy.All, new CheatParameter[0], "void"),
		new CheatMethodInfoInternal(new Action(TwitchDropsTest.TestLinked), "void TestLinked()", "twitch_check_linked", "", "", ExecutionPolicy.PlayMode, new CheatParameter[0], "void"),
		new CheatMethodInfoInternal(new Action(TwitchDropsTest.TestGetDrops), "void TestGetDrops()", "twitch_get_drops", "", "", ExecutionPolicy.PlayMode, new CheatParameter[0], "void"),
		new CheatMethodInfoInternal(new Action(TwitchDropsTest.TestOpenLinkpage), "void TestOpenLinkpage()", "twitch_open_link_page", "", "", ExecutionPolicy.PlayMode, new CheatParameter[0], "void"),
		new CheatMethodInfoInternal(new Action<string>(TwitchDropsTest.SetFakeGameUid), "void SetFakeGameUid(string fakeUid = null)", "twitch_set_fake_uid", "", "", ExecutionPolicy.PlayMode, new CheatParameter[1]
		{
			new CheatParameter
			{
				Name = "fakeUid",
				Type = "System.String",
				HasDefaultValue = true
			}
		}, "void"),
		new CheatMethodInfoInternal(new Func<bool>(InputLog.LogInput), "bool LogInput()", "log_key_input", "", "", ExecutionPolicy.PlayMode, new CheatParameter[0], "bool"),
		new CheatMethodInfoInternal(new Action<float>(InputLog.LogCurrentInput), "void LogCurrentInput(float delay = 5)", "log_current_input_state", "", "", ExecutionPolicy.PlayMode, new CheatParameter[1]
		{
			new CheatParameter
			{
				Name = "delay",
				Type = "System.Single",
				HasDefaultValue = true
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action(CommonVM.ClearCanSwitchDlcAfterPurchasePrefs), "void ClearCanSwitchDlcAfterPurchasePrefs()", "clear_can_switch_dlc_after_purchase", "", "", ExecutionPolicy.All, new CheatParameter[0], "void"),
		new CheatMethodInfoInternal(new Action(CommonVM.SetCanSwitchDlcAfterPurchasePrefs), "void SetCanSwitchDlcAfterPurchasePrefs()", "set_can_switch_dlc_after_purchase", "", "", ExecutionPolicy.All, new CheatParameter[0], "void"),
		new CheatMethodInfoInternal(new Action(FirstLaunchSettingsVM.ClearFirstLaunchPrefs), "void ClearFirstLaunchPrefs()", "clear_first_launch", "", "", ExecutionPolicy.All, new CheatParameter[0], "void"),
		new CheatMethodInfoInternal(new Action(FirstLaunchSettingsVM.SetFirstLaunchPrefs), "void SetFirstLaunchPrefs()", "set_first_launch", "", "", ExecutionPolicy.All, new CheatParameter[0], "void"),
		new CheatMethodInfoInternal(new Action(NetLobbyVM.ClearFirstLaunchPrefs), "void ClearFirstLaunchPrefs()", "clear_net_lobby_tutorial", "", "", ExecutionPolicy.All, new CheatParameter[0], "void"),
		new CheatMethodInfoInternal(new Action(NetLobbyVM.SetFirstLaunchPrefs), "void SetFirstLaunchPrefs()", "set_net_lobby_tutorial", "", "", ExecutionPolicy.All, new CheatParameter[0], "void"),
		new CheatMethodInfoInternal(new Action(UnitGroupAttackFactionsValidator.Validate), "void Validate()", "validate_unit_groups", "", "", ExecutionPolicy.All, new CheatParameter[0], "void"),
		new CheatMethodInfoInternal(new Action(UnitHelper.CheatRespecUnit), "void CheatRespecUnit()", "respec", "Respec selected unit", "", ExecutionPolicy.PlayMode, new CheatParameter[0], "void"),
		new CheatMethodInfoInternal(new Action(MemoryUsageHelper.DumpMemoryStats), "void DumpMemoryStats()", "memory_stats_dump", "", "", ExecutionPolicy.All, new CheatParameter[0], "void"),
		new CheatMethodInfoInternal(new Action(Kingmaker.Utility.OwlcatAnalyticsExtensions.SendFatalError), "void SendFatalError()", "send_crash", "", "", ExecutionPolicy.All, new CheatParameter[0], "void"),
		new CheatMethodInfoInternal(new Action<ReportSendingMode>(ReportingUtilsCheats.ResendReports), "void ResendReports(ReportSendingMode mode)", "resend_reports", "", "", ExecutionPolicy.All, new CheatParameter[1]
		{
			new CheatParameter
			{
				Name = "mode",
				Type = "Kingmaker.Utility.ReportSendingMode",
				HasDefaultValue = false
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action<string, string>(Screenshot.Capture), "void Capture(string path = null, string name = null)", "screenshot", "capture screenshot", "", ExecutionPolicy.All, new CheatParameter[2]
		{
			new CheatParameter
			{
				Name = "path",
				Type = "System.String",
				HasDefaultValue = true
			},
			new CheatParameter
			{
				Name = "name",
				Type = "System.String",
				HasDefaultValue = true
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action<double>(VideoPlayerHelper.CheatSeekVideo), "void CheatSeekVideo(double seekTime)", "seek_video", "", "", ExecutionPolicy.PlayMode, new CheatParameter[1]
		{
			new CheatParameter
			{
				Name = "seekTime",
				Type = "System.Double",
				HasDefaultValue = false
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action<float, float>(CameraRig.StartShakeCheat), "void StartShakeCheat(float amplitude, float speed)", "start_shake", "", "", ExecutionPolicy.PlayMode, new CheatParameter[2]
		{
			new CheatParameter
			{
				Name = "amplitude",
				Type = "System.Single",
				HasDefaultValue = false
			},
			new CheatParameter
			{
				Name = "speed",
				Type = "System.Single",
				HasDefaultValue = false
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action<string, bool>(InteractionDoorPart.ToggleDoor), "void ToggleDoor(string name, bool newState)", "toggle_door", "", "", ExecutionPolicy.PlayMode, new CheatParameter[2]
		{
			new CheatParameter
			{
				Name = "name",
				Type = "System.String",
				HasDefaultValue = false
			},
			new CheatParameter
			{
				Name = "newState",
				Type = "System.Boolean",
				HasDefaultValue = false
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action<int>(InteractionHelper.AddMeltaCharge), "void AddMeltaCharge(int count)", "add_melta_charge", "", "", ExecutionPolicy.PlayMode, new CheatParameter[1]
		{
			new CheatParameter
			{
				Name = "count",
				Type = "System.Int32",
				HasDefaultValue = false
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action<string, int>(InteractionHelper.AddItem), "void AddItem(string blueprintName, int count = 1)", "add_item", "", "", ExecutionPolicy.PlayMode, new CheatParameter[2]
		{
			new CheatParameter
			{
				Name = "blueprintName",
				Type = "System.String",
				HasDefaultValue = false
			},
			new CheatParameter
			{
				Name = "count",
				Type = "System.Int32",
				HasDefaultValue = true
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action<BlueprintUnitAsksList>(UnitBarksManager.OverridePlayerAsks), "void OverridePlayerAsks(BlueprintUnitAsksList asksList)", "overrideplayerasks", "", "", ExecutionPolicy.PlayMode, new CheatParameter[1]
		{
			new CheatParameter
			{
				Name = "asksList",
				Type = "Kingmaker.Visual.Sound.BlueprintUnitAsksList",
				HasDefaultValue = false
			}
		}, "void")
	};

	public static readonly List<CheatPropertyInfoInternal> Properties = new List<CheatPropertyInfoInternal>
	{
		new CheatPropertyInfoInternal(new CheatPropertyMethods
		{
			Getter = (Func<float>)(() => AiBrainController.SecondsToWaitAtStart),
			Setter = (Action<float>)delegate(float value)
			{
				AiBrainController.SecondsToWaitAtStart = value;
			}
		}, "SecondsToWaitAtStart", "", "", ExecutionPolicy.All, "float"),
		new CheatPropertyInfoInternal(new CheatPropertyMethods
		{
			Getter = (Func<float>)(() => AiBrainController.SecondsToWaitAtEnd),
			Setter = (Action<float>)delegate(float value)
			{
				AiBrainController.SecondsToWaitAtEnd = value;
			}
		}, "SecondsToWaitAtEnd", "", "", ExecutionPolicy.All, "float"),
		new CheatPropertyInfoInternal(new CheatPropertyMethods
		{
			Getter = (Func<float>)(() => AiBrainController.SecondsAiTimeout),
			Setter = (Action<float>)delegate(float value)
			{
				AiBrainController.SecondsAiTimeout = value;
			}
		}, "SecondsAiTimeout", "", "", ExecutionPolicy.All, "float"),
		new CheatPropertyInfoInternal(new CheatPropertyMethods
		{
			Getter = (Func<bool>)(() => AiBrainController.AutoCombat),
			Setter = (Action<bool>)delegate(bool value)
			{
				AiBrainController.AutoCombat = value;
			}
		}, "AutoCombat", "", "", ExecutionPolicy.All, "bool"),
		new CheatPropertyInfoInternal(new CheatPropertyMethods
		{
			Getter = (Func<float>)(() => CheatsAnimation.SpeedForce),
			Setter = (Action<float>)delegate(float value)
			{
				CheatsAnimation.SpeedForce = value;
			}
		}, "am_forcespeed", "Set to override movement speed for player characters, in feet per standard action. Set to 0 to revert to default speed", "", ExecutionPolicy.All, "float"),
		new CheatPropertyInfoInternal(new CheatPropertyMethods
		{
			Getter = (Func<int>)(() => CheatsAnimation.MoveType),
			Setter = (Action<int>)delegate(int value)
			{
				CheatsAnimation.MoveType = value;
			}
		}, "am_movetype", "Set to override default move type for player characters.\n0 = charge\n1 = walk\n2 = run\n3 = crouch\n", "", ExecutionPolicy.All, "int"),
		new CheatPropertyInfoInternal(new CheatPropertyMethods
		{
			Getter = (Func<bool>)(() => CheatsAnimation.SpeedLock),
			Setter = (Action<bool>)delegate(bool value)
			{
				CheatsAnimation.SpeedLock = value;
			}
		}, "am_speedlock", "When true, all player characters move with the same speed out of combat.\nWhen false, everyone uses their own default speed.", "", ExecutionPolicy.All, "bool"),
		new CheatPropertyInfoInternal(new CheatPropertyMethods
		{
			Getter = (Func<bool>)(() => CheatsCommon.RandomEncounters),
			Setter = (Action<bool>)delegate(bool value)
			{
				CheatsCommon.RandomEncounters = value;
			}
		}, "random_enc", "When false, random encounters on global map are disabled", "", ExecutionPolicy.All, "bool"),
		new CheatPropertyInfoInternal(new CheatPropertyMethods
		{
			Getter = (Func<bool>)(() => CheatsCommon.SendAnalyticEvents),
			Setter = (Action<bool>)delegate(bool value)
			{
				CheatsCommon.SendAnalyticEvents = value;
			}
		}, "send_unity_events", "When true, send unity analytic events as normal game does", "", ExecutionPolicy.All, "bool"),
		new CheatPropertyInfoInternal(new CheatPropertyMethods
		{
			Getter = (Func<bool>)(() => CheatsCommon.IgnoreEncumbrance),
			Setter = (Action<bool>)delegate(bool value)
			{
				CheatsCommon.IgnoreEncumbrance = value;
			}
		}, "ignore_encumbrance", "When true, encumbrance is always Light", "", ExecutionPolicy.All, "bool"),
		new CheatPropertyInfoInternal(new CheatPropertyMethods
		{
			Getter = (Func<bool>)(() => CheatsDebug.DrawFps),
			Setter = (Action<bool>)delegate(bool value)
			{
				CheatsDebug.DrawFps = value;
			}
		}, "draw_fps", "When false, FPS Counter is disabled", "", ExecutionPolicy.All, "bool"),
		new CheatPropertyInfoInternal(new CheatPropertyMethods
		{
			Getter = (Func<bool>)(() => CheatsDebug.DrawCutscenes),
			Setter = (Action<bool>)delegate(bool value)
			{
				CheatsDebug.DrawCutscenes = value;
			}
		}, "draw_cutscenes", "When false, Cutscenes debug info is disabled", "", ExecutionPolicy.All, "bool"),
		new CheatPropertyInfoInternal(new CheatPropertyMethods
		{
			Getter = (Func<bool>)(() => CheatsDebug.DrawSpaceCombatDebugDecals),
			Setter = (Action<bool>)delegate(bool value)
			{
				CheatsDebug.DrawSpaceCombatDebugDecals = value;
			}
		}, "draw_space_combat_debug_decals", "When false, space combat debug decals are disabled", "", ExecutionPolicy.PlayMode, "bool"),
		new CheatPropertyInfoInternal(new CheatPropertyMethods
		{
			Getter = (Func<bool>)(() => CheatsJira.QaMode),
			Setter = (Action<bool>)delegate(bool value)
			{
				CheatsJira.QaMode = value;
			}
		}, "qa_mode", "Set to true to see all exceptions as a message box", "", ExecutionPolicy.All, "bool"),
		new CheatPropertyInfoInternal(new CheatPropertyMethods
		{
			Getter = (Func<bool>)(() => PointerController.PointerDebug),
			Setter = (Action<bool>)delegate(bool value)
			{
				PointerController.PointerDebug = value;
			}
		}, "pointer_debug", "", "", ExecutionPolicy.All, "bool"),
		new CheatPropertyInfoInternal(new CheatPropertyMethods
		{
			Getter = (Func<float>)(() => StarSystemTimeController.TimeMultiplier),
			Setter = (Action<float>)delegate(float value)
			{
				StarSystemTimeController.TimeMultiplier = value;
			}
		}, "space_time_multiplier", "", "", ExecutionPolicy.All, "float"),
		new CheatPropertyInfoInternal(new CheatPropertyMethods
		{
			Getter = (Func<float>)(() => SlowMoController.SlowMoFactor),
			Setter = (Action<float>)delegate(float value)
			{
				SlowMoController.SlowMoFactor = value;
			}
		}, "SlowMoFactor", "", "", ExecutionPolicy.All, "float"),
		new CheatPropertyInfoInternal(new CheatPropertyMethods
		{
			Getter = (Func<bool>)(() => EditorSafeThreading.AsyncSaves),
			Setter = (Action<bool>)delegate(bool value)
			{
				EditorSafeThreading.AsyncSaves = value;
			}
		}, "enable_editor_async_save", "", "", ExecutionPolicy.PlayMode, "bool"),
		new CheatPropertyInfoInternal(new CheatPropertyMethods
		{
			Getter = (Func<bool>)(() => SaveManager.AllowSaveInCutscenesAndDialogs),
			Setter = (Action<bool>)delegate(bool value)
			{
				SaveManager.AllowSaveInCutscenesAndDialogs = value;
			}
		}, "allow_save_in_cutscenes_and_dialogs", "", "", ExecutionPolicy.All, "bool"),
		new CheatPropertyInfoInternal(new CheatPropertyMethods
		{
			Getter = (Func<bool>)(() => SteamSavesReplicator.ForceFullDownload),
			Setter = (Action<bool>)delegate(bool value)
			{
				SteamSavesReplicator.ForceFullDownload = value;
			}
		}, "steam_force_full_download", "", "", ExecutionPolicy.All, "bool"),
		new CheatPropertyInfoInternal(new CheatPropertyMethods
		{
			Getter = (Func<float>)(() => UnitJumpAsideDodgeParams.Speed),
			Setter = (Action<float>)delegate(float value)
			{
				UnitJumpAsideDodgeParams.Speed = value;
			}
		}, "jump_aside_dodge_speed", "", "", ExecutionPolicy.All, "float"),
		new CheatPropertyInfoInternal(new CheatPropertyMethods
		{
			Getter = (Func<bool>)(() => IgnorePrerequisites.IgnorePrerequisitesAlways),
			Setter = (Action<bool>)delegate(bool value)
			{
				IgnorePrerequisites.IgnorePrerequisitesAlways = value;
			}
		}, "ignore_prereq", "When true, prerequisites will be ignored", "", ExecutionPolicy.All, "bool"),
		new CheatPropertyInfoInternal(new CheatPropertyMethods
		{
			Getter = (Func<bool>)(() => CameraRig.DebugCameraScroll),
			Setter = (Action<bool>)delegate(bool value)
			{
				CameraRig.DebugCameraScroll = value;
			}
		}, "debug_camera_scroll", "", "", ExecutionPolicy.All, "bool"),
		new CheatPropertyInfoInternal(new CheatPropertyMethods
		{
			Getter = (Func<float>)(() => CameraRig.ConsoleScrollMod),
			Setter = (Action<float>)delegate(float value)
			{
				CameraRig.ConsoleScrollMod = value;
			}
		}, "camera_scroll_mod", "", "", ExecutionPolicy.All, "float"),
		new CheatPropertyInfoInternal(new CheatPropertyMethods
		{
			Getter = (Func<float>)(() => CameraRig.ConsoleRotationMod),
			Setter = (Action<float>)delegate(float value)
			{
				CameraRig.ConsoleRotationMod = value;
			}
		}, "camera_rotation_mod", "", "", ExecutionPolicy.All, "float"),
		new CheatPropertyInfoInternal(new CheatPropertyMethods
		{
			Getter = (Func<bool>)(() => CameraRig.IsShakingCheat),
			Setter = (Action<bool>)delegate(bool value)
			{
				CameraRig.IsShakingCheat = value;
			}
		}, "is_shaking", "", "", ExecutionPolicy.PlayMode, "bool"),
		new CheatPropertyInfoInternal(new CheatPropertyMethods
		{
			Getter = (Func<bool>)(() => UnitMovementAgentBase.FallbackToRayCast),
			Setter = (Action<bool>)delegate(bool value)
			{
				UnitMovementAgentBase.FallbackToRayCast = value;
			}
		}, "movement_use_raycast", "", "", ExecutionPolicy.All, "bool"),
		new CheatPropertyInfoInternal(new CheatPropertyMethods
		{
			Getter = (Func<bool>)(() => ForcedCullingService.CheatDisabled),
			Setter = (Action<bool>)delegate(bool value)
			{
				ForcedCullingService.CheatDisabled = value;
			}
		}, "forced_culling_disabled", "", "", ExecutionPolicy.All, "bool")
	};

	public static readonly List<(ArgumentConverter.ConvertDelegate, int)> ArgConverters = new List<(ArgumentConverter.ConvertDelegate, int)>
	{
		(CheatArgConverters.UnitConverter, 0),
		(CheatArgConverters.MechanicEntityConverter, 0),
		(CheatArgConverters.Vector3Converter, 0),
		(Utilities.BlueprintConverter, 0)
	};

	public static readonly List<(ArgumentConverter.PreprocessDelegate, int)> ArgPreprocessors = new List<(ArgumentConverter.PreprocessDelegate, int)>
	{
		(CheatArgPreprocessors.SelectedUnits, 0),
		(CheatArgPreprocessors.Mouseover, 0),
		(CheatArgPreprocessors.Cursor, 0)
	};
}
