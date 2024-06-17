using System;
using RogueTrader.Code.ShaderConsts;
using UnityEngine;

namespace Kingmaker.Visual;

[ExecuteInEditMode]
public class PlanarFog : MonoBehaviour
{
	private void OnDrawGizmosSelected()
	{
		if (!Application.isPlaying)
		{
			Renderer component = GetComponent<Renderer>();
			if (component != null && component.sharedMaterial != null)
			{
				TimeSpan timeSpan = DateTime.Now - DateTime.Today;
				component.sharedMaterial.SetFloat(ShaderProps._TimeEditor, (float)timeSpan.TotalSeconds);
			}
		}
	}
}
