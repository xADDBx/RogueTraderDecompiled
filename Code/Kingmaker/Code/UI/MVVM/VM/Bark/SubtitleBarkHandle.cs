using Kingmaker.Controllers;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.Common;
using Kingmaker.UI.Sound.Base;

namespace Kingmaker.Code.UI.MVVM.VM.Bark;

public class SubtitleBarkHandle : IBarkHandle, IUpdatable
{
	private VoiceOverStatus m_VoiceOverStatus;

	private float m_RemainingTime;

	public SubtitleBarkHandle(string text, float duration, VoiceOverStatus voiceOverStatus)
	{
		m_VoiceOverStatus = voiceOverStatus;
		m_RemainingTime = ((duration > 0f) ? duration : UIUtility.DefaultBarkTime);
		Game.Instance.CustomUpdateController.Add(this);
		EventBus.RaiseEvent(delegate(ISubtitleBarkHandler h)
		{
			h.HandleOnShowBark(text, duration);
		});
	}

	void IUpdatable.Tick(float delta)
	{
		if (m_RemainingTime > 0f)
		{
			m_RemainingTime -= Game.Instance.TimeController.DeltaTime;
			if (m_RemainingTime <= 0f)
			{
				InterruptBark();
			}
		}
	}

	public bool IsPlayingBark()
	{
		if (m_VoiceOverStatus == null)
		{
			return m_RemainingTime > 0f;
		}
		return m_VoiceOverStatus.IsEnded;
	}

	public void InterruptBark()
	{
		m_RemainingTime = 0f;
		m_VoiceOverStatus?.Stop();
		m_VoiceOverStatus = null;
		Game.Instance.CustomUpdateController.Remove(this);
		EventBus.RaiseEvent(delegate(ISubtitleBarkHandler h)
		{
			h.HandleOnHideBark();
		});
	}
}
