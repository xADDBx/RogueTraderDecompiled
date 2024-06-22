using Unity.Mathematics;
using UnityEngine.Experimental.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.Waaagh.Debugging;

public class RenderGraphDebugResources
{
	public ComputeBufferHandle FullScreenDebugBuffer;

	public int2 FullScreenDebugBufferDimensions;
}
