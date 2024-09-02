using System;
using Owlcat.Runtime.Visual.Effects.LineRenderer;
using Owlcat.Runtime.Visual.Effects.RayView;
using Owlcat.Runtime.Visual.Lighting;
using Owlcat.Runtime.Visual.OcclusionGeometryClip;
using Owlcat.Runtime.Visual.RenderPipeline.Lighting;
using Owlcat.Runtime.Visual.RenderPipeline.RendererFeatures.Highlighting.Passes;
using Owlcat.Runtime.Visual.Waaagh.Lighting;
using Owlcat.Runtime.Visual.Waaagh.RendererFeatures.Highlighting;
using Owlcat.Runtime.Visual.Waaagh.RendererFeatures.VolumetricLighting.Jobs;
using Owlcat.Runtime.Visual.Waaagh.Shadows;
using Unity.Jobs;
using UnityEngine;

[Unity.Jobs.DOTSCompilerGenerated]
internal class __JobReflectionRegistrationOutput__3731508335
{
	public static void CreateJobReflectionData()
	{
		try
		{
			IJobExtensions.EarlyJobInit<Owlcat.Runtime.Visual.Waaagh.RendererFeatures.VolumetricLighting.Jobs.CullingJob>();
			IJobParallelForExtensions.EarlyJobInit<ExtractLocalFogDataJob>();
			IJobParallelForExtensions.EarlyJobInit<Owlcat.Runtime.Visual.Waaagh.RendererFeatures.VolumetricLighting.Jobs.MinMaxZJob>();
			IJobForExtensions.EarlyJobInit<Owlcat.Runtime.Visual.Waaagh.RendererFeatures.VolumetricLighting.Jobs.ZBinningJob>();
			IJobExtensions.EarlyJobInit<CountJob>();
			IJobParallelForExtensions.EarlyJobInit<Owlcat.Runtime.Visual.Waaagh.RendererFeatures.Highlighting.CullingJob>();
			IJobExtensions.EarlyJobInit<ShadowJob>();
			IJobForExtensions.EarlyJobInit<Owlcat.Runtime.Visual.Waaagh.Lighting.ExtractLightDataJob>();
			IJobForExtensions.EarlyJobInit<Owlcat.Runtime.Visual.Waaagh.Lighting.MinMaxZJob>();
			IJobExtensions.EarlyJobInit<Owlcat.Runtime.Visual.Waaagh.Lighting.RadixSortJob>();
			IJobForExtensions.EarlyJobInit<Owlcat.Runtime.Visual.Waaagh.Lighting.ZBinningJob>();
			IJobParallelForExtensions.EarlyJobInit<HighlighterPass.CullingJob>();
			IJobForExtensions.EarlyJobInit<Owlcat.Runtime.Visual.RenderPipeline.Lighting.ExtractLightDataJob>();
			IJobForExtensions.EarlyJobInit<Owlcat.Runtime.Visual.RenderPipeline.Lighting.MinMaxZJob>();
			IJobExtensions.EarlyJobInit<Owlcat.Runtime.Visual.RenderPipeline.Lighting.RadixSortJob>();
			IJobForExtensions.EarlyJobInit<Owlcat.Runtime.Visual.RenderPipeline.Lighting.ZBinningJob>();
			IJobExtensions.EarlyJobInit<Service.BuildCastGeometryJob>();
			IJobParallelForExtensions.EarlyJobInit<Service.AnimationJob>();
			IJobExtensions.EarlyJobInit<LightCookieManager.Job>();
			IJobParallelForExtensions.EarlyJobInit<RayViewUpdateJob>();
			IJobParallelForExtensions.EarlyJobInit<CompositeLineUpdateJob>();
			IJobParallelForExtensions.EarlyJobInit<UpdateGeometryJob>();
			IJobForExtensions.EarlyJobInit<Owlcat.Runtime.Visual.RenderPipeline.Lighting.ReorderJob<Owlcat.Runtime.Visual.RenderPipeline.Lighting.LightDescriptor>>();
			IJobExtensions.EarlyJobInit<Owlcat.Runtime.Visual.RenderPipeline.Lighting.CopyArrayJob<Owlcat.Runtime.Visual.RenderPipeline.Lighting.LightDescriptor>>();
		}
		catch (Exception ex)
		{
			EarlyInitHelpers.JobReflectionDataCreationFailed(ex);
		}
	}

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
	public static void EarlyInit()
	{
		CreateJobReflectionData();
	}
}
