using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Core.Cheats;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem.EditorDatabase;
using Kingmaker.Blueprints.Root;
using Kingmaker.Controllers.Combat;
using Kingmaker.Designers;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.GameModes;
using Kingmaker.Items;
using Kingmaker.Mechanics.Damage;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.Sound;
using Kingmaker.UI.Common;
using Kingmaker.UI.InputSystems;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Levelup.Obsolete.Blueprints;
using Kingmaker.UnitLogic.Mechanics.Damage;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility;
using Kingmaker.Utility.BuildModeUtils;
using Kingmaker.Utility.UnityExtensions;
using Kingmaker.View;
using Kingmaker.View.Roaming;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;
using UnityEngine.Profiling;

namespace Kingmaker.Cheats;

internal class CheatsCombat
{
	private const string CrTable = "Assets/Mechanics/Blueprints/Classes/Basic/CRTable.asset";

	private const string CrTableGuid = "19b09eaa18b203645b6f1d5f2edcb1e4";

	private static readonly string[] peacefulUnitsGuids = new string[1] { "bb66cc962d7144b78a11a5e5218753f0" };

	private static readonly string[] enemyUnitsGuids = new string[5] { "5f12c3037d7a4b298ba5456dee8a75a8", "9656dd2d816f48ad81c6e3238b1ed7de", "22abcda97c6644d797d0cc5978890f1e", "61fa3e7bf2c44545aaaa9899ae7d688a", "1c88397dedee413cb886359cb487379a" };

	public static void RegisterCommands(KeyboardAccess keyboard)
	{
		if (BuildModeUtility.CheatsEnabled)
		{
			EventBus.Subscribe(new CheatsCombat());
			keyboard.Bind("Kill", delegate
			{
				CheatsHelper.Run("kill @mouseover");
			});
			keyboard.Bind("KillAll", delegate
			{
				CheatsHelper.Run("kill_all");
			});
			keyboard.Bind("Damage", delegate
			{
				CheatsHelper.Run("damage @mouseover");
			});
			keyboard.Bind("DamageALot", delegate
			{
				CheatsHelper.Run("damage @mouseover true");
			});
			keyboard.Bind("Heal", delegate
			{
				CheatsHelper.Run("heal @mouseover");
			});
			keyboard.Bind("SetMP100", delegate
			{
				CheatsHelper.Run("setmp100");
			});
			keyboard.Bind("Cheat_MakeEnemy", delegate
			{
				CheatsHelper.Run("summon null null @cursor");
			});
			keyboard.Bind("RestAll", delegate
			{
				CheatsHelper.Run("rest_all");
			});
			keyboard.Bind("AllAudioMute", AudioMuteManager.ToggleAllMute);
			keyboard.Bind("MusicMute", AudioMuteManager.ToggleMusicMute);
			SmartConsole.RegisterCommand("attach_buff", AttachBuff);
			SmartConsole.RegisterCommand("list_mobs", ListMobs);
			SmartConsole.RegisterCommand("summon_zoo", SpawnInspectedEnemiesUnderCursor);
			SmartConsole.RegisterCommand("iddqd", Iddqd);
			SmartConsole.RegisterCommand("full_buff_please", FullBuffPlease);
			SmartConsole.RegisterCommand("empowered", Empowered);
			SmartConsole.RegisterCommand("damage_ability_score", DamageAbilityScore);
			SmartConsole.RegisterCommand("drain_ability_score", DrainAbilityScore);
			SmartConsole.RegisterCommand("info", Info);
			SmartConsole.RegisterCommand("encounter_info", EncounterInfo);
			SmartConsole.RegisterCommand("add_condition", AddCondition);
			SmartConsole.RegisterCommand("remove_condition", RemoveCondition);
			SmartConsole.RegisterCommand("manual_combat_begin", ManualCombatBegin);
			SmartConsole.RegisterCommand("manual_combat_end", ManualCombatEnd);
			SmartConsole.RegisterCommand("manual_combat_next_round", ManualCombatNextRound);
			SmartConsole.RegisterCommand("end_preparation_turn", EndPreparationTurn);
		}
	}

	private static void ManualCombatBegin(string parameters)
	{
		Game.Instance.TurnController.BeginManualCombat();
	}

	private static void ManualCombatEnd(string parameters)
	{
		Game.Instance.TurnController.EndManualCombat();
	}

	private static void ManualCombatNextRound(string parameters)
	{
		Game.Instance.TurnController.NextRoundForManualCombat();
	}

	private static void EndPreparationTurn(string parameters)
	{
		Game.Instance.TurnController.RequestEndPreparationTurn();
	}

	private static void AddCondition(string parameters)
	{
		PFLog.SmartConsole.Log("Not implemented");
	}

	private static void RemoveCondition(string parameters)
	{
		PFLog.SmartConsole.Log("Not implemented");
	}

	private static void SpawnInspectedEnemiesUnderCursor(string parameters)
	{
		Vector3 worldPosition = Game.Instance.ClickEventsController.WorldPosition;
		List<string> obj = new List<string> { "CR15_MedusaSorcerer ", "CR0.5_KoboldMeleeWarriorLevel1 ", "CR3_KoboldRangedFighterLevel4", "CR10_GolemClay", "CR11_AirElementalElder", "CR16_AstradaemonStandard", "CR14_Ankou", "CR9_FrostGiantStandard", "CR13_ThanadaemonStandard", "CR4_HydraStandard" };
		List<BlueprintUnit> list = new List<BlueprintUnit>();
		foreach (string item in obj)
		{
			list.Add(Utilities.GetBlueprint<BlueprintUnit>(item) ?? Game.Instance.BlueprintRoot.Cheats.Enemy);
		}
		foreach (BaseUnitEntity activeCompanion in Game.Instance.Player.ActiveCompanions)
		{
			activeCompanion.Skills.SkillLoreImperium.BaseStat.BaseValue = 75;
			activeCompanion.Skills.SkillTechUse.BaseStat.BaseValue = 75;
			activeCompanion.Skills.SkillDemolition.BaseStat.BaseValue = 75;
			activeCompanion.Skills.SkillLoreWarp.BaseStat.BaseValue = 75;
		}
		float num = 0f;
		foreach (BlueprintUnit item2 in list)
		{
			if (item2 != null)
			{
				PFLog.SmartConsole.Log("Summoning: " + Utilities.GetBlueprintPath(item2));
				Game.Instance.EntitySpawner.SpawnUnit(item2, new Vector3(worldPosition.x + 2f * num, worldPosition.y, worldPosition.z + 2f * num), Quaternion.identity, Game.Instance.State.LoadedAreaState.MainState);
				num += 1f;
			}
		}
	}

	private static void EncounterInfo(string parameters)
	{
		List<BlueprintUnit> list = (from u in Game.Instance.State.AllUnits
			where u.IsInCombat && !u.IsPlayerFaction
			select u.Blueprint).ToList();
		PFLog.SmartConsole.Log($"Encounter CR: {GetEncounterCr()}");
		foreach (BlueprintUnit item in list)
		{
			PFLog.SmartConsole.Log(Utilities.GetBlueprintPath(item) ?? "");
		}
	}

	private static int GetEncounterCr()
	{
		BlueprintStatProgression blueprint = Utilities.GetBlueprint<BlueprintStatProgression>("Assets/Mechanics/Blueprints/Classes/Basic/CRTable.asset");
		if (!blueprint)
		{
			blueprint = Utilities.GetBlueprint<BlueprintStatProgression>("19b09eaa18b203645b6f1d5f2edcb1e4");
		}
		if (!blueprint)
		{
			PFLog.SmartConsole.Log("CR table not found at Assets/Mechanics/Blueprints/Classes/Basic/CRTable.asset or 19b09eaa18b203645b6f1d5f2edcb1e4, cannot calculate");
			return -1;
		}
		return Utilities.GetTotalChallengeRating((from u in Game.Instance.State.AllUnits
			where u.IsInCombat && !u.IsPlayerFaction
			select u.Blueprint).ToList());
	}

	private static void Info(string parameters)
	{
		BaseUnitEntity unitUnderMouse = Utilities.GetUnitUnderMouse();
		if (unitUnderMouse == null)
		{
			PFLog.SmartConsole.Log("No unit under mouse");
		}
		else
		{
			PFLog.SmartConsole.Log("AssetPath: " + Utilities.GetBlueprintPath(unitUnderMouse.Blueprint));
		}
	}

	private static void ListMobs(string parameters)
	{
		string value = Utilities.GetParamString(parameters, 1, null) ?? "";
		foreach (BlueprintUnit scriptableObject in Utilities.GetScriptableObjects<BlueprintUnit>())
		{
			string blueprintPath = Utilities.GetBlueprintPath(scriptableObject);
			if (blueprintPath.Contains(value))
			{
				PFLog.SmartConsole.Log(blueprintPath);
			}
		}
	}

	[Cheat(Name = "kill_all", ExecutionPolicy = ExecutionPolicy.PlayMode)]
	public static void KillAll()
	{
		foreach (BaseUnitEntity allBaseUnit in Game.Instance.State.AllBaseUnits)
		{
			if (allBaseUnit.CombatState.IsInCombat && allBaseUnit.CombatGroup.IsEnemy(GameHelper.GetPlayerCharacter()))
			{
				Damage(allBaseUnit, tryToKill: true);
			}
		}
		if (Game.Instance.IsPaused)
		{
			Game.Instance.StopMode(GameModeType.Pause);
		}
	}

	[Cheat(ExecutionPolicy = ExecutionPolicy.PlayMode)]
	public static void Kill(BaseUnitEntity unit)
	{
		if (unit == null)
		{
			UIUtility.SendWarning("No unit under mouse");
		}
		else
		{
			KillUnit(unit);
		}
	}

	[Cheat(ExecutionPolicy = ExecutionPolicy.PlayMode)]
	public static void Damage(MechanicEntity entity, bool tryToKill = false)
	{
		if (entity == null)
		{
			UIUtility.SendWarning("No entity under mouse");
		}
		else if (tryToKill)
		{
			int value = entity.GetHealthOptional()?.HitPointsLeft ?? 1000;
			Rulebook.Trigger(new RuleDealDamage(entity, entity, DamageType.Direct.CreateDamage(value)));
		}
		else
		{
			Rulebook.Trigger(new RuleDealDamage(entity, entity, BlueprintRoot.Instance.Cheats.TestDamage.CreateDamage()));
		}
	}

	[Cheat(ExecutionPolicy = ExecutionPolicy.PlayMode)]
	public static void Heal(MechanicEntity entity)
	{
		if (entity == null)
		{
			UIUtility.SendWarning("No entity under mouse");
		}
		else
		{
			Rulebook.Trigger(new RuleHealDamage(entity, entity, DiceFormula.Zero, 10));
		}
	}

	public static void KillUnit(BaseUnitEntity unit)
	{
		unit.LifeState.MarkedForDeath = true;
		unit.Wake(1f);
		unit.Health.LastHandledDamage = null;
	}

	[Cheat(Name = "list_buffs", ExecutionPolicy = ExecutionPolicy.PlayMode)]
	public static void ListBuffs(BaseUnitEntity targetUnit)
	{
		if (targetUnit != null)
		{
			PFLog.SmartConsole.Log("Buffs on " + Utilities.GetBlueprintPath(targetUnit.Blueprint));
			PrintBuffs(targetUnit.Buffs.Enumerable);
			return;
		}
		foreach (BaseUnitEntity selectedUnit in Game.Instance.SelectionCharacter.SelectedUnits)
		{
			PFLog.Default.Log("Buffs on " + selectedUnit.CharacterName);
			PrintBuffs(selectedUnit.Buffs.Enumerable);
		}
	}

	public static void PrintBuffs(IEnumerable<Buff> buffs)
	{
		foreach (Buff buff in buffs)
		{
			PFLog.SmartConsole.Log(Utilities.GetBlueprintPath(buff.Blueprint) + "; Rounds Left: " + buff.ExpirationInRounds);
		}
	}

	[Cheat(Name = "detach_all_buffs", ExecutionPolicy = ExecutionPolicy.PlayMode)]
	public static void DetachAllBuffs(BaseUnitEntity unit)
	{
		if (unit == null)
		{
			throw new Exception("Need target unit");
		}
		foreach (Buff item in new List<Buff>(unit.Buffs.Enumerable))
		{
			unit.Facts.Remove(item);
			PFLog.SmartConsole.Log(unit.CharacterName, item?.ToString() + "detached");
		}
	}

	[Cheat(Name = "rest_all", ExecutionPolicy = ExecutionPolicy.PlayMode)]
	public static void RestAll()
	{
		foreach (BaseUnitEntity allCharactersAndStarship in Game.Instance.Player.AllCharactersAndStarships)
		{
			PartHealth.RestUnit(allCharactersAndStarship);
		}
		foreach (ItemEntity item in Game.Instance.Player.Inventory)
		{
			item.RestoreCharges();
		}
	}

	[Cheat(ExecutionPolicy = ExecutionPolicy.PlayMode)]
	public static void SetMP100()
	{
		if (Game.Instance.Player.IsInCombat)
		{
			Game.Instance.SelectionCharacter.SelectedUnitInUI.Value.CombatState.ResetActionPointsBlueCheat();
		}
	}

	private static void AttachBuff(string parameters)
	{
		string paramString = Utilities.GetParamString(parameters, 1, "Can't parse buff name from given parameters");
		BlueprintBuff blueprint = Utilities.GetBlueprint<BlueprintBuff>(paramString);
		if (blueprint == null)
		{
			PFLog.SmartConsole.Log("Cannot find buff by name: {0}", paramString);
		}
		else
		{
			Utilities.GetUnitUnderMouse()?.AddFact(blueprint);
		}
	}

	[Cheat(Name = "detach_fact", ExecutionPolicy = ExecutionPolicy.PlayMode)]
	public static void DetachFact(BlueprintUnitFact fact)
	{
		if (fact == null)
		{
			PFLog.SmartConsole.Log("Cannot find fact from given parameters");
		}
		else
		{
			Utilities.GetUnitUnderMouse()?.Facts.Remove(fact);
		}
	}

	[Cheat(Name = "attach_fact", ExecutionPolicy = ExecutionPolicy.PlayMode)]
	public static void AttachFact(BlueprintUnitFact fact)
	{
		if (fact == null)
		{
			PFLog.SmartConsole.Log("Cannot find fact from given parameters");
		}
		else
		{
			Utilities.GetUnitUnderMouse()?.AddFact(fact);
		}
	}

	[Cheat(Name = "summon", ExecutionPolicy = ExecutionPolicy.PlayMode)]
	public static void SpawnEnemyUnderCursor(BlueprintUnit bp = null, BlueprintFaction factionBp = null, Vector3 position = default(Vector3))
	{
		Vector3 position2 = ((position != default(Vector3)) ? position : Game.Instance.ClickEventsController.WorldPosition);
		if (bp == null)
		{
			bp = Game.Instance.BlueprintRoot.Cheats.Enemy;
		}
		PFLog.SmartConsole.Log("Summoning: " + Utilities.GetBlueprintPath(bp));
		BaseUnitEntity baseUnitEntity = Game.Instance.EntitySpawner.SpawnUnit(bp, position2, Quaternion.identity, Game.Instance.State.LoadedAreaState.MainState);
		if (factionBp != null)
		{
			baseUnitEntity.Faction.Set(factionBp);
		}
	}

	private static void SpawnFromList(string[] guids, int number, bool nearPlayer, bool isExtra, bool roam = false)
	{
		for (int i = 0; i < number; i++)
		{
			int num = UnityEngine.Random.Range(0, guids.Length);
			Vector3 pos;
			if (nearPlayer)
			{
				Vector3 position = Game.Instance.Player.MainCharacter.Entity.Position;
				float num2 = UnityEngine.Random.Range(-10f, 10f);
				float num3 = UnityEngine.Random.Range(-10f, 10f);
				pos = new Vector3(position.x + num2, position.y, position.z + num3);
				pos = ObstacleAnalyzer.GetNearestNode(pos).position;
			}
			else
			{
				Bounds mechanicBounds = Game.Instance.CurrentlyLoadedArea.Bounds.MechanicBounds;
				float x = UnityEngine.Random.Range(mechanicBounds.min.x, mechanicBounds.max.x);
				float z = UnityEngine.Random.Range(mechanicBounds.min.z, mechanicBounds.max.z);
				pos = new Vector3(x, 0f, z);
				pos = ObstacleAnalyzer.GetNearestNode(pos).position;
			}
			BlueprintUnit blueprintUnit = BlueprintsDatabase.LoadById<BlueprintUnit>(guids[num]);
			PFLog.SmartConsole.Log("Summoning: " + Utilities.GetBlueprintPath(blueprintUnit));
			BaseUnitEntity baseUnitEntity = Game.Instance.EntitySpawner.SpawnUnit(blueprintUnit, pos, Quaternion.identity, Game.Instance.State.LoadedAreaState.MainState);
			if (isExtra)
			{
				baseUnitEntity.MarkExtra();
			}
			if (roam)
			{
				UnitPartRoaming orCreate = baseUnitEntity.GetOrCreate<UnitPartRoaming>();
				orCreate.Settings = new RoamingUnitSettings();
				orCreate.Settings.Radius = 10f;
				orCreate.Settings.MinIdleTime = 1f;
				orCreate.Settings.MaxIdleTime = 5f;
			}
		}
	}

	[Cheat(Name = "spawn_units_dense", ExecutionPolicy = ExecutionPolicy.PlayMode)]
	public static void SpawnUnitsDense(int number, bool roaming = false)
	{
		SpawnFromList(peacefulUnitsGuids, number, nearPlayer: true, isExtra: false, roaming);
	}

	[Cheat(Name = "spawn_units_sparse", ExecutionPolicy = ExecutionPolicy.PlayMode)]
	public static void SpawnUnitsSparse(int number, bool roaming = false)
	{
		SpawnFromList(peacefulUnitsGuids, number, nearPlayer: false, isExtra: false, roaming);
	}

	[Cheat(Name = "spawn_extra_dense", ExecutionPolicy = ExecutionPolicy.PlayMode)]
	public static void SpawnExtraDense(int number, bool roaming = false)
	{
		SpawnFromList(peacefulUnitsGuids, number, nearPlayer: true, isExtra: true, roaming);
	}

	[Cheat(Name = "spawn_extra_sparse", ExecutionPolicy = ExecutionPolicy.PlayMode)]
	public static void SpawnExtraSparse(int number, bool roaming = false)
	{
		SpawnFromList(peacefulUnitsGuids, number, nearPlayer: false, isExtra: true, roaming);
	}

	[Cheat(Name = "spawn_enemies", ExecutionPolicy = ExecutionPolicy.PlayMode)]
	public static void SpawnEnemies(int number)
	{
		SpawnFromList(enemyUnitsGuids, number, nearPlayer: true, isExtra: false);
	}

	public static IEnumerator SpawnTestCoroutine()
	{
		int i = 0;
		while (i < 10)
		{
			SpawnFromList(peacefulUnitsGuids, 10, nearPlayer: true, isExtra: true, roam: true);
			int num;
			for (int frame = 0; frame < 10; frame = num)
			{
				yield return null;
				num = frame + 1;
			}
			Profiler.enabled = true;
			for (int frame = 0; frame < 10; frame = num)
			{
				yield return null;
				num = frame + 1;
			}
			Profiler.enabled = false;
			PFLog.Default.Log($"Spawn step {i} complete");
			num = i + 1;
			i = num;
		}
		Profiler.logFile = "";
		Profiler.enabled = false;
		PFLog.Default.Log("ALL DONE!");
	}

	[Cheat(Name = "spawn_test", ExecutionPolicy = ExecutionPolicy.PlayMode)]
	public static void SpawnTest()
	{
		Profiler.logFile = "profiler";
		Profiler.enableBinaryLog = true;
		Profiler.maxUsedMemory = int.MaxValue;
		MonoSingleton<CoroutineRunner>.Instance.StartCoroutine(SpawnTestCoroutine());
	}

	[Cheat(Name = "set_action_points_yellow", ExecutionPolicy = ExecutionPolicy.PlayMode)]
	public static void SetActionPointsYellow(int yellow)
	{
		MechanicEntity currentUnit = Game.Instance.TurnController.CurrentUnit;
		if (currentUnit == null)
		{
			PFLog.Default.Log("No current unit in turn base");
			return;
		}
		PartUnitCombatState combatStateOptional = currentUnit.GetCombatStateOptional();
		if (combatStateOptional == null)
		{
			PFLog.Default.Log($"No combat state for unit {currentUnit}");
		}
		else
		{
			combatStateOptional.SetActionPoints(yellow);
		}
	}

	[Cheat(Name = "set_action_points_blue", ExecutionPolicy = ExecutionPolicy.PlayMode)]
	public static void SetActionPointsBlue(float blue)
	{
		MechanicEntity currentUnit = Game.Instance.TurnController.CurrentUnit;
		if (currentUnit == null)
		{
			PFLog.Default.Log("No current unit in turn base");
			return;
		}
		PartUnitCombatState combatStateOptional = currentUnit.GetCombatStateOptional();
		if (combatStateOptional == null)
		{
			PFLog.Default.Log($"No combat state for unit {currentUnit}");
		}
		else
		{
			combatStateOptional.SetActionPoints(0, blue);
		}
	}

	[Cheat(Name = "add_bonus_ability_usage", ExecutionPolicy = ExecutionPolicy.PlayMode)]
	public static void AddBonusAbilityUsage(int count = 1, int costBonus = -5, RestrictionsHolder restrictions = null)
	{
		MechanicEntity currentUnit = Game.Instance.TurnController.CurrentUnit;
		if (currentUnit == null)
		{
			PFLog.Default.Log("No current unit in turn base");
			return;
		}
		EntityFactSource source = new EntityFactSource(currentUnit);
		currentUnit.GetOrCreate<UnitPartBonusAbility>().AddBonusAbility(source, count, costBonus, restrictions.ToReference<RestrictionsHolder.Reference>());
	}

	private static void Iddqd(string parameters)
	{
		foreach (BaseUnitEntity selectedUnit in Game.Instance.SelectionCharacter.SelectedUnits)
		{
			Buff(Game.Instance.BlueprintRoot.Cheats.Iddqd, force: false, selectedUnit);
		}
	}

	private static void FullBuffPlease(string parameters)
	{
		foreach (BaseUnitEntity item in Game.Instance.Player.Party)
		{
			foreach (BlueprintBuff fullBuff in Game.Instance.BlueprintRoot.Cheats.FullBuffList)
			{
				GameHelper.ApplyBuff(item, fullBuff, 50.Rounds());
			}
		}
	}

	private static void Empowered(string parameters)
	{
		foreach (BaseUnitEntity selectedUnit in Game.Instance.SelectionCharacter.SelectedUnits)
		{
			Buff(Game.Instance.BlueprintRoot.Cheats.Empowered, force: false, selectedUnit);
		}
	}

	private static void Buff(BlueprintBuff buff, bool force, BaseUnitEntity unit)
	{
		if (!unit.Buffs.Contains(buff))
		{
			GameHelper.ApplyBuff(unit, buff);
		}
		else if (!force)
		{
			unit.Facts.Remove(buff);
		}
	}

	private static void DamageAbilityScore(string p)
	{
		DamageOrDrainAbilityScore(p, drain: false);
	}

	private static void DrainAbilityScore(string p)
	{
		DamageOrDrainAbilityScore(p, drain: true);
	}

	private static void DamageOrDrainAbilityScore(string p, bool drain)
	{
		string stat = Utilities.GetParamString(p, 1, "Missing ability score: str|dex|con|int|wis|cha").ToLowerInvariant();
		int? paramInt = Utilities.GetParamInt(p, 2, "Missing damage value");
		if (!paramInt.HasValue)
		{
			return;
		}
		int value = paramInt.Value;
		if (value <= 0)
		{
			SmartConsole.Print("Damage value must be >= 1");
			return;
		}
		StatType? statType = GetStatType(stat);
		if (!statType.HasValue)
		{
			SmartConsole.Print("Can't parse ability score, use one of these: str|dex|con|int|wis|cha");
			return;
		}
		foreach (BaseUnitEntity selectedUnit in Game.Instance.SelectionCharacter.SelectedUnits)
		{
			if (drain)
			{
				selectedUnit.Stats.GetStat<ModifiableValueAttributeStat>(statType.Value).Drain += value;
			}
			else
			{
				selectedUnit.Stats.GetStat<ModifiableValueAttributeStat>(statType.Value).Damage += value;
			}
		}
	}

	private static StatType? GetStatType(string stat)
	{
		return null;
	}

	private static string GetDifficulty()
	{
		int num = GetEncounterCr() - Game.Instance.Player.PartyLevel;
		if (num < 3)
		{
			return "Easy";
		}
		if (num < 5)
		{
			return "Hard";
		}
		return "Boss";
	}
}
