using System.Collections.Generic;
using System.Linq;
using Owlcat.Runtime.Visual.CustomPostProcess;
using Owlcat.Runtime.Visual.Overrides;
using Owlcat.Runtime.Visual.Waaagh.Passes;
using Owlcat.Runtime.Visual.Waaagh.RendererFeatures.CustomPostProcess.Passes;
using UnityEngine;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.Waaagh.RendererFeatures.CustomPostProcess;

[CreateAssetMenu(menuName = "Renderer Features/Waaagh/Custom Post Process/Feature", fileName = "CustomPostProcessFeature")]
public class CustomPostProcessFeature : ScriptableRendererFeature
{
	private List<CustomPostProcessPass> m_Passes;

	public List<CustomPostProcessEffect> Effects;

	public override void Create()
	{
		m_Passes = new List<CustomPostProcessPass>();
		foreach (IGrouping<CustomPostProcessRenderEvent, CustomPostProcessEffect> item in from e in Effects.Where((CustomPostProcessEffect e) => e != null).Distinct()
			group e by e.Event)
		{
			List<CustomPostProcessEffect> effects = item.Where((CustomPostProcessEffect e) => e != null).ToList();
			m_Passes.Add(new CustomPostProcessPass(ConvertEvent(item.Key), effects));
		}
	}

	public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
	{
		Validate(renderingData.CameraData.Camera);
		bool postProcessEnabled = renderingData.CameraData.PostProcessEnabled;
		CustomPostProcessOverride component = VolumeManager.instance.stack.GetComponent<CustomPostProcessOverride>();
		if (!postProcessEnabled || !component.IsActive())
		{
			return;
		}
		foreach (CustomPostProcessPass pass in m_Passes)
		{
			renderer.EnqueuePass(pass);
		}
	}

	private void Validate(Camera camera)
	{
		bool flag = false;
		for (int i = 0; i < m_Passes.Count; i++)
		{
			CustomPostProcessPass customPostProcessPass = m_Passes[i];
			RenderPassEvent renderPassEvent = customPostProcessPass.RenderPassEvent;
			customPostProcessPass.Validate();
			for (int j = 0; j < customPostProcessPass.Effects.Count; j++)
			{
				CustomPostProcessEffect customPostProcessEffect = customPostProcessPass.Effects[j];
				if (ConvertEvent(customPostProcessEffect.Event) != renderPassEvent)
				{
					flag = true;
					break;
				}
				foreach (CustomPostProcessEffectPass pass in customPostProcessEffect.Passes)
				{
					if (pass.Shader != null && !customPostProcessPass.ValidateMaterial(camera, pass))
					{
						flag = true;
						break;
					}
				}
				if (flag)
				{
					break;
				}
			}
			if (flag)
			{
				break;
			}
		}
		if (flag)
		{
			Dispose();
			Create();
		}
	}

	protected override void Dispose(bool disposing)
	{
		base.Dispose(disposing);
		foreach (CustomPostProcessPass pass in m_Passes)
		{
			pass.Cleanup();
		}
	}

	public RenderPassEvent ConvertEvent(CustomPostProcessRenderEvent evt)
	{
		return evt switch
		{
			CustomPostProcessRenderEvent.BeforeMainPostProcess => RenderPassEvent.BeforeRenderingPostProcessing, 
			CustomPostProcessRenderEvent.AfterMainPostProcess => RenderPassEvent.AfterRenderingPostProcessing, 
			_ => RenderPassEvent.AfterRenderingPostProcessing, 
		};
	}

	public CustomPostProcessRenderEvent ConvertEvent(RenderPassEvent evt)
	{
		return evt switch
		{
			RenderPassEvent.BeforeRenderingPostProcessing => CustomPostProcessRenderEvent.BeforeMainPostProcess, 
			RenderPassEvent.AfterRenderingPostProcessing => CustomPostProcessRenderEvent.AfterMainPostProcess, 
			_ => CustomPostProcessRenderEvent.AfterMainPostProcess, 
		};
	}
}
