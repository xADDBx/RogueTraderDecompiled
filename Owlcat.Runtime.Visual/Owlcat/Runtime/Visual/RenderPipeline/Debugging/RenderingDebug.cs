using System;

namespace Owlcat.Runtime.Visual.RenderPipeline.Debugging;

[Serializable]
public class RenderingDebug
{
	public bool DebugMipMap;

	public DebugBuffers DebugBuffers;

	public DebugMaterial DebugMaterial;

	public DebugVertexAttribute DebugVertexAttribute;
}
