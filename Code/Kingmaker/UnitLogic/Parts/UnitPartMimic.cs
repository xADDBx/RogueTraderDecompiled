using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Mechanics;
using Newtonsoft.Json;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.Parts;

public class UnitPartMimic : BaseUnitPart, IHashable
{
	[JsonProperty]
	private string m_AmbushObjectEntityId;

	private MapObjectEntity m_AmbushObjectEntity;

	[CanBeNull]
	public MapObjectEntity AmbushObject
	{
		get
		{
			if (string.IsNullOrEmpty(m_AmbushObjectEntityId))
			{
				return null;
			}
			if (m_AmbushObjectEntity == null || !m_AmbushObjectEntity.IsInState)
			{
				foreach (MapObjectEntity mapObject in Game.Instance.State.MapObjects)
				{
					if (mapObject.UniqueId == m_AmbushObjectEntityId)
					{
						m_AmbushObjectEntity = mapObject;
					}
				}
			}
			return m_AmbushObjectEntity;
		}
	}

	public bool AmbushObjectAttached => !string.IsNullOrEmpty(m_AmbushObjectEntityId);

	public void AttachAmbushObject(MapObjectEntity ambushObject)
	{
		if (AmbushObjectAttached)
		{
			PFLog.Default.Error("Ambush object is already attached to mimic " + base.Owner.ToString());
			return;
		}
		m_AmbushObjectEntityId = ambushObject.UniqueId;
		m_AmbushObjectEntity = ambushObject;
	}

	public void HideAmbushObject()
	{
		if (AmbushObject != null)
		{
			AmbushObject.IsInGame = false;
		}
		m_AmbushObjectEntityId = null;
		m_AmbushObjectEntity = null;
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		result.Append(m_AmbushObjectEntityId);
		return result;
	}
}
