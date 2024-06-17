using System;
using System.Collections.Generic;

namespace Kingmaker.Settings;

public class InSaveSettingsProvider : DictionarySettingsProvider
{
	protected override Dictionary<string, object> Dictionary => Game.Instance.Player?.Settings.List;

	public InSaveSettingsProvider()
	{
		if (Dictionary == null)
		{
			throw new NullReferenceException("[Settings] You can't create InSaveSettingsProvider if Player is not loaded");
		}
		IsEmpty = Dictionary.Count == 0;
	}

	public override void SaveAll()
	{
	}
}
