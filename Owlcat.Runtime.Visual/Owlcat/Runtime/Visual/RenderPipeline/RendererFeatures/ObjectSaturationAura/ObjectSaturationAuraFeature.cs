using System;
using Owlcat.Runtime.Visual.RenderPipeline.Passes;
using Owlcat.Runtime.Visual.RenderPipeline.RendererFeatures.ObjectSaturationAura.Passes;
using UnityEngine;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.RenderPipeline.RendererFeatures.ObjectSaturationAura;

[CreateAssetMenu(menuName = "Renderer Features/Object Saturation Aura")]
public class ObjectSaturationAuraFeature : ScriptableRendererFeature
{
	[Serializable]
	[ReloadGroup]
	public sealed class ShaderResources
	{
		[SerializeField]
		[Reload("Runtime/RenderPipeline/RendererFeatures/ObjectSaturationAura/Shaders/ObjectSaturationAura.shader", ReloadAttribute.Package.Root)]
		public Shader ObjectSaturationAuraShader;
	}

	private const string kBasePath = "Assets/Code/Owlcat/";

	[SerializeField]
	private ShaderResources m_Shaders;

	private ObjectSaturationAuraPass m_Pass;

	public override void Create()
	{
		Material material = CoreUtils.CreateEngineMaterial(m_Shaders.ObjectSaturationAuraShader);
		material.enableInstancing = true;
		m_Pass = new ObjectSaturationAuraPass(RenderPassEvent.BeforeRenderingPrepasses, material);
	}

	public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
	{
		m_Pass.Setup(this);
		renderer.EnqueuePass(m_Pass);
		Shader.SetGlobalFloat(CameraBuffer._ObjectSaturationAuraFeatureEnabled, 1f);
	}

	public override void DisableFeature()
	{
		Shader.SetGlobalFloat(CameraBuffer._ObjectSaturationAuraFeatureEnabled, 0f);
	}

	public override string GetFeatureIdentifier()
	{
		return "ObjectSaturationAuraFeature";
	}
}
