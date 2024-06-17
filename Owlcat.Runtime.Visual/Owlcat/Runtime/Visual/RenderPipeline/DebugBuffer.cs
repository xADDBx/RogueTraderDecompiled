using UnityEngine;

namespace Owlcat.Runtime.Visual.RenderPipeline;

public static class DebugBuffer
{
	public static int _DebugMipMap = Shader.PropertyToID("_DebugMipMap");

	public static int _DebugLightingMode = Shader.PropertyToID("_DebugLightingMode");

	public static int _DebugTerrain = Shader.PropertyToID("_DebugTerrain");

	public static int _DebugMaterial = Shader.PropertyToID("_DebugMaterial");

	public static int _DebugOverdrawLevel = Shader.PropertyToID("_DebugOverdrawLevel");

	public static int _DebugOverdrawChannelMask = Shader.PropertyToID("_DebugOverdrawChannelMask");

	public static int _DebugVertexAttribute = Shader.PropertyToID("_DebugVertexAttribute");

	public static int _MipMapDebugMap = Shader.PropertyToID("_MipMapDebugMap");
}
