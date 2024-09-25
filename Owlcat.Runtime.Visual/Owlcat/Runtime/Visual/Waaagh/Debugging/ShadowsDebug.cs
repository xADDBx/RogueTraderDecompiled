using System;
using UnityEngine;

namespace Owlcat.Runtime.Visual.Waaagh.Debugging;

[Serializable]
public class ShadowsDebug
{
	public DebugShadowBufferType AtlasOccupancy;

	public Color AtlasNodesOccupied = Color.red;

	public Color AtlasNodesPartiallyOccupied = Color.yellow;

	public Color AtlasNodesOccupiedInHierarchy = Color.blue;

	public DebugShadowBufferType ViewAtlas;

	[Range(0.1f, 1f)]
	public float DebugScale = 1f;

	[Range(1f, 100f)]
	public float DebugColorMultiplier = 1f;
}
