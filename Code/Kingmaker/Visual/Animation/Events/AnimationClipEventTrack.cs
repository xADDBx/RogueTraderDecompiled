using System.Collections.Generic;
using UnityEngine;

namespace Kingmaker.Visual.Animation.Events;

[CreateAssetMenu(fileName = "AnimationEventTrack", menuName = "Animation Manager/Animation Events Track")]
public class AnimationClipEventTrack : ScriptableObject
{
	[SerializeReference]
	private List<AnimationClipEvent> m_Events = new List<AnimationClipEvent>();

	public List<AnimationClipEvent> Events
	{
		get
		{
			return m_Events;
		}
		set
		{
			m_Events = ((value != null) ? new List<AnimationClipEvent>(value) : null);
		}
	}
}
