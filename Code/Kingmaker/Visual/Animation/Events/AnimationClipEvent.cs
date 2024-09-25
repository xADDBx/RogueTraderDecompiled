using System;
using UnityEngine;

namespace Kingmaker.Visual.Animation.Events;

[Serializable]
public class AnimationClipEvent : ICloneable, IComparable
{
	[SerializeField]
	private float m_Time;

	[SerializeField]
	private bool m_IsLooped;

	[SerializeField]
	private bool m_IsIstant;

	public float Time
	{
		get
		{
			return m_Time;
		}
		set
		{
			if (value < 0f)
			{
				value = 0f;
			}
			m_Time = value;
		}
	}

	public bool IsLooped
	{
		get
		{
			return m_IsLooped;
		}
		set
		{
			m_IsLooped = value;
		}
	}

	public bool IsInstant
	{
		get
		{
			return m_IsIstant;
		}
		set
		{
			m_IsIstant = value;
		}
	}

	public AnimationClipEvent()
	{
	}

	public AnimationClipEvent(float time)
		: this(time, isLooped: false)
	{
	}

	public AnimationClipEvent(float time, bool isLooped)
	{
		m_Time = time;
		m_IsLooped = isLooped;
	}

	public virtual Action Start(AnimationManager animationManager)
	{
		throw new NotImplementedException();
	}

	public virtual object Clone()
	{
		return new AnimationClipEvent(Time);
	}

	public virtual bool DoNotStartIfStarted(AnimationClipEvent @event)
	{
		return false;
	}

	public int CompareTo(object @object)
	{
		if (@object == null)
		{
			throw new ArgumentNullException("object");
		}
		if (!(@object is AnimationClipEvent animationClipEvent))
		{
			throw new InvalidCastException($"{@object} is of type {@object.GetType()} and is not of type {typeof(AnimationClipEvent)}.");
		}
		return Mathf.RoundToInt((Time - animationClipEvent.Time) * 1000f);
	}

	public override string ToString()
	{
		return string.Format("{0}AnimationClipEvent at {1}", (IsInstant && IsLooped) ? "Instant looped " : (IsInstant ? "Instant " : (IsLooped ? "Looped " : "")), Time);
	}
}
