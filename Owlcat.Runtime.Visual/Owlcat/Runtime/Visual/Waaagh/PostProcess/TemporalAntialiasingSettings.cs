using System;
using UnityEngine;

namespace Owlcat.Runtime.Visual.Waaagh.PostProcess;

[Serializable]
public class TemporalAntialiasingSettings
{
	[Range(0f, 1f)]
	public float FrameInfluence;
}
