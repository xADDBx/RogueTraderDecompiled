using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.EntitySystem.Persistence.JsonUtility;
using Kingmaker.Items;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.ResourceLinks.BaseInterfaces;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility;
using Kingmaker.Visual.HitSystem;
using Newtonsoft.Json;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.View.MapObjects;

[RequireComponent(typeof(InteractionLoot))]
[KnowledgeDatabaseID("e543a12d94400d9448819b9e7206cf65")]
public class DroppedLoot : MapObjectView, IResource
{
	public class EntityPartBreathOfMoney : EntityPart<EntityData>, IAreaHandler, ISubscriber, IHashable
	{
		public void OnAreaBeginUnloading()
		{
			Game.Instance.EntityDestroyer.Destroy(base.Owner);
		}

		public void OnAreaDidLoad()
		{
		}

		public override Hash128 GetHash128()
		{
			Hash128 result = default(Hash128);
			Hash128 val = base.GetHash128();
			result.Append(ref val);
			return result;
		}
	}

	public new class EntityData : MapObjectEntity, IHashable
	{
		[JsonProperty]
		private Vector3 m_SavedPosition;

		[JsonProperty]
		private EntityRef<Entity> m_DroppedBy;

		[JsonProperty]
		public BloodType BloodType { get; set; }

		[JsonProperty]
		public SurfaceType SurfaceType { get; set; }

		[JsonProperty]
		public bool IsDismember { get; set; }

		[JsonProperty]
		public bool IsDroppedByPlayer { get; set; }

		public ItemsCollection Loot
		{
			get
			{
				return GetOrCreate<InteractionLootPart>()?.Loot;
			}
			set
			{
				GetOrCreate<InteractionLootPart>().Loot = value;
			}
		}

		public EntityRef<Entity> DroppedBy
		{
			get
			{
				return m_DroppedBy;
			}
			set
			{
				IsDroppedByPlayer = (value.Entity?.GetOptional<PartFaction>()?.IsPlayer).GetValueOrDefault();
				m_DroppedBy = value;
			}
		}

		public override ViewHandlingOnDisposePolicyType DefaultViewHandlingOnDisposePolicy => ViewHandlingOnDisposePolicyType.FadeOutAndDestroy;

		public EntityData(MapObjectView mapObjectView)
			: base(mapObjectView)
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
		}

		protected override IEntityViewBase CreateViewForData()
		{
			return Object.Instantiate(GetPrefab(), m_SavedPosition, Quaternion.identity);
		}

		private EntityViewBase GetPrefab()
		{
			EntityViewBase entityViewBase = null;
			if (IsDismember)
			{
				entityViewBase = BlueprintRoot.Instance.HitSystemRoot.GetDismemberLoot(SurfaceType);
			}
			if ((bool)GetOptional<EntityPartBreathOfMoney>())
			{
				entityViewBase = BlueprintRoot.Instance.Prefabs.BreathOfMoneyLootBag;
			}
			if (entityViewBase == null)
			{
				entityViewBase = BlueprintRoot.Instance.Prefabs.DroppedLootBag;
			}
			return entityViewBase;
		}

		public override Hash128 GetHash128()
		{
			Hash128 result = default(Hash128);
			Hash128 val = base.GetHash128();
			result.Append(ref val);
			result.Append(ref m_SavedPosition);
			EntityRef<Entity> obj = m_DroppedBy;
			Hash128 val2 = StructHasher<EntityRef<Entity>>.GetHash128(ref obj);
			result.Append(ref val2);
			BloodType val3 = BloodType;
			result.Append(ref val3);
			SurfaceType val4 = SurfaceType;
			result.Append(ref val4);
			bool val5 = IsDismember;
			result.Append(ref val5);
			bool val6 = IsDroppedByPlayer;
			result.Append(ref val6);
			return result;
		}
	}

	public ItemsCollection Loot
	{
		get
		{
			return Data.Loot;
		}
		set
		{
			Data.Loot = value;
		}
	}

	public bool IsSkinningDisabled => Data.DroppedBy.Entity == null;

	public bool IsDroppedByPlayer => Data.IsDroppedByPlayer;

	public EntityRef<Entity> DroppedBy
	{
		get
		{
			return Data.DroppedBy;
		}
		set
		{
			Data.DroppedBy = value;
		}
	}

	public new EntityData Data => (EntityData)base.Data;

	public override void HandleHoverChange(bool isHover)
	{
		base.HandleHoverChange(isHover);
		MassLootHelper.HighlightLoot(this, isHover);
	}

	protected override MapObjectEntity CreateMapObjectEntityData(bool load)
	{
		return Entity.Initialize(new EntityData(this));
	}
}
