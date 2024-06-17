using System;
using Owlcat.Runtime.Visual.CustomPostProcess;

namespace Owlcat.Runtime.Visual.Overrides;

[Serializable]
public class ShaderPropertyParameter
{
	public bool OverrideState;

	public int PassIndex;

	public ShaderPropertyDescriptor Property;
}
