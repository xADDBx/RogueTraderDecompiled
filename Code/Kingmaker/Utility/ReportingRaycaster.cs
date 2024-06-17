using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Kingmaker.Blueprints;
using Kingmaker.Code.UI.MVVM.View.Overtips.CommonOvertipParts;
using Kingmaker.Code.UI.MVVM.View.ServiceWindows.Inventory;
using Kingmaker.Code.UI.MVVM.View.Slots;
using Kingmaker.Code.UI.MVVM.VM.Dialog.BookEvent;
using Kingmaker.Code.UI.MVVM.VM.Dialog.Dialog;
using Kingmaker.Code.UI.MVVM.VM.Dialog.Epilog;
using Kingmaker.Code.UI.MVVM.VM.Dialog.Interchapter;
using Kingmaker.Code.UI.MVVM.VM.EscMenu;
using Kingmaker.Code.UI.MVVM.VM.Formation;
using Kingmaker.Code.UI.MVVM.VM.GameOver;
using Kingmaker.Code.UI.MVVM.VM.InfoWindow;
using Kingmaker.Code.UI.MVVM.VM.IngameMenu;
using Kingmaker.Code.UI.MVVM.VM.LoadingScreen;
using Kingmaker.Code.UI.MVVM.VM.Loot;
using Kingmaker.Code.UI.MVVM.VM.MainMenu;
using Kingmaker.Code.UI.MVVM.VM.NewGame;
using Kingmaker.Code.UI.MVVM.VM.Other;
using Kingmaker.Code.UI.MVVM.VM.Overtips;
using Kingmaker.Code.UI.MVVM.VM.Party;
using Kingmaker.Code.UI.MVVM.VM.SaveLoad;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.CargoManagement;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.CharacterInfo;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.Abilities;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.ColonyManagement;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.Inventory;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.Journal;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.LocalMap;
using Kingmaker.Code.UI.MVVM.VM.Settings;
using Kingmaker.Code.UI.MVVM.VM.ShipCustomization;
using Kingmaker.Code.UI.MVVM.VM.Slots;
using Kingmaker.Code.UI.MVVM.VM.SpaceCombat;
using Kingmaker.Code.UI.MVVM.VM.Surface;
using Kingmaker.Code.UI.MVVM.VM.SurfaceCombat;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Templates;
using Kingmaker.Code.UI.MVVM.VM.Vendor;
using Kingmaker.GameModes;
using Kingmaker.UI;
using Kingmaker.UI.Canvases;
using Kingmaker.UI.Common;
using Kingmaker.UI.Legacy.MainMenuUI;
using Kingmaker.UI.MVVM.View.CharGen.Common;
using Kingmaker.UI.MVVM.VM.CharGen;
using Kingmaker.UI.MVVM.VM.CombatLog;
using Kingmaker.UI.MVVM.VM.Inspect;
using Kingmaker.UI.MVVM.VM.Tutorial;
using Kingmaker.UI.Pointer;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.Tooltips;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI.Extensions;

namespace Kingmaker.Utility;

public class ReportingRaycaster : MonoBehaviour
{
	private enum uifeatureenum
	{
		NoUiFeature,
		ActionBar,
		BookEvent,
		Buffs,
		CargoManagement,
		CharGen,
		CharScreen,
		CityBuilder,
		Colonization,
		CombatLog,
		Controls,
		Credits,
		Crusade,
		Cursor,
		DeadMenu,
		Dialog,
		EscMenu,
		Formation,
		Glossary,
		GroupManager,
		Inspect,
		Interactions,
		Interchapter,
		Epilog,
		Inventory,
		Journal,
		LevelUp,
		LoadingScreen,
		LocalMap,
		Loot,
		MainMenu,
		NewGame,
		Options,
		Overtips,
		Party,
		Pedia,
		SaveLoad,
		Selection,
		Spellbook,
		SplashScreen,
		VoidshipCombatHUD,
		SurfaceHUD,
		ShipCustomization,
		TBM,
		Tutorial,
		Vendor,
		Tooltips,
		Finnean,
		LeaderLevelup,
		SystemMap
	}

	private Dictionary<uifeatureenum, string> m_FeatureToLabelDict = new Dictionary<uifeatureenum, string>
	{
		{
			uifeatureenum.ActionBar,
			"ActionBar"
		},
		{
			uifeatureenum.BookEvent,
			"BookEvent"
		},
		{
			uifeatureenum.Buffs,
			"Buffs"
		},
		{
			uifeatureenum.CargoManagement,
			"CargoManagement"
		},
		{
			uifeatureenum.CharGen,
			"Chargen"
		},
		{
			uifeatureenum.CharScreen,
			"CharacterScreen"
		},
		{
			uifeatureenum.Colonization,
			"Colonization"
		},
		{
			uifeatureenum.CombatLog,
			"CombatLog"
		},
		{
			uifeatureenum.Controls,
			"Controls"
		},
		{
			uifeatureenum.Credits,
			"Credits"
		},
		{
			uifeatureenum.Cursor,
			"Cursor"
		},
		{
			uifeatureenum.DeadMenu,
			"DeadMenu"
		},
		{
			uifeatureenum.Dialog,
			"Dialog"
		},
		{
			uifeatureenum.EscMenu,
			"EscMenu"
		},
		{
			uifeatureenum.Formation,
			"Formation"
		},
		{
			uifeatureenum.Glossary,
			"Glossary"
		},
		{
			uifeatureenum.GroupManager,
			"GroupManager"
		},
		{
			uifeatureenum.Inspect,
			"Inspect"
		},
		{
			uifeatureenum.Interactions,
			"Interactions"
		},
		{
			uifeatureenum.Interchapter,
			"Interchapter"
		},
		{
			uifeatureenum.Inventory,
			"Inventory"
		},
		{
			uifeatureenum.Journal,
			"Journal"
		},
		{
			uifeatureenum.LevelUp,
			"LevelUp"
		},
		{
			uifeatureenum.LoadingScreen,
			"LoadingScreen"
		},
		{
			uifeatureenum.LocalMap,
			"LocalMap"
		},
		{
			uifeatureenum.Loot,
			"Loot"
		},
		{
			uifeatureenum.MainMenu,
			"MainMenu"
		},
		{
			uifeatureenum.NewGame,
			"NewGame"
		},
		{
			uifeatureenum.Options,
			"Options"
		},
		{
			uifeatureenum.Overtips,
			"Overtips"
		},
		{
			uifeatureenum.Party,
			"Party"
		},
		{
			uifeatureenum.Pedia,
			"Pedia"
		},
		{
			uifeatureenum.SaveLoad,
			"SaveLoadManager"
		},
		{
			uifeatureenum.Selection,
			"Selection"
		},
		{
			uifeatureenum.Spellbook,
			"Spellbook"
		},
		{
			uifeatureenum.SplashScreen,
			"SplashScreen"
		},
		{
			uifeatureenum.TBM,
			""
		},
		{
			uifeatureenum.Tutorial,
			"Tutorial"
		},
		{
			uifeatureenum.Vendor,
			"Vendor"
		},
		{
			uifeatureenum.Tooltips,
			"Tooltips"
		},
		{
			uifeatureenum.Finnean,
			""
		},
		{
			uifeatureenum.LeaderLevelup,
			""
		},
		{
			uifeatureenum.ShipCustomization,
			"VoidshipCustomization"
		},
		{
			uifeatureenum.SystemMap,
			"SystemMap"
		}
	};

	public static ReportingRaycaster Instance { get; private set; }

	public string GetJiraLabel(string feature)
	{
		try
		{
			uifeatureenum key = Enum.Parse<uifeatureenum>(feature);
			return m_FeatureToLabelDict[key];
		}
		catch
		{
			return string.Empty;
		}
	}

	public string GetFeatureName()
	{
		Vector2 position = ((Input.GetJoystickNames().Length != 0) ? new Vector2(Screen.width / 2, Screen.height / 2) : ((Vector2)Input.mousePosition));
		PointerEventData eventData = new PointerEventData(EventSystem.current)
		{
			position = position
		};
		List<RaycastResult> list = new List<RaycastResult>();
		CanvasGroup canvasGroup = null;
		MainCanvas instance = MainCanvas.Instance;
		if ((object)instance != null)
		{
			CanvasGroup component = instance.GetComponent<CanvasGroup>();
			if ((object)component != null && !component.blocksRaycasts)
			{
				component.blocksRaycasts = true;
				canvasGroup = component;
			}
		}
		try
		{
			EventSystem.current.RaycastAll(eventData, list);
		}
		finally
		{
			if ((bool)canvasGroup)
			{
				canvasGroup.blocksRaycasts = false;
			}
		}
		foreach (RaycastResult item in list)
		{
			uifeatureenum uifeature = GetUifeature(item.gameObject.transform);
			if (uifeature != 0)
			{
				return RetString(uifeature);
			}
			Transform parent = item.gameObject.transform.parent;
			while (parent != null)
			{
				uifeature = GetUifeature(parent.gameObject.transform);
				if (uifeature != 0)
				{
					return RetString(uifeature);
				}
				parent = parent.parent;
			}
		}
		if (Game.Instance.IsModeActive(GameModeType.StarSystem))
		{
			return uifeatureenum.SystemMap.ToString();
		}
		return string.Empty;
		static string RetString(uifeatureenum uifeatureenum)
		{
			if (Game.Instance.IsModeActive(GameModeType.StarSystem))
			{
				return uifeatureenum.SystemMap.ToString() + " " + uifeatureenum;
			}
			return uifeatureenum.ToString();
		}
	}

	private uifeatureenum GetUifeature(Transform transf)
	{
		MonoBehaviour[] components = transf.gameObject.GetComponents<MonoBehaviour>();
		for (int i = 0; i < components.Length; i++)
		{
			Type type = components[i].GetType();
			if (IsType(type, typeof(BookEventVM)))
			{
				return uifeatureenum.BookEvent;
			}
			if (IsType(type, typeof(BuffVM)))
			{
				return uifeatureenum.Buffs;
			}
			if (IsType(type, typeof(CargoManagementVM)))
			{
				return uifeatureenum.CargoManagement;
			}
			if (IsType(type, typeof(CharacterInfoVM)))
			{
				return uifeatureenum.CharScreen;
			}
			if (IsType(type, typeof(CharGenVM)))
			{
				if (!IsCharGen())
				{
					return uifeatureenum.LevelUp;
				}
				return uifeatureenum.CharGen;
			}
			if (IsType(type, typeof(ColonyManagementVM)))
			{
				return uifeatureenum.Colonization;
			}
			if (IsType(type, typeof(CombatLogVM)))
			{
				return uifeatureenum.CombatLog;
			}
			if (IsType(type, typeof(PCCursor)))
			{
				return uifeatureenum.Cursor;
			}
			if (IsType(type, typeof(GameOverVM)))
			{
				return uifeatureenum.DeadMenu;
			}
			if (IsType(type, typeof(DialogVM)))
			{
				return uifeatureenum.Dialog;
			}
			if (IsType(type, typeof(EscMenuVM)))
			{
				return uifeatureenum.EscMenu;
			}
			if (IsType(type, typeof(FormationVM)))
			{
				return uifeatureenum.Formation;
			}
			if (IsType(type, typeof(Glossary)))
			{
				return uifeatureenum.Glossary;
			}
			if (IsType(type, typeof(InspectVM)))
			{
				return uifeatureenum.Inspect;
			}
			if (IsType(type, typeof(InteractionSlotPartVM)))
			{
				return uifeatureenum.Interactions;
			}
			if (IsType(type, typeof(InterchapterVM)))
			{
				return uifeatureenum.Interchapter;
			}
			if (IsType(type, typeof(EpilogVM)))
			{
				return uifeatureenum.Epilog;
			}
			if (IsType(type, typeof(InventoryVM)))
			{
				return uifeatureenum.Inventory;
			}
			if (IsType(type, typeof(JournalVM)))
			{
				return uifeatureenum.Journal;
			}
			if (IsType(type, typeof(LoadingScreenVM)))
			{
				return uifeatureenum.LoadingScreen;
			}
			if (IsType(type, typeof(LocalMapVM)))
			{
				return uifeatureenum.LocalMap;
			}
			if (IsType(type, typeof(LootVM)))
			{
				return uifeatureenum.Loot;
			}
			if (IsType(type, typeof(MainMenuVM)))
			{
				return uifeatureenum.MainMenu;
			}
			if (IsType(type, typeof(NewGameVM)))
			{
				return uifeatureenum.NewGame;
			}
			if (IsType(type, typeof(SettingsVM)))
			{
				return uifeatureenum.Options;
			}
			if (IsType(type, typeof(SurfaceOvertipsVM)) || IsType(type, typeof(SpaceOvertipsVM)))
			{
				return uifeatureenum.Overtips;
			}
			if (IsType(type, typeof(PartyVM)))
			{
				return uifeatureenum.Party;
			}
			if (IsType(type, typeof(SaveLoadVM)))
			{
				return uifeatureenum.SaveLoad;
			}
			if (IsType(type, typeof(SelectionBox)))
			{
				return uifeatureenum.Selection;
			}
			if (IsType(type, typeof(SplashScreenController)))
			{
				return uifeatureenum.SplashScreen;
			}
			if (IsType(type, typeof(InitiativeTrackerVM)) || IsType(type, typeof(ViewBase<SpaceCombatVM>)))
			{
				return uifeatureenum.VoidshipCombatHUD;
			}
			if (IsType(type, typeof(SurfaceCombatPartVM)) || IsType(type, typeof(SurfaceStaticPartVM)))
			{
				return uifeatureenum.SurfaceHUD;
			}
			if (IsType(type, typeof(TutorialVM)))
			{
				return uifeatureenum.Tutorial;
			}
			if (IsType(type, typeof(VendorVM)))
			{
				return uifeatureenum.Vendor;
			}
			if (IsType(type, typeof(ToolTip)) || IsType(type, typeof(InfoWindowVM)))
			{
				return uifeatureenum.Tooltips;
			}
			if (IsType(type, typeof(ShipCustomizationVM)))
			{
				return uifeatureenum.ShipCustomization;
			}
		}
		return uifeatureenum.NoUiFeature;
	}

	private bool IsType(Type tMono, Type t)
	{
		if (tMono == t)
		{
			return true;
		}
		Type baseType = tMono.BaseType;
		if (baseType == typeof(MonoBehaviour))
		{
			return false;
		}
		if (baseType == t)
		{
			return true;
		}
		if (baseType != null && baseType.IsGenericType && baseType.GetGenericArguments().Length != 0)
		{
			Type[] genericArguments = baseType.GetGenericArguments();
			for (int i = 0; i < genericArguments.Length; i++)
			{
				if (genericArguments[i] == t)
				{
					return true;
				}
			}
		}
		return IsType(baseType, t);
	}

	public string GetUnderMouseBlueprintName()
	{
		PointerEventData eventData = new PointerEventData(EventSystem.current)
		{
			position = Input.mousePosition
		};
		List<RaycastResult> list = new List<RaycastResult>();
		EventSystem.current.RaycastAll(eventData, list);
		foreach (RaycastResult item in list)
		{
			string blueprintName = GetBlueprintName(item.gameObject.transform);
			if (!string.IsNullOrEmpty(blueprintName))
			{
				return blueprintName;
			}
			Transform parent = item.gameObject.transform.parent;
			while (parent != null)
			{
				blueprintName = GetBlueprintName(parent.gameObject.transform);
				if (!string.IsNullOrEmpty(blueprintName))
				{
					return blueprintName;
				}
				parent = parent.parent;
			}
		}
		return string.Empty;
	}

	private string GetBlueprintName(Transform transf)
	{
		MonoBehaviour[] components = transf.gameObject.GetComponents<MonoBehaviour>();
		List<Func<MonoBehaviour, string>> list = new List<Func<MonoBehaviour, string>>
		{
			(MonoBehaviour monoBehaviour) => GetNameFromViewBase(monoBehaviour, (EquipSlotVM vm) => vm.Item.Value.Blueprint.NameSafe()),
			(MonoBehaviour monoBehaviour) => GetNameFromViewBase(monoBehaviour, (ItemSlotVM vm) => vm.Item.Value.Blueprint.NameSafe()),
			(MonoBehaviour monoBehaviour) => GetNameFromViewBase(monoBehaviour, (BuffVM vm) => vm.Buff.Blueprint.NameSafe()),
			(MonoBehaviour monoBehaviour) => GetNameFromViewBase<CharInfoFeatureVM>(monoBehaviour, GetNameFromCharInfoFeatureVM),
			(MonoBehaviour monoBehaviour) => GetNameFromViewBase(monoBehaviour, (PartyCharacterVM vm) => vm.UnitEntityData.Blueprint.NameSafe() ?? ""),
			(MonoBehaviour monoBehaviour) => GetNameFromViewBase(monoBehaviour, (ItemSlotVM vm) => vm.Item.Value.Blueprint.NameSafe()),
			(MonoBehaviour monoBehaviour) => GetNameFromViewBase(monoBehaviour, (JournalNavigationGroupVM vm) => vm.Title),
			(MonoBehaviour monoBehaviour) => GetNameFromViewBase(monoBehaviour, (JournalQuestVM vm) => vm.Quest.Blueprint.NameSafe()),
			(MonoBehaviour monoBehaviour) => GetNameFromViewBase(monoBehaviour, (JournalQuestObjectiveVM vm) => vm.Objective.Blueprint.NameSafe())
		};
		MonoBehaviour[] array = components;
		foreach (MonoBehaviour arg in array)
		{
			foreach (Func<MonoBehaviour, string> item in list)
			{
				string text = item(arg);
				if (text != null)
				{
					return text;
				}
			}
		}
		return string.Empty;
	}

	private string GetNameFromCharInfoFeatureVM(CharInfoFeatureVM vm)
	{
		try
		{
			TooltipBaseTemplate value = vm.Tooltip.Value;
			if (value is TooltipTemplateAbility tooltipTemplateAbility)
			{
				return tooltipTemplateAbility.BlueprintAbility?.NameSafe();
			}
			if (value is TooltipTemplateActivatableAbility tooltipTemplateActivatableAbility)
			{
				return tooltipTemplateActivatableAbility.BlueprintActivatableAbility?.NameSafe();
			}
			if (value is TooltipTemplateFeature tooltipTemplateFeature)
			{
				return tooltipTemplateFeature.BlueprintFeatureBase?.NameSafe();
			}
			if (value is TooltipTemplateBuff tooltipTemplateBuff)
			{
				return tooltipTemplateBuff.Buff.Blueprint.NameSafe();
			}
			if (value is TooltipTemplateUIFeature tooltipTemplateUIFeature)
			{
				try
				{
					return ((UIFeature)(tooltipTemplateUIFeature.GetType().GetField("m_UIFeature", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.GetField)?.GetValue(tooltipTemplateUIFeature)))?.Feature.NameSafe();
				}
				catch
				{
				}
			}
		}
		catch
		{
		}
		return vm.Acronym;
	}

	private string GetNameFromViewBase<T>(MonoBehaviour mono, Func<T, string> extractor) where T : class, IViewModel
	{
		try
		{
			if (!(mono is ViewBase<T> viewBase))
			{
				return null;
			}
			T arg = (T)viewBase.GetViewModel();
			return extractor(arg);
		}
		catch
		{
			return null;
		}
	}

	public BlueprintScriptableObject GetUnderMouseBlueprint()
	{
		PointerEventData eventData = new PointerEventData(EventSystem.current)
		{
			position = Input.mousePosition
		};
		List<RaycastResult> list = new List<RaycastResult>();
		EventSystem.current.RaycastAll(eventData, list);
		foreach (RaycastResult item in list)
		{
			BlueprintScriptableObject blueprint = GetBlueprint(item.gameObject.transform);
			if (blueprint != null)
			{
				return blueprint;
			}
			Transform parent = item.gameObject.transform.parent;
			while (parent != null)
			{
				blueprint = GetBlueprint(parent.gameObject.transform);
				if (blueprint != null)
				{
					return blueprint;
				}
				parent = parent.parent;
			}
		}
		return null;
	}

	public string GetFeatureProgressionBlueprintName()
	{
		try
		{
			return string.Empty;
		}
		catch
		{
			return string.Empty;
		}
	}

	private BlueprintScriptableObject GetBlueprint(Transform transf)
	{
		MonoBehaviour[] components = transf.gameObject.GetComponents<MonoBehaviour>();
		foreach (MonoBehaviour monoBehaviour in components)
		{
			if (monoBehaviour is InventoryEquipSlotView inventoryEquipSlotView)
			{
				return inventoryEquipSlotView.Item?.Blueprint;
			}
			if (monoBehaviour is ViewBase<ItemSlotVM> viewBase)
			{
				object obj = viewBase.GetType().GetProperty("ViewModel", BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(viewBase);
				if (obj != null)
				{
					return ((ItemSlotVM)obj).Item.Value.Blueprint;
				}
			}
			if (monoBehaviour is ItemSlotView<ItemSlotVM> itemSlotView)
			{
				return itemSlotView.Item?.Blueprint;
			}
		}
		return null;
	}

	public string GetOtherUiFeatureName()
	{
		PointerEventData eventData = new PointerEventData(EventSystem.current)
		{
			position = Input.mousePosition
		};
		List<RaycastResult> list = new List<RaycastResult>();
		EventSystem.current.RaycastAll(eventData, list);
		foreach (RaycastResult item in list)
		{
			string otherUi = GetOtherUi(item.gameObject.transform);
			if (!string.IsNullOrEmpty(otherUi))
			{
				return otherUi;
			}
			Transform parent = item.gameObject.transform.parent;
			while (parent != null)
			{
				otherUi = GetOtherUi(parent.gameObject.transform);
				if (!string.IsNullOrEmpty(otherUi))
				{
					return otherUi;
				}
				parent = parent.parent;
			}
		}
		foreach (OvertipBarkBlockView item2 in UnityEngine.Object.FindObjectsOfType<OvertipBarkBlockView>().ToList())
		{
			try
			{
				RectTransform component = item2.transform.GetChild(0).GetComponent<RectTransform>();
				if (component != null)
				{
					RectTransformUtility.ScreenPointToLocalPointInRectangle(component, Input.mousePosition, UICamera.Instance, out var localPoint);
					if (Math.Abs(localPoint.x) < component.rect.width && localPoint.y >= 0f && localPoint.y < component.rect.height)
					{
						return "Bark";
					}
				}
			}
			catch
			{
			}
		}
		return string.Empty;
	}

	private string GetOtherUi(Transform transf)
	{
		MonoBehaviour[] components = transf.gameObject.GetComponents<MonoBehaviour>();
		for (int i = 0; i < components.Length; i++)
		{
			if (components[i] is ViewBase<IngameMenuVM>)
			{
				return "Ingame Menu";
			}
		}
		return string.Empty;
	}

	private static bool IsCharGen()
	{
		return UnityEngine.Object.FindObjectOfType<CharGenView>() != null;
	}

	public void OnEnable()
	{
		Instance = this;
	}

	public void OnDisable()
	{
		if (Instance == this)
		{
			Instance = null;
		}
	}
}
