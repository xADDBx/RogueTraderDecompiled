using System;
using Owlcat.Runtime.Core.Physics.PositionBasedDynamics.Constraints;

namespace Owlcat.Runtime.Core.Physics.PositionBasedDynamics.Scene.Parameters;

[Serializable]
public abstract class PBDBodyConstraintParametersBase
{
	public bool Enabled;

	public abstract ConstraintType Type { get; }
}
[Serializable]
public abstract class PBDBodyConstraintParametersBase<T> : PBDBodyConstraintParametersBase where T : PBDParticleConstraintParametersBase
{
	public abstract void CalculateParameters(T parameters, float t);
}
