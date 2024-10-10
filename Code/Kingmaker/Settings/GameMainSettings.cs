using Kingmaker.Localization.Enums;
using Kingmaker.Settings.Entities;
using Kingmaker.Settings.Interfaces;
using Kingmaker.Settings.LINQ;

namespace Kingmaker.Settings;

public class GameMainSettings
{
	public readonly SettingsEntityEnum<Locale> Localization;

	public readonly SettingsEntityBool AutofillActionbarSlots;

	public readonly SettingsEntityBool LootInCombat;

	public readonly SettingsEntityString SendGameStatisticServer;

	public readonly SettingsEntityBool SendGameStatistic;

	public readonly IReadOnlySettingEntity<bool> AskedSendGameStatistic;

	public readonly SettingsEntityBool SendSaves;

	public readonly SettingsEntityBool UseHotAreas;

	public readonly SettingsEntityBool BloodOnCharacters;

	public readonly SettingsEntityBool DismemberCharacters;

	public readonly SettingsEntityBool AcceleratedMove;

	public readonly SettingsEntityString PromoEmails;

	public readonly IReadOnlySettingEntity<bool> LocalizationWasTouched;

	public GameMainSettings(ISettingsController settingsController, GameMainSettingsDefaultValues defaultValues)
	{
		Localization = new SettingsEntityEnum<Locale>(settingsController, "locale", defaultValues.Localization, saveDependent: false, requireReboot: true);
		AutofillActionbarSlots = new SettingsEntityBool(settingsController, "autofill-actionbar-slots", defaultValues.AutofillActionbarSlots);
		LootInCombat = new SettingsEntityBool(settingsController, "loot-in-combat", defaultValues.LootInCombat);
		SendGameStatisticServer = new SettingsEntityString(settingsController, "send-game-statistics-server", null);
		SendGameStatistic = new SettingsEntityBool(settingsController, "send-game-statistics", defaultValues.SendGameStatistic);
		AskedSendGameStatistic = SendGameStatistic.WasTouched();
		SendSaves = new SettingsEntityBool(settingsController, "send-saves", defaultValues.SendSaves);
		UseHotAreas = new SettingsEntityBool(settingsController, "use-hot-areas", defaultValues.UseHotAreas);
		BloodOnCharacters = new SettingsEntityBool(settingsController, "blood-on-characters", defaultValues.BloodOnCharacters);
		DismemberCharacters = new SettingsEntityBool(settingsController, "dismember", defaultValues.DismemberCharacters);
		AcceleratedMove = new SettingsEntityBool(settingsController, "accelerated-move", defaultValues.AcceleratedMove);
		LocalizationWasTouched = Localization.WasTouched();
		PromoEmails = new SettingsEntityString(settingsController, "promo-emails", "");
	}
}
