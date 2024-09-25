using System.Collections.Generic;
using Kingmaker.Utility;
using Kingmaker.Utility.FlagCountable;
using Kingmaker.View;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.Visual.FogOfWar;
using Owlcat.Runtime.Visual.Waaagh.Data;
using Owlcat.Runtime.Visual.Waaagh.RendererFeatures;
using Owlcat.Runtime.Visual.Waaagh.RendererFeatures.FogOfWar;
using UnityEngine;
using UnityEngine.Rendering;

namespace Kingmaker.Controllers;

public static class FogOfWarControllerData
{
	public static readonly CountableFlag Suppressed = new CountableFlag();

	public static readonly MultiSet<Transform> m_AdditionalRevealers = new MultiSet<Transform>();

	public static FogOfWarFeature GetFogOfWarFeature()
	{
		WaaaghPipelineAsset waaaghPipelineAsset = GraphicsSettings.renderPipelineAsset as WaaaghPipelineAsset;
		if (waaaghPipelineAsset != null)
		{
			ScriptableRendererData[] rendererDataList = waaaghPipelineAsset.RendererDataList;
			for (int i = 0; i < rendererDataList.Length; i++)
			{
				foreach (ScriptableRendererFeature rendererFeature in rendererDataList[i].RendererFeatures)
				{
					FogOfWarFeature fogOfWarFeature = rendererFeature as FogOfWarFeature;
					if (fogOfWarFeature != null)
					{
						return fogOfWarFeature;
					}
				}
			}
		}
		return null;
	}

	public static void AddRevealer(Transform revealer)
	{
		m_AdditionalRevealers.Add(revealer);
	}

	public static void RemoveRevealer(Transform revealer)
	{
		if (m_AdditionalRevealers.Contains(revealer))
		{
			m_AdditionalRevealers.Remove(revealer);
			FogOfWarRevealerSettings component = revealer.GetComponent<FogOfWarRevealerSettings>();
			if (component != null && component.Revealer != null)
			{
				FogOfWarRevealer.All.Remove(component.Revealer);
			}
		}
	}

	public static void CleanupRevealers()
	{
		List<Transform> list = TempList.Get<Transform>();
		foreach (Transform value in m_AdditionalRevealers.Values)
		{
			if (!value)
			{
				list.Add(value);
			}
		}
		foreach (Transform item in list)
		{
			m_AdditionalRevealers.RemoveAll(item);
		}
	}

	public static IEnumerable<Transform> GetAdditionalRevealers()
	{
		return m_AdditionalRevealers.Values;
	}
}
