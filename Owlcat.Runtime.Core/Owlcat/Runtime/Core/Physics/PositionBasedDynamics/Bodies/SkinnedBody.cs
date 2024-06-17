using System.Collections.Generic;
using Owlcat.Runtime.Core.Physics.PositionBasedDynamics.Constraints;
using Owlcat.Runtime.Core.Physics.PositionBasedDynamics.Particles;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace Owlcat.Runtime.Core.Physics.PositionBasedDynamics.Bodies;

public class SkinnedBody : Body
{
	public NativeArray<Matrix4x4> Boneposes;

	public NativeArray<Matrix4x4> Bindposes;

	public NativeArray<int> BoneIndicesMap;

	public NativeArray<int> ParentMap;

	public Matrix4x4 RootBone;

	public int BonesOffset;

	public int BoneIndicesMapOffset;

	public SkinnedBody(string name, List<Particle> particles, NativeArray<Matrix4x4> boneposes, NativeArray<Matrix4x4> bindposes, NativeArray<int> boneIndicesMap, NativeArray<int> parentMap, List<Constraint> constraints, List<int2> disconnectedConstraintsOffsetCount)
		: base(name, particles, constraints, disconnectedConstraintsOffsetCount)
	{
		Boneposes = boneposes;
		Bindposes = bindposes;
		ParentMap = parentMap;
		BoneIndicesMap = boneIndicesMap;
	}

	public override void Dispose()
	{
		Boneposes.Dispose();
		Bindposes.Dispose();
		ParentMap.Dispose();
		BoneIndicesMap.Dispose();
	}
}
