using System;

namespace Owlcat.Runtime.Core.Physics.PositionBasedDynamics.Scene.Parameters;

[Serializable]
public class StretchShearConstraintParticle : PBDParticleConstraintParametersBase
{
	public ScalableFloat StretchStiffness = new ScalableFloat(0f, 1f);

	public ScalableFloat ShearUpStiffness = new ScalableFloat(0f, 1f);

	public ScalableFloat ShearSideStiffness = new ScalableFloat(0f, 1f);
}
