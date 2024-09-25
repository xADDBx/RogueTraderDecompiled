using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.Waaagh.Passes.Base;

internal class BasePasses
{
	private VFXPreparePass m_VfxPreparePass;

	private SetShaderTimePass m_SetShaderTimePass;

	private SetupFogPass m_SetupFogPass;

	private CameraSetupPass m_CameraSetupPass;

	private SetCameraShaderVariablesPass m_SetCameraShaderVariablePass;

	private SetShaderTimePass m_SetShaderTimeAfterCameraSetupPass;

	private DrawGizmosPass m_DrawGizmosBeforePostProcessPass;

	private DrawWireframePass m_DrawWireframePass;

	private DrawGizmosPass m_DrawGizmosAfterPostProcessPass;

	private VFXBuffersSetupPass m_VFXBuffersSetupPass;

	public BasePasses()
	{
		m_VfxPreparePass = new VFXPreparePass(RenderPassEvent.BeforeRenderingInternal);
		m_SetShaderTimePass = new SetShaderTimePass(RenderPassEvent.BeforeRenderingInternal);
		m_SetupFogPass = new SetupFogPass(RenderPassEvent.BeforeRenderingInternal);
		m_CameraSetupPass = new CameraSetupPass(RenderPassEvent.AfterRenderingPrePasses);
		m_SetCameraShaderVariablePass = new SetCameraShaderVariablesPass(RenderPassEvent.AfterRenderingPrePasses);
		m_SetShaderTimeAfterCameraSetupPass = new SetShaderTimePass(RenderPassEvent.AfterRenderingPrePasses);
		m_DrawGizmosBeforePostProcessPass = new DrawGizmosPass(GizmoSubset.PreImageEffects);
		m_DrawWireframePass = new DrawWireframePass((RenderPassEvent)1001);
		m_DrawGizmosAfterPostProcessPass = new DrawGizmosPass(GizmoSubset.PostImageEffects);
		m_VFXBuffersSetupPass = new VFXBuffersSetupPass(RenderPassEvent.BeforeRenderingTransparents);
	}

	public void Setup(ScriptableRenderer renderer, ref RenderingData renderingData)
	{
		_ = ref renderingData.CameraData;
		renderer.EnqueuePass(m_SetupFogPass);
		renderer.EnqueuePass(m_VfxPreparePass);
		renderer.EnqueuePass(m_SetShaderTimePass);
		renderer.EnqueuePass(m_CameraSetupPass);
		renderer.EnqueuePass(m_SetCameraShaderVariablePass);
		renderer.EnqueuePass(m_SetShaderTimeAfterCameraSetupPass);
		renderer.EnqueuePass(m_VFXBuffersSetupPass);
	}
}
