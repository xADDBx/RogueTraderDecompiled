using System.Collections.Generic;
using UnityEngine;

namespace Kingmaker.Visual.Animation.Events;

[CreateAssetMenu(fileName = "SimpleAnimationEventTrack", menuName = "Animation Manager/Simple Animation Events Track")]
public class SimpleAnimationClipEventTrack : ScriptableObject
{
	[SerializeReference]
	private List<SimpleAnimationClipEvent> m_Events = new List<SimpleAnimationClipEvent>();

	public List<SimpleAnimationClipEvent> Events
	{
		get
		{
			return m_Events;
		}
		set
		{
			m_Events = ((value != null) ? new List<SimpleAnimationClipEvent>(value) : null);
		}
	}
}
