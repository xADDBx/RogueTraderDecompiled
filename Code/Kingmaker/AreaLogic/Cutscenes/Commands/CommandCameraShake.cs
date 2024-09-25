using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Utility.Attributes;
using Kingmaker.View;
using UnityEngine;

namespace Kingmaker.AreaLogic.Cutscenes.Commands;

[TypeId("03fc6437c7e02bb46bdb0bf5000e2209")]
public class CommandCameraShake : CommandBase
{
	[SerializeField]
	private bool m_Continuous;

	[SerializeField]
	[ConditionalHide("m_Continuous")]
	private float m_Lifetime = 1f;

	[SerializeField]
	private float m_Amplitude;

	[SerializeField]
	private float m_Speed;

	private bool m_Finished;

	public override bool IsContinuous => m_Continuous;

	protected override void OnRun(CutscenePlayerData player, bool skipping)
	{
		m_Finished = skipping || m_Speed <= 0f || m_Amplitude < 0f || m_Lifetime <= 0f;
		if (!m_Finished)
		{
			CameraRig.Instance.StartShake(m_Amplitude, m_Speed);
		}
	}

	protected override void OnStop(CutscenePlayerData player)
	{
		CameraRig.Instance.StopShake();
	}

	public override bool IsFinished(CutscenePlayerData player)
	{
		return m_Finished;
	}

	protected override void OnSetTime(double time, CutscenePlayerData player)
	{
		m_Finished = m_Finished || (!m_Continuous && time >= (double)m_Lifetime);
		if (!m_Finished && !CameraRig.Instance.IsShaking())
		{
			CameraRig.Instance.StartShake(m_Amplitude, m_Speed);
		}
	}

	public override string GetCaption()
	{
		return "Shake camera";
	}
}
