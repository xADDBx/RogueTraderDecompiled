namespace Owlcat.Runtime.Visual.Waaagh;

internal enum WaaaghProfileId
{
	PipelineBeginContextRendering,
	PipelineEndContextRendering,
	PipelineBeginCameraRendering,
	PipelineEndCameraRendering,
	RendererSetupCullingParameters,
	RendererSetup,
	RendererExecute,
	RendererSortRenderPasses,
	RendererConfigureRendererLists,
	WaaaghLightsSetup,
	LightCookieSetup,
	ShadowsSetup,
	RenderPostProcess,
	RenderPostProcessFinal,
	RenderBeforeTransparentPostProcess
}
