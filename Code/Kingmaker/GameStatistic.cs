using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Core.Async;
using Kingmaker.Blueprints;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.EntitySystem.Persistence;
using Kingmaker.EntitySystem.Persistence.JsonUtility;
using Kingmaker.GameInfo;
using Kingmaker.GameModes;
using Kingmaker.Items;
using Kingmaker.Items.Slots;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.Tutorial;
using Kingmaker.UI.Models;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Enums;
using Kingmaker.UnitLogic.Groups;
using Kingmaker.UnitLogic.Levelup.Obsolete;
using Kingmaker.UnitLogic.Levelup.Obsolete.Blueprints;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Utility.Reporting.Base;
using Kingmaker.Utility.Serialization;
using Kingmaker.Utility.UnityExtensions;
using Newtonsoft.Json;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker;

[JsonObject]
public class GameStatistic : IDisposable, IUnitLifeStateChanged, ISubscriber<IAbstractUnitEntity>, ISubscriber, ITutorialTriggerFailedHandler, INewTutorialUIHandler, IUnitGainExperienceHandler, ISubscriber<IBaseUnitEntity>, ILevelUpInitiateUIHandler, ILevelUpCompleteUIHandler, IGameOverHandler, IAreaHandler, IFullScreenUIHandler
{
	public enum AppType
	{
		Default,
		NonDefault
	}

	public class AppStatus
	{
		private string appStatus = string.Empty;

		public void Add(string data)
		{
			if (data == null)
			{
				return;
			}
			byte[] bytes = Encoding.Unicode.GetBytes(data);
			using SHA1 sHA = SHA1.Create();
			byte[] array = sHA.ComputeHash(bytes);
			appStatus = BitConverter.ToString(array);
			appStatus = appStatus.Replace("-", "").ToLower();
		}

		public string Get()
		{
			return appStatus;
		}

		public string Randomize()
		{
			char[] array = new char[16]
			{
				'0', '1', '2', '3', '4', '5', '6', '7', '8', '9',
				'a', 'b', 'c', 'd', 'e', 'f'
			};
			System.Random random = new System.Random(Environment.TickCount);
			StringBuilder stringBuilder = new StringBuilder("");
			stringBuilder.Append(array[random.Next(1, array.Length)]);
			for (int i = 1; i < 40; i++)
			{
				stringBuilder.Append(array[random.Next(0, array.Length)]);
			}
			appStatus = stringBuilder.ToString();
			return appStatus;
		}
	}

	public class StatisticLimit
	{
		public TimeSpan LastReportTime = TimeSpan.Zero;

		private int s_ReportsPerHourLimit;

		public TimeSpan ReportInterval { get; private set; }

		public int ReportsPerHourLimit
		{
			get
			{
				return s_ReportsPerHourLimit;
			}
			set
			{
				if (value == 0)
				{
					ReportInterval = TimeSpan.MaxValue;
				}
				else
				{
					ReportInterval = TimeSpan.FromSeconds(3600 / value);
				}
				s_ReportsPerHourLimit = value;
			}
		}

		public StatisticLimit(int reportsPerHour)
		{
			ReportsPerHourLimit = reportsPerHour;
		}

		public bool CanBeReported()
		{
			if (s_ReportsPerHourLimit == 0)
			{
				return false;
			}
			if (Game.Instance.TimeController.RealTime - LastReportTime > ReportInterval)
			{
				return true;
			}
			return false;
		}

		public void UpdateReportedTime()
		{
			LastReportTime = Game.Instance.TimeController.RealTime;
		}
	}

	public class MergeableStatistic
	{
		public int MergedCount { get; set; } = 1;


		public virtual string GetSerializedString()
		{
			return string.Empty;
		}

		public virtual bool CanMerge(MergeableStatistic other)
		{
			return false;
		}

		public virtual void Merge(MergeableStatistic other)
		{
			MergedCount++;
		}

		public virtual void UnmergeLast()
		{
			MergedCount--;
		}
	}

	[JsonObject]
	public class PlayTimeStatistic : MergeableStatistic
	{
		public enum OtherStateType
		{
			None,
			Chargen,
			LevelUp,
			Afk,
			Combat,
			LoadingScreen,
			TotalPlaytime
		}

		private readonly TimeSpan m_NoInputLimit = TimeSpan.FromSeconds(90.0);

		private TimeSpan m_NoInputTimer = TimeSpan.Zero;

		[JsonProperty]
		private Dictionary<string, TimeStatistic> m_Areas = new Dictionary<string, TimeStatistic>();

		[JsonProperty]
		private Dictionary<string, TimeStatistic> m_Chapters = new Dictionary<string, TimeStatistic>();

		[JsonProperty]
		private Dictionary<string, TimeStatistic> m_GameMods = new Dictionary<string, TimeStatistic>();

		[JsonProperty]
		private Dictionary<string, TimeStatistic> m_FullScreenUIs = new Dictionary<string, TimeStatistic>();

		[JsonProperty]
		private Dictionary<string, TimeStatistic> m_OtherStates = new Dictionary<string, TimeStatistic>();

		public OtherStateType CurrentLevelUpType { get; set; }

		public bool IsLevelUpInProcess => CurrentLevelUpType != OtherStateType.None;

		public bool IsEmpty()
		{
			return GetSerializedString().Length < 5;
		}

		public override string GetSerializedString()
		{
			return GetSerializedDict(m_Areas) + ";" + GetSerializedDict(m_Chapters) + ";" + GetSerializedDict(m_GameMods) + ";" + GetSerializedDict(m_FullScreenUIs) + ";" + GetSerializedDict(m_OtherStates);
		}

		public override bool CanMerge(MergeableStatistic other)
		{
			return false;
		}

		private string GetSerializedDict(Dictionary<string, TimeStatistic> data)
		{
			return string.Join(",", data.Select((KeyValuePair<string, TimeStatistic> x) => $"{x.Key}:{(int)x.Value.m_RealTime.TotalSeconds}:{(int)x.Value.m_GameTime.TotalSeconds}"));
		}

		private void TickTimer(Dictionary<string, TimeStatistic> timer, string key, TimeSpan gameTime, TimeSpan realTime)
		{
			if (!timer.ContainsKey(key))
			{
				timer[key] = new TimeStatistic();
			}
			timer[key].Add(gameTime, realTime);
		}

		public void TickTimers(Game game, FullScreenUIType activeFullScreenUIType, TimeSpan gameTime, TimeSpan realTime, bool isMainMenu)
		{
			m_OtherStates[6.ToString()] = new TimeStatistic(game.Player.GameTime, game.Player.RealTime);
			if (realTime == TimeSpan.Zero)
			{
				realTime = Time.unscaledDeltaTime.Seconds();
			}
			if (isMainMenu)
			{
				if (IsLevelUpInProcess)
				{
					TickTimer(m_Areas, "MainMenu", gameTime, realTime);
					if (CheckAfk(game, realTime))
					{
						TickTimer(m_OtherStates, 3.ToString(), gameTime, realTime);
					}
					else
					{
						TickTimer(m_OtherStates, ((int)CurrentLevelUpType).ToString(), gameTime, realTime);
					}
				}
				return;
			}
			if (LoadingProcess.Instance.IsLoadingInProcess)
			{
				TickTimer(m_OtherStates, 5.ToString(), gameTime, realTime);
				m_NoInputTimer = TimeSpan.Zero;
				return;
			}
			if (CheckAfk(game, realTime))
			{
				TickTimer(m_OtherStates, 3.ToString(), gameTime, realTime);
				return;
			}
			AreaPersistentState loadedAreaState = game.LoadedAreaState;
			string key = ((loadedAreaState == null) ? null : SimpleBlueprintExtendAsObject.Or(loadedAreaState.Blueprint, null)?.ToString()) ?? s_EmptyValue;
			TickTimer(m_Areas, key, gameTime, realTime);
			int chapter = Game.Instance.Player.Chapter;
			TickTimer(m_Chapters, chapter.ToString(), gameTime, realTime);
			GameModeType gameMode = GetGameMode(game);
			TickTimer(m_GameMods, ((int)gameMode).ToString(), gameTime, realTime);
			if (activeFullScreenUIType != 0)
			{
				Dictionary<string, TimeStatistic> fullScreenUIs = m_FullScreenUIs;
				int num = (int)activeFullScreenUIType;
				TickTimer(fullScreenUIs, num.ToString(), gameTime, realTime);
			}
			if (game.Player.IsInCombat)
			{
				TickTimer(m_OtherStates, 4.ToString(), gameTime, realTime);
			}
			if (IsLevelUpInProcess)
			{
				TickTimer(m_OtherStates, ((int)CurrentLevelUpType).ToString(), gameTime, realTime);
			}
		}

		private bool CheckAfk(Game game, TimeSpan realTime)
		{
			GameModeType gameMode = GetGameMode(game);
			if (Input.anyKey || gameMode == GameModeType.Cutscene || gameMode == GameModeType.CutsceneGlobalMap)
			{
				m_NoInputTimer = TimeSpan.Zero;
			}
			else
			{
				m_NoInputTimer += realTime;
			}
			return m_NoInputTimer > m_NoInputLimit;
		}

		private static GameModeType GetGameMode(Game game)
		{
			GameModeType currentMode = game.CurrentMode;
			if (currentMode != GameModeType.Default)
			{
				return currentMode;
			}
			return game.CurrentMode;
		}
	}

	[JsonObject]
	public class CharacterStatistic
	{
		[JsonProperty]
		public bool m_Main;

		[JsonProperty]
		public string m_Name = s_EmptyValue;

		[JsonProperty]
		public string m_DisplayName = s_EmptyValue;

		[JsonProperty]
		public UnitLifeState m_LifeState;

		[JsonProperty]
		public int m_CurrentHP;

		[JsonProperty]
		public int m_MaxHP;

		public void Fill(bool main, BaseUnitEntity unitEntityData)
		{
			m_Main = main;
			m_Name = unitEntityData.Blueprint.name;
			m_DisplayName = unitEntityData.Blueprint.CharacterName;
			m_LifeState = unitEntityData.LifeState.State;
			m_CurrentHP = unitEntityData.Health.HitPointsLeft;
			m_MaxHP = unitEntityData.Health.MaxHitPoints;
		}
	}

	[JsonObject]
	public class MobStatistic
	{
		[JsonProperty]
		public int m_CR;

		[JsonProperty]
		public string m_Name = s_EmptyValue;

		[JsonProperty]
		public string m_DisplayName = s_EmptyValue;

		[JsonProperty]
		public UnitLifeState m_LifeState;

		[JsonProperty]
		public int m_CurrentHP;

		[JsonProperty]
		public int m_MaxHP;

		public void Fill(int cr, string name, string displayName, UnitLifeState lifeState, int currentHP, int maxHP)
		{
			m_CR = cr;
			m_Name = name;
			m_DisplayName = displayName;
			m_LifeState = lifeState;
			m_CurrentHP = currentHP;
			m_MaxHP = maxHP;
		}
	}

	[JsonObject]
	public class LevelupStatistic : MergeableStatistic
	{
		[JsonProperty]
		public int Level;

		[JsonProperty]
		public string LevelupGUID;

		[JsonProperty]
		public string EndGameFakeLevelup;

		public void Fill(string levelupGUID, int level, bool endgameFake)
		{
			LevelupGUID = levelupGUID;
			Level = level;
			EndGameFakeLevelup = (endgameFake ? "1" : "0");
		}
	}

	[JsonObject]
	public class LevelupInventoryStatistic : LevelupStatistic
	{
		[JsonProperty]
		public string CharacterName;

		[JsonProperty]
		public string HandData;

		[JsonProperty]
		public string InventoryData;

		[JsonProperty]
		public string QuickSlotData;

		public void Fill(PartUnitBody body, string characterName, string levelupGUID, int level, bool endgameFake)
		{
			Fill(levelupGUID, level, endgameFake);
			CharacterName = characterName;
			HandData = string.Empty;
			for (int i = 0; i < body.HandsEquipmentSets.Count; i++)
			{
				HandsEquipmentSet handsEquipmentSet = body.HandsEquipmentSets[i];
				if (handsEquipmentSet.PrimaryHand.HasItem)
				{
					HandData += GetItemNameWithUniqueMarker(handsEquipmentSet.PrimaryHand.Item);
				}
				HandData += ",";
				if (handsEquipmentSet.SecondaryHand.HasItem)
				{
					HandData += GetItemNameWithUniqueMarker(handsEquipmentSet.SecondaryHand.Item);
				}
				HandData += ",";
			}
			InventoryData = string.Empty;
			AddInventoryData(body.Armor.HasItem, body.Armor.MaybeItem);
			AddInventoryData(body.Shirt.HasItem, body.Shirt.MaybeItem);
			AddInventoryData(body.Belt.HasItem, body.Belt.MaybeItem);
			AddInventoryData(body.Head.HasItem, body.Head.MaybeItem);
			AddInventoryData(body.Feet.HasItem, body.Feet.MaybeItem);
			AddInventoryData(body.Gloves.HasItem, body.Gloves.MaybeItem);
			AddInventoryData(body.Neck.HasItem, body.Neck.MaybeItem);
			AddInventoryData(body.Ring1.HasItem, body.Ring1.MaybeItem);
			AddInventoryData(body.Ring2.HasItem, body.Ring2.MaybeItem);
			AddInventoryData(body.Wrist.HasItem, body.Wrist.MaybeItem);
			AddInventoryData(body.Shoulders.HasItem, body.Shoulders.MaybeItem);
			AddInventoryData(body.Glasses.HasItem, body.Glasses.MaybeItem);
			QuickSlotData = string.Empty;
			UsableSlot[] quickSlots = body.QuickSlots;
			foreach (UsableSlot usableSlot in quickSlots)
			{
				AddQuickSlotData(usableSlot.HasItem, usableSlot.MaybeItem);
			}
		}

		public void AddInventoryData(bool hasItem, ItemEntity item)
		{
			if (hasItem)
			{
				InventoryData += GetItemNameWithUniqueMarker(item);
			}
			InventoryData += ",";
		}

		public void AddQuickSlotData(bool hasItem, ItemEntity item)
		{
			if (hasItem)
			{
				QuickSlotData += GetItemNameWithUniqueMarker(item);
			}
			QuickSlotData += ",";
		}

		public override string GetSerializedString()
		{
			string text = $"{LevelupGUID};{Level};{EndGameFakeLevelup};{CharacterName};{HandData};{InventoryData}";
			if (text.Length + QuickSlotData.Length < 400)
			{
				text = text + ";" + QuickSlotData;
			}
			return text;
		}

		public override bool CanMerge(MergeableStatistic other)
		{
			return false;
		}
	}

	[JsonObject]
	public class LevelupPlayerStatistic : LevelupStatistic
	{
		[JsonProperty]
		public string Money;

		[JsonProperty]
		public string Class;

		[JsonProperty]
		public string MythicClass;

		public void Fill(int level, BaseUnitEntity unitEntityData, string levelupGUID, bool endgameFake)
		{
			Fill(levelupGUID, level, endgameFake);
			Money = Game.Instance.Player.Money.ToString();
			Class = GetUnitClass(unitEntityData);
			MythicClass = GetUnitMythicClass(unitEntityData);
		}

		public override string GetSerializedString()
		{
			return $"{LevelupGUID};{Level};{EndGameFakeLevelup};{Money};{Class};{MythicClass}";
		}

		public override bool CanMerge(MergeableStatistic other)
		{
			return false;
		}
	}

	[JsonObject]
	public class CharacterNotGoodStatistic : MergeableStatistic
	{
		[JsonProperty]
		public string ReportReason;

		[JsonProperty]
		public string Name;

		[JsonProperty]
		public string Area;

		[JsonProperty]
		public string Point;

		[JsonProperty]
		public string Boss;

		public void Fill(BaseUnitEntity unitEntityData, bool mainCharacter)
		{
			Name = unitEntityData.Blueprint.name;
			AreaPersistentState loadedAreaState = Game.Instance.LoadedAreaState;
			Area = ((loadedAreaState == null) ? null : SimpleBlueprintExtendAsObject.Or(loadedAreaState.Blueprint, null)?.ToString()) ?? s_EmptyValue;
			Vector3 vector = unitEntityData?.Position ?? new Vector3(0f, 0f, 0f);
			Point = $"{(int)vector.x}_{(int)vector.z}";
			MobStatistic boss = GetBoss();
			Boss = $"{boss.m_Name}_{boss.m_CR}";
			if (unitEntityData.LifeState.IsDead)
			{
				ReportReason = (mainCharacter ? "baron_death" : "companion_death");
			}
			else
			{
				ReportReason = (mainCharacter ? "baron_unconscious" : "companion_unconscious");
			}
		}

		public override string GetSerializedString()
		{
			return Name + ";" + Area + ";" + Point + ";" + Boss + ";" + ReportReason;
		}

		public override bool CanMerge(MergeableStatistic other)
		{
			return other is CharacterNotGoodStatistic;
		}

		public override void Merge(MergeableStatistic other)
		{
			base.Merge(other);
			if (other is CharacterNotGoodStatistic characterNotGoodStatistic)
			{
				Name = Name + "," + characterNotGoodStatistic.Name;
				Area = Area + "," + characterNotGoodStatistic.Area;
				Point = Point + "," + characterNotGoodStatistic.Point;
				Boss = Boss + "," + characterNotGoodStatistic.Boss;
				ReportReason = ReportReason + "," + characterNotGoodStatistic.ReportReason;
			}
		}

		public override void UnmergeLast()
		{
			base.UnmergeLast();
			Name = Name.Substring(0, Name.LastIndexOf(','));
			Area = Area.Substring(0, Area.LastIndexOf(','));
			Point = Point.Substring(0, Point.LastIndexOf(','));
			Boss = Boss.Substring(0, Boss.LastIndexOf(','));
			ReportReason = ReportReason.Substring(0, ReportReason.LastIndexOf(','));
		}
	}

	[JsonObject]
	public class LoadStatistic : MergeableStatistic
	{
		[JsonProperty]
		public string LoadReason;

		[JsonProperty]
		public string Party;

		[JsonProperty]
		public string Area;

		[JsonProperty]
		public string Point;

		[JsonProperty]
		public string Boss;

		[JsonProperty]
		public string GameSessionGuid;

		public void Fill()
		{
			GameSessionGuid = Game.Instance.Statistic.m_gameSessionGUID;
			BaseUnitEntity baseUnitEntity = Game.Instance.Player.MainCharacterEntity.ToBaseUnitEntity();
			bool flag = baseUnitEntity?.LifeState.IsDead ?? false;
			bool flag2 = baseUnitEntity != null && !baseUnitEntity.LifeState.IsConscious;
			bool isInCombat = Game.Instance.Player.IsInCombat;
			bool flag3 = Game.Instance.CurrentMode == GameModeType.GameOver;
			bool flag4 = Game.Instance.LoadedAreaState == null || Game.Instance.Player.MainCharacter.Entity == null;
			List<CharacterStatistic> characters = new List<CharacterStatistic>();
			AddCharacter(characters, mainCharacter: true, Game.Instance.Player.MainCharacter.Entity.ToBaseUnitEntity());
			foreach (BaseUnitEntity activeCompanion in Game.Instance.Player.ActiveCompanions)
			{
				if (activeCompanion.LifeState.IsConscious)
				{
					flag2 = false;
				}
				AddCharacter(characters, mainCharacter: false, activeCompanion);
			}
			Party = GetPartyString(characters);
			if (flag2)
			{
				LoadReason = "all_party_not_good";
			}
			else if (flag)
			{
				LoadReason = "baron_finally_dead";
			}
			else if (isInCombat)
			{
				LoadReason = "party_in_combat";
			}
			else if (flag3)
			{
				LoadReason = "game_over";
			}
			else if (flag4)
			{
				LoadReason = "load_from_main_menu";
			}
			else
			{
				LoadReason = "load_regular";
			}
			object area;
			if (!flag4)
			{
				AreaPersistentState loadedAreaState = Game.Instance.LoadedAreaState;
				area = ((loadedAreaState == null) ? null : SimpleBlueprintExtendAsObject.Or(loadedAreaState.Blueprint, null)?.ToString()) ?? s_EmptyValue;
			}
			else
			{
				area = "MainMenu";
			}
			Area = (string)area;
			Vector3 vector = baseUnitEntity?.Position ?? new Vector3(0f, 0f, 0f);
			Point = $"{(int)vector.x}_{(int)vector.z}";
			MobStatistic boss = GetBoss();
			Boss = $"{boss.m_Name}_{boss.m_CR}";
		}

		public override string GetSerializedString()
		{
			return Area + ";" + Point + ";" + Boss + ";" + Party + ";" + LoadReason;
		}

		public override bool CanMerge(MergeableStatistic other)
		{
			return other is LoadStatistic;
		}

		public override void Merge(MergeableStatistic other)
		{
			base.Merge(other);
			if (other is LoadStatistic loadStatistic)
			{
				Area = Area + "," + loadStatistic.Area;
				Point = Point + "," + loadStatistic.Point;
				Boss = Boss + "," + loadStatistic.Boss;
				Party = Party + "," + loadStatistic.Party;
				LoadReason = LoadReason + "," + loadStatistic.LoadReason;
			}
		}

		public override void UnmergeLast()
		{
			base.UnmergeLast();
			Area = Area.Substring(0, Area.LastIndexOf(','));
			Point = Point.Substring(0, Point.LastIndexOf(','));
			Boss = Boss.Substring(0, Boss.LastIndexOf(','));
			Party = Party.Substring(0, Party.LastIndexOf(','));
			LoadReason = LoadReason.Substring(0, LoadReason.LastIndexOf(','));
		}
	}

	[JsonObject]
	public class EnterCombatStatistic : MergeableStatistic
	{
		[JsonProperty]
		public string Party;

		[JsonProperty]
		public string Area;

		[JsonProperty]
		public string Point;

		[JsonProperty]
		public string Boss;

		public void Fill()
		{
			AreaPersistentState loadedAreaState = Game.Instance.LoadedAreaState;
			Area = ((loadedAreaState == null) ? null : SimpleBlueprintExtendAsObject.Or(loadedAreaState.Blueprint, null)?.ToString()) ?? s_EmptyValue;
			Vector3 vector = Game.Instance.Player.MainCharacter.Entity?.Position ?? new Vector3(0f, 0f, 0f);
			Point = $"{(int)vector.x}_{(int)vector.z}";
			MobStatistic boss = GetBoss();
			Boss = $"{boss.m_Name}_{boss.m_CR}";
			List<CharacterStatistic> characters = new List<CharacterStatistic>();
			AddCharacter(characters, mainCharacter: true, Game.Instance.Player.MainCharacter.Entity.ToBaseUnitEntity());
			foreach (BaseUnitEntity activeCompanion in Game.Instance.Player.ActiveCompanions)
			{
				AddCharacter(characters, mainCharacter: false, activeCompanion);
			}
			Party = GetPartyString(characters);
		}

		public override string GetSerializedString()
		{
			return Area + ";" + Point + ";" + Boss + ";" + Party;
		}

		public override bool CanMerge(MergeableStatistic other)
		{
			return other is EnterCombatStatistic;
		}

		public override void Merge(MergeableStatistic other)
		{
			base.Merge(other);
			if (other is EnterCombatStatistic enterCombatStatistic)
			{
				Area = Area + "," + enterCombatStatistic.Area;
				Point = Point + "," + enterCombatStatistic.Point;
				Boss = Boss + "," + enterCombatStatistic.Boss;
				Party = Party + "," + enterCombatStatistic.Party;
			}
		}

		public override void UnmergeLast()
		{
			base.UnmergeLast();
			Area = Area.Substring(0, Area.LastIndexOf(','));
			Point = Point.Substring(0, Point.LastIndexOf(','));
			Boss = Boss.Substring(0, Boss.LastIndexOf(','));
			Party = Party.Substring(0, Party.LastIndexOf(','));
		}
	}

	[JsonObject]
	public class TimeStatistic
	{
		[JsonProperty]
		public TimeSpan m_GameTime = TimeSpan.Zero;

		[JsonProperty]
		public TimeSpan m_RealTime = TimeSpan.Zero;

		public TimeStatistic()
		{
		}

		public TimeStatistic(TimeSpan gameTime, TimeSpan realTime)
		{
			m_GameTime = gameTime;
			m_RealTime = realTime;
		}

		public void Add(TimeSpan gameTime, TimeSpan realTime)
		{
			m_GameTime += gameTime;
			m_RealTime += realTime;
		}
	}

	public class TutorialStatistic : MergeableStatistic
	{
		public enum TutorialTriggerResult
		{
			Success,
			HigherPriorityCooldown,
			LowerOrEqualPriorityCooldown,
			FrequencyCounting,
			LimitReached,
			TagBanned,
			UnknownFailure
		}

		[JsonProperty]
		public string TutorialName;

		[JsonProperty]
		public string Area;

		[JsonProperty]
		public string Point;

		[JsonProperty]
		public string Time;

		[JsonProperty]
		public string FirstTime;

		[JsonProperty]
		public string ShowResult;

		public void Fill(Kingmaker.Tutorial.Tutorial tutorial, TutorialContext context, TutorialTriggerResult triggerResult = TutorialTriggerResult.UnknownFailure)
		{
			int num = (int)triggerResult;
			ShowResult = num.ToString();
			FirstTime = ((tutorial.ShowedTimes == 0) ? "1" : "0");
			Time = ((int)Game.Instance.Player.RealTime.TotalSeconds).ToString();
			AreaPersistentState loadedAreaState = Game.Instance.LoadedAreaState;
			Area = ((loadedAreaState == null) ? null : SimpleBlueprintExtendAsObject.Or(loadedAreaState.Blueprint, null)?.ToString()) ?? s_EmptyValue;
			Vector3 vector = context.SourceUnit?.Position ?? Game.Instance.Player.MainCharacter.Entity?.Position ?? Vector3.zero;
			Point = $"{(int)vector.x}_{(int)vector.z}";
			TutorialName = tutorial.Blueprint.name;
		}

		public void Fill(TutorialData tutorialData, TutorialTriggerResult triggerResult = TutorialTriggerResult.UnknownFailure)
		{
			Kingmaker.Tutorial.Tutorial tutorial = null;
			try
			{
				tutorial = BlueprintComponentExtendAsObject.Or(tutorialData.Trigger, null)?.Fact;
			}
			catch
			{
			}
			int num = (int)triggerResult;
			ShowResult = num.ToString();
			FirstTime = ((tutorial == null) ? "?" : ((tutorial.ShowedTimes == 0) ? "1" : "0"));
			Time = ((int)Game.Instance.Player.RealTime.TotalSeconds).ToString();
			AreaPersistentState loadedAreaState = Game.Instance.LoadedAreaState;
			Area = ((loadedAreaState == null) ? null : SimpleBlueprintExtendAsObject.Or(loadedAreaState.Blueprint, null)?.ToString()) ?? s_EmptyValue;
			Vector3 vector = tutorialData.SourceUnit?.Position ?? Game.Instance.Player.MainCharacter.Entity?.Position ?? Vector3.zero;
			Point = $"{(int)vector.x}_{(int)vector.z}";
			TutorialName = tutorialData.Blueprint.name;
		}

		public override string GetSerializedString()
		{
			return TutorialName + ";" + Area + ";" + Point + ";" + Time + ";" + FirstTime + ";" + ShowResult;
		}

		public override bool CanMerge(MergeableStatistic other)
		{
			if (other is TutorialStatistic tutorialStatistic)
			{
				return Area == tutorialStatistic.Area;
			}
			return false;
		}

		public override void Merge(MergeableStatistic other)
		{
			base.Merge(other);
			if (other is TutorialStatistic tutorialStatistic)
			{
				TutorialName = TutorialName + "," + tutorialStatistic.TutorialName;
				Point = Point + "," + tutorialStatistic.Point;
				Time = Time + "," + tutorialStatistic.Time;
				FirstTime = FirstTime + "," + tutorialStatistic.FirstTime;
				ShowResult = ShowResult + "," + tutorialStatistic.ShowResult;
			}
		}

		public override void UnmergeLast()
		{
			base.UnmergeLast();
			TutorialName = TutorialName.Substring(0, TutorialName.LastIndexOf(','));
			Point = Point.Substring(0, Point.LastIndexOf(','));
			Time = Time.Substring(0, Time.LastIndexOf(','));
			FirstTime = FirstTime.Substring(0, FirstTime.LastIndexOf(','));
			ShowResult = ShowResult.Substring(0, ShowResult.LastIndexOf(','));
		}
	}

	[JsonObject]
	public class MoneyFlowStatistic : MergeableStatistic
	{
		public enum ActionType
		{
			Buy,
			Sell,
			Quest,
			Loot
		}

		[JsonProperty]
		public string PlayerLevel;

		[JsonProperty]
		public string Area;

		[JsonProperty]
		public string Source;

		[JsonProperty]
		public ActionType Operation;

		[JsonProperty]
		public string AssetName;

		[JsonProperty]
		public string Value;

		[JsonProperty]
		public string Count;

		public void Fill(ActionType operation, string source, string assetName, float value, int count)
		{
			Operation = operation;
			AreaPersistentState loadedAreaState = Game.Instance.LoadedAreaState;
			Area = ((loadedAreaState == null) ? null : SimpleBlueprintExtendAsObject.Or(loadedAreaState.Blueprint, null)?.ToString()) ?? s_EmptyValue;
			Source = source ?? s_EmptyValue;
			AssetName = assetName;
			Value = value.ToString();
			Count = count.ToString();
			PlayerLevel = LevelUpController.GetEffectiveLevel(Game.Instance.Player.MainCharacterEntity).ToString();
		}

		public override string GetSerializedString()
		{
			return $"{PlayerLevel};{Area};{Source};{(int)Operation};{AssetName};{Value};{Count}";
		}

		public override bool CanMerge(MergeableStatistic other)
		{
			if (other is MoneyFlowStatistic moneyFlowStatistic && PlayerLevel == moneyFlowStatistic.PlayerLevel && Area == moneyFlowStatistic.Area && Source == moneyFlowStatistic.Source)
			{
				return Operation == moneyFlowStatistic.Operation;
			}
			return false;
		}

		public override void Merge(MergeableStatistic other)
		{
			base.Merge(other);
			if (other is MoneyFlowStatistic moneyFlowStatistic)
			{
				AssetName = AssetName + "," + moneyFlowStatistic.AssetName;
				Value = Value + "," + moneyFlowStatistic.Value;
				Count = Count + "," + moneyFlowStatistic.Count;
			}
		}

		public override void UnmergeLast()
		{
			base.UnmergeLast();
			AssetName = AssetName.Substring(0, AssetName.LastIndexOf(','));
			Value = Value.Substring(0, Value.LastIndexOf(','));
			Count = Count.Substring(0, Count.LastIndexOf(','));
		}
	}

	private static readonly bool s_ActuallySendReports = true;

	private static readonly bool s_TrackInitialLevel = true;

	private static readonly bool s_CheckOnlyAliveMobs = true;

	private static readonly string s_EmptyValue = "<not found>";

	private static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
	{
		Formatting = Formatting.None,
		Converters = { (JsonConverter)new DictionaryConverter() }
	};

	private static readonly JsonSerializer MainThreadSerializer = JsonSerializer.Create(Settings);

	[JsonProperty]
	public Dictionary<string, List<MergeableStatistic>> m_StatisticsBuffers = new Dictionary<string, List<MergeableStatistic>>();

	public Dictionary<string, StatisticLimit> m_StatisticsLimits = new Dictionary<string, StatisticLimit>();

	[JsonProperty]
	public PlayTimeStatistic m_PlayTimeStatistic = new PlayTimeStatistic();

	[JsonProperty]
	public HashSet<string> m_PoiCaptured = new HashSet<string>();

	[JsonProperty]
	public HashSet<string> m_TutorialBanned = new HashSet<string>();

	[JsonProperty]
	public Dictionary<string, long> m_SentPackets = new Dictionary<string, long>();

	[JsonProperty]
	public GameMetaData Meta;

	private static GameMetaData s_CachedMeta;

	[JsonProperty]
	public string m_gameSessionGUID = Guid.NewGuid().ToString();

	[JsonProperty]
	public string m_AppStatus = string.Empty;

	[JsonIgnore]
	public AppType m_AppType;

	[JsonProperty]
	public string m_DeviceUniqueIdentifier = string.Empty;

	[JsonProperty]
	public int m_CurrentLevel = -1;

	private bool m_FirstTime = true;

	private TimeSpan m_LastGameTime = TimeSpan.Zero;

	private TimeSpan m_LastRealTime = TimeSpan.Zero;

	private bool m_lastIsInCombat;

	private FullScreenUIType m_ActiveFullScreenUIType;

	public static JsonSerializer Serializer
	{
		get
		{
			if (!UnitySyncContextHolder.IsInUnity)
			{
				return JsonSerializer.Create(Settings);
			}
			return MainThreadSerializer;
		}
	}

	public GameStatistic()
	{
		EventBus.Subscribe(this);
		Register(typeof(PlayTimeStatistic), 10);
		Register(typeof(LevelupPlayerStatistic), 4);
		Register(typeof(LevelupInventoryStatistic), 6);
		Register(typeof(CharacterNotGoodStatistic), 10);
		Register(typeof(LoadStatistic), 5);
		Register(typeof(MoneyFlowStatistic), 14);
		Register(typeof(TutorialStatistic), 20);
	}

	public void Register(Type statisticType, int reportsPerHour)
	{
		string name = statisticType.Name;
		m_StatisticsLimits[name] = new StatisticLimit(reportsPerHour);
		m_StatisticsBuffers[name] = new List<MergeableStatistic>();
	}

	public void Dispose()
	{
		EventBus.Unsubscribe(this);
	}

	private void CreateCustomEvent(string eventName, Dictionary<string, object> data)
	{
		if (s_ActuallySendReports && this == Game.Instance.Statistic)
		{
			AnalyticsHelper.SendEvent(eventName, data);
		}
	}

	public void Tick(Game gameInstance, bool isMainMenu = false)
	{
		if (!AnalyticsHelper.IsStatisticsEnabled)
		{
			return;
		}
		try
		{
			if (gameInstance.Player != null)
			{
				if (m_FirstTime)
				{
					m_LastGameTime = gameInstance.Player.GameTime;
					m_LastRealTime = gameInstance.Player.RealTime;
					m_lastIsInCombat = gameInstance.Player.IsInCombat;
					m_FirstTime = false;
				}
				else
				{
					AddTime(gameInstance, gameInstance.Player.GameTime - m_LastGameTime, gameInstance.Player.RealTime - m_LastRealTime, isMainMenu);
					m_LastGameTime = gameInstance.Player.GameTime;
					m_LastRealTime = gameInstance.Player.RealTime;
				}
			}
		}
		catch (Exception ex)
		{
			PFLog.GameStatistics.Exception(ex);
		}
	}

	private void AddTime(Game gameInstance, TimeSpan gameTime, TimeSpan realTime, bool isMainMenu)
	{
		m_PlayTimeStatistic.TickTimers(gameInstance, m_ActiveFullScreenUIType, gameTime, realTime, isMainMenu);
		if (m_lastIsInCombat != gameInstance.Player.IsInCombat && gameInstance.Player.IsInCombat)
		{
			AddEnterCombat();
		}
		m_lastIsInCombat = gameInstance.Player.IsInCombat;
		if (!isMainMenu)
		{
			CheckTimeAndReport();
		}
	}

	private void CheckTimeAndReport()
	{
		List<string> list = TempList.Get<string>();
		int num = 0;
		foreach (string key in m_StatisticsLimits.Keys)
		{
			if (m_StatisticsLimits[key].CanBeReported())
			{
				if (m_StatisticsBuffers[key].Empty())
				{
					num++;
				}
				else
				{
					ReportMergeableStatistic(key, m_StatisticsBuffers[key]);
				}
				m_StatisticsLimits[key].UpdateReportedTime();
			}
			else if (!m_StatisticsBuffers[key].Empty())
			{
				list.Add(key);
			}
		}
		foreach (string item in list)
		{
			if (num > 0)
			{
				num--;
				ReportMergeableStatistic(item, m_StatisticsBuffers[item]);
				continue;
			}
			break;
		}
	}

	private void ReportMergeableStatistic(string eventName)
	{
		ReportMergeableStatistic(eventName, m_StatisticsBuffers[eventName]);
		m_StatisticsLimits[eventName].UpdateReportedTime();
	}

	public static MergeableStatistic GetMergedFirstPacks(List<MergeableStatistic> statistics, int maxSerializedLength)
	{
		if (statistics.Empty())
		{
			return null;
		}
		MergeableStatistic mergeableStatistic = null;
		foreach (MergeableStatistic statistic in statistics)
		{
			if (mergeableStatistic == null)
			{
				if (statistic.GetSerializedString().Length > maxSerializedLength)
				{
					return null;
				}
				mergeableStatistic = statistic;
				continue;
			}
			mergeableStatistic.Merge(statistic);
			if (mergeableStatistic.GetSerializedString().Length <= maxSerializedLength)
			{
				continue;
			}
			mergeableStatistic.UnmergeLast();
			break;
		}
		statistics.RemoveRange(0, mergeableStatistic.MergedCount);
		return mergeableStatistic;
	}

	public void ReportMergeableStatistic(string reportName, List<MergeableStatistic> statistics)
	{
		if (!statistics.Empty())
		{
			int num = 400;
			int num2 = 7;
			int num3 = 0;
			int num4 = 0;
			Dictionary<string, object> dictionary = new Dictionary<string, object>();
			MergeableStatistic mergedFirstPacks = GetMergedFirstPacks(statistics, num - 1);
			while (mergedFirstPacks != null && num4 < num2)
			{
				string serializedString = mergedFirstPacks.GetSerializedString();
				dictionary.Add(num4.ToString(), serializedString);
				num4++;
				num3 += serializedString.Length + 1;
				mergedFirstPacks = GetMergedFirstPacks(statistics, num - num3);
			}
			if (dictionary.Keys.Count > 0)
			{
				CreateCustomEvent(reportName, dictionary);
			}
		}
	}

	private void AddStatisticToBuffer(MergeableStatistic statstic)
	{
		if (AnalyticsHelper.IsStatisticsEnabled && m_StatisticsLimits.ContainsKey(statstic.GetType().Name) && m_StatisticsLimits[statstic.GetType().Name].ReportsPerHourLimit != 0)
		{
			m_StatisticsBuffers[statstic.GetType().Name].Add(statstic);
		}
	}

	private static string GetUnitClass(BaseUnitEntity unitEntityData)
	{
		return s_EmptyValue;
	}

	private static string GetUnitMythicClass(BaseUnitEntity unitEntityData)
	{
		try
		{
			BlueprintCharacterClass blueprintCharacterClass = unitEntityData.Progression.Classes.Find((ClassData x) => x.CharacterClass.IsMythic)?.CharacterClass;
			return (blueprintCharacterClass != null) ? $"{blueprintCharacterClass.name} {unitEntityData.Progression.GetClassLevel(blueprintCharacterClass)}" : "no_mythic";
		}
		catch (Exception ex)
		{
			PFLog.GameStatistics.Exception(ex);
		}
		return s_EmptyValue;
	}

	private static string GetItemNameWithUniqueMarker(ItemEntity item)
	{
		if (item.Blueprint.Description.IsNullOrEmpty())
		{
			return item.Blueprint.name;
		}
		return item.Blueprint.name + "@";
	}

	private void AddLevelUp(Game gameInstance, bool endGameFake = false)
	{
		try
		{
			BaseUnitEntity baseUnitEntity = gameInstance.Player.MainCharacter.Entity.ToBaseUnitEntity();
			int effectiveLevel = LevelUpController.GetEffectiveLevel(baseUnitEntity);
			string text = Guid.NewGuid().ToString();
			LevelupPlayerStatistic levelupPlayerStatistic = new LevelupPlayerStatistic();
			levelupPlayerStatistic.Fill(effectiveLevel, baseUnitEntity, text ?? "", endGameFake);
			AddStatisticToBuffer(levelupPlayerStatistic);
			LevelupInventoryStatistic levelupInventoryStatistic = new LevelupInventoryStatistic();
			levelupInventoryStatistic.Fill(baseUnitEntity.Body, "Player", text ?? "", effectiveLevel, endGameFake);
			AddStatisticToBuffer(levelupInventoryStatistic);
			foreach (BaseUnitEntity activeCompanion in gameInstance.Player.ActiveCompanions)
			{
				effectiveLevel = LevelUpController.GetEffectiveLevel(activeCompanion);
				levelupInventoryStatistic = new LevelupInventoryStatistic();
				levelupInventoryStatistic.Fill(activeCompanion.Body, activeCompanion.Blueprint.name, text ?? "", effectiveLevel, endGameFake);
				AddStatisticToBuffer(levelupInventoryStatistic);
			}
		}
		catch (Exception ex)
		{
			PFLog.GameStatistics.Exception(ex);
		}
	}

	private static MobStatistic GetBoss()
	{
		MobStatistic mobStatistic = new MobStatistic();
		if (Game.Instance.Player.MainCharacter.Entity != null)
		{
			int num = 0;
			string name = s_EmptyValue;
			string characterName = s_EmptyValue;
			UnitLifeState lifeState = UnitLifeState.Conscious;
			int currentHP = 0;
			int maxHP = 0;
			bool flag = false;
			foreach (UnitGroup readyForCombatUnitGroup in Game.Instance.ReadyForCombatUnitGroups)
			{
				for (int i = 0; i < readyForCombatUnitGroup.Count; i++)
				{
					BaseUnitEntity baseUnitEntity = readyForCombatUnitGroup[i];
					if (baseUnitEntity != null && baseUnitEntity != null && (!s_CheckOnlyAliveMobs || !baseUnitEntity.LifeState.IsDead) && baseUnitEntity.CombatGroup.IsEnemy(Game.Instance.Player.MainCharacter.Entity.ToBaseUnitEntity()) && (!flag || num < baseUnitEntity.Blueprint.CR))
					{
						num = baseUnitEntity.Blueprint.CR;
						name = baseUnitEntity.Blueprint.name;
						characterName = baseUnitEntity.Blueprint.CharacterName;
						lifeState = baseUnitEntity.LifeState.State;
						currentHP = baseUnitEntity.Health.HitPointsLeft;
						maxHP = baseUnitEntity.Health.MaxHitPoints;
						flag = true;
					}
				}
			}
			if (flag)
			{
				mobStatistic.Fill(num, name, characterName, lifeState, currentHP, maxHP);
			}
		}
		return mobStatistic;
	}

	private static string GetPartyString(List<CharacterStatistic> characters)
	{
		StringBuilder stringBuilder = new StringBuilder();
		foreach (CharacterStatistic character in characters)
		{
			if (stringBuilder.Length > 0)
			{
				stringBuilder.Append('|');
			}
			stringBuilder.Append($"{character.m_Name}|{(int)character.m_LifeState}|{character.m_CurrentHP}|{character.m_MaxHP}");
		}
		return stringBuilder.ToString();
	}

	private void AddPartyCharacterNotGood(Game gameInstance, BaseUnitEntity unitEntityData, bool mainCharacter)
	{
		try
		{
			CharacterNotGoodStatistic characterNotGoodStatistic = new CharacterNotGoodStatistic();
			characterNotGoodStatistic.Fill(unitEntityData, mainCharacter);
			AddStatisticToBuffer(characterNotGoodStatistic);
		}
		catch (Exception ex)
		{
			PFLog.GameStatistics.Exception(ex);
		}
	}

	private static void AddCharacter(List<CharacterStatistic> characters, bool mainCharacter, BaseUnitEntity unitEntityData)
	{
		try
		{
			if (unitEntityData != null && unitEntityData.Blueprint != null)
			{
				CharacterStatistic characterStatistic = new CharacterStatistic();
				characterStatistic.Fill(mainCharacter, unitEntityData);
				characters.Add(characterStatistic);
			}
		}
		catch (Exception ex)
		{
			PFLog.GameStatistics.Exception(ex);
		}
	}

	private void AddLoad()
	{
		try
		{
			LoadStatistic loadStatistic = new LoadStatistic();
			loadStatistic.Fill();
			AddStatisticToBuffer(loadStatistic);
		}
		catch (Exception ex)
		{
			PFLog.GameStatistics.Exception(ex);
		}
	}

	private void AddEnterCombat()
	{
		try
		{
			EnterCombatStatistic enterCombatStatistic = new EnterCombatStatistic();
			enterCombatStatistic.Fill();
			AddStatisticToBuffer(enterCombatStatistic);
		}
		catch (Exception ex)
		{
			PFLog.GameStatistics.Exception(ex);
		}
	}

	private void TrackUnitNotGood(Game gameInstance, BaseUnitEntity unitEntityData)
	{
		if (gameInstance.Player.MainCharacter.Entity == unitEntityData)
		{
			AddPartyCharacterNotGood(gameInstance, unitEntityData, mainCharacter: true);
			return;
		}
		foreach (BaseUnitEntity activeCompanion in gameInstance.Player.ActiveCompanions)
		{
			if (activeCompanion == unitEntityData)
			{
				AddPartyCharacterNotGood(gameInstance, unitEntityData, mainCharacter: false);
				break;
			}
		}
	}

	private void TrackGainXp(Game gameInstance, int gainded)
	{
	}

	private void UpdateAppStatus(AppStatus appStatus)
	{
		if (string.Equals(m_AppStatus, appStatus.Get(), StringComparison.Ordinal))
		{
			m_AppType = AppType.Default;
		}
		else
		{
			m_AppType = AppType.NonDefault;
		}
	}

	private void CreateAppStatus(AppStatus appStatus)
	{
		if (m_AppType == AppType.Default)
		{
			m_AppStatus = appStatus.Get();
		}
		else
		{
			m_AppStatus = appStatus.Randomize();
		}
	}

	private void PreLoad()
	{
		AddLoad();
	}

	private void PostLoad(GameStatistic old, AppStatus appStatus, Game gameInstance)
	{
		UpdateAppStatus(appStatus);
		if (m_gameSessionGUID.Equals(old.m_gameSessionGUID))
		{
			m_PlayTimeStatistic = old.m_PlayTimeStatistic;
			m_StatisticsBuffers = old.m_StatisticsBuffers;
			m_PoiCaptured = old.m_PoiCaptured;
			m_TutorialBanned = old.m_TutorialBanned;
			m_SentPackets = old.m_SentPackets;
		}
		foreach (string key in m_StatisticsLimits.Keys)
		{
			if (!m_StatisticsBuffers.ContainsKey(key))
			{
				m_StatisticsBuffers[key] = new List<MergeableStatistic>();
			}
			else if (m_StatisticsBuffers[key].Count > 100)
			{
				PFLog.GameStatistics.Error($"Too many [{key}] events: {m_StatisticsBuffers[key].Count}");
			}
		}
	}

	public void PreSave(AppStatus appStatus)
	{
		m_DeviceUniqueIdentifier = GameVersion.DeviceUniqueIdentifier;
		CreateAppStatus(appStatus);
		if (s_CachedMeta == null)
		{
			s_CachedMeta = GameMetaData.Create(ReportDllChecksumManager.GetDllCRC(), ReportDllChecksumManager.IsUnityModManagerActive());
		}
		Meta = s_CachedMeta;
	}

	public void HandleFullScreenUiChanged(bool active, FullScreenUIType fullScreenUIType)
	{
		m_ActiveFullScreenUIType = (active ? fullScreenUIType : FullScreenUIType.Unknown);
	}

	public void Quit()
	{
	}

	public void Reset()
	{
	}

	public static string Serialize(SaveInfo saveInfo, AppStatus appStatus)
	{
		return Serializer.SerializeObject(Game.Instance.Statistic);
	}

	public static void PreDeserialize(SaveInfo saveInfo)
	{
		try
		{
			Game.Instance.Statistic.PreLoad();
		}
		catch (Exception ex)
		{
			PFLog.GameStatistics.Exception(ex);
		}
	}

	public static void Deserialize(SaveInfo saveInfo, string data, AppStatus appStatus)
	{
		GameStatistic statistic = Game.Instance.Statistic;
		EventBus.Unsubscribe(statistic);
		try
		{
			Game.Instance.Statistic = Serializer.DeserializeObject<GameStatistic>(data) ?? new GameStatistic();
		}
		catch (Exception arg)
		{
			PFLog.GameStatistics.Error($"Failed to read game statistic from save, creating new. Error: {arg}");
			Game.Instance.Statistic = new GameStatistic();
		}
		Game.Instance.Statistic.PostLoad(statistic, appStatus, Game.Instance);
	}

	public void HandleMoneyFlow(string source, string assetName, MoneyFlowStatistic.ActionType operation, long value, int count = 1)
	{
		try
		{
			MoneyFlowStatistic moneyFlowStatistic = new MoneyFlowStatistic();
			moneyFlowStatistic.Fill(operation, source, assetName, value, count);
			AddStatisticToBuffer(moneyFlowStatistic);
		}
		catch (Exception ex)
		{
			PFLog.GameStatistics.Exception(ex);
		}
	}

	public void HandleVendorDeal(MechanicEntity vendor, IEnumerable<ItemEntity> items)
	{
		try
		{
			string source = SimpleBlueprintExtendAsObject.Or(vendor.Blueprint, null)?.name ?? s_EmptyValue;
			Dictionary<string, MoneyFlowStatistic> dictionary = new Dictionary<string, MoneyFlowStatistic>();
			foreach (ItemEntity item in items)
			{
				if (dictionary.ContainsKey(item.Blueprint.name))
				{
					int num = int.Parse(dictionary[item.Blueprint.name].Count);
					dictionary[item.Blueprint.name].Count = (num + item.Count).ToString();
				}
				else
				{
					float itemBuyPrice = Game.Instance.Vendor.GetItemBuyPrice(item);
					dictionary[item.Blueprint.name] = new MoneyFlowStatistic();
					dictionary[item.Blueprint.name].Fill(MoneyFlowStatistic.ActionType.Buy, source, item.Blueprint.name, itemBuyPrice, item.Count);
				}
			}
			foreach (MoneyFlowStatistic value in dictionary.Values)
			{
				AddStatisticToBuffer(value);
			}
		}
		catch (Exception ex)
		{
			PFLog.GameStatistics.Exception(ex);
		}
	}

	public void HandleUnitLifeStateChanged(UnitLifeState prevLifeState)
	{
		try
		{
			if (this == Game.Instance.Statistic)
			{
				BaseUnitEntity baseUnitEntity = EventInvokerExtensions.BaseUnitEntity;
				if (baseUnitEntity != null && !baseUnitEntity.LifeState.IsConscious)
				{
					TrackUnitNotGood(Game.Instance, baseUnitEntity);
				}
			}
		}
		catch (Exception ex)
		{
			PFLog.GameStatistics.Exception(ex);
		}
	}

	public void HandleUnitGainExperience(int gained, bool withSound = false)
	{
		try
		{
			if (this == Game.Instance.Statistic && EventInvokerExtensions.BaseUnitEntity == Game.Instance.Player.MainCharacterEntity)
			{
				TrackGainXp(Game.Instance, gained);
			}
		}
		catch (Exception ex)
		{
			PFLog.GameStatistics.Exception(ex);
		}
	}

	public void HandleLevelUpStart(BaseUnitEntity unit, Action onCommit = null, Action onStop = null, LevelUpState.CharBuildMode mode = LevelUpState.CharBuildMode.LevelUp)
	{
		if (mode == LevelUpState.CharBuildMode.LevelUp)
		{
			m_PlayTimeStatistic.CurrentLevelUpType = PlayTimeStatistic.OtherStateType.LevelUp;
		}
		if (mode == LevelUpState.CharBuildMode.CharGen)
		{
			m_PlayTimeStatistic.CurrentLevelUpType = PlayTimeStatistic.OtherStateType.Chargen;
		}
	}

	public void HandleLevelUpComplete(bool isChargen)
	{
		m_PlayTimeStatistic.CurrentLevelUpType = PlayTimeStatistic.OtherStateType.None;
	}

	public void OnAreaBeginUnloading()
	{
		if (!m_PlayTimeStatistic.IsEmpty())
		{
			AddStatisticToBuffer(m_PlayTimeStatistic);
			m_PlayTimeStatistic = new PlayTimeStatistic();
		}
	}

	public void OnAreaDidLoad()
	{
	}

	public void HandleGameOver(Player.GameOverReasonType reason)
	{
		if (reason == Player.GameOverReasonType.Won)
		{
			AddLevelUp(Game.Instance, endGameFake: true);
			string name = typeof(LevelupPlayerStatistic).Name;
			while (!m_StatisticsBuffers[name].Empty())
			{
				m_StatisticsBuffers[name].SwapItemsAt(0, m_StatisticsBuffers[name].Count - 1);
				ReportMergeableStatistic(name);
			}
			name = typeof(LevelupInventoryStatistic).Name;
			while (!m_StatisticsBuffers[name].Empty())
			{
				m_StatisticsBuffers[name].SwapItemsAt(0, m_StatisticsBuffers[name].Count - 1);
				ReportMergeableStatistic(name);
			}
		}
	}

	public void AddTutorialStatistic(Kingmaker.Tutorial.Tutorial tutorial, TutorialContext context, TutorialStatistic.TutorialTriggerResult triggerResult)
	{
		try
		{
			if (!tutorial.Blueprint.DisableAnalyticsTracking)
			{
				TutorialStatistic tutorialStatistic = new TutorialStatistic();
				tutorialStatistic.Fill(tutorial, context, triggerResult);
				AddStatisticToBuffer(tutorialStatistic);
			}
		}
		catch (Exception ex)
		{
			PFLog.GameStatistics.Exception(ex);
		}
	}

	public void HandleLimitReached(Kingmaker.Tutorial.Tutorial tutorial, TutorialContext context)
	{
		AddTutorialStatistic(tutorial, context, TutorialStatistic.TutorialTriggerResult.LimitReached);
	}

	public void HandleTagBanned(Kingmaker.Tutorial.Tutorial tutorial, TutorialContext context)
	{
		if (!m_TutorialBanned.Contains(tutorial.Blueprint.name))
		{
			m_TutorialBanned.Add(tutorial.Blueprint.name);
			AddTutorialStatistic(tutorial, context, TutorialStatistic.TutorialTriggerResult.TagBanned);
		}
	}

	public void HandleFrequencyReached(Kingmaker.Tutorial.Tutorial tutorial, TutorialContext context)
	{
		AddTutorialStatistic(tutorial, context, TutorialStatistic.TutorialTriggerResult.FrequencyCounting);
	}

	public void HandleHigherPriorityCooldown(Kingmaker.Tutorial.Tutorial tutorial, TutorialContext context)
	{
		AddTutorialStatistic(tutorial, context, TutorialStatistic.TutorialTriggerResult.HigherPriorityCooldown);
	}

	public void HandleLowerOrEqualPriorityCooldown(Kingmaker.Tutorial.Tutorial tutorial, TutorialContext context)
	{
		AddTutorialStatistic(tutorial, context, TutorialStatistic.TutorialTriggerResult.LowerOrEqualPriorityCooldown);
	}

	public void ShowTutorial(TutorialData data)
	{
		try
		{
			TutorialStatistic tutorialStatistic = new TutorialStatistic();
			tutorialStatistic.Fill(data, TutorialStatistic.TutorialTriggerResult.Success);
			AddStatisticToBuffer(tutorialStatistic);
		}
		catch (Exception ex)
		{
			PFLog.GameStatistics.Exception(ex);
		}
	}

	public void HideTutorial(TutorialData data)
	{
	}
}
