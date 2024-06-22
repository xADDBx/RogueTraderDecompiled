using System.Linq;
using Kingmaker.Visual.Animation.Events;
using UnityEngine;

namespace Kingmaker.AreaLogic.SceneControllables;

[RequireComponent(typeof(Animator))]
public class EventAnimatorController : MonoBehaviour
{
	[SerializeField]
	private SimpleAnimationClipEventTrack m_EventTrack;

	public void PlayEventById(int id)
	{
		if (!(m_EventTrack == null))
		{
			m_EventTrack.Events.FirstOrDefault((SimpleAnimationClipEvent x) => x.ID == id)?.Start(base.gameObject);
		}
	}
}
