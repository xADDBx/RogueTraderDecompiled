using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Area;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.EntitySystem.Persistence.JsonUtility;
using Newtonsoft.Json;
using StateHasher.Core;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.View.MapObjects;

[KnowledgeDatabaseID("6672130d750674146b2fe0549b847142")]
public class DynamicMapObjectView : MapObjectView
{
	public new class EntityData : MapObjectEntity, IHashable
	{
		[JsonProperty]
		private Vector3 m_SavedPosition;

		[JsonProperty]
		private float m_SavedOrientation;

		private new BlueprintDynamicMapObject Blueprint => (BlueprintDynamicMapObject)base.Blueprint;

		private new BlueprintDynamicMapObject OriginalBlueprint => (BlueprintDynamicMapObject)base.OriginalBlueprint;

		public EntityData(DynamicMapObjectView mapObjectView)
			: base(mapObjectView.UniqueId, mapObjectView.IsInGameBySettings, mapObjectView.Blueprint)
		{
		}

		public EntityData(JsonConstructorMark _)
			: base(_)
		{
		}

		protected override void OnPreSave()
		{
			base.OnPreSave();
			m_SavedPosition = base.View.ViewTransform.position;
			m_SavedOrientation = base.View.ViewTransform.rotation.eulerAngles.y;
		}

		protected override IEntityViewBase CreateViewForData()
		{
			if (Blueprint.Prefab == null)
			{
				return null;
			}
			return Object.Instantiate(Blueprint.Prefab.GetComponent<EntityViewBase>(), rotation: Quaternion.Euler(0f, m_SavedOrientation, 0f), position: m_SavedPosition);
		}

		public override Hash128 GetHash128()
		{
			Hash128 result = default(Hash128);
			Hash128 val = base.GetHash128();
			result.Append(ref val);
			result.Append(ref m_SavedPosition);
			result.Append(ref m_SavedOrientation);
			return result;
		}
	}

	[HideInInspector]
	[SerializeField]
	[FormerlySerializedAs("Blueprint")]
	private BlueprintDynamicMapObjectReference m_Blueprint;

	public override bool CreatesDataOnLoad => false;

	public BlueprintDynamicMapObject Blueprint
	{
		get
		{
			return m_Blueprint?.Get();
		}
		set
		{
			m_Blueprint = value.ToReference<BlueprintDynamicMapObjectReference>();
		}
	}

	protected override MapObjectEntity CreateMapObjectEntityData(bool load)
	{
		return Entity.Initialize(new EntityData(this));
	}
}
