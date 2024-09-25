using System;
using UnityEngine;

namespace Kingmaker.Visual.CharacterSystem.Dismemberment;

[Serializable]
public class DismembermentPieceDescriptor
{
	public Transform Root;

	public Transform Skeleton;

	public Rigidbody[] ImpulseRigidBodies;

	public Collider[] Colliders;

	public CharacterJoint[] Joints;

	public Vector3 Impulse;

	public Vector2 ImpulseMultiplier = new Vector2(4f, 6f);

	public Vector2 IncomingImpulseMultiplier = new Vector2(1f, 1.2f);

	public Vector2 ChildrenImpulseMultiplier = new Vector2(0.4f, 0.5f);

	public float PieceMass = 1f;
}
