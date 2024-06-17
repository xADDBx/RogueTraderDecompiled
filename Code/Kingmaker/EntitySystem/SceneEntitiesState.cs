using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Persistence.JsonUtility;
using Kingmaker.Networking.Serialization;
using Kingmaker.Utility.DotNetExtensions;
using Newtonsoft.Json;
using Owlcat.Runtime.Core.Logging;
using Owlcat.Runtime.Core.Utility;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Kingmaker.EntitySystem;

[HashRoot]
public class SceneEntitiesState : IHashable
{
	public class DisposeInProgress : ContextFlag<DisposeInProgress>
	{
	}

	public const int CycleLengthA = 5;

	public const int CycleLengthB = 8;

	public const int CompleteCycleLength = 40;

	[JsonProperty]
	public string SceneName;

	private readonly List<Entity> m_EntityData = new List<Entity>();

	[JsonProperty]
	public bool HasEntityData { get; set; }

	[JsonProperty(PropertyName = "m_EntityData")]
	private List<Entity> EntityData
	{
		get
		{
			if (ContextData<GameStateSerializationContext>.Current != null)
			{
				if (!ContextData<GameStateSerializationContext>.Current.SplitState)
				{
					return m_EntityData;
				}
				int count = m_EntityData.Count;
				int num = count / 8 + 1;
				int num2 = Game.Instance.RealTimeController.CurrentNetworkTick / 5 % 8 * num;
				int num3 = Mathf.Min(num2 + num, count);
				List<Entity> list = TempList.Get<Entity>();
				list.IncreaseCapacity(num3 - num2);
				for (int i = num2; i < num3; i++)
				{
					Entity item = m_EntityData[i];
					list.Add(item);
				}
				return list;
			}
			return m_EntityData;
		}
	}

	public bool SkipSerialize { get; set; }

	public List<Entity> AllEntityData => m_EntityData;

	public bool IsSceneLoaded => SceneManager.GetSceneByName(SceneName).isLoaded;

	public bool IsSceneLoadedThreadSafe { get; set; }

	public bool IsPostLoadExecuted { get; private set; }

	public static event Action<SceneEntitiesState, Entity> OnAdded;

	public static event Action<SceneEntitiesState, Entity> OnRemoved;

	public static void ClearSubscriptions()
	{
		SceneEntitiesState.OnAdded = null;
		SceneEntitiesState.OnRemoved = null;
	}

	public SceneEntitiesState(string sceneName)
	{
		SceneName = sceneName;
		IsPostLoadExecuted = true;
	}

	[UsedImplicitly]
	private SceneEntitiesState(JsonConstructorMark _)
	{
	}

	public void Dispose()
	{
		using (ContextData<DisposeInProgress>.Request())
		{
			foreach (Entity item in AllEntityData.ToList())
			{
				if (item.HoldingState == this)
				{
					RemoveEntityData(item);
					item.Dispose();
				}
			}
			IsSceneLoadedThreadSafe = false;
		}
	}

	public void AddEntityData([NotNull] Entity data)
	{
		if (m_EntityData.HasItem((Entity e) => e.UniqueId == data.UniqueId))
		{
			PFLog.Default.Error($"Can't add {data} to state {SceneName}: duplicate id {data.UniqueId}");
			return;
		}
		m_EntityData.Add(data);
		data.SetHoldingState(this);
		SceneEntitiesState.OnAdded?.Invoke(this, data);
	}

	public void RemoveEntityData([NotNull] Entity data)
	{
		data.SetHoldingState(null);
		if (m_EntityData.Remove(data))
		{
			SceneEntitiesState.OnRemoved?.Invoke(this, data);
		}
	}

	public void PostLoad()
	{
		if (IsPostLoadExecuted)
		{
			PFLog.System.Error("AreaPersistentState.PostLoad: already executed");
			return;
		}
		IsPostLoadExecuted = true;
		using (new UtilityTimer("PrePostLoad"))
		{
			foreach (Entity entityDatum in m_EntityData)
			{
				entityDatum.PrePostLoad();
			}
		}
		using (new UtilityTimer("PostLoad"))
		{
			foreach (Entity entityDatum2 in m_EntityData)
			{
				try
				{
					entityDatum2.PostLoad();
				}
				catch (Exception ex)
				{
					PFLog.Default.Exception(ex);
					entityDatum2.SetHoldingState(null);
				}
			}
			foreach (Entity entityDatum3 in m_EntityData)
			{
				try
				{
					ApplyPostLoadFixes(entityDatum3);
				}
				catch (Exception ex2)
				{
					PFLog.Default.Exception(ex2);
					entityDatum3.SetHoldingState(null);
				}
			}
		}
		using (new UtilityTimer("AddToState"))
		{
			foreach (Entity entityDatum4 in m_EntityData)
			{
				if (entityDatum4 is MechanicEntity { Blueprint: null })
				{
					Game.Instance.Player.BrokenEntities.Add(entityDatum4.UniqueId);
					entityDatum4.SetHoldingState(null);
					UberDebug.LogError("Broken unit in state {0}, id: {1}", SceneName, entityDatum4.UniqueId);
				}
				else
				{
					SceneEntitiesState.OnAdded?.Invoke(this, entityDatum4);
					entityDatum4.SetHoldingState(this);
				}
			}
		}
	}

	private void ApplyPostLoadFixes(Entity data)
	{
		using (new UtilityTimer("ApplyPostLoadFixes"))
		{
			using (new UtilityTimer("Entities"))
			{
				data.ApplyPostLoadFixes();
			}
		}
	}

	public void PreSave()
	{
		foreach (Entity entityDatum in m_EntityData)
		{
			try
			{
				entityDatum.PreSave();
			}
			catch (Exception ex)
			{
				PFLog.Default.Exception(ex);
			}
		}
	}

	public void Subscribe()
	{
		foreach (Entity entityDatum in m_EntityData)
		{
			try
			{
				entityDatum.Subscribe();
			}
			catch (Exception ex)
			{
				PFLog.Default.Exception(ex);
			}
		}
	}

	public void Unsubscribe()
	{
		foreach (Entity entityDatum in m_EntityData)
		{
			try
			{
				entityDatum.Unsubscribe();
			}
			catch (Exception ex)
			{
				PFLog.Default.Exception(ex);
			}
		}
	}

	public override string ToString()
	{
		return GetType().Name + "#" + SceneName;
	}

	public virtual Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		result.Append(SceneName);
		bool val = HasEntityData;
		result.Append(ref val);
		List<Entity> entityData = EntityData;
		if (entityData != null)
		{
			for (int i = 0; i < entityData.Count; i++)
			{
				Hash128 val2 = ClassHasher<Entity>.GetHash128(entityData[i]);
				result.Append(ref val2);
			}
		}
		return result;
	}
}
