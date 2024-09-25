using System.Collections;
using Kingmaker.Blueprints.Area;
using Kingmaker.Blueprints.Root;
using Kingmaker.Code.UI.MVVM.VM.MainMenu;

namespace Kingmaker.QA.Arbiter;

public class LoadPresetTask : ArbiterTask
{
	private BlueprintAreaPreset m_Preset;

	public LoadPresetTask(ArbiterTask parentTask, BlueprintAreaPreset preset)
		: base(parentTask)
	{
		m_Preset = preset;
	}

	protected override IEnumerator Routine()
	{
		base.Status = "Load preset '" + m_Preset.Area.Name + "'";
		using (ArbiterClientMeasurements.StartTimer("StartNewGameFromPreset"))
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
				PFLog.Arbiter.Log("Wait for CurrentlyLoadedArea '{0}' shuold be '{1}'", Game.Instance.CurrentlyLoadedArea, area);
				return Game.Instance.CurrentlyLoadedArea == area;
			});
			base.Status = "Preset '{m_Preset.Area.Name}' loaded";
		}
	}
}
