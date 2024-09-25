using System.Collections.Generic;
using UnityEngine;

public class RealtimeReflectionProbesTest : MonoBehaviour
{
	public List<ReflectionProbe> Probes = new List<ReflectionProbe>();

	private void Update()
	{
		foreach (ReflectionProbe probe in Probes)
		{
			probe.RenderProbe();
		}
	}
}
