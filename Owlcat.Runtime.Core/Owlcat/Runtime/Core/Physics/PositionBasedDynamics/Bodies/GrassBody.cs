using System.Collections.Generic;
using Owlcat.Runtime.Core.Physics.PositionBasedDynamics.Constraints;
using Owlcat.Runtime.Core.Physics.PositionBasedDynamics.Particles;

namespace Owlcat.Runtime.Core.Physics.PositionBasedDynamics.Bodies;

public class GrassBody : Body
{
	public GrassBody(string name, List<Particle> particles, List<Constraint> constraints)
		: base(name, particles, constraints, null)
	{
	}
}
