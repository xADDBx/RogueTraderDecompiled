using System;
using Owlcat.Runtime.Core.Physics.PositionBasedDynamics.Constraints;
using UnityEngine;

namespace Owlcat.Runtime.Core.Physics.PositionBasedDynamics.Scene.Parameters;

[Serializable]
public class BendTwistConstraintBody : PBDBodyConstraintParametersBase<BendTwistConstraintParticle>
{
	public AnimationCurve TorsionStiffness = AnimationCurve.Linear(0f, 1f, 1f, 1f);

	public AnimationCurve Bend1Stiffness = AnimationCurve.Linear(0f, 1f, 1f, 1f);

	public AnimationCurve Bend2Stiffness = AnimationCurve.Linear(0f, 1f, 1f, 1f);

	public override ConstraintType Type => ConstraintType.BendTwist;

	public override void CalculateParameters(BendTwistConstraintParticle parameters, float t)
	{
		parameters.TorsionStiffness.Value = TorsionStiffness.Evaluate(t);
		parameters.Bend1Stiffness.Value = Bend1Stiffness.Evaluate(t);
		parameters.Bend2Stiffness.Value = Bend2Stiffness.Evaluate(t);
	}
}
