using System;
using Owlcat.Runtime.Core.Physics.PositionBasedDynamics.Constraints;
using UnityEngine;

namespace Owlcat.Runtime.Core.Physics.PositionBasedDynamics.Scene.Parameters;

[Serializable]
public class DistanceConstraintBody : PBDBodyConstraintParametersBase<DistanceConstraintParticle>
{
	public AnimationCurve StretchStiffness = AnimationCurve.Linear(0f, 1f, 1f, 1f);

	public AnimationCurve CompressionStiffness = AnimationCurve.Linear(0f, 1f, 1f, 1f);

	public bool UseParentChildDirection;

	public override ConstraintType Type => ConstraintType.Distance;

	public override void CalculateParameters(DistanceConstraintParticle parameters, float t)
	{
		parameters.StretchStiffness.Value = StretchStiffness.Evaluate(t);
		parameters.CompressionStiffness.Value = CompressionStiffness.Evaluate(t);
	}
}
