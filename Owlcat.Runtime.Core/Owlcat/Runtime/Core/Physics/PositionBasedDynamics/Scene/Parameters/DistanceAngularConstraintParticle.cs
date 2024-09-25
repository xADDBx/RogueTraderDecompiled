using System;

namespace Owlcat.Runtime.Core.Physics.PositionBasedDynamics.Scene.Parameters;

[Serializable]
public class DistanceAngularConstraintParticle : PBDParticleConstraintParametersBase
{
	public ScalableFloat StretchStiffness = new ScalableFloat(0f, 1f);

	public ScalableFloat CompressionStiffness = new ScalableFloat(0f, 1f);

	public ScalableFloat AngularStiffness = new ScalableFloat(0f, 1f);

	public ScalableFloat AngularVelocityInfluence = new ScalableFloat(0f, 1f);
}
