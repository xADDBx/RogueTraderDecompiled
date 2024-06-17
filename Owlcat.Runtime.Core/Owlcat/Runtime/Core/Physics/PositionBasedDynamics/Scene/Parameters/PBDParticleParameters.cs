using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace Owlcat.Runtime.Core.Physics.PositionBasedDynamics.Scene.Parameters;

[Serializable]
public class PBDParticleParameters
{
	public bool IsStatic;

	public bool IsCollidable;

	public float3 Position;

	public quaternion Orientation;

	public ScalableFloat Mass = new ScalableFloat(0f, 1f);

	public ScalableFloat Radius = new ScalableFloat(0f, 1f);

	public DistanceConstraintParticle DistanceConstraintParameters = new DistanceConstraintParticle();

	public DistanceAngularConstraintParticle DistanceAngularConstraintParameters = new DistanceAngularConstraintParticle();

	public StretchShearConstraintParticle StretchShearParameters = new StretchShearConstraintParticle();

	public BendTwistConstraintParticle BendTwistParameters = new BendTwistConstraintParticle();

	public ShapeMatchingConstraintParticle ShapeMatchingParameters = new ShapeMatchingConstraintParticle();

	public Transform Transform;

	public List<int> ConstraintIndices = new List<int>();

	public void ResetAllScales()
	{
		Mass.ResetScale();
		Radius.ResetScale();
		DistanceConstraintParameters.StretchStiffness.ResetScale();
		DistanceConstraintParameters.CompressionStiffness.ResetScale();
		DistanceAngularConstraintParameters.StretchStiffness.ResetScale();
		DistanceAngularConstraintParameters.CompressionStiffness.ResetScale();
		DistanceAngularConstraintParameters.AngularStiffness.ResetScale();
		DistanceAngularConstraintParameters.AngularVelocityInfluence.ResetScale();
		StretchShearParameters.StretchStiffness.ResetScale();
		StretchShearParameters.ShearUpStiffness.ResetScale();
		StretchShearParameters.ShearSideStiffness.ResetScale();
		BendTwistParameters.TorsionStiffness.ResetScale();
		BendTwistParameters.Bend1Stiffness.ResetScale();
		BendTwistParameters.Bend2Stiffness.ResetScale();
		ShapeMatchingParameters.Stiffness.ResetScale();
	}
}
