using System;

namespace Owlcat.Runtime.Core.Physics.PositionBasedDynamics.Scene.Parameters;

[Serializable]
public class ShapeMatchingConstraintParticle : PBDParticleConstraintParametersBase
{
	public ScalableFloat Stiffness = new ScalableFloat(0f, 1f);
}
