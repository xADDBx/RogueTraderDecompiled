using System;

namespace Kingmaker.Sound;

[Serializable]
public class AkStateReference : AkReferenceWithGroupBase
{
	public void Set()
	{
		if (base.ValueHash != 0)
		{
			AkSoundEngine.SetState(base.GroupHash, base.ValueHash);
		}
	}
}
