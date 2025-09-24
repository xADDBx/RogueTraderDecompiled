using Owlcat.Runtime.Visual.Waaagh.Debugging;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.Waaagh.Passes.Debug;

public class ShadowsDebugPassData : PassDataBase
{
	public ShadowData ShadowData;

	public WaaaghDebugData DebugData;

	public Material ShadowsDebugMaterial;

	public float2 ScreenSize;

	public TextureHandle CameraFinalTarget;

	public TextureHandle ShadowBuffer;

	public bool ShadowsCacheEnabled;
}
