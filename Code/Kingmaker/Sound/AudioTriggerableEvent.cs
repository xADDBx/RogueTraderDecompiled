using Kingmaker.Utility.Attributes;
using UnityEngine;

namespace Kingmaker.Sound;

internal class AudioTriggerableEvent : AkAudioTriggerable
{
	[SerializeField]
	private AkEventReference m_Event;

	[SerializeField]
	private bool m_ActionMode;

	[SerializeField]
	[ShowIf("m_ActionMode")]
	private AkActionOnEventType m_Action;

	[SerializeField]
	[ShowIf("m_ActionMode")]
	private float m_TransitionDuration;

	[SerializeField]
	[ShowIf("m_ActionMode")]
	private AkCurveInterpolation m_CurveInterpolation = AkCurveInterpolation.AkCurveInterpolation_Linear;

	public override void OnTrigger()
	{
		if (m_ActionMode)
		{
			m_Event.ExecuteAction(base.gameObject, m_Action, (int)(1000f * m_TransitionDuration), m_CurveInterpolation);
		}
		else
		{
			m_Event.Post(base.gameObject);
		}
	}

	protected override void OnStop(int fade)
	{
		m_Event.ExecuteAction(base.gameObject, AkActionOnEventType.AkActionOnEventType_Stop, fade, AkCurveInterpolation.AkCurveInterpolation_Linear);
	}
}
