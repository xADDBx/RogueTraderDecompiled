using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.View;
using Kingmaker.View.MapObjects;
using Owlcat.Runtime.Core.Utility;
using StateHasher.Core;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Kingmaker.Controllers.Optimization;

public class AreaEffectBoundsPart : EntityPart<AreaEffectEntity>, IInGameHandler<EntitySubscriber>, IInGameHandler, ISubscriber<IEntity>, ISubscriber, IEventTag<IInGameHandler, EntitySubscriber>, EntityBoundsController.IHasTick, AreaEffectEntity.IUnitWithinBoundsHandler, IHashable
{
	private static readonly HashSet<UnitReference> Empty = new HashSet<UnitReference>();

	[CanBeNull]
	private CircleCollider2D SphereVisionCollider { get; set; }

	[CanBeNull]
	private AreaEffectTrigger Trigger { get; set; }

	public HashSet<UnitReference> Inside
	{
		get
		{
			if (!Trigger)
			{
				return Empty;
			}
			return Trigger.Inside;
		}
	}

	public HashSet<UnitReference> Entered
	{
		get
		{
			if (!Trigger)
			{
				return Empty;
			}
			return Trigger.Entered;
		}
	}

	public HashSet<UnitReference> Exited
	{
		get
		{
			if (!Trigger)
			{
				return Empty;
			}
			return Trigger.Exited;
		}
	}

	public void ClearDelta()
	{
		if ((bool)Trigger)
		{
			Trigger.ClearDelta();
		}
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
	}

	private void DestroyColliders()
	{
		if ((bool)SphereVisionCollider)
		{
			UnityEngine.Object.Destroy(SphereVisionCollider.gameObject);
			SphereVisionCollider = null;
			Trigger = null;
		}
	}

	public void Tick()
	{
		AreaEffectView areaEffectView = base.Owner?.View;
		if (!areaEffectView)
		{
			EntityBoundsController.Logger.Warning("Unexpected tick for {0} that has no view", base.Owner);
		}
		else if ((bool)SphereVisionCollider)
		{
			Bounds bounds = areaEffectView.Shape.GetBounds();
			Transform transform = SphereVisionCollider.transform;
			Vector3 vector = bounds.center.To2D();
			if (transform.position != vector)
			{
				transform.position = vector;
			}
			float magnitude = bounds.extents.magnitude;
			Vector3 lossyScale = areaEffectView.ViewTransform.lossyScale;
			magnitude = ScaleRadius(magnitude, lossyScale);
			if (Math.Abs(SphereVisionCollider.radius - magnitude) > 0.05f)
			{
				SphereVisionCollider.radius = magnitude;
			}
		}
	}

	private void SetupObjectCollision()
	{
		AreaEffectView areaEffectView = base.Owner?.View;
		if (!areaEffectView)
		{
			EntityBoundsController.Logger.Error("Unexpected tick for {0} that has no view", base.Owner);
			return;
		}
		float magnitude = areaEffectView.Shape.GetBounds().extents.magnitude;
		Vector3 lossyScale = areaEffectView.ViewTransform.lossyScale;
		magnitude = ScaleRadius(magnitude, lossyScale);
		SetupSphereVisionCollider(areaEffectView, magnitude);
	}

	private static float ScaleRadius(float radius, Vector3 lossyScale)
	{
		float num = Math.Max(Math.Abs(lossyScale.x), Math.Abs(lossyScale.z));
		return Math.Max(0.05f, radius * num);
	}

	private void SetupSphereVisionCollider(EntityViewBase view, float radius)
	{
		GameObject obj = new GameObject(view.name + "_SphereVision")
		{
			layer = 23
		};
		SceneManager.MoveGameObjectToScene(obj, Game.Instance.EntityBoundsController.Scene);
		CircleCollider2D circleCollider2D = obj.AddComponent<CircleCollider2D>();
		circleCollider2D.radius = radius;
		circleCollider2D.transform.position = view.ViewTransform.position.To2D();
		circleCollider2D.isTrigger = true;
		SphereVisionCollider = circleCollider2D;
		AreaEffectTrigger areaEffectTrigger = circleCollider2D.gameObject.AddComponent<AreaEffectTrigger>();
		areaEffectTrigger.Unit = base.Owner;
		Trigger = areaEffectTrigger;
	}

	public void HandleObjectInGameChanged()
	{
		if ((bool)SphereVisionCollider)
		{
			SphereVisionCollider.gameObject.SetActive(base.Owner.IsInGame);
		}
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
