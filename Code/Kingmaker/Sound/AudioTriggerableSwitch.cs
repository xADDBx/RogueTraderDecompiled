using UnityEngine;

namespace Kingmaker.Sound;

public class AudioTriggerableSwitch : AkAudioTriggerable
{
	[SerializeField]
	private AkSwitchReference m_Switch;

	public override void OnTrigger()
	{
		m_Switch.Set(base.gameObject);
	}
}
