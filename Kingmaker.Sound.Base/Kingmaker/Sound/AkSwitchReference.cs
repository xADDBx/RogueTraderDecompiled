using System;
using UnityEngine;

namespace Kingmaker.Sound;

[Serializable]
public class AkSwitchReference : AkReferenceWithGroupBase
{
	public void Set(GameObject obj)
	{
		if (base.ValueHash != 0)
		{
			AkSoundEngine.SetSwitch(base.GroupHash, base.ValueHash, obj);
		}
	}
}
