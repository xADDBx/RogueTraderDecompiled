using Kingmaker.Utility.Random;
using UnityEngine;

namespace Kingmaker.Visual.Particles;

public class ArrowDeflectorSurface : MonoBehaviour
{
	public Bounds Bounds;

	public float BackImpulseMin = 1f;

	public float BackImpulseMax = 3f;

	public float ShieldImpulseMin = 7f;

	public float ShieldImpulseMax = 12f;

	public float TorqueXMin;

	public float TorqueXMax = 30f;

	public float TorqueYMin = -3f;

	public float TorqueYMax = 3f;

	public Vector3 GetRandomArrowLocalPosition()
	{
		return Bounds.center + new Vector3(PFStatefulRandom.Visuals.Particles.Range(0f - Bounds.extents.x, Bounds.extents.x), PFStatefulRandom.Visuals.Particles.Range(0f - Bounds.extents.y, Bounds.extents.z), PFStatefulRandom.Visuals.Particles.Range(0f - Bounds.extents.y, Bounds.extents.z));
	}

	public void ApplyDeflectImpulse(Rigidbody arrow, Vector3 impactPoint)
	{
		arrow.AddForceAtPosition(-arrow.transform.forward * PFStatefulRandom.Visuals.Particles.Range(BackImpulseMin, BackImpulseMax), impactPoint, ForceMode.Impulse);
		arrow.AddForceAtPosition(base.transform.forward * PFStatefulRandom.Visuals.Particles.Range(ShieldImpulseMin, ShieldImpulseMax), impactPoint, ForceMode.Impulse);
		arrow.AddRelativeTorque(PFStatefulRandom.Visuals.Particles.Range(TorqueXMin, TorqueXMax), PFStatefulRandom.Visuals.Particles.Range(TorqueYMin, TorqueYMax), 0f, ForceMode.Impulse);
	}

	public void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.green;
		Gizmos.matrix = base.transform.localToWorldMatrix;
		Gizmos.DrawWireCube(Bounds.center, Bounds.size);
		Gizmos.matrix = Matrix4x4.identity;
	}
}
