using System;

namespace Owlcat.Runtime.Core.Physics.PositionBasedDynamics.Scene.Parameters;

[Serializable]
public class BendTwistConstraintParticle : PBDParticleConstraintParametersBase
{
	public ScalableFloat TorsionStiffness = new ScalableFloat(0f, 1f);

	public ScalableFloat Bend1Stiffness = new ScalableFloat(0f, 1f);

	public ScalableFloat Bend2Stiffness = new ScalableFloat(0f, 1f);
}
