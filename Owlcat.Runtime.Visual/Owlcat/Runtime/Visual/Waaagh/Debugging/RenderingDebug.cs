using System;
using Owlcat.ShaderLibrary.Visual.Debug;

namespace Owlcat.Runtime.Visual.Waaagh.Debugging;

[Serializable]
public class RenderingDebug
{
	public DebugMaterialMode DebugMaterialMode;

	public DebugOverdrawMode OverdrawMode;

	public bool DebugMipMap;
}
