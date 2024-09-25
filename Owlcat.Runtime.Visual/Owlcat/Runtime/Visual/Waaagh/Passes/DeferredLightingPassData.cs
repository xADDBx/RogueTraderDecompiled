using Unity.Collections;
using UnityEngine;
using UnityEngine.Experimental.Rendering.RenderGraphModule;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.Waaagh.Passes;

public class DeferredLightingPassData : PassDataBase
{
	public TextureHandle CameraColorRT;

	public TextureHandle CameraDepthRT;

	public TextureHandle CameraDepthCopytRT;

	public TextureHandle CameraAlbedoRT;

	public TextureHandle CameraNormalsRT;

	public TextureHandle CameraBakedGIRT;

	public TextureHandle CameraShadowmaskRT;

	public TextureHandle CameraTranslucencyRT;

	public NativeArray<VisibleReflectionProbe> VisibleReflectionProbes;

	public Material DeferredReflectionsMaterial;

	public Material DeferredLightingMaterial;

	public bool SsrEnabled;

	public Color GlossyEnvironmentColor;

	public Color GlossyBlackColor;
}
