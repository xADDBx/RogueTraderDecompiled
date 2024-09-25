using System;
using Owlcat.Runtime.Core.Physics.PositionBasedDynamics.Constraints;
using UnityEngine;

namespace Owlcat.Runtime.Core.Physics.PositionBasedDynamics.Scene.Parameters;

[Serializable]
public class ShapeMatchinConstraintBody : PBDBodyConstraintParametersBase<ShapeMatchingConstraintParticle>
{
	public AnimationCurve Stiffness = AnimationCurve.Linear(0f, 1f, 1f, 1f);

	public override ConstraintType Type => ConstraintType.ShapeMatching;

	public override void CalculateParameters(ShapeMatchingConstraintParticle parameters, float t)
	{
		parameters.Stiffness.Value = Stiffness.Evaluate(t);
	}
}
