using System.Collections.Generic;
using Owlcat.Runtime.Core.Physics.PositionBasedDynamics.Bodies;
using Owlcat.Runtime.Core.Physics.PositionBasedDynamics.Constraints;
using Owlcat.Runtime.Core.Physics.PositionBasedDynamics.Particles;
using Unity.Mathematics;
using UnityEngine;

namespace Owlcat.Runtime.Core.Physics.PositionBasedDynamics.Scene;

public abstract class PBDBodyBase : MonoBehaviour
{
	protected Body m_Body;

	[SerializeField]
	[HideInInspector]
	protected bool m_IsStatic;

	[SerializeField]
	protected float m_Restitution = 1f;

	[SerializeField]
	protected float m_Friction;

	[SerializeField]
	protected float m_TeleportDistanceTreshold;

	[SerializeField]
	protected List<PBDColliderBase> m_LocalColliders = new List<PBDColliderBase>();

	[SerializeField]
	protected List<Particle> m_Particles = new List<Particle>();

	[SerializeField]
	protected List<Constraint> m_Constraints = new List<Constraint>();

	[SerializeField]
	protected List<int2> m_DisconnectedConstraintsOffsetCount = new List<int2>();

	[SerializeField]
	private BodyInitializationMode m_BodyInitializationMode;

	[SerializeField]
	public List<PBDColliderBase> LocalColliders
	{
		get
		{
			return m_LocalColliders;
		}
		set
		{
			m_LocalColliders = value;
		}
	}

	public List<Particle> Particles => m_Particles;

	public List<Constraint> Constraints => m_Constraints;

	public List<int2> DisconnectedConstraintsOffsetCount => m_DisconnectedConstraintsOffsetCount;

	public BodyInitializationMode BodyInitializationMode
	{
		get
		{
			return m_BodyInitializationMode;
		}
		set
		{
			m_BodyInitializationMode = value;
		}
	}

	public float Restitution => m_Restitution;

	public float Friction => m_Friction;

	public float TeleportDistanceTreshold => m_TeleportDistanceTreshold;

	public bool IsStatic => m_IsStatic;

	public Body GetBody()
	{
		return m_Body;
	}

	protected void RegisterLocalColliders()
	{
		foreach (PBDColliderBase localCollider in m_LocalColliders)
		{
			if (localCollider != null)
			{
				localCollider.RegisterAsLocalCollider(this);
			}
		}
	}

	protected void UnregisterLocalColliders()
	{
		foreach (PBDColliderBase localCollider in m_LocalColliders)
		{
			if (localCollider != null)
			{
				localCollider.UnregisterAsLocalCollider(this);
			}
		}
		PBD.UnregisterLocalColliders(m_Body);
	}

	private void OnEnable()
	{
		PBD.RegisterSceneBody(this);
		if (m_BodyInitializationMode == BodyInitializationMode.OnEnable)
		{
			Initialize();
		}
	}

	private void OnDisable()
	{
		UnregisterLocalColliders();
		PBD.UnregisterSceneBody(this);
		if (m_Body != null)
		{
			Dispose();
		}
	}

	internal virtual void OnBeforeSimulationTick()
	{
		if (m_Body != null)
		{
			m_Body.LocalToWorld = base.transform.localToWorldMatrix;
		}
	}

	protected internal virtual void OnBodyDataUpdated()
	{
	}

	internal void DoUpdate()
	{
		if (m_Body != null && !PBD.IsGpu)
		{
			UpdateInternal();
		}
	}

	protected virtual void UpdateInternal()
	{
	}

	public void Initialize()
	{
		if (ValidateBeforeInitialize())
		{
			InitializeInternal();
		}
	}

	protected abstract void InitializeInternal();

	protected abstract bool ValidateBeforeInitialize();

	protected virtual void Dispose()
	{
	}

	public void MarkAsStatic()
	{
		m_IsStatic = true;
	}
}
public abstract class PBDBodyBase<T> : PBDBodyBase where T : Body
{
	public T Body
	{
		get
		{
			return (T)m_Body;
		}
		protected set
		{
			m_Body = value;
		}
	}
}
