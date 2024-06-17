using System.Collections.Generic;
using UnityEngine;

namespace Owlcat.Runtime.Visual.RenderPipeline.RendererFeatures.ObjectSaturationAura;

public class SaturationAuraCarrier : MonoBehaviour
{
	public float Radius;

	[Range(-1f, 1f)]
	public float Strength;

	[Min(1f)]
	public float Power = 1f;

	public float OffsetToCamera;

	public static List<SaturationAuraCarrier> All { get; private set; } = new List<SaturationAuraCarrier>();


	private void OnEnable()
	{
		All.Add(this);
	}

	private void OnDisable()
	{
		All.Remove(this);
	}
}
