using System;
using Kingmaker.Sound.Base;
using UnityEngine;

namespace Kingmaker.Sound;

[Serializable]
public class AkEventReference : AkReferenceBase
{
	public void Post(GameObject obj)
	{
		if (base.ValueHash != 0)
		{
			SoundEventsManager.PostEvent(base.ValueHash, obj);
		}
	}

	public void ExecuteAction(GameObject gameObject, AkActionOnEventType actionOnEventType, int transitionDuration, AkCurveInterpolation curveInterpolation)
	{
		if (base.ValueHash != 0)
		{
			AkSoundEngine.ExecuteActionOnEvent(base.ValueHash, actionOnEventType, gameObject, transitionDuration, curveInterpolation);
		}
	}
}
