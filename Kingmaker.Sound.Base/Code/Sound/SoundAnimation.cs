using Kingmaker.Sound.Base;
using UnityEngine;

namespace Code.Sound;

public class SoundAnimation : MonoBehaviour
{
	public void PostEvent(string eventName)
	{
		SoundEventsManager.PostEvent(eventName, base.gameObject);
	}
}
