using System;

namespace Kingmaker.Settings;

[Serializable]
public class GameSaveSettingsDefaultValues
{
	public bool AutosaveEnabled;

	public int AutosaveSlots;

	public int QuicksaveSlots;
}
