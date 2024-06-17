using System;
using System.Collections.Generic;
using Owlcat.Runtime.Core.Physics.PositionBasedDynamics.Constraints;

namespace Owlcat.Runtime.Core.Physics.PositionBasedDynamics.Scene.Parameters;

[Serializable]
public class PBDAdditionalConstraintsParameters
{
	public List<Constraint> Constraints = new List<Constraint>();
}
