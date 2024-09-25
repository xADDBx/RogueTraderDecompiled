using System;
using System.Collections.Generic;

namespace Owlcat.Runtime.Visual.Overrides;

[Serializable]
public class CustomPostProcessSettings
{
	public List<EffectOverride> EffectOverrides = new List<EffectOverride>();
}
