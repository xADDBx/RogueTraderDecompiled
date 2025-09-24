using UnityEngine;
using UnityEngine.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.Waaagh.Passes;

public class CameraMotionVectorsPassData : PassDataBase
{
	public TextureHandle CameraMotionVectors;

	public TextureHandle CameraDepthRT;

	public Material Material;

	public Matrix4x4 UnjitteredVP;

	public Matrix4x4 PrevVP;
}
