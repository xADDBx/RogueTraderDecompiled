using System;
using System.Collections.Generic;
using Core.Cheats;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items.Components;
using Kingmaker.Blueprints.Root;
using Kingmaker.Code.UI.MVVM.VM.MessageBox;
using Kingmaker.Designers;
using Kingmaker.DialogSystem.Blueprints;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.Items;
using Kingmaker.Localization;
using Kingmaker.Localization.Enums;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.Settings;
using Kingmaker.UI.Common;
using Kingmaker.UI.InputSystems;
using Kingmaker.UI.Models.SettingsUI;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Alignments;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility;
using Kingmaker.Utility.BuildModeUtils;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.View.MapObjects;
using Kingmaker.Visual.Particles;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.Visual.Effects.ParticleSumEmitter;
using Owlcat.Runtime.Visual.Effects.WeatherSystem;
using UnityEngine;
using UnityHeapEx;

namespace Kingmaker.Cheats;

public class CheatsCommon
{
	public class CheatedDice : IGlobalRulebookHandler<RuleRollDice>, IRulebookHandler<RuleRollDice>, ISubscriber, IGlobalRulebookSubscriber
	{
		public static readonly CheatedDice Singletone = new CheatedDice();

		public int Additive;

		private bool m_IsSubscribed;

		public void OnEventAboutToTrigger(RuleRollDice evt)
		{
		}

		public void OnEventDidTrigger(RuleRollDice evt)
		{
			if (evt.Initiator.IsPlayerFaction)
			{
				evt.Override(Additive);
			}
		}

		public void Subscribe()
		{
			if (!m_IsSubscribed)
			{
				EventBus.Subscribe(Singletone);
				m_IsSubscribed = true;
			}
		}

		public void Unsubscribe()
		{
			if (m_IsSubscribed)
			{
				EventBus.Unsubscribe(Singletone);
				m_IsSubscribed = false;
			}
		}
	}

	private static bool s_HudHidden;

	private static Canvas[] s_HiddenCanvases;

	private static int s_Grade = 1;

	private static readonly int[] Grades = new int[3] { -999, 0, 999 };

	private static readonly System.Random s_Rnd = new System.Random(42);

	[Cheat(Name = "random_enc", Description = "When false, random encounters on global map are disabled")]
	public static bool RandomEncounters { get; set; } = true;


	[Cheat(Name = "send_unity_events", Description = "When true, send unity analytic events as normal game does")]
	public static bool SendAnalyticEvents { get; set; }

	[Cheat(Name = "ignore_encumbrance", Description = "When true, encumbrance is always Light")]
	public static bool IgnoreEncumbrance { get; set; }

	public static void RegisterCheats(KeyboardAccess keyboard)
	{
		CheatsTransfer.RegisterCommands(keyboard);
		CheatsUnlock.RegisterCommands(keyboard);
		CheatsStats.RegisterCommands();
		CheatsPooling.RegisterCommands(keyboard);
		CheatsTime.RegisterCommands(keyboard);
		CheatsDebug.RegisterCommands(keyboard);
		CheatsItems.RegisterCommands(keyboard);
		CheatsCombat.RegisterCommands(keyboard);
		CheatsSaves.RegisterCommands(keyboard);
		CheatsRomance.RegisterCheats();
		if (BuildModeUtility.CheatsEnabled)
		{
			keyboard.Bind("Action", delegate
			{
				CheatsHelper.Run("action @mouseover");
			});
			keyboard.Bind("ChecksFail", delegate
			{
				CheatsHelper.Run("checks_fail");
			});
			keyboard.Bind("ChecksSuccess", delegate
			{
				CheatsHelper.Run("checks_success");
			});
			keyboard.Bind("RandomEncounterStatusSwitch", delegate
			{
				CheatsHelper.Run("random_encounter_status_switch");
			});
			keyboard.Bind("StatCoercion", delegate
			{
				CheatsHelper.Run("stat_coercion");
			});
			keyboard.Bind("SwitchHighlightCovers", delegate
			{
				CheatsHelper.Run("switch_highlight_covers");
			});
			keyboard.Bind("ShowDebugBubble", delegate
			{
				CheatsHelper.Run("emperor_open_my_eyes");
			});
			SmartConsole.RegisterCommand("produce_exception", CheatsDebug.ProduceException);
			SmartConsole.RegisterCommand("gain_xp", GainExperience);
			SmartConsole.RegisterCommand("remove_untyped_ac", RemoveUntypedAC);
			SmartConsole.RegisterCommand("set_locale", SetLocale);
			SmartConsole.RegisterCommand("dumpmem", delegate
			{
				HeapDump.DoStuff();
			});
			SmartConsole.RegisterCommand("shuffle_party", ShuffleParty);
			SmartConsole.RegisterCommand("reload_area", delegate
			{
				Game.Instance.ReloadArea();
			});
			SmartConsole.RegisterCommand("force_weather", ForceWeather);
			SmartConsole.RegisterCommand("change_weather", ChangeWeather);
			SmartConsole.RegisterCommand("ui_toggle", ToggleHUD);
			SmartConsole.RegisterCommand("item_dialog", ItemDialog);
			SmartConsole.RegisterCommand("hardware_detect", TestHardwareDetect);
			SmartConsole.RegisterCommand("show_titles", ShowTitles);
			SmartConsole.RegisterCommand("show_net", ShowNet);
		}
	}

	private static void ShowNet(string parameters)
	{
		EventBus.RaiseEvent(delegate(INetLobbyRequest h)
		{
			h.HandleNetLobbyRequest();
		});
	}

	private static void ItemDialog(string parameters)
	{
		foreach (ItemEntity item in Game.Instance.Player.MainCharacterEntity.Inventory)
		{
			if (item.Blueprint.ComponentsArray.TryFind((BlueprintComponent x) => x is ItemDialog, out var result) && result is ItemDialog itemDialog)
			{
				itemDialog.StartDialog();
			}
		}
	}

	private static void ToggleHUD(string parameters)
	{
		s_HudHidden = !s_HudHidden;
		if (s_HudHidden)
		{
			s_HiddenCanvases = UnityEngine.Object.FindObjectsOfType<Canvas>();
		}
		Canvas[] array = s_HiddenCanvases;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].gameObject.SetActive(!s_HudHidden);
		}
	}

	private static void ChangeWeather(string parameters)
	{
		Game.Instance.Player.Weather.NextWeatherChange = TimeSpan.MinValue;
	}

	private static void ForceWeather(string parameters)
	{
		InclemencyType? paramEnum = Utilities.GetParamEnum<InclemencyType>(parameters, 1, "Usage: force_weather <inclemency_type>");
		if (paramEnum.HasValue)
		{
			Game.Instance.Player.Weather.CurrentWeather = paramEnum.Value;
			EventBus.RaiseEvent(delegate(IWeatherUpdateHandler h)
			{
				h.OnUpdateWeatherSystem(overrideWeather: true);
			});
		}
		else
		{
			EventBus.RaiseEvent(delegate(IWeatherUpdateHandler h)
			{
				h.OnUpdateWeatherSystem(overrideWeather: false);
			});
		}
	}

	[Cheat(Name = "action", ExecutionPolicy = ExecutionPolicy.PlayMode)]
	public static void ExecuteAction(MechanicEntity target)
	{
		if (target != null)
		{
			using (new MechanicsContext(Game.Instance.DefaultUnit, null, BlueprintRoot.Instance).GetDataScope((TargetWrapper)target))
			{
				BlueprintRoot.Instance.Cheats.TestAction.Run();
				return;
			}
		}
		BlueprintRoot.Instance.Cheats.TestAction.Run();
	}

	[Cheat(Name = "random_encounter_status_switch", ExecutionPolicy = ExecutionPolicy.PlayMode)]
	public static void RandomEncounterStatusSwitch()
	{
		RandomEncounters = !RandomEncounters;
		string text = "Random encounter is " + (RandomEncounters ? "enabled" : "disabled");
		PFLog.Default.Log(text);
		UIUtility.SendWarning(text);
	}

	[Cheat(Name = "checks_fail", ExecutionPolicy = ExecutionPolicy.PlayMode)]
	public static void ChecksFail()
	{
		s_Grade = Math.Max(0, s_Grade - 1);
		SetPlayerDice(Grades[s_Grade]);
	}

	[Cheat(Name = "checks_success", ExecutionPolicy = ExecutionPolicy.PlayMode)]
	public static void ChecksSuccess()
	{
		s_Grade = Math.Min(Grades.Length - 1, s_Grade + 1);
		SetPlayerDice(Grades[s_Grade]);
	}

	[Cheat(Name = "set_dice", ExecutionPolicy = ExecutionPolicy.PlayMode)]
	public static void SetPlayerDice(int value)
	{
		CheatedDice.Singletone.Subscribe();
		CheatedDice.Singletone.Additive = Math.Max(1, value);
		PFLog.Default.Log("Dice was cheated with " + CheatedDice.Singletone.Additive);
	}

	[Cheat(Name = "release_dice", ExecutionPolicy = ExecutionPolicy.PlayMode)]
	public static void ReleasePlayerDice()
	{
		CheatedDice.Singletone.Unsubscribe();
	}

	private static void GainExperience(string parameters)
	{
		int? paramInt = Utilities.GetParamInt(parameters, 1, "Can't parse xp amount");
		if (paramInt.HasValue)
		{
			GameHelper.GainExperience(paramInt.Value);
		}
	}

	private static void RemoveUntypedAC(string parameters)
	{
		PFLog.SmartConsole.Log("Not implemented");
	}

	private static void SetLocale(string parameters)
	{
		try
		{
			string paramString = Utilities.GetParamString(parameters, 1, "Locale not specified");
			Locale locale = (Locale)Enum.Parse(typeof(Locale), paramString, ignoreCase: true);
			if (Enum.IsDefined(typeof(Locale), locale))
			{
				LocalizationManager.Instance.CurrentLocale = locale;
			}
		}
		catch (Exception)
		{
		}
	}

	private static SoulMarkDirection? ParseSoulMark(string p, bool logError = true)
	{
		SoulMarkDirection value;
		switch (p)
		{
		case "faith":
			value = SoulMarkDirection.Faith;
			break;
		case "hope":
			value = SoulMarkDirection.Hope;
			break;
		case "corruption":
			value = SoulMarkDirection.Corruption;
			break;
		default:
			if (logError)
			{
				SmartConsole.Print("Can't parse soulmark, use one of these: hope|faith|corruption");
			}
			return null;
		}
		return value;
	}

	[Cheat(Name = "show_soul_marks", ExecutionPolicy = ExecutionPolicy.PlayMode)]
	public static string ShowSoulMarks()
	{
		SoulMark soulMark = SoulMarkShiftExtension.GetSoulMark(SoulMarkDirection.Faith);
		SoulMark soulMark2 = SoulMarkShiftExtension.GetSoulMark(SoulMarkDirection.Hope);
		SoulMark soulMark3 = SoulMarkShiftExtension.GetSoulMark(SoulMarkDirection.Corruption);
		return "Faith: " + (soulMark?.Rank.ToString() ?? "not available") + ", Hope: " + (soulMark2?.Rank.ToString() ?? "not available") + ", Corruption " + (soulMark3?.Rank.ToString() ?? "not available");
	}

	[Cheat(Name = "shift_soul_mark", ExecutionPolicy = ExecutionPolicy.PlayMode)]
	public static void ShiftSoulMark(string soulMark, int value)
	{
		SoulMarkDirection? soulMarkDirection = ParseSoulMark(soulMark);
		if (soulMarkDirection.HasValue)
		{
			SoulMarkShift soulMarkShift = new SoulMarkShift
			{
				Direction = soulMarkDirection.Value,
				Value = value
			};
			new BlueprintAnswer
			{
				SoulMarkShift = soulMarkShift
			}.ApplyShiftDialog();
		}
	}

	[Cheat(Name = "change_localization", ExecutionPolicy = ExecutionPolicy.PlayMode)]
	public static void ChangeLocalization(Locale locale)
	{
		if (locale == LocalizationManager.Instance.CurrentLocale)
		{
			return;
		}
		SettingsController.Instance.Sync();
		SettingsController.Instance.ConfirmAllTempValues();
		SettingsController.Instance.SaveAll();
		UISettingsRoot.Instance.UIGameMainSettings.LocalizationSetting.SetIndexValueAndConfirm((int)locale);
		bool isMainMenu = Game.Instance.SceneLoader.LoadedUIScene == GameScenes.MainMenu;
		Game.ResetUI(delegate
		{
			EventBus.RaiseEvent(delegate(ISettingsUIHandler h)
			{
				h.HandleOpenSettings(isMainMenu);
			});
		});
	}

	[Cheat(Name = "emperor_open_my_eyes", ExecutionPolicy = ExecutionPolicy.PlayMode)]
	public static void ShowDebugBubble()
	{
		Game.Instance.RootUiContext.IsDebugBlueprintsInformationShow = !Game.Instance.RootUiContext.IsDebugBlueprintsInformationShow;
		if (Game.Instance.RootUiContext.IsDebugBlueprintsInformationShow)
		{
			EventBus.RaiseEvent(delegate(IDebugInformationUIHandler h)
			{
				h.HandleShowDebugBubble();
			});
		}
		else
		{
			EventBus.RaiseEvent(delegate(IDebugInformationUIHandler h)
			{
				h.HandleHideDebugBubble();
			});
		}
	}

	[Cheat(Name = "show_message_box", ExecutionPolicy = ExecutionPolicy.PlayMode)]
	public static void ShowDebugMessageBox()
	{
		UIUtility.ShowMessageBox("this is debug message box. Hallo!", DialogMessageBoxBase.BoxType.Dialog, null);
	}

	private static void ShuffleParty(string parameters)
	{
		List<BaseUnitEntity> party = Game.Instance.Player.Party;
		int num = party.Count;
		while (num > 1)
		{
			int index = s_Rnd.Next(0, num) % num;
			num--;
			BaseUnitEntity value = party[index];
			party[index] = party[num];
			party[num] = value;
		}
	}

	[Cheat(Name = "stat_coercion", ExecutionPolicy = ExecutionPolicy.PlayMode)]
	public static void StatCoercion()
	{
		GameHelper.GetPlayerCharacter().Skills.SkillCoercion.BaseValue = 40;
		UIUtility.SendWarning("Skill perception setted to 40");
	}

	private static void TestHardwareDetect(string parameters)
	{
		switch (HardwareConfigDetect.GetConfigIndex())
		{
		case HardwareConfigDetect.HardwareLevel.Low:
			Debug.Log("Test hardware detected: LOW");
			break;
		case HardwareConfigDetect.HardwareLevel.Medium:
			Debug.Log("Test hardware detected: MEDIUM");
			break;
		case HardwareConfigDetect.HardwareLevel.High:
			Debug.Log("Test hardware detected: HIGH");
			break;
		default:
			Debug.Log("Test hardware detected: UNKNOWN");
			break;
		}
	}

	private static void ShowTitles(string parameters)
	{
		EventBus.RaiseEvent(delegate(IEndGameTitlesUIHandler h)
		{
			h.HandleShowEndGameTitles(returnToMainMenu: false);
		});
	}

	[Cheat(Name = "clean_space", ExecutionPolicy = ExecutionPolicy.PlayMode)]
	public static void CleanSpace()
	{
		foreach (Entity entity2 in Game.Instance.State.Entities)
		{
			if (entity2 is BaseUnitEntity baseUnitEntity && baseUnitEntity.LifeState.IsFinallyDead)
			{
				Game.Instance.EntityDestroyer.Destroy(baseUnitEntity);
			}
			if (entity2 is DroppedLoot.EntityData entity)
			{
				Game.Instance.EntityDestroyer.Destroy(entity);
			}
		}
		FxHelper.DestroyAll();
		MonoSingleton<ParticleSystemCustomSubEmitterDelegate>.Instance.ClearAllParticles();
	}
}
