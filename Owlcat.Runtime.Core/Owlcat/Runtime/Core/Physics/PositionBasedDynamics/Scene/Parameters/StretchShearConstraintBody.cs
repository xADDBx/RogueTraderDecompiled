using System;
using Owlcat.Runtime.Core.Physics.PositionBasedDynamics.Constraints;
using UnityEngine;

namespace Owlcat.Runtime.Core.Physics.PositionBasedDynamics.Scene.Parameters;

[Serializable]
public class StretchShearConstraintBody : PBDBodyConstraintParametersBase<StretchShearConstraintParticle>
{
	public AnimationCurve StretchStiffness = AnimationCurve.Linear(0f, 1f, 1f, 1f);

	public AnimationCurve ShearUpStiffness = AnimationCurve.Linear(0f, 1f, 1f, 1f);

	public AnimationCurve ShearSideStiffness = AnimationCurve.Linear(0f, 1f, 1f, 1f);

	public override ConstraintType Type => ConstraintType.StretchShear;

	public override void CalculateParameters(StretchShearConstraintParticle parameters, float t)
	{
		parameters.StretchStiffness.Value = StretchStiffness.Evaluate(t);
		parameters.ShearUpStiffness.Value = ShearUpStiffness.Evaluate(t);
		parameters.ShearSideStiffness.Value = ShearSideStiffness.Evaluate(t);
	}
}
