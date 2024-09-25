using Owlcat.Runtime.Core.Physics.PositionBasedDynamics.Collisions;
using Owlcat.Runtime.Core.Registry;
using Owlcat.Runtime.Core.Updatables;
using UnityEngine;

namespace Owlcat.Runtime.Core.Physics.PositionBasedDynamics.Scene;

public abstract class PBDColliderBase : RegisteredBehaviour<PBDColliderBase>, IUpdatable
{
	protected ColliderRef m_ColliderRef = new ColliderRef();

	private PBDBodyBase m_Owner;

	public float Restitution = 1f;

	public float Friction;

	public virtual bool IsGlobal => false;

	public abstract ColliderType GetColliderType();

	protected override void OnEnabled()
	{
		m_ColliderRef.IsGlobal = IsGlobal;
		m_ColliderRef.Type = GetColliderType();
		if (m_ColliderRef.IsGlobal)
		{
			PBD.RegisterCollider(m_ColliderRef);
		}
	}

	protected override void OnDisabled()
	{
		PBD.UnregisterCollider(m_ColliderRef);
	}

	internal void RegisterAsLocalCollider(PBDBodyBase owner)
	{
		m_ColliderRef.IsGlobal = IsGlobal;
		if (m_ColliderRef.IsGlobal)
		{
			Debug.LogError("Collider " + base.name + " is global but the " + owner.name + " trying to register it as local", owner);
		}
		else if (m_Owner != null && m_Owner != owner)
		{
			Debug.LogError($"Collider {base.name} is already registered by {m_Owner} but the {owner.name} trying to register it as its own", owner);
		}
		else
		{
			m_Owner = owner;
			m_ColliderRef.Owner = owner.GetBody();
			if (base.enabled)
			{
				PBD.RegisterCollider(m_ColliderRef);
			}
		}
	}

	internal void UnregisterAsLocalCollider(PBDBodyBase owner)
	{
		m_ColliderRef.IsGlobal = IsGlobal;
		if (m_ColliderRef.IsGlobal && m_ColliderRef.IsGlobal)
		{
			Debug.LogError("Collider " + base.name + " is global but the " + owner.name + " trying to unregister it as local", owner);
		}
		else if (m_Owner != null && m_Owner != owner)
		{
			Debug.LogError($"Collider {base.name} is already registered by {m_Owner} but the {owner.name} trying to unregister it as its own", owner);
		}
		else
		{
			m_Owner = null;
			if (base.enabled)
			{
				PBD.UnregisterCollider(m_ColliderRef);
			}
		}
	}

	public void DoUpdate()
	{
		if (IsGlobal || m_Owner != null)
		{
			DoUpdateOverride();
		}
	}

	protected virtual void DoUpdateOverride()
	{
		m_ColliderRef.Friction = Friction;
		m_ColliderRef.Restitution = Restitution;
	}
}
