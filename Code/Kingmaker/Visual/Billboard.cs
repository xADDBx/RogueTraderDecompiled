using System;
using Owlcat.Runtime.Core.Updatables;
using UnityEngine;

namespace Kingmaker.Visual;

[ExecuteInEditMode]
public class Billboard : UpdateableInEditorBehaviour
{
	public enum Axis
	{
		NegativeX,
		NegativeY,
		PositiveZ,
		PositiveX,
		PositiveY,
		NegativeZ
	}

	[Serializable]
	public class LockAxisSettings
	{
		public bool Enabled;

		public Vector3 Axis;

		public Space Space;
	}

	public Axis BillboardAxis = Axis.PositiveZ;

	[SerializeField]
	private LockAxisSettings m_LockAxis = new LockAxisSettings();

	public bool ApplyAtFirstFrameOnly;

	private bool m_ApplyedAtFirstFrame;

	protected override void OnEnabled()
	{
		DoUpdate();
	}

	protected override void OnDisabled()
	{
		m_ApplyedAtFirstFrame = false;
	}

	public override void DoUpdate()
	{
		UpdateBillboard();
	}

	public void UpdateBillboard()
	{
		if (m_ApplyedAtFirstFrame)
		{
			return;
		}
		Camera camera = Game.GetCamera();
		if (!(camera == null))
		{
			Vector3 normalized = (camera.transform.position - base.transform.position).normalized;
			Vector3 vector = camera.transform.up;
			if (m_LockAxis.Enabled)
			{
				vector = ((m_LockAxis.Space == Space.World) ? m_LockAxis.Axis.normalized : ((!(base.transform.parent != null)) ? m_LockAxis.Axis.normalized : base.transform.parent.TransformDirection(m_LockAxis.Axis.normalized)));
			}
			normalized = Vector3.Cross(Vector3.Cross(vector, normalized), vector);
			base.transform.LookAt(normalized + base.transform.position, vector);
			Quaternion quaternion = Quaternion.FromToRotation(Vector3.forward, GetBillboardAxis());
			base.transform.rotation *= quaternion;
			if (ApplyAtFirstFrameOnly)
			{
				m_ApplyedAtFirstFrame = true;
			}
		}
	}

	private Vector3 GetBillboardAxis()
	{
		return BillboardAxis switch
		{
			Axis.NegativeX => new Vector3(1f, 0f, 0f), 
			Axis.NegativeY => new Vector3(0f, 1f, 0f), 
			Axis.NegativeZ => new Vector3(0f, 0f, -1f), 
			Axis.PositiveX => new Vector3(-1f, 0f, 0f), 
			Axis.PositiveY => new Vector3(0f, -1f, 0f), 
			Axis.PositiveZ => new Vector3(0f, 0f, 1f), 
			_ => default(Vector3), 
		};
	}

	private void OnDrawGizmosSelected()
	{
		Camera camera = Game.GetCamera();
		if ((bool)camera)
		{
			Vector3 vector = camera.transform.position - base.transform.position;
			Vector3 vector2 = base.transform.forward * vector.magnitude;
			Gizmos.color = Color.red;
			Gizmos.DrawLine(base.transform.position, base.transform.position + vector);
			Gizmos.color = Color.yellow;
			Gizmos.DrawLine(base.transform.position, base.transform.position + vector2);
		}
	}
}
