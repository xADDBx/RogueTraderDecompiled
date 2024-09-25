using UnityEngine;

namespace Owlcat.Runtime.Visual.RenderPipeline;

public static class ComputeBufferId
{
	public static int LightDataCB = Shader.PropertyToID("LightDataCB");

	public static int LightVolumeDataCB = Shader.PropertyToID("LightVolumeDataCB");

	public static int ZBinsCB = Shader.PropertyToID("ZBinsCB");

	public static int _LightTilesBuffer = Shader.PropertyToID("_LightTilesBuffer");

	public static int _LightTilesBufferUAV = Shader.PropertyToID("_LightTilesBufferUAV");

	public static int _LightTilesBufferUAVSize = Shader.PropertyToID("_LightTilesBufferUAVSize");

	public static int _ShadowMatricesBuffer = Shader.PropertyToID("_ShadowMatricesBuffer");

	public static int _ShadowDataBuffer = Shader.PropertyToID("_ShadowDataBuffer");

	public static int _IndirectInstanceDataBuffer = Shader.PropertyToID("_IndirectInstanceDataBuffer");

	public static int _LightProbesBuffer = Shader.PropertyToID("_LightProbesBuffer");

	public static int _GpuSkinningFrames = Shader.PropertyToID("_GpuSkinningFrames");
}
