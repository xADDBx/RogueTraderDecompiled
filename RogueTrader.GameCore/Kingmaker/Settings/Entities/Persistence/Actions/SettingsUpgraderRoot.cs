using System;
using Kingmaker.Settings.Entities.Persistence.Actions.UpgradeActions;
using Kingmaker.Settings.Interfaces;
using Owlcat.Runtime.Core.Logging;

namespace Kingmaker.Settings.Entities.Persistence.Actions;

public static class SettingsUpgraderRoot
{
	private static readonly LogChannel Logger = LogChannelFactory.GetOrCreate("SettingsUpgraderRoot");

	private const string StorageKey = "applied-settings-upgraders";

	private static readonly ISettingsUpgradeAction[] Actions = new ISettingsUpgradeAction[15]
	{
		new ActionLogText("Hello Upgraders"),
		new ActionResetIfOutsideOfRangeFloat("settings.accessiability.font-size", 0.8f, 1.2f, 1f),
		new ActionResetIfOutsideOfRangeFloat("settings.sound.volume-master", 0.001f, 100.001f, 100f),
		new ActionResetIfOutsideOfRangeFloat("settings.sound.volume-voices", 0.001f, 100.001f, 100f),
		new ActionResetIfOutsideOfRangeFloat("settings.sound.volume-voices-character-in-game", 0.001f, 100.001f, 100f),
		new ActionResetIfOutsideOfRangeFloat("settings.sound.volume-voices-npc-in-game", 0.001f, 100.001f, 100f),
		new ActionResetIfOutsideOfRangeFloat("settings.sound.volume-voices-dialogues", 0.001f, 100.001f, 100f),
		new ActionResetIfOutsideOfRangeFloat("settings.sound.volume-music", 0.001f, 100.001f, 100f),
		new ActionResetIfOutsideOfRangeFloat("settings.sound.volume-sfx", 0.001f, 100.001f, 100f),
		new ActionResetIfOutsideOfRangeFloat("settings.sound.volume-ambience", 0.001f, 100.001f, 100f),
		new ActionResetIfOutsideOfRangeFloat("settings.sound.volume-abilities", 0.001f, 100.001f, 100f),
		new ActionResetIfOutsideOfRangeFloat("settings.sound.volume-ranged-weapons", 0.001f, 100.001f, 100f),
		new ActionResetIfOutsideOfRangeFloat("settings.sound.volume-melee-weapons", 0.001f, 100.001f, 100f),
		new ActionResetIfOutsideOfRangeFloat("settings.sound.volume-hits-level", 0.001f, 100.001f, 100f),
		new ActionResetIfOutsideOfRangeFloat("settings.sound.volume-ui", 0.001f, 100.001f, 100f)
	};

	public static void Apply(ISettingsProvider provider)
	{
		int num;
		try
		{
			num = (provider.HasKey("applied-settings-upgraders") ? provider.GetValue<int>("applied-settings-upgraders") : 0);
		}
		catch (Exception ex)
		{
			Logger.Error(ex, "Exception getting already applied global upgraders");
			num = 0;
		}
		for (int i = num; i < Actions.Length; i++)
		{
			Actions[i].Upgrade(provider);
		}
		num = Actions.Length;
		try
		{
			provider.SetValue("applied-settings-upgraders", num);
		}
		catch (Exception ex2)
		{
			Logger.Error(ex2, "Failed to save applied global upgraders");
			if (provider.HasKey("applied-settings-upgraders"))
			{
				provider.RemoveKey("applied-settings-upgraders");
			}
		}
	}
}
