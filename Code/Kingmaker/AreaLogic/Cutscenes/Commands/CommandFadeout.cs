using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Code.UI.MVVM.VM.Fade;
using Kingmaker.Utility.Attributes;
using UnityEngine;

namespace Kingmaker.AreaLogic.Cutscenes.Commands;

[TypeId("c70d3959f8ce9d145a883fab41d59e62")]
public class CommandFadeout : CommandBase
{
	private class SignalFlag
	{
		public bool Signalled;
	}

	[SerializeField]
	private bool m_Continuous;

	[SerializeField]
	private CommandSignalData m_OnFaded = new CommandSignalData
	{
		Name = "OnFaded"
	};

	[SerializeField]
	[ConditionalHide("m_Continuous")]
	private float m_Lifetime = 1f;

	private bool m_Finished;

	private bool m_IsFadedOut;

	public override bool IsContinuous => m_Continuous;

	public override bool TrySkip(CutscenePlayerData player)
	{
		return true;
	}

	protected override void OnRun(CutscenePlayerData player, bool skipping)
	{
		m_Finished = skipping && !IsContinuous;
		if (!m_Finished)
		{
			FadeCanvas.Fadeout(fade: true);
			m_IsFadedOut = true;
		}
	}

	protected override void OnStop(CutscenePlayerData player)
	{
		if (m_IsFadedOut)
		{
			FadeCanvas.Fadeout(fade: false);
		}
		m_Finished = true;
	}

	public override void Interrupt(CutscenePlayerData player)
	{
		base.Interrupt(player);
		m_Finished = !IsContinuous;
	}

	public override bool IsFinished(CutscenePlayerData player)
	{
		return m_Finished;
	}

	protected override void OnSetTime(double time, CutscenePlayerData player)
	{
		if (m_Continuous && (bool)m_OnFaded.Gate)
		{
			SignalFlag commandData = player.GetCommandData<SignalFlag>(this);
			if (!commandData.Signalled)
			{
				commandData.Signalled = true;
				player.SignalGate(m_OnFaded.Gate);
			}
		}
		m_Finished = !m_Continuous && time >= (double)m_Lifetime;
	}

	public override string GetCaption()
	{
		return "Fade screen";
	}

	public override CommandSignalData[] GetExtraSignals()
	{
		if (IsContinuous)
		{
			return new CommandSignalData[1] { m_OnFaded };
		}
		return base.GetExtraSignals();
	}
}
