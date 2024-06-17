using Owlcat.Runtime.Core.Physics.PositionBasedDynamics.Scene;
using UnityEngine;

[ExecuteInEditMode]
public class ClothCollider : MonoBehaviour
{
	public enum ClothColliderBodyPartType
	{
		None,
		Body,
		LeftArm,
		RightArm,
		LeftLeg,
		RightLeg
	}

	public Collider clothColliderCpu;

	public PBDColliderCapsule clothColliderGpu;

	public ClothColliderBodyPartType bodyPartType;
}
