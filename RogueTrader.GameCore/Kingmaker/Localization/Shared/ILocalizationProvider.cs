using System;
using Kingmaker.Localization.Enums;
using Kingmaker.Settings.Entities;
using Kingmaker.Settings.Interfaces;

namespace Kingmaker.Localization.Shared;

public interface ILocalizationProvider
{
	Locale CurrentLocale { get; set; }

	LocalizationPack CurrentPack { get; }

	LocalizationPack SoundPack { get; }

	event Action<Locale> LocaleChanged;

	void Init(SettingsEntityEnum<Locale> localizationSettings, ISettingsController settingsController, bool canDetect);

	LocalizationPack LoadPack(string packPath, Locale locale);
}
