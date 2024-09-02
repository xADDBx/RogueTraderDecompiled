using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.GameModes;
using Kingmaker.Modding;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.Settings.Entities;
using Kingmaker.UI.InputSystems.Enums;
using Kingmaker.UI.Models.SettingsUI;
using Kingmaker.UI.Models.SettingsUI.SettingAssets;
using Kingmaker.Utility.BuildModeUtils;
using Kingmaker.Utility.CodeTimer;
using Kingmaker.Utility.CountingGuard;
using Kingmaker.Utility.GameConst;
using Kingmaker.Utility.UnityExtensions;
using Owlcat.Runtime.Core.Utility.Locator;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Kingmaker.UI.InputSystems;

public class KeyboardAccess : IFocusHandler, ISubscriber, IService, IDisposable
{
	private struct BindingPair
	{
		public string BindName;

		public Action Callback;
	}

	public enum ModificationSide
	{
		Any,
		Left,
		Right
	}

	public class Binding
	{
		public readonly bool WorksWhenUIPaused;

		public readonly bool IsCtrlDown;

		public readonly bool IsAltDown;

		public readonly bool IsShiftDown;

		public readonly TriggerType TriggerType;

		public readonly ModificationSide Side;

		public KeyCode Key { get; private set; }

		public string Name { get; private set; }

		public GameModeType GameMode { get; private set; }

		public Binding(string name, KeyCode key, GameModeType gameMode, bool isCtrlDown, bool isAltDown, bool isShiftDown, TriggerType triggerType, ModificationSide side = ModificationSide.Any, bool worksWhenUIPaused = true)
		{
			Name = name;
			Key = key;
			GameMode = gameMode;
			IsCtrlDown = isCtrlDown;
			IsAltDown = isAltDown;
			IsShiftDown = isShiftDown;
			TriggerType = triggerType;
			Side = side;
			WorksWhenUIPaused = worksWhenUIPaused;
		}

		public bool InputMatched()
		{
			if (!IsKeyTriggered())
			{
				return false;
			}
			if (IsShiftDown != IsShiftHold(Side))
			{
				return false;
			}
			if (IsCtrlDown != IsCtrlHold(Side))
			{
				return false;
			}
			if (IsAltDown != IsAltHold(Side))
			{
				return false;
			}
			return true;
		}

		private bool IsKeyTriggered()
		{
			return TriggerType switch
			{
				TriggerType.KeyUp => Input.GetKeyUp(Key), 
				TriggerType.KeyDown => Input.GetKeyDown(Key), 
				TriggerType.Hold => Input.GetKey(Key), 
				_ => false, 
			};
		}

		public string GetDisplayText()
		{
			StringBuilder stringBuilder = new StringBuilder();
			if (IsCtrlDown)
			{
				stringBuilder.Append("Ctrl+");
			}
			if (IsAltDown)
			{
				stringBuilder.Append("Alt+");
			}
			if (IsShiftDown)
			{
				stringBuilder.Append("Shift+");
			}
			stringBuilder.Append(UIStrings.Instance.KeyboardTexts.GetStringByKeyCode(Key));
			return stringBuilder.ToString();
		}

		public override string ToString()
		{
			return $"{Name} ({GameMode}) [{GetDisplayText()}]";
		}
	}

	private readonly Dictionary<string, List<Action>> m_BindingCallbacks = new Dictionary<string, List<Action>>();

	private readonly List<Binding> m_Bindings = new List<Binding>();

	private readonly List<BindingPair> m_BindingsToUnbind = new List<BindingPair>();

	public readonly CountingGuard Disabled = new CountingGuard();

	public static readonly KeyCode[] AltCodes = new KeyCode[3]
	{
		KeyCode.AltGr,
		KeyCode.LeftAlt,
		KeyCode.RightAlt
	};

	public static readonly KeyCode[] CtrlCodes = new KeyCode[2]
	{
		KeyCode.LeftControl,
		KeyCode.RightControl
	};

	public static readonly KeyCode[] ShiftCodes = new KeyCode[2]
	{
		KeyCode.LeftShift,
		KeyCode.RightShift
	};

	private static bool IsConsole
	{
		get
		{
			if (!Application.isEditor)
			{
				return Application.isConsolePlatform;
			}
			return false;
		}
	}

	public ServiceLifetimeType Lifetime => ServiceLifetimeType.Game;

	public static KeyboardAccess Instance
	{
		get
		{
			if (Services.GetInstance<KeyboardAccess>() == null)
			{
				Services.RegisterServiceInstance(new KeyboardAccess());
			}
			return Services.GetInstance<KeyboardAccess>();
		}
	}

	public KeyboardAccess()
	{
		EventBus.Subscribe(this);
	}

	public void Dispose()
	{
		EventBus.Unsubscribe(this);
	}

	[CanBeNull]
	public Binding GetBindingByName(string name)
	{
		return m_Bindings.FirstOrDefault((Binding b) => b.Name == name);
	}

	public void Tick()
	{
		if (IsConsole || IsInputFieldSelected() || (bool)Disabled || NewBindingSelection())
		{
			return;
		}
		foreach (Binding binding in m_Bindings)
		{
			OnCallbackByBinding(binding);
		}
		using (ProfileScope.New("Do Unbind"))
		{
			foreach (BindingPair item in m_BindingsToUnbind)
			{
				DoUnbind(item.BindName, item.Callback);
			}
			m_BindingsToUnbind.Clear();
		}
	}

	private void OnCallbackByBinding(Binding binding, bool force = false)
	{
		if (binding.GameMode != Game.Instance.CurrentMode || (Game.Instance.PauseController.IsPausedByLocalPlayer && !binding.WorksWhenUIPaused) || !m_BindingCallbacks.TryGetValue(binding.Name, out var value) || value.Count == 0 || (!binding.InputMatched() && !force) || (OwlcatModificationsWindow.IsActive && UISettingsRoot.Instance.UIKeybindGeneralSettings.OpenModificationsWindow.name != binding.Name))
		{
			return;
		}
		try
		{
			using (ProfileScope.New(binding.Name))
			{
				foreach (Action item in value)
				{
					item();
				}
			}
		}
		catch (Exception ex)
		{
			PFLog.UI.Exception(ex);
		}
	}

	public static bool IsInputFieldSelected()
	{
		using (ProfileScope.New("Input Field Check"))
		{
			EventSystem current = EventSystem.current;
			if (current == null)
			{
				return false;
			}
			GameObject currentSelectedGameObject = current.currentSelectedGameObject;
			if (currentSelectedGameObject == null)
			{
				return false;
			}
			if (currentSelectedGameObject.GetComponent<TMP_InputField>() == null)
			{
				return false;
			}
			if (Input.GetKeyDown(KeyCode.Escape))
			{
				current.SetSelectedGameObject(null);
			}
			return true;
		}
	}

	private bool NewBindingSelection()
	{
		return Game.Instance.UISettingsManager.IsNewKeyBindingSelectionHappening;
	}

	public void RegisterBinding(string name, KeyCode key, IEnumerable<GameModeType> gameModes, bool worksWhenUIPaused = true)
	{
		foreach (GameModeType gameMode in gameModes)
		{
			RegisterBinding(name, key, gameMode, ctrl: false, alt: false, shift: false, TriggerType.KeyDown, ModificationSide.Any, worksWhenUIPaused);
		}
	}

	public void RegisterBinding(string name, KeyCode key, IEnumerable<GameModeType> gameModes, bool ctrl, bool alt, bool shift, TriggerType release = TriggerType.KeyDown, ModificationSide side = ModificationSide.Any, bool worksWhenUIPaused = true)
	{
		foreach (GameModeType gameMode in gameModes)
		{
			RegisterBinding(name, key, gameMode, ctrl, alt, shift, release, side, worksWhenUIPaused);
		}
	}

	private void RegisterBinding(string name, KeyCode key, GameModeType gameMode, bool ctrl, bool alt, bool shift, TriggerType release, ModificationSide side, bool worksWhenUIPaused)
	{
		foreach (Binding binding in m_Bindings)
		{
			if (binding.Key == key && !(binding.GameMode != gameMode) && binding.IsShiftDown == shift && binding.IsAltDown == alt && binding.IsCtrlDown == ctrl && binding.TriggerType == release && binding.Side == side && binding.WorksWhenUIPaused == worksWhenUIPaused)
			{
				if (binding.Name == name)
				{
					return;
				}
				PFLog.UI.Warning("Key binding {0}[shift={1}; alt={2}; ctrl={3}; side={4}; worksWhenUIPaused={5}] conflicts with {6}", name, binding.IsShiftDown, binding.IsCtrlDown, binding.IsAltDown, binding.Side, binding.WorksWhenUIPaused, binding.Name);
			}
		}
		Binding item = new Binding(name, key, gameMode, ctrl, alt, shift, release, side, worksWhenUIPaused);
		m_Bindings.Add(item);
	}

	public void UnregisterBinding(string name)
	{
		List<Binding> list = new List<Binding>();
		foreach (Binding binding in m_Bindings)
		{
			if (!(binding.Name != name))
			{
				list.Add(binding);
			}
		}
		foreach (Binding item in list)
		{
			m_Bindings.Remove(item);
		}
	}

	public void RegisterBinding(string name, KeyBindingData data, GameModesGroup group, bool isTriggerOnHold, bool isHoldTrigger = false)
	{
		if (data.Key != 0)
		{
			bool worksWhenUIPaused = group == GameModesGroup.All || group == GameModesGroup.AllExceptBugReport || group == GameModesGroup.ServiceWindows || group == GameModesGroup.WorldFullscreenUI;
			if (!isTriggerOnHold)
			{
				TriggerType release = ((!isHoldTrigger) ? TriggerType.KeyDown : TriggerType.Hold);
				RegisterBinding(name, data.Key, GetGameModesArray(group), data.IsCtrlDown, data.IsAltDown, data.IsShiftDown, release, ModificationSide.Any, worksWhenUIPaused);
			}
			else
			{
				RegisterBinding(name + UIConsts.SuffixOn, data.Key, GetGameModesArray(group), data.IsCtrlDown, data.IsAltDown, data.IsShiftDown, TriggerType.KeyDown, ModificationSide.Any, worksWhenUIPaused);
				RegisterBinding(name + UIConsts.SuffixOff, data.Key, GetGameModesArray(group), data.IsCtrlDown, data.IsAltDown, data.IsShiftDown, TriggerType.KeyUp, ModificationSide.Any, worksWhenUIPaused);
			}
		}
	}

	public IDisposable Bind(string bindingName, Action callback)
	{
		if (IsConsole)
		{
			return null;
		}
		if (m_Bindings.All((Binding b) => b.Name != bindingName))
		{
			PFLog.UI.Warning("Bind: no binding named {0}", bindingName);
		}
		m_BindingsToUnbind.RemoveAll((BindingPair v) => v.BindName == bindingName && v.Callback == callback);
		if (!m_BindingCallbacks.TryGetValue(bindingName, out var value))
		{
			value = (m_BindingCallbacks[bindingName] = new List<Action>());
		}
		if (!value.Contains(callback))
		{
			value.Add(callback);
		}
		return Disposable.Create(delegate
		{
			Unbind(bindingName, callback);
		});
	}

	public void Unbind(string bindingName, Action callback)
	{
		if (!IsConsole)
		{
			m_BindingsToUnbind.Add(new BindingPair
			{
				BindName = bindingName,
				Callback = callback
			});
		}
	}

	private void DoUnbind(string bindingName, Action callback)
	{
		if (m_Bindings.All((Binding b) => b.Name != bindingName))
		{
			PFLog.UI.Warning("Bind: no binding named {0}", bindingName);
		}
		if (m_BindingCallbacks.TryGetValue(bindingName, out var value))
		{
			value.Remove(callback);
		}
	}

	public void UnbindAll()
	{
		m_BindingCallbacks.Clear();
	}

	public void RegisterBuiltinBindings()
	{
		GameModeType[] gameModesArray = GetGameModesArray(GameModesGroup.All);
		GameModeType[] gameModesArray2 = GetGameModesArray(GameModesGroup.AllExceptBugReport);
		Game.Instance.UISettingsManager.Initialize();
		foreach (UISettingsEntityKeyBinding keyBinding in Game.Instance.UISettingsManager.KeyBindings)
		{
			keyBinding.RenewRegisteredBindings();
		}
		RegisterBinding("EscPressed", KeyCode.Escape, gameModesArray);
		if (BuildModeUtility.IsDevelopment)
		{
			GameModeType[] gameModesArray3 = GetGameModesArray(GameModesGroup.World);
			RegisterBinding("SwitchUberConsole", KeyCode.BackQuote, gameModesArray2);
			RegisterBinding("SwitchNetMenu", KeyCode.BackQuote, gameModesArray2, ctrl: true, alt: false, shift: false);
			RegisterBinding("Kill", KeyCode.K, gameModesArray3);
			RegisterBinding("KillAll", KeyCode.K, gameModesArray3, ctrl: false, alt: true, shift: false);
			RegisterBinding("ChecksFail", KeyCode.Keypad1, gameModesArray, ctrl: true, alt: false, shift: false);
			RegisterBinding("ChecksFail", KeyCode.End, gameModesArray, ctrl: true, alt: false, shift: false);
			RegisterBinding("ChecksSuccess", KeyCode.Keypad2, gameModesArray, ctrl: true, alt: false, shift: false);
			RegisterBinding("ChecksSuccess", KeyCode.Home, gameModesArray, ctrl: true, alt: false, shift: false);
			RegisterBinding("Tp2GlobalMap", KeyCode.Keypad4, gameModesArray, ctrl: true, alt: false, shift: false);
			RegisterBinding("TimeScaleUp", KeyCode.KeypadPlus, gameModesArray);
			RegisterBinding("TimeScaleUp", KeyCode.PageUp, gameModesArray, ctrl: true, alt: false, shift: false);
			RegisterBinding("TimeScaleDown", KeyCode.KeypadMinus, gameModesArray);
			RegisterBinding("TimeScaleDown", KeyCode.PageDown, gameModesArray, ctrl: true, alt: false, shift: false);
			RegisterBinding("Manchkin", KeyCode.Keypad0, gameModesArray);
			RegisterBinding("Damage", KeyCode.Z, gameModesArray3);
			RegisterBinding("DamageALot", KeyCode.Z, gameModesArray3, ctrl: true, alt: false, shift: true);
			RegisterBinding("Heal", KeyCode.Z, gameModesArray3, ctrl: false, alt: false, shift: true);
			RegisterBinding("SetMP100", KeyCode.M, gameModesArray3, ctrl: false, alt: true, shift: false);
			RegisterBinding("GameInfo", KeyCode.C, gameModesArray, ctrl: true, alt: false, shift: false);
			RegisterBinding("AddGameInfo", KeyCode.C, gameModesArray, ctrl: true, alt: true, shift: false);
			RegisterBinding("ExtendedInfo", KeyCode.E, gameModesArray, ctrl: false, alt: true, shift: false);
			RegisterBinding("RestAll", KeyCode.R, gameModesArray, ctrl: false, alt: false, shift: true);
			RegisterBinding("DetachDebuff", KeyCode.X, gameModesArray, ctrl: false, alt: false, shift: true);
			RegisterBinding("RandomEncounterStatusSwitch", KeyCode.Keypad6, gameModesArray, ctrl: true, alt: false, shift: false);
			RegisterBinding("LocalTeleport", KeyCode.T, gameModesArray3, ctrl: true, alt: false, shift: false);
			RegisterBinding("ExecuteFromBuffer", KeyCode.V, gameModesArray, ctrl: true, alt: false, shift: false);
			RegisterBinding("StatCoercion", KeyCode.P, gameModesArray, ctrl: false, alt: false, shift: true);
			RegisterBinding("Cheat_MakeEnemy", KeyCode.E, gameModesArray, ctrl: true, alt: false, shift: true);
			RegisterBinding("AoELoot", KeyCode.G, gameModesArray, ctrl: true, alt: false, shift: true);
			RegisterBinding("Action", KeyCode.O, gameModesArray);
			RegisterBinding("PersistantQuickSave", KeyCode.F5, gameModesArray, ctrl: false, alt: false, shift: true);
			RegisterBinding("DebugQuestNext", KeyCode.Keypad2, gameModesArray, ctrl: true, alt: false, shift: true);
			RegisterBinding("DebugQuestPrevious", KeyCode.Keypad8, gameModesArray, ctrl: true, alt: false, shift: true);
			RegisterBinding("DebugObjectiveNext", KeyCode.Keypad4, gameModesArray, ctrl: true, alt: false, shift: true);
			RegisterBinding("DebugObjectivePrevious", KeyCode.Keypad6, gameModesArray, ctrl: true, alt: false, shift: true);
			RegisterBinding("DebugObjectiveComplete", KeyCode.Keypad5, gameModesArray, ctrl: true, alt: false, shift: true);
			RegisterBinding("SkipDay", KeyCode.KeypadDivide, gameModesArray, ctrl: true, alt: false, shift: false);
			RegisterBinding("SkipWeek", KeyCode.KeypadDivide, gameModesArray, ctrl: true, alt: false, shift: true);
			RegisterBinding("AllAudioMute", KeyCode.M, gameModesArray, ctrl: true, alt: false, shift: false);
			RegisterBinding("MusicMute", KeyCode.M, gameModesArray, ctrl: true, alt: false, shift: true);
			RegisterBinding("SwitchHighlightCovers", KeyCode.C, gameModesArray, ctrl: false, alt: false, shift: true);
			RegisterBinding("ReloadUI", KeyCode.R, gameModesArray, ctrl: true, alt: false, shift: false);
			RegisterBinding("ChangeUINextPlatform", KeyCode.RightArrow, gameModesArray, ctrl: false, alt: true, shift: true);
			RegisterBinding("ChangeUIPrevPlatform", KeyCode.LeftArrow, gameModesArray, ctrl: false, alt: true, shift: true);
			RegisterBinding("ShowDebugBubble", KeyCode.E, gameModesArray, ctrl: true, alt: true, shift: false);
		}
		else
		{
			RegisterBinding("Console", KeyCode.BackQuote, gameModesArray2, ctrl: true, alt: true, shift: true);
		}
		RegisterBinding("RapidAreaBug", KeyCode.A, gameModesArray, ctrl: true, alt: true, shift: true, TriggerType.KeyDown, ModificationSide.Left);
		RegisterBinding("RapidInterfaceBug", KeyCode.U, gameModesArray, ctrl: true, alt: true, shift: true, TriggerType.KeyDown, ModificationSide.Left);
		RegisterBinding("RapidEngineBug", KeyCode.E, gameModesArray, ctrl: true, alt: true, shift: true, TriggerType.KeyDown, ModificationSide.Left);
		RegisterBinding("RapidCoreMechanicsBug", KeyCode.M, gameModesArray, ctrl: true, alt: true, shift: true, TriggerType.KeyDown, ModificationSide.Left);
		RegisterBinding("RapidLoreBug", KeyCode.L, gameModesArray, ctrl: true, alt: true, shift: true, TriggerType.KeyDown, ModificationSide.Left);
		RegisterBinding("RapidGlobalMapBug", KeyCode.G, gameModesArray, ctrl: true, alt: true, shift: true, TriggerType.KeyDown, ModificationSide.Left);
		RegisterBinding("RapidCreaturesBug", KeyCode.C, gameModesArray, ctrl: true, alt: true, shift: true, TriggerType.KeyDown, ModificationSide.Left);
		RegisterBinding("RapidItemsBug", KeyCode.T, gameModesArray, ctrl: true, alt: true, shift: true, TriggerType.KeyDown, ModificationSide.Left);
		RegisterBinding("RapidLightingBug", KeyCode.H, gameModesArray, ctrl: true, alt: true, shift: true, TriggerType.KeyDown, ModificationSide.Left);
		RegisterBinding("RapidKingdomBug", KeyCode.K, gameModesArray, ctrl: true, alt: true, shift: true, TriggerType.KeyDown, ModificationSide.Left);
		RegisterBinding("RapidAreaBugNoSave", KeyCode.A, gameModesArray, ctrl: true, alt: true, shift: true, TriggerType.KeyDown, ModificationSide.Right);
		RegisterBinding("RapidInterfaceBugNoSave", KeyCode.U, gameModesArray, ctrl: true, alt: true, shift: true, TriggerType.KeyDown, ModificationSide.Right);
		RegisterBinding("RapidEngineBugNoSave", KeyCode.E, gameModesArray, ctrl: true, alt: true, shift: true, TriggerType.KeyDown, ModificationSide.Right);
		RegisterBinding("RapidCoreMechanicsBugNoSave", KeyCode.M, gameModesArray, ctrl: true, alt: true, shift: true, TriggerType.KeyDown, ModificationSide.Right);
		RegisterBinding("RapidLoreBugNoSave", KeyCode.L, gameModesArray, ctrl: true, alt: true, shift: true, TriggerType.KeyDown, ModificationSide.Right);
		RegisterBinding("RapidGlobalMapBugNoSave", KeyCode.G, gameModesArray, ctrl: true, alt: true, shift: true, TriggerType.KeyDown, ModificationSide.Right);
		RegisterBinding("RapidCreaturesBugNoSave", KeyCode.C, gameModesArray, ctrl: true, alt: true, shift: true, TriggerType.KeyDown, ModificationSide.Right);
		RegisterBinding("RapidItemsBugNoSave", KeyCode.T, gameModesArray, ctrl: true, alt: true, shift: true, TriggerType.KeyDown, ModificationSide.Right);
		RegisterBinding("RapidLightingBugNoSave", KeyCode.H, gameModesArray, ctrl: true, alt: true, shift: true, TriggerType.KeyDown, ModificationSide.Right);
		RegisterBinding("RapidKingdomBugNoSave", KeyCode.K, gameModesArray, ctrl: true, alt: true, shift: true, TriggerType.KeyDown, ModificationSide.Right);
		RegisterBinding("RapidBugReportWindowOpen", KeyCode.B, gameModesArray, ctrl: false, alt: true, shift: false);
		RegisterBinding("BrowserOpenLastReport", KeyCode.F12, gameModesArray, ctrl: true, alt: true, shift: false);
	}

	public static GameModeType[] GetGameModesArray(GameModesGroup gameModesGroup)
	{
		return gameModesGroup switch
		{
			GameModesGroup.All => new GameModeType[12]
			{
				GameModeType.None,
				GameModeType.Default,
				GameModeType.Dialog,
				GameModeType.Pause,
				GameModeType.SpaceCombat,
				GameModeType.StarSystem,
				GameModeType.GlobalMap,
				GameModeType.Pause,
				GameModeType.Cutscene,
				GameModeType.GameOver,
				GameModeType.BugReport,
				GameModeType.CutsceneGlobalMap
			}, 
			GameModesGroup.AllExceptBugReport => new GameModeType[11]
			{
				GameModeType.None,
				GameModeType.Default,
				GameModeType.Dialog,
				GameModeType.Pause,
				GameModeType.SpaceCombat,
				GameModeType.StarSystem,
				GameModeType.GlobalMap,
				GameModeType.Pause,
				GameModeType.Cutscene,
				GameModeType.GameOver,
				GameModeType.CutsceneGlobalMap
			}, 
			GameModesGroup.Dialog => new GameModeType[1] { GameModeType.Dialog }, 
			GameModesGroup.World => new GameModeType[4]
			{
				GameModeType.Default,
				GameModeType.Pause,
				GameModeType.SpaceCombat,
				GameModeType.StarSystem
			}, 
			GameModesGroup.WorldFullscreenUI => new GameModeType[5]
			{
				GameModeType.Default,
				GameModeType.Pause,
				GameModeType.SpaceCombat,
				GameModeType.StarSystem,
				GameModeType.GlobalMap
			}, 
			GameModesGroup.ForPause => new GameModeType[5]
			{
				GameModeType.Default,
				GameModeType.Pause,
				GameModeType.SpaceCombat,
				GameModeType.StarSystem,
				GameModeType.GlobalMap
			}, 
			GameModesGroup.ServiceWindows => new GameModeType[5]
			{
				GameModeType.Default,
				GameModeType.Pause,
				GameModeType.SpaceCombat,
				GameModeType.StarSystem,
				GameModeType.GlobalMap
			}, 
			GameModesGroup.CameraControls => new GameModeType[6]
			{
				GameModeType.Default,
				GameModeType.Pause,
				GameModeType.SpaceCombat,
				GameModeType.StarSystem,
				GameModeType.GlobalMap,
				GameModeType.Dialog
			}, 
			_ => Array.Empty<GameModeType>(), 
		};
	}

	public bool CanBeRegistered(string name, KeyCode key, GameModesGroup gameModes, bool ctrl, bool alt, bool shift)
	{
		if (key == KeyCode.Mouse0 || key == KeyCode.Mouse1)
		{
			return false;
		}
		GameModeType[] gameModesArray = GetGameModesArray(gameModes);
		foreach (GameModeType gameModeType in gameModesArray)
		{
			foreach (Binding binding in m_Bindings)
			{
				if (binding.Key == key && !(binding.GameMode != gameModeType) && binding.IsShiftDown == shift && binding.IsAltDown == alt && binding.IsCtrlDown == ctrl)
				{
					if (binding.Name == name)
					{
						return true;
					}
					return false;
				}
			}
		}
		return true;
	}

	public static bool IsAltHold(ModificationSide side = ModificationSide.Any)
	{
		if (IsConsole)
		{
			return false;
		}
		if ((side != 0 || !AltCodes.Any(Input.GetKey)) && (side != ModificationSide.Left || !Input.GetKey(KeyCode.LeftAlt)))
		{
			if (side == ModificationSide.Right)
			{
				return Input.GetKey(KeyCode.RightAlt);
			}
			return false;
		}
		return true;
	}

	public static bool IsCtrlHold(ModificationSide side = ModificationSide.Any)
	{
		if (IsConsole)
		{
			return false;
		}
		if ((side != 0 || !CtrlCodes.Any(Input.GetKey)) && (side != ModificationSide.Left || !Input.GetKey(KeyCode.LeftControl)))
		{
			if (side == ModificationSide.Right)
			{
				return Input.GetKey(KeyCode.RightControl);
			}
			return false;
		}
		return true;
	}

	public static bool IsShiftHold(ModificationSide side = ModificationSide.Any)
	{
		if (IsConsole)
		{
			return false;
		}
		if ((side != 0 || !ShiftCodes.Any(Input.GetKey)) && (side != ModificationSide.Left || !Input.GetKey(KeyCode.LeftShift)))
		{
			if (side == ModificationSide.Right)
			{
				return Input.GetKey(KeyCode.RightShift);
			}
			return false;
		}
		return true;
	}

	public static bool IsAltDown(ModificationSide side = ModificationSide.Any)
	{
		if (IsConsole)
		{
			return false;
		}
		if ((side != 0 || !AltCodes.Any(Input.GetKeyDown)) && (side != ModificationSide.Left || !Input.GetKeyDown(KeyCode.LeftAlt)))
		{
			if (side == ModificationSide.Right)
			{
				return Input.GetKeyDown(KeyCode.RightAlt);
			}
			return false;
		}
		return true;
	}

	public static bool IsCtrlDown(ModificationSide side = ModificationSide.Any)
	{
		if (IsConsole)
		{
			return false;
		}
		if ((side != 0 || !CtrlCodes.Any(Input.GetKeyDown)) && (side != ModificationSide.Left || !Input.GetKeyDown(KeyCode.LeftControl)))
		{
			if (side == ModificationSide.Right)
			{
				return Input.GetKeyDown(KeyCode.RightControl);
			}
			return false;
		}
		return true;
	}

	public static bool IsShiftDown(ModificationSide side = ModificationSide.Any)
	{
		if (IsConsole)
		{
			return false;
		}
		if ((side != 0 || !ShiftCodes.Any(Input.GetKeyDown)) && (side != ModificationSide.Left || !Input.GetKeyDown(KeyCode.LeftShift)))
		{
			if (side == ModificationSide.Right)
			{
				return Input.GetKeyDown(KeyCode.RightShift);
			}
			return false;
		}
		return true;
	}

	public static bool IsAltUp(ModificationSide side = ModificationSide.Any)
	{
		if (IsConsole)
		{
			return false;
		}
		if ((side != 0 || !AltCodes.Any(Input.GetKeyUp)) && (side != ModificationSide.Left || !Input.GetKeyUp(KeyCode.LeftAlt)))
		{
			if (side == ModificationSide.Right)
			{
				return Input.GetKeyUp(KeyCode.RightAlt);
			}
			return false;
		}
		return true;
	}

	public static bool IsCtrlUp(ModificationSide side = ModificationSide.Any)
	{
		if (IsConsole)
		{
			return false;
		}
		if ((side != 0 || !CtrlCodes.Any(Input.GetKeyUp)) && (side != ModificationSide.Left || !Input.GetKeyUp(KeyCode.LeftControl)))
		{
			if (side == ModificationSide.Right)
			{
				return Input.GetKeyUp(KeyCode.RightControl);
			}
			return false;
		}
		return true;
	}

	public static bool IsShiftUp(ModificationSide side = ModificationSide.Any)
	{
		if (IsConsole)
		{
			return false;
		}
		if ((side != 0 || !ShiftCodes.Any(Input.GetKeyUp)) && (side != ModificationSide.Left || !Input.GetKeyUp(KeyCode.LeftShift)))
		{
			if (side == ModificationSide.Right)
			{
				return Input.GetKeyUp(KeyCode.RightShift);
			}
			return false;
		}
		return true;
	}

	public void OnApplicationFocusChanged(bool isFocused)
	{
		if (!ApplicationFocusEvents.KeyboardDisabled && !isFocused)
		{
			m_Bindings.FindAll((Binding b) => b.TriggerType == TriggerType.KeyUp).ForEach(delegate(Binding b)
			{
				OnCallbackByBinding(b, force: true);
			});
		}
	}
}
