using System;
using Code.GameCore.Mics;
using JetBrains.Annotations;
using Kingmaker.EntitySystem.Interfaces;
using Newtonsoft.Json;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Blueprints;

[Serializable]
[HashRoot]
public class EntityReference : IHashable
{
	private IEntity m_CachedData;

	[HideInInspector]
	public string EntityNameInEditor;

	[HideInInspector]
	[JsonProperty("_entity_id")]
	public string UniqueId;

	[HideInInspector]
	[JsonProperty("SceneAssetGuid")]
	public string SceneAssetGuid;

	[CanBeNull]
	public IEntityViewBase FindView()
	{
		return FindData()?.View;
	}

	[CanBeNull]
	public IEntity FindData()
	{
		if (m_CachedData == null || !m_CachedData.IsInState)
		{
			IPersistentState service = InterfaceServiceLocator.GetService<IPersistentState>();
			m_CachedData = service.GetEntityDataFromLoadedAreaState(UniqueId);
			EntityReferenceTracker.Register(this);
		}
		return m_CachedData;
	}

	internal void DropCached()
	{
		m_CachedData = null;
	}

	public override string ToString()
	{
		return EntityNameInEditor ?? UniqueId ?? "";
	}

	public virtual Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		result.Append(UniqueId);
		result.Append(SceneAssetGuid);
		return result;
	}
}
