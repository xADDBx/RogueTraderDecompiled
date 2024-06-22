using System;
using Newtonsoft.Json;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.AreaLogic.SceneControllables;

[Serializable]
[HashRoot]
public class ControllableReference : IHashable
{
	[HideInInspector]
	public string EntityNameInEditor;

	[HideInInspector]
	[JsonProperty("_entity_id")]
	public string UniqueId;

	[HideInInspector]
	[JsonProperty("SceneAssetGuid")]
	public string SceneAssetGuid;

	public bool TryGetValue(out ControllableComponent controllable)
	{
		if (!Game.Instance.SceneControllables.TryGetControllable(UniqueId, out controllable))
		{
			PFLog.Default.Error("[Controllables] No controllable with id " + UniqueId);
			return false;
		}
		return true;
	}

	public virtual Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		result.Append(UniqueId);
		result.Append(SceneAssetGuid);
		return result;
	}
}
