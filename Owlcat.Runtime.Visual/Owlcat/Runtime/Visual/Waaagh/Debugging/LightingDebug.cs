using System;
using Owlcat.ShaderLibrary.Visual.Debug;
using UnityEngine;

namespace Owlcat.Runtime.Visual.Waaagh.Debugging;

[Serializable]
public class LightingDebug
{
	public DebugClustersMode DebugClustersMode;

	public DebugLightingMode DebugLightingMode;

	public bool ShowLightSortingCurve;

	public Color LightSortingCurveColorStart = Color.red;

	public Color LightSortingCurveColorEnd = Color.green;
}
