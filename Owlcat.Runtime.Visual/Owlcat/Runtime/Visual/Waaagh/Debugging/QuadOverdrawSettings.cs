using System;

namespace Owlcat.Runtime.Visual.Waaagh.Debugging;

[Serializable]
public class QuadOverdrawSettings
{
	public int MaxQuadCost = 10;

	public QuadOverdrawDepthTestMode DepthTestMode;

	public QuadOverdrawObjectFilter ObjectFilter;

	public bool DepthHelperPlaneEneabled;

	public float DepthHelperPlaneLevel;
}
