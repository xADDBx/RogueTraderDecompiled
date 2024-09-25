using System;
using Owlcat.Runtime.Core.Physics.PositionBasedDynamics.Constraints;
using UnityEngine;

namespace Owlcat.Runtime.Core.Physics.PositionBasedDynamics.Scene.Parameters;

[Serializable]
public class DistanceAngularConstraintBody : PBDBodyConstraintParametersBase<DistanceAngularConstraintParticle>
{
	public AnimationCurve StretchStiffness = AnimationCurve.Linear(0f, 1f, 1f, 1f);

	public AnimationCurve CompressionStiffness = AnimationCurve.Linear(0f, 1f, 1f, 1f);

	public AnimationCurve AngularStiffness = AnimationCurve.Linear(0f, 1f, 1f, 1f);

	public AnimationCurve AngularVelocityInfluence = AnimationCurve.Linear(0f, 1f, 1f, 1f);

	public bool UseParentChildDirection;

	public override ConstraintType Type => ConstraintType.DistanceAngular;

	public override void CalculateParameters(DistanceAngularConstraintParticle parameters, float t)
	{
		parameters.StretchStiffness.Value = StretchStiffness.Evaluate(t);
		parameters.CompressionStiffness.Value = CompressionStiffness.Evaluate(t);
		parameters.AngularStiffness.Value = AngularStiffness.Evaluate(t);
		parameters.AngularVelocityInfluence.Value = AngularVelocityInfluence.Evaluate(t);
	}
}
