using System;
using System.Collections.Generic;
using UnityEngine;

namespace Owlcat.Runtime.Visual.CustomPostProcess;

[Serializable]
public class CustomPostProcessEffectPass
{
	public string Name;

	public Shader Shader;

	[NonReorderable]
	public List<ShaderPropertyDescriptor> Properties;
}
