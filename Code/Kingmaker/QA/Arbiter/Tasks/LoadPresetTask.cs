using System.Collections.Generic;
using Kingmaker.Blueprints.Area;
using Kingmaker.Blueprints.Root;
using Kingmaker.Code.UI.MVVM.VM.MainMenu;
using Kingmaker.QA.Arbiter.Service;

namespace Kingmaker.QA.Arbiter.Tasks;

public class LoadPresetTask : ArbiterTask
{
	private BlueprintAreaPreset m_Preset;

	public LoadPresetTask(ArbiterTask parentTask, BlueprintAreaPreset preset)
		: base(parentTask)
	{
		m_Preset = preset;
	}

	protected override IEnumerator<ArbiterTask> Routine()
	{
		base.Status = "Load preset '" + m_Preset.Area.Name + "'";
		using (ArbiterMeasurementTimer.StartTimer("StartNewGameFromPreset"))
		{
			Runner.ClearError();
			m_Preset.OverrideGameDifficulty = BlueprintRoot.Instance.DifficultyList.CoreDifficulty;
			MainMenuUI.Instance.EnterGame(delegate
			{
				Game.Instance.LoadNewGame(m_Preset);
			});
			BlueprintArea area = m_Preset.Area;
			yield return new GameLoadingWaitTask(this);
			yield return new WaitTask(this, delegate
			{
				ArbiterService.Logger.Log("Wait for CurrentlyLoadedArea '{0}' should be '{1}'", Game.Instance.CurrentlyLoadedArea, area);
				return Game.Instance.CurrentlyLoadedArea == area;
			});
			base.Status = "Preset '{m_Preset.Area.Name}' loaded";
		}
	}
}
