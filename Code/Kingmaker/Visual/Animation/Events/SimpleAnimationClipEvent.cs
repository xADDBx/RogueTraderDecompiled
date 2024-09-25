using System;
using UnityEngine;

namespace Kingmaker.Visual.Animation.Events;

[Serializable]
public class SimpleAnimationClipEvent
{
	[SerializeField]
	private int m_ID;

	public int ID
	{
		get
		{
			return m_ID;
		}
		set
		{
			m_ID = value;
		}
	}

	public SimpleAnimationClipEvent(float time)
		: this(time, isLooped: false)
	{
	}

	public SimpleAnimationClipEvent(float time, bool isLooped)
	{
	}

	public SimpleAnimationClipEvent()
	{
	}

	public virtual Action Start(GameObject animationManager)
	{
		throw new NotImplementedException();
	}
}
