using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.AreaLogic;
using Kingmaker.AreaLogic.Cutscenes;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Area;
using Kingmaker.Code.AreaLogic;
using Kingmaker.Designers.EventConditionActionSystem.ContextData;
using Kingmaker.Designers.EventConditionActionSystem.Events;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Persistence;
using Kingmaker.EntitySystem.Persistence.JsonUtility;
using Kingmaker.StateHasher.Hashers;
using Kingmaker.Utility.GuidUtility;
using Newtonsoft.Json;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.EntitySystem;

[JsonObject(MemberSerialization = MemberSerialization.OptIn)]
[HashRoot]
public class AreaPersistentState : IHashable
{
	private string m_AreaGuid;

	[JsonProperty]
	private Area m_Area;

	[JsonProperty]
	private readonly SceneEntitiesState m_MainState;

	private readonly List<SceneEntitiesState> m_AddStates = new List<SceneEntitiesState>();

	[JsonProperty]
	public UnitReference ServiceCaster;

	public readonly SavedFogMasks SavedFogOfWarMasks = new SavedFogMasks();

	public readonly RuntimeAreaSettings Settings = new RuntimeAreaSettings();

	public bool ShouldLoad { get; set; }

	public AreaVailValuePart AreaVailPart => m_Area.GetOptional<AreaVailValuePart>();

	public IEnumerable<Entity> AllEntityData => GetAllSceneStates().SelectMany((SceneEntitiesState s) => s.AllEntityData);

	public BlueprintArea Blueprint
	{
		get
		{
			if (m_Area != null)
			{
				return m_Area.Blueprint;
			}
			return ResourcesLibrary.TryGetBlueprint<BlueprintArea>(m_AreaGuid);
		}
	}

	[NotNull]
	public Area Area => m_Area;

	public SceneEntitiesState MainState => m_MainState;

	public string AreaGuid => m_AreaGuid;

	public AreaPersistentState([NotNull] BlueprintArea blueprint)
	{
		m_Area = Entity.Initialize(new Area(blueprint));
		m_AreaGuid = blueprint.AssetGuid;
		m_MainState = new SceneEntitiesState(Blueprint.DynamicScene.SceneName);
		if (m_Area.GetOptional<AreaVailValuePart>() == null)
		{
			AreaVailValuePart orCreate = m_Area.GetOrCreate<AreaVailValuePart>();
			orCreate.MinimalVailForCurrentArea = blueprint.StartVailValueForLocation;
			orCreate.Vail = orCreate.MinimalVailForCurrentArea;
		}
		Uuid.Instance.CreateString();
	}

	public AreaPersistentState(string areaId)
	{
		m_AreaGuid = areaId;
		m_MainState = new SceneEntitiesState("");
	}

	public void RestoreAreaBlueprint()
	{
		if (m_Area == null)
		{
			m_Area = Entity.Initialize(new Area(Blueprint));
		}
		m_MainState.SceneName = Blueprint.DynamicScene.SceneName;
	}

	[UsedImplicitly]
	private AreaPersistentState(JsonConstructorMark _)
	{
	}

	public void Dispose()
	{
		using (ContextData<SceneEntitiesState.DisposeInProgress>.Request())
		{
			foreach (SceneEntitiesState allSceneState in GetAllSceneStates())
			{
				foreach (Entity allEntityDatum in allSceneState.AllEntityData)
				{
					if (allEntityDatum is BaseUnitEntity { IsInCombat: not false } baseUnitEntity)
					{
						baseUnitEntity.CombatState.LeaveCombat();
					}
					(allEntityDatum as CutscenePlayerData)?.Stop();
				}
			}
			m_Area?.Dispose();
			m_MainState.Dispose();
			foreach (SceneEntitiesState addState in m_AddStates)
			{
				if (addState.IsSceneLoaded)
				{
					addState.Dispose();
				}
			}
		}
	}

	[NotNull]
	public SceneEntitiesState GetStateForScene(string sceneName)
	{
		if (sceneName == Area.Blueprint.DynamicScene.SceneName)
		{
			return m_MainState;
		}
		SceneEntitiesState sceneEntitiesState = m_AddStates.SingleOrDefault((SceneEntitiesState s) => s.SceneName == sceneName);
		if (sceneEntitiesState == null)
		{
			sceneEntitiesState = new SceneEntitiesState(sceneName);
			m_AddStates.Add(sceneEntitiesState);
		}
		return sceneEntitiesState;
	}

	[NotNull]
	public SceneEntitiesState GetStateForScene(SceneReference sr)
	{
		return GetStateForScene(sr.SceneName);
	}

	public void PostLoad()
	{
		if (m_Area == null)
		{
			RestoreAreaBlueprint();
		}
		m_MainState.PostLoad();
		foreach (SceneEntitiesState addState in m_AddStates)
		{
			if (addState.IsSceneLoaded)
			{
				addState.PostLoad();
			}
		}
		m_Area.PostLoad();
	}

	public void PreSave()
	{
		m_MainState.PreSave();
		foreach (SceneEntitiesState addState in m_AddStates)
		{
			if (addState.IsSceneLoaded)
			{
				addState.PreSave();
			}
		}
		m_Area.PreSave();
	}

	public void Subscribe()
	{
		m_MainState.Subscribe();
		foreach (SceneEntitiesState addState in m_AddStates)
		{
			if (addState.IsSceneLoaded)
			{
				addState.Subscribe();
			}
		}
		m_Area.Subscribe();
	}

	public void Unsubscribe()
	{
		m_MainState.Unsubscribe();
		foreach (SceneEntitiesState addState in m_AddStates)
		{
			if (addState.IsSceneLoaded)
			{
				addState.Unsubscribe();
			}
		}
		m_Area.Unsubscribe();
	}

	public void Activate()
	{
		if (!m_Area.MainFact.Active)
		{
			m_Area.MainFact.Activate();
			return;
		}
		using (ContextData<FactData>.Request().Setup(m_Area.MainFact))
		{
			m_Area.MainFact.CallComponents(delegate(ITriggerOnLoad c)
			{
				c.TriggerOnLoad();
			});
		}
	}

	public void Deactivate()
	{
		if (m_Area.MainFact.Active)
		{
			m_Area.MainFact.Deactivate();
		}
	}

	public IEnumerable<SceneEntitiesState> GetAllSceneStates()
	{
		yield return m_MainState;
		foreach (SceneEntitiesState addState in m_AddStates)
		{
			yield return addState;
		}
	}

	public List<SceneEntitiesState> GetAdditionalSceneStates()
	{
		return m_AddStates;
	}

	public void CollectAllEntities<T>(List<T> result) where T : Entity
	{
		CollectAllEntities(m_MainState, result);
		foreach (SceneEntitiesState addState in m_AddStates)
		{
			CollectAllEntities(addState, result);
		}
	}

	private void CollectAllEntities<T>(SceneEntitiesState sceneState, List<T> result) where T : Entity
	{
		foreach (Entity allEntityDatum in sceneState.AllEntityData)
		{
			if (allEntityDatum is T item)
			{
				result.Add(item);
			}
		}
	}

	public void SetDeserializedSceneState([NotNull] SceneEntitiesState sceneState)
	{
		m_AddStates.RemoveAll((SceneEntitiesState s) => s.SceneName == sceneState.SceneName);
		m_AddStates.Add(sceneState);
	}

	public virtual Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = ClassHasher<Area>.GetHash128(m_Area);
		result.Append(ref val);
		Hash128 val2 = ClassHasher<SceneEntitiesState>.GetHash128(m_MainState);
		result.Append(ref val2);
		UnitReference obj = ServiceCaster;
		Hash128 val3 = UnitReferenceHasher.GetHash128(ref obj);
		result.Append(ref val3);
		return result;
	}
}
