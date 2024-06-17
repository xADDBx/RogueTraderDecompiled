using Kingmaker.EntitySystem.Entities.Base;
using UnityEngine;

namespace Kingmaker.View;

public class LocatorView : EntityViewBase
{
	public override bool CreatesDataOnLoad => true;

	public override Entity CreateEntityData(bool load)
	{
		return Entity.Initialize(new LocatorEntity(this));
	}

	protected override void OnDrawGizmos()
	{
		Gizmos.DrawIcon(base.ViewTransform.position, "locator");
		Gizmos.color = Color.red;
		Vector3 vector = base.ViewTransform.rotation * Vector3.forward;
		Gizmos.DrawLine(base.ViewTransform.position, base.ViewTransform.position + vector);
	}
}
