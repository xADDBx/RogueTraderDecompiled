using System;
using UnityEngine;

namespace Owlcat.Runtime.Core.Physics.PositionBasedDynamics.Scene.Parameters;

[Serializable]
public class PBDBodyParameters
{
	public AnimationCurve Mass = AnimationCurve.Linear(0f, 1f, 1f, 1f);

	public AnimationCurve Radius = AnimationCurve.Linear(0f, 0.1f, 1f, 0.1f);

	[Experimental]
	public ConstraintsSortMode ConstraintsSort;

	public DistanceConstraintBody DistanceConstraint = new DistanceConstraintBody();

	public DistanceAngularConstraintBody DistanceAngularConstraint = new DistanceAngularConstraintBody();

	public ShapeMatchinConstraintBody ShapeMatchingConstraint = new ShapeMatchinConstraintBody();

	public StretchShearConstraintBody StretchShearConstraint = new StretchShearConstraintBody();

	public BendTwistConstraintBody BendTwistConstraint = new BendTwistConstraintBody();

	public bool AnyConstraintEnabled()
	{
		if (!DistanceConstraint.Enabled && !DistanceAngularConstraint.Enabled && !StretchShearConstraint.Enabled && !BendTwistConstraint.Enabled)
		{
			return ShapeMatchingConstraint.Enabled;
		}
		return true;
	}
}
