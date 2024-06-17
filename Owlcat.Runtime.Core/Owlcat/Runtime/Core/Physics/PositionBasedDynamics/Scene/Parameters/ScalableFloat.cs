using System;

namespace Owlcat.Runtime.Core.Physics.PositionBasedDynamics.Scene.Parameters;

[Serializable]
public struct ScalableFloat
{
	public float Value;

	public float Scale;

	public float ScaledValue => Value * Scale;

	public ScalableFloat(float value, float scale)
	{
		Value = value;
		Scale = scale;
	}

	public void ResetScale()
	{
		Scale = 1f;
	}

	public override string ToString()
	{
		return $"Value: {Value} Scale: {Scale}";
	}
}
