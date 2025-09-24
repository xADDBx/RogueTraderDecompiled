using Kingmaker.Blueprints.JsonSystem.Helpers;
using UnityEngine;

namespace Kingmaker.AreaLogic.Cutscenes.Commands;

[TypeId("18c7e400a67a4ebca8903d5947e5698b")]
public abstract class CommandFadeoutBase : CommandBase
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

	private bool m_Finished;

	private bool m_IsFadedOut;

	protected abstract float Lifetime { get; }

	public override bool IsContinuous => m_Continuous;

	public override bool TrySkip(CutscenePlayerData player)
	{
		return true;
	}

	protected abstract void Fadeout(bool fade);

	protected override void OnRun(CutscenePlayerData player, bool skipping)
	{
		m_Finished = skipping && !IsContinuous;
		if (!m_Finished)
		{
			Fadeout(fade: true);
			m_IsFadedOut = true;
		}
	}

	protected override void OnStop(CutscenePlayerData player)
	{
		if (m_IsFadedOut)
		{
			Fadeout(fade: false);
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
		m_Finished = !m_Continuous && time >= (double)Lifetime;
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
