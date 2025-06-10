using System.Collections.Generic;
using Kingmaker.Blueprints.Area;
using Kingmaker.Code.UI.MVVM.VM.MainMenu;
using Kingmaker.QA.Arbiter.Tasks;

namespace Kingmaker.QA.Arbiter;

public class StartNewGameFromPresetTask : ArbiterTask
{
	private BlueprintAreaPreset m_Preset;

	public StartNewGameFromPresetTask(ArbiterTask parentTask, BlueprintAreaPreset preset)
		: base(parentTask)
	{
		m_Preset = preset;
	}

	protected override IEnumerator<ArbiterTask> Routine()
	{
		yield return new ResetToMainMenuTask(this);
		if (MainMenuUI.Instance == null)
		{
			throw new ArbiterException("Failed to start game from preset: MainMenu not found!");
		}
		yield return new LoadPresetTask(this, m_Preset);
	}
}
