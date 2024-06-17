using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Core.Async;
using JetBrains.Annotations;
using Kingmaker.Blueprints.Base;
using Kingmaker.Localization.Enums;
using Kingmaker.Localization.Shared;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.Settings.Entities;
using Kingmaker.Settings.Interfaces;
using Kingmaker.Stores;
using Kingmaker.Utility.BuildModeUtils;
using Kingmaker.Utility.CodeTimer;
using Kingmaker.Utility.CommandLineArgs;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Utility.UnityExtensions;
using Newtonsoft.Json;
using Owlcat.Runtime.Core.Logging;
using Steamworks;
using UnityEngine;

namespace Kingmaker.Localization;

public class LocalizationManager : ILocalizationProvider
{
	private static readonly LogChannel Logger = LogChannelFactory.GetOrCreate("LocalizationManager");

	public static readonly LocalizationManager Instance = new LocalizationManager();

	private readonly List<ILocaleStorageProvider> m_LocaleStorageProviders = new List<ILocaleStorageProvider>();

	private static readonly Dictionary<string, Locale> SteamCodesToLocales = new Dictionary<string, Locale>
	{
		{
			"english",
			Locale.enGB
		},
		{
			"german",
			Locale.deDE
		},
		{
			"french",
			Locale.frFR
		},
		{
			"russian",
			Locale.ruRU
		},
		{
			"schinese",
			Locale.zhCN
		},
		{
			"tchinese",
			Locale.zhCN
		}
	};

	private static readonly Dictionary<SystemLanguage, Locale> UnityCodesToLocales = new Dictionary<SystemLanguage, Locale>
	{
		{
			SystemLanguage.English,
			Locale.enGB
		},
		{
			SystemLanguage.German,
			Locale.deDE
		},
		{
			SystemLanguage.French,
			Locale.frFR
		},
		{
			SystemLanguage.Russian,
			Locale.ruRU
		},
		{
			SystemLanguage.Chinese,
			Locale.zhCN
		},
		{
			SystemLanguage.ChineseSimplified,
			Locale.zhCN
		},
		{
			SystemLanguage.ChineseTraditional,
			Locale.zhCN
		}
	};

	public Locale CurrentLocale
	{
		get
		{
			return m_LocaleStorageProviders.Last().Locale;
		}
		set
		{
			if (CurrentLocale == value)
			{
				return;
			}
			foreach (ILocaleStorageProvider localeStorageProvider in m_LocaleStorageProviders)
			{
				localeStorageProvider.Locale = value;
			}
		}
	}

	[CanBeNull]
	public LocalizationPack CurrentPack { get; private set; }

	[CanBeNull]
	public LocalizationPack SoundPack { get; private set; }

	public event Action<Locale> LocaleChanged;

	private LocalizationManager()
	{
		RegisterLocaleStorageProvider(StaticLocaleStorageProvider.Instance);
	}

	private static void SetCulture(Locale locale)
	{
		CultureInfo cultureInfo2 = (CultureInfo.DefaultThreadCurrentUICulture = (CultureInfo.DefaultThreadCurrentCulture = locale.GetCulture()));
		Thread.CurrentThread.CurrentCulture = cultureInfo2;
		Thread.CurrentThread.CurrentUICulture = cultureInfo2;
	}

	private void OnLocaleProviderValueChanged(Locale value)
	{
		if (BundlesUsageProvider.UseBundles)
		{
			LocalizationPack currentPack = CurrentPack;
			if (currentPack == null || currentPack.Locale != value)
			{
				CurrentPack?.Dispose();
				CurrentPack = LoadPack(value);
			}
		}
		SetCulture(value);
		this.LocaleChanged?.Invoke(value);
		EventBus.RaiseEvent(delegate(ILocalizationHandler h)
		{
			h.HandleLanguageChanged();
		});
	}

	private static void DetectAndSaveLocale(SettingsEntityEnum<Locale> localizationSettings, ISettingsController settingsController, bool canDetect)
	{
		if (!Application.isEditor)
		{
			string text = CommandLineArguments.Parse().Get("locale");
			if (text != null && !string.IsNullOrWhiteSpace(text) && Enum.TryParse<Locale>(text, out var result))
			{
				localizationSettings.SetValueAndConfirm(result);
				settingsController.SaveAll();
			}
			else if (canDetect)
			{
				result = DetectSystemLanguage();
				localizationSettings.SetValueAndConfirm(result);
				settingsController.SaveAll();
			}
		}
	}

	public void RegisterLocaleStorageProvider(ILocaleStorageProvider provider)
	{
		ILocaleStorageProvider localeStorageProvider = m_LocaleStorageProviders.LastOrDefault();
		if (localeStorageProvider != null)
		{
			localeStorageProvider.Changed -= OnLocaleProviderValueChanged;
		}
		m_LocaleStorageProviders.Remove(provider);
		m_LocaleStorageProviders.Add(provider);
		ILocaleStorageProvider localeStorageProvider2 = m_LocaleStorageProviders.LastOrDefault();
		if (localeStorageProvider2 != null)
		{
			localeStorageProvider2.Changed += OnLocaleProviderValueChanged;
		}
	}

	public void Init(SettingsEntityEnum<Locale> localizationSettings, ISettingsController settingsController, bool canDetect)
	{
		using (CodeTimer.New("LocalizationManager.Init()"))
		{
			Logger.Log("Initialize in progress.");
			DetectAndSaveLocale(localizationSettings, settingsController, canDetect);
			RegisterLocaleStorageProvider(new SettingsLocaleStorageProvider(localizationSettings));
			Logger.Log("Loading localization packs...");
			LoadAndSetSoundPack();
			if (BundlesUsageProvider.UseBundles)
			{
				LoadAndSetCurrentPack(CurrentLocale);
			}
		}
		async void LoadAndSetCurrentPack(Locale locale)
		{
			Logger.Log("Loading {0} Pack Async...", locale);
			await Awaiters.ThreadPool;
			LocalizationPack pack = LoadPack(locale);
			await Awaiters.UnityThread;
			if (pack == null)
			{
				Logger.Error("===== FAILED TO LOAD AND SET LOCALIZATION PACK!! Check if localization is missing. Quitting... ======");
				Application.Quit(-1);
			}
			CurrentPack = pack;
			this.LocaleChanged?.Invoke(locale);
			SetCulture(locale);
		}
		async void LoadAndSetSoundPack()
		{
			Logger.Log("Loading Sound Pack Async...");
			SoundPack = await Task.Run(() => LoadPack(Locale.Sound));
		}
	}

	[CanBeNull]
	private LocalizationPack LoadPack(Locale locale)
	{
		if (!BuildModeUtility.Data.UsePackedLocalization)
		{
			return LoadPack(Path.Combine(ApplicationPaths.streamingAssetsPath, "Localization/" + locale.ToString() + ".json"), locale);
		}
		return LoadBinPack(Path.Combine(ApplicationPaths.streamingAssetsPath, "Localization/" + locale.ToString() + ".bin"), locale);
	}

	public LocalizationPack LoadPack(string packPath, Locale locale)
	{
		try
		{
			if (!File.Exists(packPath))
			{
				throw new FileNotFoundException($"Pack file {packPath} for locale {locale} not found", packPath);
			}
			JsonSerializer jsonSerializer = new JsonSerializer();
			using StreamReader reader = new StreamReader(packPath);
			using (CodeTimer.New(Logger, "Loc pack loading: " + locale))
			{
				Logger.Log("Loading {0} Pack Started.", locale);
				LocalizationPack localizationPack;
				using (JsonTextReader reader2 = new JsonTextReader(reader))
				{
					localizationPack = jsonSerializer.Deserialize<LocalizationPack>(reader2);
					localizationPack.Locale = locale;
				}
				Logger.Log("Loaded localization pack {0}", locale);
				return localizationPack;
			}
		}
		catch (Exception ex)
		{
			Logger.Error(ex, "Failed to load localization pack " + locale);
			return null;
		}
	}

	private LocalizationPack LoadBinPack(string packPath, Locale locale)
	{
		try
		{
			if (!File.Exists(packPath))
			{
				throw new FileNotFoundException($"Pack file {packPath} for locale {locale} not found", packPath);
			}
			using (CodeTimer.New(Logger, "Loc pack loading: " + locale))
			{
				LocalizationPack localizationPack = new LocalizationPack
				{
					Locale = locale
				};
				localizationPack.InitFromBinary(packPath);
				Logger.Log("Loaded localization pack {0}", locale);
				return localizationPack;
			}
		}
		catch (Exception ex)
		{
			Logger.Error(ex, "Failed to load localization pack {0}", locale);
			return null;
		}
	}

	private static Locale DetectSystemLanguage()
	{
		Locale value;
		if (StoreManager.Store == StoreType.Steam && SteamManager.Initialized)
		{
			string currentGameLanguage = SteamApps.GetCurrentGameLanguage();
			if (SteamCodesToLocales.TryGetValue(currentGameLanguage, out value))
			{
				return value;
			}
		}
		if (!UnityCodesToLocales.TryGetValue(Application.systemLanguage, out value))
		{
			return Locale.enGB;
		}
		return value;
	}
}
