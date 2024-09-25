using System;

namespace Kingmaker.Settings;

[Serializable]
public class SettingsDefaultValues : IValidatable
{
	public SoundSettingsDefaultValues Sound;

	public GraphicsSettingsDefaultValues Graphics;

	public GameSettingsDefaultValues Game;

	public DifficultySettingsDefaultValues Difficulty;

	public ControlsSettingsDefaultValues Controls;

	public DisplaySettingsDefaultValues Display;

	public AccessiabilitySettingsDefaultValues Accessiability;

	public void OnValidate()
	{
		Controls.OnValidate();
	}
}
