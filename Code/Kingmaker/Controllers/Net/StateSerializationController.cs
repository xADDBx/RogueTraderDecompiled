using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Code.Visual.Animation;
using Core.Cheats;
using Kingmaker.Controllers.Interfaces;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.Networking;
using Kingmaker.Networking.Serialization;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.Settings.Interfaces;
using Kingmaker.Signals;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.Utility.CodeTimer;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Utility.Random;
using Kingmaker.Utility.Serialization;
using Kingmaker.Utility.StatefulRandom;
using Kingmaker.Utility.UnityExtensions;
using Kingmaker.View;
using Kingmaker.View.Mechanics.Entities;
using Newtonsoft.Json;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.Visual.Effects.WeatherSystem;
using UnityEngine;

namespace Kingmaker.Controllers.Net;

public class StateSerializationController : IControllerTick, IController, IControllerStart, IControllerStop
{
	[JsonObject(IsReference = false)]
	public struct UnitMovementAgentData
	{
		[JsonProperty]
		public string unitId;

		[JsonProperty]
		public float speed;

		[JsonProperty]
		public float? maxSpeedOverride;

		[JsonProperty]
		public float? currentSpeedMps;

		[JsonProperty]
		public float? modifiedSpeedMps;

		[JsonProperty]
		public float? commandOverrideSpeed;

		[JsonProperty]
		public WalkSpeedType? movementType;

		[JsonProperty]
		public bool? isAccelerated;

		public static List<UnitMovementAgentData> GetUnitMovementAgentData()
		{
			List<UnitMovementAgentBase> allAgents = UnitMovementAgentBase.AllAgents;
			List<UnitMovementAgentData> list = TempList.Get<UnitMovementAgentData>();
			list.IncreaseCapacity(allAgents.Count);
			int i = 0;
			for (int count = allAgents.Count; i < count; i++)
			{
				UnitMovementAgentBase unitMovementAgentBase = allAgents[i];
				AbstractUnitEntityView unit2 = unitMovementAgentBase.Unit;
				if (!(unit2 == null) && unit2.Data != null)
				{
					list.Add(new UnitMovementAgentData
					{
						unitId = unit2.UniqueId,
						speed = unitMovementAgentBase.Speed,
						maxSpeedOverride = unitMovementAgentBase.MaxSpeedOverride,
						currentSpeedMps = unit2.EntityData?.Movable.CurrentSpeedMps,
						modifiedSpeedMps = unit2.EntityData?.Movable.ModifiedSpeedMps,
						commandOverrideSpeed = GetCommandOverrideSpeed(unit2),
						movementType = GetMovementType(unit2),
						isAccelerated = IsAccelerated(unit2)
					});
				}
			}
			list.Sort((UnitMovementAgentData a, UnitMovementAgentData b) => string.Compare(a.unitId, b.unitId, StringComparison.Ordinal));
			return list;
			static float? GetCommandOverrideSpeed(AbstractUnitEntityView unit)
			{
				if (unit == null || unit.EntityData == null)
				{
					return null;
				}
				return (unit.EntityData.Movable.ConcreteOwner.GetOptional<PartUnitCommands>()?.Current)?.OverrideSpeed;
			}
			static WalkSpeedType? GetMovementType(AbstractUnitEntityView unit)
			{
				if (unit == null || unit.EntityData == null)
				{
					return null;
				}
				return (unit.EntityData.Movable.ConcreteOwner.GetOptional<PartUnitCommands>()?.Current)?.MovementType;
			}
			static bool? IsAccelerated(AbstractUnitEntityView unit)
			{
				if (unit == null || unit.EntityData == null)
				{
					return null;
				}
				return (unit.EntityData.Movable.ConcreteOwner.GetOptional<PartUnitCommands>()?.Current)?.IsAccelerated;
			}
		}
	}

	[JsonObject(IsReference = false)]
	public struct UnitCommandsData
	{
		[JsonProperty]
		public string type;

		[JsonProperty]
		public long startTime;

		[JsonProperty]
		public string executor;

		[JsonProperty]
		public long timeSinceStart;

		[JsonProperty]
		public bool isSynchronized;

		[JsonProperty(IsReference = false)]
		public Vector3 position;

		public static List<UnitCommandsData> Get()
		{
			List<BaseUnitEntity> list = Game.Instance.State.AllBaseUnits.ToList();
			List<UnitCommandsData> list2 = TempList.Get<UnitCommandsData>();
			foreach (BaseUnitEntity item in list)
			{
				AbstractUnitCommand current2 = item.Commands.Current;
				if (current2 != null)
				{
					list2.Add(new UnitCommandsData
					{
						type = current2.GetType().FullName,
						startTime = current2.StartTime.TotalMillisecondsLong(),
						executor = current2.Executor.UniqueId,
						timeSinceStart = current2.TimeSinceStart.Seconds().TotalMillisecondsLong(),
						isSynchronized = current2.Params.IsSynchronized,
						position = item.Position
					});
				}
				foreach (UnitCommandParams item2 in item.Commands.Queue)
				{
					list2.Add(new UnitCommandsData
					{
						type = item2.GetType().FullName,
						startTime = 0L,
						executor = null,
						timeSinceStart = 0L,
						isSynchronized = item2.IsSynchronized,
						position = item.Position
					});
				}
			}
			return list2;
		}
	}

	[JsonObject(IsReference = false)]
	public struct SettingsData
	{
		[JsonProperty]
		public string k;

		[JsonProperty]
		public string v;

		public static List<SettingsData> Get()
		{
			ISettingsEntity[] settingsForSync = PhotonManager.Settings.SettingsForSync;
			List<SettingsData> list = TempList.Get<SettingsData>();
			list.IncreaseCapacity(settingsForSync.Length);
			ISettingsEntity[] array = settingsForSync;
			foreach (ISettingsEntity settingsEntity in array)
			{
				list.Add(new SettingsData
				{
					k = settingsEntity.Key,
					v = settingsEntity.GetStringValue()
				});
			}
			return list;
		}
	}

	[JsonObject(IsReference = false)]
	private struct StateData
	{
		[JsonProperty]
		public InclemencyType weather;

		[JsonProperty]
		[GameStateIgnore]
		public int unityRandomNext;

		[JsonProperty]
		public StatefulRandom[] randoms;

		[JsonProperty]
		public List<UnitMovementAgentData> UnitMovementAgent;

		[JsonProperty]
		public List<UnitCommandsData> UnitCommands;

		[JsonProperty]
		public List<SettingsData> Settings;

		[JsonProperty]
		public Player Player;

		[JsonProperty]
		public SceneEntitiesState Party;

		[JsonProperty]
		public AreaPersistentState Area;

		[JsonProperty]
		public SignalService.SignalServiceState SignalService;
	}

	private static StateSerializationController s_Instance;

	private bool m_CollectRequested;

	private bool m_ApplyRequested;

	private string m_MainCharData;

	private bool m_SaveStateRequested;

	private string m_FolderName;

	private long m_LockStepIndex;

	private readonly StringWriter m_StringWriter = new StringWriter(CultureInfo.InvariantCulture);

	private static JsonSerializer JsonSerializer => GameStateJsonSerializer.Serializer;

	public static void SaveState(string folderName, int lockStepIndex)
	{
		if (s_Instance != null)
		{
			s_Instance.m_SaveStateRequested = true;
			s_Instance.m_FolderName = folderName;
			s_Instance.m_LockStepIndex = lockStepIndex;
		}
	}

	[Cheat]
	public static void Collect_State()
	{
		if (s_Instance != null)
		{
			s_Instance.m_CollectRequested = true;
		}
	}

	[Cheat]
	public static void Save_State()
	{
		if (s_Instance != null)
		{
			string folderName = Path.Combine(ApplicationPaths.persistentDataPath, "States");
			s_Instance.m_SaveStateRequested = true;
			s_Instance.m_FolderName = folderName;
			s_Instance.m_LockStepIndex = DateTime.Now.ToFileTime();
		}
	}

	[Cheat]
	public static void Apply_State()
	{
		if (s_Instance != null)
		{
			s_Instance.m_ApplyRequested = true;
		}
	}

	TickType IControllerTick.GetTickType()
	{
		return TickType.Simulation;
	}

	void IControllerTick.Tick()
	{
		if (m_SaveStateRequested)
		{
			m_SaveStateRequested = false;
			using (ProfileScope.New("Saving State"))
			{
				SaveState();
			}
		}
		if (m_CollectRequested)
		{
			m_CollectRequested = false;
			Collect();
		}
		if (m_ApplyRequested)
		{
			m_ApplyRequested = false;
			Apply();
		}
	}

	private void Collect()
	{
		IAbstractUnitEntity entity = Game.Instance.Player.MainCharacter.Entity;
		m_MainCharData = JsonSerializer.SerializeObject(entity, m_StringWriter);
	}

	private void Apply()
	{
		if (string.IsNullOrWhiteSpace(m_MainCharData))
		{
			throw new Exception("No Saved Data");
		}
		BaseUnitEntity mainCharacterEntity = Game.Instance.Player.MainCharacterEntity;
		using (ContextData<EntityOwnerContextData>.Request().Setup(mainCharacterEntity))
		{
			using StringReader reader = new StringReader(m_MainCharData);
			JsonSerializer.Populate(reader, mainCharacterEntity);
			using (ContextData<Entity.ForcePostLoad>.Request())
			{
				mainCharacterEntity.PostLoad();
			}
		}
	}

	public static string GetState()
	{
		if (s_Instance == null || Game.Instance == null || Game.Instance.State.PlayerState == null || Game.Instance.State.PlayerState.CrossSceneState == null || Game.Instance.State.LoadedAreaState == null)
		{
			return null;
		}
		StateData stateData = default(StateData);
		stateData.weather = Game.Instance.Player.Weather.ActualWeather;
		stateData.unityRandomNext = 0;
		stateData.randoms = PFStatefulRandom.Serializable;
		stateData.UnitMovementAgent = UnitMovementAgentData.GetUnitMovementAgentData();
		stateData.UnitCommands = UnitCommandsData.Get();
		stateData.Player = Game.Instance.State.PlayerState;
		stateData.Party = Game.Instance.State.PlayerState.CrossSceneState;
		stateData.Area = Game.Instance.State.LoadedAreaState;
		stateData.SignalService = SignalService.Instance.State;
		StateData source = stateData;
		using (ContextData<GameStateSerializationContext>.Request())
		{
			return JsonSerializer.SerializeObject(source, s_Instance.m_StringWriter);
		}
	}

	private void SaveState()
	{
		long num = m_LockStepIndex / 4;
		StateData stateData = default(StateData);
		stateData.weather = Game.Instance.Player.Weather.ActualWeather;
		stateData.unityRandomNext = UnityEngine.Random.Range(int.MinValue, int.MaxValue);
		stateData.randoms = PFStatefulRandom.Serializable;
		stateData.UnitMovementAgent = UnitMovementAgentData.GetUnitMovementAgentData();
		stateData.UnitCommands = UnitCommandsData.Get();
		stateData.Player = ((num % 6 == 0L) ? Game.Instance.State.PlayerState : null);
		stateData.Party = ((num % 6 == 2) ? Game.Instance.State.PlayerState.CrossSceneState : null);
		stateData.Area = ((num % 6 == 4) ? Game.Instance.State.LoadedAreaState : null);
		stateData.SignalService = SignalService.Instance.State;
		StateData source = stateData;
		Directory.CreateDirectory(m_FolderName);
		string path = Path.Combine(m_FolderName, $"{m_LockStepIndex:0000}.json");
		using (ProfileScope.New("Saving Json"))
		{
			using GameStateSerializationContext gameStateSerializationContext = ContextData<GameStateSerializationContext>.Request();
			gameStateSerializationContext.SplitState = true;
			string contents = JsonSerializer.SerializeObject(source, m_StringWriter);
			File.WriteAllText(path, contents);
		}
		using (ProfileScope.New("Saving ReplayLog"))
		{
		}
	}

	public void OnStart()
	{
		s_Instance = this;
	}

	public void OnStop()
	{
		s_Instance = null;
	}
}
