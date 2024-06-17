using System;
using System.Collections.Generic;
using Kingmaker.Code.UI.MVVM.View.Overtips.CommonOvertipParts;
using Kingmaker.Code.UI.MVVM.VM.Dialog.BookEvent;
using Kingmaker.Code.UI.MVVM.VM.Dialog.Dialog;
using Kingmaker.Code.UI.MVVM.VM.Dialog.Epilog;
using Kingmaker.Code.UI.MVVM.VM.Dialog.Interchapter;
using Kingmaker.Code.UI.MVVM.VM.Formation;
using Kingmaker.Code.UI.MVVM.VM.GameOver;
using Kingmaker.Code.UI.MVVM.VM.IngameMenu;
using Kingmaker.Code.UI.MVVM.VM.LoadingScreen;
using Kingmaker.Code.UI.MVVM.VM.Loot;
using Kingmaker.Code.UI.MVVM.VM.NewGame;
using Kingmaker.Code.UI.MVVM.VM.Overtips;
using Kingmaker.Code.UI.MVVM.VM.Party;
using Kingmaker.Code.UI.MVVM.VM.SaveLoad;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.CharacterInfo;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.Inventory;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.Journal;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.LocalMap;
using Kingmaker.Code.UI.MVVM.VM.Settings;
using Kingmaker.Code.UI.MVVM.VM.SurfaceCombat;
using Kingmaker.Code.UI.MVVM.VM.Vendor;
using Kingmaker.UI;
using Kingmaker.UI.Legacy.MainMenuUI;
using Kingmaker.UI.MVVM.VM.CombatLog;
using Kingmaker.UI.MVVM.VM.Inspect;
using Kingmaker.UI.MVVM.VM.Tutorial;
using Kingmaker.Utility.DotNetExtensions;
using UnityEngine;
using UnityEngine.UI.Extensions;

namespace Kingmaker.Utility;

[Serializable]
public class InterfaceContextHelper
{
	private enum uifeatureenum
	{
		NoUiFeature,
		ActionBar,
		BookEvent,
		Buffs,
		CharScreen,
		CityBuilder,
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
		Epilog,
		Interchapter,
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
		Rest,
		SaveLoad,
		Selection,
		Spellbook,
		SplashScreen,
		TBM,
		Tutorial,
		Vendor,
		Tooltips
	}

	public List<ContextRow> ContextRows;

	private Dictionary<Type, string> m_Dictionary = new Dictionary<Type, string>
	{
		{
			typeof(BookEventVM),
			uifeatureenum.BookEvent.ToString()
		},
		{
			typeof(CharacterInfoVM),
			uifeatureenum.CharScreen.ToString()
		},
		{
			typeof(CombatLogVM),
			uifeatureenum.CombatLog.ToString()
		},
		{
			typeof(GameOverVM),
			uifeatureenum.DeadMenu.ToString()
		},
		{
			typeof(DialogVM),
			uifeatureenum.Dialog.ToString()
		},
		{
			typeof(FormationVM),
			uifeatureenum.Formation.ToString()
		},
		{
			typeof(InspectVM),
			uifeatureenum.Inspect.ToString()
		},
		{
			typeof(EpilogVM),
			uifeatureenum.Epilog.ToString()
		},
		{
			typeof(InterchapterVM),
			uifeatureenum.Interchapter.ToString()
		},
		{
			typeof(InventoryVM),
			uifeatureenum.Inventory.ToString()
		},
		{
			typeof(JournalVM),
			uifeatureenum.Journal.ToString()
		},
		{
			typeof(LoadingScreenVM),
			uifeatureenum.LoadingScreen.ToString()
		},
		{
			typeof(LocalMapVM),
			uifeatureenum.LocalMap.ToString()
		},
		{
			typeof(LootVM),
			uifeatureenum.Loot.ToString()
		},
		{
			typeof(GameMainMenu),
			uifeatureenum.MainMenu.ToString()
		},
		{
			typeof(NewGameVM),
			uifeatureenum.NewGame.ToString()
		},
		{
			typeof(SettingsVM),
			uifeatureenum.Options.ToString()
		},
		{
			typeof(SurfaceOvertipsVM),
			uifeatureenum.Overtips.ToString()
		},
		{
			typeof(SpaceOvertipsVM),
			uifeatureenum.Overtips.ToString()
		},
		{
			typeof(PartyVM),
			uifeatureenum.Party.ToString()
		},
		{
			typeof(SaveLoadVM),
			uifeatureenum.SaveLoad.ToString()
		},
		{
			typeof(SelectionBox),
			uifeatureenum.Selection.ToString()
		},
		{
			typeof(SplashScreenController),
			uifeatureenum.SplashScreen.ToString()
		},
		{
			typeof(InitiativeTrackerVM),
			uifeatureenum.TBM.ToString()
		},
		{
			typeof(TutorialVM),
			uifeatureenum.Tutorial.ToString()
		},
		{
			typeof(VendorVM),
			uifeatureenum.Vendor.ToString()
		},
		{
			typeof(ToolTip),
			uifeatureenum.Tooltips.ToString()
		},
		{
			typeof(IngameMenuVM),
			"Ingame Menu"
		},
		{
			typeof(OvertipBarkBlockView),
			"Bark"
		}
	};

	public InterfaceContextHelper()
	{
		try
		{
			ContextRows = new List<ContextRow>();
			MonoBehaviour[] source = UnityEngine.Object.FindObjectsOfType<MonoBehaviour>();
			string text = "";
			string text2 = "";
			foreach (KeyValuePair<Type, string> kvp in m_Dictionary)
			{
				try
				{
					MonoBehaviour monoBehaviour = source.FirstOrDefault((MonoBehaviour x) => x.isActiveAndEnabled && IsType(x.GetType(), kvp.Key));
					if (!(monoBehaviour == null))
					{
						if (IsVisible(monoBehaviour.transform))
						{
							text = text + kvp.Value + "\n";
						}
						else
						{
							text2 = text2 + kvp.Value + "\n";
						}
					}
				}
				catch
				{
				}
			}
			ContextRows.Add(new ContextRow(new List<ContextParameter>
			{
				new ContextParameter("Visible Interfaces", text)
			}));
			ContextRows.Add(new ContextRow(new List<ContextParameter>
			{
				new ContextParameter("Overlapped Interfaces", text2)
			}));
		}
		catch (Exception ex)
		{
			ContextRows.Add(new ContextRow(new List<ContextParameter>
			{
				new ContextParameter("Exception", ex.Message)
			}));
		}
	}

	private bool IsType(Type tMono, Type t)
	{
		if (tMono == t)
		{
			return true;
		}
		Type baseType = tMono.BaseType;
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
		return false;
	}

	private bool IsVisible(Transform transf)
	{
		RectTransform component = transf.GetComponent<RectTransform>();
		if (component != null && CountCornersVisibleFrom(component) > 2)
		{
			return true;
		}
		foreach (Transform item in transf)
		{
			if (IsVisible(item))
			{
				return true;
			}
		}
		return false;
	}

	private int CountCornersVisibleFrom(RectTransform rectTransform)
	{
		Camera instance = UICamera.Instance;
		Rect rect = new Rect(0f, 0f, Screen.width, Screen.height);
		Vector3[] array = new Vector3[4];
		rectTransform.GetWorldCorners(array);
		int num = 0;
		for (int i = 0; i < array.Length; i++)
		{
			Vector3 point = instance.WorldToScreenPoint(array[i]);
			if (rect.Contains(point))
			{
				num++;
			}
		}
		return num;
	}
}
