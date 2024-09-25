using System;
using System.Collections.Generic;
using Owlcat.Runtime.Core.Physics.PositionBasedDynamics.Particles;
using Unity.Mathematics;
using UnityEngine;

namespace Owlcat.Runtime.Core.Physics.PositionBasedDynamics.Constraints;

[Serializable]
public struct Constraint
{
	public int index0;

	public int index1;

	public int index2;

	public int index3;

	public float4 parameters0;

	public float4 parameters1;

	public ConstraintType type;

	public int id;

	public IEnumerable<int> GetIndices()
	{
		switch (type)
		{
		case ConstraintType.Distance:
			yield return index0;
			yield return index1;
			break;
		case ConstraintType.DistanceAngular:
			yield return index0;
			yield return index1;
			break;
		case ConstraintType.TriangleBend:
			yield return index0;
			yield return index1;
			yield return index2;
			yield return index3;
			break;
		case ConstraintType.ShapeMatching:
			yield return index0;
			break;
		case ConstraintType.Grass:
			yield return index0;
			yield return index1;
			break;
		}
	}

	public void Refresh(IList<Particle> particles)
	{
		switch (type)
		{
		case ConstraintType.Distance:
			RefreshDistance(particles);
			break;
		case ConstraintType.DistanceAngular:
			RefreshDistanceAngular(particles);
			break;
		case ConstraintType.TriangleBend:
			RefreshTriangleBend(particles);
			break;
		case ConstraintType.StretchShear:
			RefreshStretchShear(particles);
			break;
		case ConstraintType.BendTwist:
			RefreshBendTwist(particles);
			break;
		case ConstraintType.ShapeMatching:
		case ConstraintType.Grass:
			break;
		}
	}

	private void RefreshDistance(IList<Particle> particles)
	{
		parameters0.w = math.distance(particles[index0].BasePosition, particles[index1].BasePosition);
	}

	private void RefreshDistanceAngular(IList<Particle> particles)
	{
		parameters1.x = math.distance(particles[index0].BasePosition, particles[index1].BasePosition);
	}

	private void RefreshTriangleBend(IList<Particle> particles)
	{
	}

	private void RefreshStretchShear(IList<Particle> particles)
	{
		float3 x = particles[index1].BasePosition - particles[index0].BasePosition;
		x = math.normalize(x);
		Quaternion quaternion = Quaternion.FromToRotation(Vector3.forward, x);
		parameters1.x = quaternion.x;
		parameters1.y = quaternion.y;
		parameters1.z = quaternion.z;
		parameters1.w = quaternion.w;
	}

	private void RefreshBendTwist(IList<Particle> particles)
	{
		Particle particle = particles[index0];
		Particle particle2 = particles[index1];
		float3 x = particle2.BasePosition - particle.BasePosition;
		x = math.normalize(x);
		Quaternion.FromToRotation(Vector3.forward, x);
		Quaternion quaternion = Quaternion.Inverse(particle.Orientation) * particle2.Orientation;
		Vector4 vector = new Vector4(quaternion.w, quaternion.x, quaternion.y, quaternion.z) + new Vector4(1f, 0f, 0f, 0f);
		if ((new Vector4(quaternion.w, quaternion.x, quaternion.y, quaternion.z) - new Vector4(1f, 0f, 0f, 0f)).sqrMagnitude > vector.sqrMagnitude)
		{
			quaternion = new Quaternion(quaternion.x * -1f, quaternion.y * -1f, quaternion.z * -1f, quaternion.w * -1f);
		}
		parameters1.x = quaternion.x;
		parameters1.y = quaternion.y;
		parameters1.z = quaternion.z;
		parameters1.w = quaternion.w;
	}

	public void SetIndicesOffset(int offset)
	{
		index0 += offset;
		index1 += offset;
		index2 += offset;
		index3 += offset;
	}

	public void SetDistanceParameters(float compressionStiffness, float stretchStiffness, bool useParentChildDirection)
	{
		parameters0.x = compressionStiffness;
		parameters0.y = stretchStiffness;
		parameters0.z = (useParentChildDirection ? 1 : 0);
	}

	public void SetDistanceAngularParameters(float compressionStiffness, float stretchStiffness, float angularStiffness, float velocityInfluence, bool useParentChildDirection)
	{
		parameters0.x = compressionStiffness;
		parameters0.y = stretchStiffness;
		parameters0.z = angularStiffness;
		parameters0.w = velocityInfluence;
		parameters1.y = (useParentChildDirection ? 1 : 0);
	}

	public void SetShapeMatchingParameters(float shapeMatchingStiffness)
	{
		parameters0.x = shapeMatchingStiffness;
	}

	public void SetStretchShearParameters(float stretchStiffness, float shearUpStiffness, float shearSideStiffness)
	{
		parameters0.x = stretchStiffness;
		parameters0.y = shearUpStiffness;
		parameters0.z = shearSideStiffness;
	}

	public void SetBendTwistParameters(float torsionStiffness, float bend1Stiffness, float bend2Stiffness)
	{
		parameters0.x = torsionStiffness;
		parameters0.y = bend1Stiffness;
		parameters0.z = bend2Stiffness;
	}
}
