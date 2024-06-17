using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.View;
using Owlcat.Runtime.Core.Utility;
using StateHasher.Core;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Kingmaker.Controllers.Optimization;

public class EntityBoundsPart : EntityPart, IEntityPositionChangedHandler<EntitySubscriber>, IEntityPositionChangedHandler, ISubscriber<IEntity>, ISubscriber, IEventTag<IEntityPositionChangedHandler, EntitySubscriber>, IUnitSizeHandler<EntitySubscriber>, IUnitSizeHandler, ISubscriber<IMechanicEntity>, IEntitySubscriber, IEventTag<IUnitSizeHandler, EntitySubscriber>, IInGameHandler<EntitySubscriber>, IInGameHandler, IEventTag<IInGameHandler, EntitySubscriber>, BaseUnitEntity.IUnitAsleepHandler<EntitySubscriber>, AbstractUnitEntity.IUnitAsleepHandler, IEventTag<AbstractUnitEntity.IUnitAsleepHandler, EntitySubscriber>, EntityBoundsController.IHasTick, IHashable
{
	private readonly LinkedListNode<EntityBoundsController.IHasTick> m_UpdateNode;

	[CanBeNull]
	public CircleCollider2D SphereBoundsCollider { get; private set; }

	[CanBeNull]
	public CircleCollider2D SphereVisionCollider { get; private set; }

	public EntityBoundsPart()
	{
		m_UpdateNode = new LinkedListNode<EntityBoundsController.IHasTick>(this);
	}

	protected override void OnViewDidAttach()
	{
		base.OnViewDidAttach();
		SetupObjectCollision();
		if (!base.Owner.IsInGame)
		{
			HandleObjectInGameChanged();
		}
	}

	protected override void OnViewWillDetach()
	{
		base.OnViewWillDetach();
		DestroyColliders();
		Game.Instance.EntityBoundsController.CancelUpdate(m_UpdateNode);
	}

	private void DestroyColliders()
	{
		if ((bool)SphereBoundsCollider)
		{
			UnityEngine.Object.Destroy(SphereBoundsCollider.gameObject);
			SphereBoundsCollider = null;
		}
		if ((bool)SphereVisionCollider)
		{
			UnityEngine.Object.Destroy(SphereVisionCollider.gameObject);
			SphereVisionCollider = null;
		}
	}

	public void HandleEntityPositionChanged()
	{
		ScheduleUpdate();
	}

	public void HandleUnitSizeChanged()
	{
		ScheduleUpdate();
	}

	private void ScheduleUpdate()
	{
		if ((bool)SphereBoundsCollider || (bool)SphereVisionCollider)
		{
			Game.Instance.EntityBoundsController.ScheduleUpdate(m_UpdateNode);
		}
	}

	public void Tick()
	{
		if (!(base.Owner?.View?.GO))
		{
			EntityBoundsController.Logger.Warning("Unexpected tick for {0} that has no view", base.Owner);
			return;
		}
		if ((bool)SphereBoundsCollider)
		{
			Transform transform = SphereBoundsCollider.transform;
			Vector3 vector = base.Owner.Position.To2D();
			if (transform.position != vector)
			{
				transform.position = vector;
			}
		}
		if ((bool)SphereVisionCollider)
		{
			Transform transform2 = SphereVisionCollider.transform;
			Vector3 vector2 = base.Owner.Position.To2D();
			if (transform2.position != vector2)
			{
				transform2.position = vector2;
			}
		}
		if (!(base.Owner is AbstractUnitEntity abstractUnitEntity))
		{
			return;
		}
		if ((bool)SphereBoundsCollider)
		{
			Transform transform3 = SphereBoundsCollider.transform;
			float num = ScaleRadius(abstractUnitEntity.Corpulence, transform3.lossyScale);
			if (Math.Abs(SphereBoundsCollider.radius - num) > 0.05f)
			{
				SphereBoundsCollider.radius = num;
			}
		}
		if ((bool)SphereVisionCollider)
		{
			float num2 = abstractUnitEntity.GetVisionOptional()?.RangeMeters ?? 0f;
			if (Math.Abs(SphereVisionCollider.radius - num2) > 0.05f)
			{
				SphereVisionCollider.radius = num2;
			}
		}
	}

	private void SetupObjectCollision()
	{
		EntityViewBase entityViewBase = base.Owner?.View as EntityViewBase;
		if (!entityViewBase)
		{
			EntityBoundsController.Logger.Error("Unexpected tick for {0} that has no view", base.Owner);
			return;
		}
		IEntity owner = base.Owner;
		float radius;
		if (!(owner is AbstractUnitEntity abstractUnitEntity))
		{
			if (owner is MapObjectEntity mapObjectEntity)
			{
				radius = Math.Max((mapObjectEntity.View.Renderers.Any((Renderer v) => v) ? (from v in mapObjectEntity.View.Renderers
					where v
					select v.bounds).Aggregate(delegate(Bounds a, Bounds b)
				{
					a.Encapsulate(b);
					return a;
				}) : default(Bounds)).extents.magnitude, 0.05f);
			}
			else
			{
				Vector3 lossyScale = entityViewBase.ViewTransform.lossyScale;
				radius = ScaleRadius(0.05f, lossyScale);
			}
		}
		else
		{
			Vector3 lossyScale2 = entityViewBase.ViewTransform.lossyScale;
			radius = ScaleRadius(abstractUnitEntity.Corpulence, lossyScale2);
			SetupSphereVisionCollider(entityViewBase, abstractUnitEntity.GetVisionOptional()?.RangeMeters);
		}
		SetupSphereBoundsCollider(entityViewBase, radius);
	}

	private static float ScaleRadius(float radius, Vector3 lossyScale)
	{
		float num = Math.Max(Math.Abs(lossyScale.x), Math.Abs(lossyScale.z));
		return Math.Max(0.05f, radius * num);
	}

	private void SetupSphereBoundsCollider(EntityViewBase view, float radius)
	{
		GameObject obj = new GameObject(view.name + "_SphereBounds")
		{
			layer = 24
		};
		SceneManager.MoveGameObjectToScene(obj, Game.Instance.EntityBoundsController.Scene);
		CircleCollider2D circleCollider2D = obj.AddComponent<CircleCollider2D>();
		circleCollider2D.radius = radius;
		circleCollider2D.transform.position = view.ViewTransform.position.To2D();
		SphereBoundsCollider = circleCollider2D;
		circleCollider2D.gameObject.AddComponent<EntityDataLink>().Entity = base.ConcreteOwner;
		Rigidbody2D rigidbody2D = circleCollider2D.gameObject.AddComponent<Rigidbody2D>();
		rigidbody2D.bodyType = RigidbodyType2D.Kinematic;
		rigidbody2D.interpolation = RigidbodyInterpolation2D.None;
	}

	private void SetupSphereVisionCollider(EntityViewBase view, float? radius)
	{
		if (radius.HasValue)
		{
			GameObject obj = new GameObject(view.name + "_SphereVision")
			{
				layer = 23
			};
			SceneManager.MoveGameObjectToScene(obj, Game.Instance.EntityBoundsController.Scene);
			CircleCollider2D circleCollider2D = obj.AddComponent<CircleCollider2D>();
			circleCollider2D.radius = radius.Value;
			circleCollider2D.transform.position = view.ViewTransform.position.To2D();
			circleCollider2D.isTrigger = true;
			SphereVisionCollider = circleCollider2D;
			Rigidbody2D rigidbody2D = circleCollider2D.gameObject.AddComponent<Rigidbody2D>();
			rigidbody2D.bodyType = RigidbodyType2D.Kinematic;
			rigidbody2D.interpolation = RigidbodyInterpolation2D.None;
			circleCollider2D.gameObject.AddComponent<EntityViewTrigger>().Unit = (BaseUnitEntity)base.Owner;
		}
	}

	public void HandleObjectInGameChanged()
	{
		if ((bool)SphereBoundsCollider)
		{
			SphereBoundsCollider.gameObject.SetActive(base.Owner.IsInGame);
		}
		if ((bool)SphereVisionCollider)
		{
			SphereVisionCollider.gameObject.SetActive(base.Owner.IsInGame && (!(base.Owner is BaseUnitEntity baseUnitEntity) || !baseUnitEntity.IsSleeping));
		}
	}

	public void OnIsSleepingChanged(bool sleeping)
	{
		HandleObjectInGameChanged();
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
