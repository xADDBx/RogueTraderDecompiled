using System;
using Kingmaker.Sound.Base;
using Kingmaker.Visual.Sound;
using UnityEngine;

namespace Kingmaker.Visual.Animation.Events;

public class SimpleAnimationClipEventSound : SimpleAnimationClipEvent
{
	[SerializeField]
	[AkEventReference]
	private string m_Name;

	public string Name
	{
		get
		{
			return m_Name;
		}
		set
		{
			m_Name = value;
		}
	}

	public override Action Start(GameObject gameObject)
	{
		SoundEventsManager.PostEvent(m_Name, gameObject);
		return null;
	}
}
