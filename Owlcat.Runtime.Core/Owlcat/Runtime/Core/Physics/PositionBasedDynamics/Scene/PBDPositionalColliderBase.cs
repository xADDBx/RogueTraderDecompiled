using UnityEngine;

namespace Owlcat.Runtime.Core.Physics.PositionBasedDynamics.Scene;

public abstract class PBDPositionalColliderBase : PBDColliderBase
{
	[SerializeField]
	private bool m_IsGlobal;

	public override bool IsGlobal => m_IsGlobal;

	protected override void DoUpdateOverride()
	{
		base.DoUpdateOverride();
		if (m_ColliderRef.IsGlobal != m_IsGlobal)
		{
			if (m_ColliderRef.IsGlobal)
			{
				PBD.UnregisterCollider(m_ColliderRef);
			}
			m_ColliderRef.IsGlobal = m_IsGlobal;
			if (m_ColliderRef.IsGlobal)
			{
				PBD.RegisterCollider(m_ColliderRef);
			}
		}
	}
}
