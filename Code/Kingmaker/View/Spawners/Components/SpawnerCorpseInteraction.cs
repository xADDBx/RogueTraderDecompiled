using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Area;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.StateHasher.Hashers;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.View.MapObjects;
using Newtonsoft.Json;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.View.Spawners.Components;

[RequireComponent(typeof(UnitSpawnerBase))]
public class SpawnerCorpseInteraction : EntityPartComponent<SpawnerCorpseInteraction.Part>
{
	public class Part : ViewBasedPart, IUnitInitializer, IHashable
	{
		public void OnSpawn(AbstractUnitEntity unit)
		{
			unit.GetOrCreate<CorpseInteractionPart>().SetSource(base.ConcreteOwner);
		}

		public void OnInitialize(AbstractUnitEntity unit)
		{
		}

		public void OnDispose(AbstractUnitEntity unit)
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

	public class CorpseInteractionPart : BaseUnitPart, IUnitDieHandler, ISubscriber<IAbstractUnitEntity>, ISubscriber, IAreaHandler, IHashable
	{
		[JsonProperty]
		private EntityRef m_SourceRef;

		[JsonProperty]
		public EntityRef<DynamicMapObjectView.EntityData> InteractionObjectRef { get; private set; }

		public void SetSource(Entity source)
		{
			m_SourceRef = source?.Ref ?? default(EntityRef);
		}

		void IUnitDieHandler.OnUnitDie()
		{
			if (InteractionObjectRef == null || InteractionObjectRef.IsNull)
			{
				AddInteraction();
			}
		}

		void IAreaHandler.OnAreaBeginUnloading()
		{
		}

		void IAreaHandler.OnAreaDidLoad()
		{
			if (InteractionObjectRef != null && !InteractionObjectRef.IsNull)
			{
				AddInteraction();
			}
		}

		protected override void OnAttach()
		{
			base.Owner.Features.SuppressedDismember.Retain();
			base.Owner.Features.SuppressedDecomposition.Retain();
		}

		protected override void OnDetach()
		{
			base.Owner.Features.SuppressedDismember.Release();
			base.Owner.Features.SuppressedDecomposition.Release();
		}

		protected override void OnPostLoad()
		{
			OnAttach();
		}

		private void AddInteraction()
		{
			EntityViewBase entityViewBase = (EntityViewBase)(m_SourceRef.Entity?.View);
			if (entityViewBase == null)
			{
				return;
			}
			SpawnerCorpseInteraction component = entityViewBase.GetComponent<SpawnerCorpseInteraction>();
			if (!(component == null))
			{
				DynamicMapObjectView.EntityData entityData = EnsureObject(component);
				InteractionSkillCheck component2 = entityData.View.GetComponent<InteractionSkillCheck>();
				if (!(component2 == null))
				{
					component2.EnsurePart().SetSettings(component.m_Settings);
					InteractionObjectRef = entityData;
				}
			}
		}

		private DynamicMapObjectView.EntityData EnsureObject(SpawnerCorpseInteraction source)
		{
			if (InteractionObjectRef != null)
			{
				return InteractionObjectRef.Entity;
			}
			BlueprintDynamicMapObject blueprint = source.m_ObjectReference.Get();
			SceneEntitiesState mainState = Game.Instance.LoadedAreaState.MainState;
			DynamicMapObjectView.EntityData entityData = Game.Instance.EntitySpawner.SpawnMapObject(blueprint, base.Owner.Position, Quaternion.identity, mainState);
			entityData.IsInGame = source.m_EnableInteractionOnDeath;
			return entityData;
		}

		public override Hash128 GetHash128()
		{
			Hash128 result = default(Hash128);
			Hash128 val = base.GetHash128();
			result.Append(ref val);
			EntityRef obj = m_SourceRef;
			Hash128 val2 = EntityRefHasher.GetHash128(ref obj);
			result.Append(ref val2);
			EntityRef<DynamicMapObjectView.EntityData> obj2 = InteractionObjectRef;
			Hash128 val3 = StructHasher<EntityRef<DynamicMapObjectView.EntityData>>.GetHash128(ref obj2);
			result.Append(ref val3);
			return result;
		}
	}

	[SerializeField]
	private InteractionSkillCheckSettings m_Settings;

	[SerializeField]
	private BlueprintDynamicMapObjectReference m_ObjectReference;

	[SerializeField]
	private bool m_EnableInteractionOnDeath = true;
}
