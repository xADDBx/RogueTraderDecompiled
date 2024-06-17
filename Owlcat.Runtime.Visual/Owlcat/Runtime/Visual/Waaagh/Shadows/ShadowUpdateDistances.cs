using System;

namespace Owlcat.Runtime.Visual.Waaagh.Shadows;

[Serializable]
public class ShadowUpdateDistances : Cascades
{
	public ShadowUpdateMode Distance0UpdateMode = ShadowUpdateMode.EveryFrame;

	public ShadowUpdateMode Distance1UpdateMode = ShadowUpdateMode.EverySecondFrame;

	public ShadowUpdateMode Distance2UpdateMode = ShadowUpdateMode.EveryThirdFrame;

	public ShadowUpdateMode Distance3UpdateMode = ShadowUpdateMode.EveryFourthFrame;
}
