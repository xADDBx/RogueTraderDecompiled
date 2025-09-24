using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.Waaagh.Passes;

public class DrawColorPyramidPassData : PassDataBase
{
	public TextureHandle Input;

	public TextureHandle Output;

	public TextureHandle TempDownsampleRT;

	public TextureHandle TempColorRT;

	public Material BlitMaterial;

	public Material ColorPyramidMaterial;

	public int2 TextureSize;
}
