using UnityEngine;

namespace Owlcat.Runtime.Visual.Waaagh.Passes.Base;

public class SetShaderTimePassData : PassDataBase
{
	public Vector4 Time;

	public Vector4 SinTime;

	public Vector4 CosTime;

	public Vector4 DeltaTime;

	public Vector4 TimeParameters;

	public Vector4 UnscaledTime;

	public Vector4 UnscaledTimeParameters;
}
