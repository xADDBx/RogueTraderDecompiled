using UnityEngine;

namespace Kingmaker.Sound;

internal class AudioTriggerableState : AkAudioTriggerable
{
	[SerializeField]
	private AkStateReference m_State;

	public override void OnTrigger()
	{
		m_State.Set();
	}
}
