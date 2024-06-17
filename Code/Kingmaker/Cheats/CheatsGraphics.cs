using System.Reflection;
using Core.Cheats;
using Owlcat.Runtime.Visual.Overrides;
using Owlcat.Runtime.Visual.Waaagh;
using Owlcat.Runtime.Visual.Waaagh.Data;
using UnityEngine;
using UnityEngine.Rendering;

namespace Kingmaker.Cheats;

internal class CheatsGraphics
{
	[Cheat(Name = "gl_ToggleStochasticSSR", Description = "Toggle stochastic SSR algorithm in PostProcessing_Global volume")]
	public static string ToggleStochasticSSR()
	{
		string text = "Stochastic SSR old value = ";
		Volume[] array = Object.FindObjectsOfType<Volume>();
		foreach (Volume volume in array)
		{
			if (!(volume.gameObject.name != "PostProcessing_Global"))
			{
				if (!volume.profile.TryGet<ScreenSpaceReflections>(out var component))
				{
					return "Can't find SSR Component";
				}
				text = text + component.StochasticSSR.value + ". New value = ";
				component.StochasticSSR.overrideState = !component.StochasticSSR.overrideState;
				component.StochasticSSR.SetValue(new BoolParameter(!component.StochasticSSR.value));
				text += component.StochasticSSR.value;
			}
		}
		return text;
	}

	[Cheat(Name = "gl_DisableKeyword", Description = "Disables shader keyword in all materials in all loaded scenes")]
	public static string DisableKeyword(string keyword)
	{
		int num = 0;
		if (keyword == "")
		{
			return "Keyword is empty, nothing to set. Abort.";
		}
		Renderer[] array = Object.FindObjectsOfType<Renderer>();
		Renderer[] array2 = array;
		foreach (Renderer renderer in array2)
		{
			if (renderer == null)
			{
				continue;
			}
			Material[] sharedMaterials = renderer.sharedMaterials;
			foreach (Material material in sharedMaterials)
			{
				if (!(material == null))
				{
					material.DisableKeyword(keyword);
					num++;
				}
			}
		}
		return "Renderers modified : " + array.Length + ", Materials modified : " + num;
	}

	[Cheat(Name = "gl_SetMaterialsFloat", Description = "Sets float value in all materials in all loaded scenes")]
	public static string SetMaterialsFloat(string name = "", float value = -1f)
	{
		if (name == "")
		{
			return "Material property name shouldn't be null";
		}
		if (value == -1f)
		{
			return "Target value shouldn't be -1";
		}
		int num = 0;
		Renderer[] array = Object.FindObjectsOfType<Renderer>();
		foreach (Renderer renderer in array)
		{
			if (renderer == null)
			{
				continue;
			}
			Material[] sharedMaterials = renderer.sharedMaterials;
			foreach (Material material in sharedMaterials)
			{
				if (material.HasFloat(name))
				{
					material.SetFloat(name, value);
					num++;
				}
			}
		}
		return "Materials modified : " + num;
	}

	[Cheat(Name = "gl_ToggleSRPBatching")]
	public static string ToggleSRPBatching()
	{
		FieldInfo field = typeof(WaaaghPipelineAsset).GetField("m_UseSRPBatcher", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.GetField);
		if (field == null)
		{
			return "Failed to toggle SRP batching";
		}
		field.SetValue(WaaaghPipeline.Asset, !WaaaghPipeline.Asset.UseSRPBatcher);
		return "SRP batching is now turned " + (WaaaghPipeline.Asset.UseSRPBatcher ? "on" : "off");
	}
}
