using Unity.Collections;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.RenderPipeline;

public struct LightingData
{
	public NativeArray<VisibleLight> VisibleLights;

	public int MainLightIndex;
}
