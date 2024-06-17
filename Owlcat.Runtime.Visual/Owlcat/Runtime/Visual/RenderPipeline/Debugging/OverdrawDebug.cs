using System;
using UnityEngine;

namespace Owlcat.Runtime.Visual.RenderPipeline.Debugging;

[Serializable]
public class OverdrawDebug
{
	public OverdrawMode OverdrawMode;

	[Range(0f, 32f)]
	public int ShowOnlyPixelsWithOverdraw;
}
