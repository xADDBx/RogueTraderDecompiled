using System;
using System.Collections.Generic;
using Kingmaker.AreaLogic.Etudes;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Area;
using Kingmaker.Blueprints.Items;
using Kingmaker.Cheats;
using Kingmaker.Code.UI.MVVM.VM.MainMenu;
using Kingmaker.DialogSystem.Blueprints;
using Kingmaker.GameModes;
using Kingmaker.UI.Models;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Levelup.Obsolete.Blueprints;
using Kingmaker.UnitLogic.Levelup.Obsolete.Blueprints.Selection;
using Kingmaker.UnitLogic.Progression.Features;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Utility.UnityExtensions;

namespace Kingmaker.Utility;

public class BugContext : IComparable<BugContext>
{
	public enum ContextType
	{
		None,
		Crash,
		Exception,
		ExceptionSpam,
		Dialog,
		Spell,
		SpellSpace,
		Item,
		Colonization,
		Exploration,
		GlobalMap,
		Unit,
		Army,
		Area,
		CharacterClass,
		Crusade,
		Interface,
		SurfaceCombat,
		SpaceCombat,
		Encounter,
		Debug,
		Coop,
		Desync,
		TransitionMap,
		GroupChanger
	}

	public enum AspectType
	{
		None,
		Sound,
		Mechanics,
		Narrative,
		UI,
		Code,
		Visual,
		Animation,
		Localization
	}

	public enum InnerContextType
	{
		None,
		Dialog,
		Unit,
		Spell,
		Buff,
		Condition,
		Item,
		CharacterClass,
		Area
	}

	public bool IsTooltip;

	public string ClassArchetypes;

	public string UiFeature;

	public string UiFeatureAssignee;

	public string OtherUiFeature;

	private readonly List<(AspectType Aspect, string Name)> m_AspectAssigneeList = new List<(AspectType, string)>();

	public FullScreenUIType ActiveFullScreenUIType { get; set; }

	public ContextType Type { get; private set; }

	public AspectType Aspect { get; private set; }

	public BlueprintScriptableObject ContextObject { get; set; }

	public List<BlueprintScriptableObject> AdditionalContextObjects { get; } = new List<BlueprintScriptableObject>();


	public int CompareTo(BugContext other)
	{
		return Type.CompareTo(other.Type);
	}

	private void FillAssigneeList()
	{
		m_AspectAssigneeList.Clear();
		if (!ReportingUtils.Instance.Assignees.IsCompletedSuccessfully || !ReportingUtils.Instance.Assignees.Result.MainTable.Assignees.TryGetValue(Type, out var value))
		{
			return;
		}
		foreach (AspectType value3 in Enum.GetValues(typeof(AspectType)))
		{
			AssigneeQa value2;
			string text = (value.TryGetValue(value3, out value2) ? value2.Assignee : string.Empty);
			if (text == "area_designer")
			{
				text = Utilities.GetDesigner(CheatsJira.GetCurrentArea());
			}
			if (!string.IsNullOrEmpty(text))
			{
				m_AspectAssigneeList.Add((value3, text));
			}
		}
	}

	public List<(AspectType? Aspect, string Assignee)> GetContextAspectAssignees()
	{
		List<(AspectType?, string)> list = new List<(AspectType?, string)>();
		foreach (var (value, text) in m_AspectAssigneeList)
		{
			if (text.Contains("ui_designer") && ReportingUtils.Instance.Assignees.IsCompletedSuccessfully)
			{
				Dictionary<string, string> uiDesigners = ReportingUtils.Instance.Assignees.Result.UiDesigners;
				string text2 = UiFeature;
				if (string.IsNullOrEmpty(text2))
				{
					text2 = OtherUiFeature;
				}
				if (string.IsNullOrEmpty(text2))
				{
					text2 = GetContextObjectBlueprintName();
				}
				if (string.IsNullOrEmpty(text2))
				{
					text2 = ActiveFullScreenUIType.ToString();
				}
				if (string.IsNullOrEmpty(text2))
				{
					continue;
				}
				if (uiDesigners.TryGetValue(text2, out var value2))
				{
					string item = value2;
					list.Add((null, item));
					continue;
				}
			}
			list.Add((value, text));
		}
		return list;
	}

	public static string[] GetLeadAssignees()
	{
		if (!ReportingUtils.Instance.Assignees.IsCompletedSuccessfully)
		{
			return Array.Empty<string>();
		}
		return ReportingUtils.Instance.Assignees.Result.Leads;
	}

	public string GetContextDescription()
	{
		if (Type == ContextType.Encounter)
		{
			return ContextObject.NameSafe() + "_Encounter";
		}
		if (Type == ContextType.Exploration)
		{
			return "Exploration";
		}
		if (ContextObject != null)
		{
			return GetContextObjectDescription();
		}
		if (!string.IsNullOrEmpty(UiFeature))
		{
			return "<B>Interface:</B> " + UiFeature;
		}
		if (!string.IsNullOrEmpty(OtherUiFeature))
		{
			return "<B>Interface:</B> " + OtherUiFeature;
		}
		if (Type == ContextType.Interface)
		{
			if (MainMenuUI.Instance != null)
			{
				if (ActiveFullScreenUIType == FullScreenUIType.Chargen)
				{
					return $"<B>Interface:</B> {ActiveFullScreenUIType:G}";
				}
				return "<B>MainMenu</B>";
			}
			if (ActiveFullScreenUIType != 0)
			{
				return $"<B>Interface:</B> {ActiveFullScreenUIType:G}";
			}
		}
		return Type.ToString("G");
	}

	public void SelectAspect(int aspectIdx)
	{
		Aspect = (AspectType)aspectIdx;
	}

	public string GetContextObjectBlueprintName()
	{
		if (ContextObject != null)
		{
			return Utilities.GetBlueprintName(ContextObject);
		}
		if (Type == ContextType.Crash)
		{
			return "TECHNICAL_CRASH";
		}
		if (!string.IsNullOrEmpty(UiFeature))
		{
			return UiFeature ?? "";
		}
		if (!string.IsNullOrEmpty(OtherUiFeature))
		{
			return OtherUiFeature ?? "";
		}
		if (ActiveFullScreenUIType != 0)
		{
			return ActiveFullScreenUIType.ToString("G");
		}
		if (Type == ContextType.Interface)
		{
			if (MainMenuUI.Instance == null)
			{
				return "Interface";
			}
			return "MainMenu";
		}
		return string.Empty;
	}

	public string GetDialogGuid()
	{
		if (!(ContextObject is BlueprintDialog))
		{
			return string.Empty;
		}
		return ContextObject.AssetGuid;
	}

	private void ResetContextType()
	{
		if (ContextObject != null)
		{
			if (ContextObject is BlueprintDialog)
			{
				Type = ContextType.Dialog;
			}
			if (ContextObject is BlueprintItem)
			{
				Type = ContextType.Item;
			}
			if (ContextObject is BlueprintCharacterClass)
			{
				Type = ContextType.CharacterClass;
			}
			if (ContextObject is BlueprintFeatureBase || ContextObject is BlueprintAbility || ContextObject is BlueprintFeatureSelection_Obsolete || ContextObject is BlueprintActivatableAbility || ContextObject is BlueprintBuff)
			{
				Type = ((Game.Instance.IsModeActive(GameModeType.SpaceCombat) || Game.Instance.IsModeActive(GameModeType.StarSystem)) ? ContextType.SpellSpace : ContextType.Spell);
			}
			if (ContextObject is BlueprintUnit || ContextObject is BlueprintRace)
			{
				Type = ContextType.Unit;
			}
			if (ContextObject is BlueprintArea || ContextObject is BlueprintEtude)
			{
				Type = ContextType.Area;
			}
		}
		else
		{
			Type = ContextType.Area;
		}
	}

	private string GetContextObjectDescription()
	{
		BlueprintScriptableObject contextObject = ContextObject;
		if (!(contextObject is BlueprintArea blueprintArea))
		{
			if (!(contextObject is BlueprintDialog))
			{
				if (!(contextObject is BlueprintItem blueprintItem))
				{
					if (!(contextObject is BlueprintRace blueprintRace))
					{
						if (!(contextObject is BlueprintFeatureSelection_Obsolete blueprintFeatureSelection_Obsolete))
						{
							if (!(contextObject is BlueprintFeatureBase blueprintFeatureBase))
							{
								if (!(contextObject is BlueprintAbility blueprintAbility))
								{
									if (!(contextObject is BlueprintActivatableAbility blueprintActivatableAbility))
									{
										if (!(contextObject is BlueprintBuff blueprintBuff))
										{
											if (!(contextObject is BlueprintUnit blueprintUnit))
											{
												if (!(contextObject is BlueprintCharacterClass blueprintCharacterClass))
												{
													if (!(contextObject is BlueprintEtude blueprintEtude))
													{
														if (contextObject != null)
														{
															return "<B>Blueprint:</B> " + contextObject.name;
														}
														return "<B>None</B>";
													}
													return "<B>Etude:</B> " + GetBlueprintNameIfEmpty(blueprintEtude.name);
												}
												string text = "";
												try
												{
													text = GetBlueprintNameIfEmpty((AdditionalContextObjects.FirstOrDefault() as BlueprintUnit)?.CharacterName);
												}
												catch
												{
												}
												return "<B>Character:</B> " + text + "; <B>Class:</B> " + GetBlueprintNameIfEmpty(blueprintCharacterClass.LocalizedName);
											}
											return "<B>Unit:</B> " + GetBlueprintNameIfEmpty(blueprintUnit.CharacterName);
										}
										return "<B>Buff:</B> " + GetBlueprintNameIfEmpty(blueprintBuff.Name);
									}
									return "<B>Activatable Ability:</B> " + GetBlueprintNameIfEmpty(blueprintActivatableAbility.Name);
								}
								return "<B>Ability:</B> " + GetBlueprintNameIfEmpty(blueprintAbility.Name);
							}
							return "<B>Feature:</B> " + GetBlueprintNameIfEmpty(blueprintFeatureBase.Name);
						}
						return "<B>Feature Selection:</B> " + GetBlueprintNameIfEmpty(blueprintFeatureSelection_Obsolete.Name);
					}
					return "<B>Race:</B> " + GetBlueprintNameIfEmpty(blueprintRace.Name);
				}
				return "<B>Item:</B> " + GetBlueprintNameIfEmpty(blueprintItem.Name);
			}
			return "<B>Current Dialog</B>";
		}
		if (Type == ContextType.Exploration)
		{
			return "<B>Exploration:</B> " + GetBlueprintNameIfEmpty(blueprintArea.AreaDisplayName);
		}
		return "<B>Area:</B> " + GetBlueprintNameIfEmpty(blueprintArea.AreaDisplayName);
	}

	private string GetBlueprintNameIfEmpty(string localizedName)
	{
		if (!localizedName.IsNullOrEmpty())
		{
			return localizedName;
		}
		return Utilities.GetBlueprintName(ContextObject);
	}

	public string GetContextLink()
	{
		BlueprintScriptableObject contextObject = ContextObject;
		if (contextObject != null)
		{
			if (contextObject is BlueprintDialog)
			{
				BlueprintCue blueprintCue = Game.Instance?.DialogController.CurrentCue;
				return CheatsJira.MakeOpenString("Cue", Utilities.GetBlueprintName(blueprintCue), "dialog", blueprintCue.AssetGuid);
			}
			return CheatsJira.MakeOpenString("Blueprint", Utilities.GetBlueprintName(ContextObject), "simple", ContextObject.AssetGuid);
		}
		return string.Empty;
	}

	public string GetHeader()
	{
		if (Type == ContextType.Encounter && ContextObject != null)
		{
			return "[" + ContextObject.NameSafe() + "_Encounter]";
		}
		if (Type == ContextType.Exploration)
		{
			return "[" + ContextObject.NameSafe() + "_Exploration]";
		}
		if (!string.IsNullOrEmpty(UiFeature))
		{
			return "[" + UiFeature + "]";
		}
		BlueprintArea currentArea = CheatsJira.GetCurrentArea();
		if (ContextObject is BlueprintDialog)
		{
			return "[" + Utilities.GetBlueprintName(ContextObject) + "]";
		}
		if (ContextObject is BlueprintCharacterClass blueprint && Type == ContextType.CharacterClass)
		{
			string text = (string.IsNullOrEmpty(ClassArchetypes) ? string.Empty : ("[" + ClassArchetypes.Trim() + "]"));
			return "[" + Utilities.GetBlueprintName(AdditionalContextObjects.FirstOrDefault()) + "][" + Utilities.GetBlueprintName(blueprint) + "]" + text;
		}
		BlueprintScriptableObject contextObject = ContextObject;
		if (contextObject is BlueprintAbility || contextObject is BlueprintActivatableAbility)
		{
			return "[" + Utilities.GetBlueprintName(ContextObject) + "]";
		}
		if ((IsTooltip || ActiveFullScreenUIType != 0) && ContextObject != null)
		{
			return "[" + Utilities.GetBlueprintName(ContextObject) + "]";
		}
		if (ContextObject != null)
		{
			return "[" + Utilities.GetBlueprintName(ContextObject) + "]";
		}
		if (Type == ContextType.Interface)
		{
			if (IsTooltip || ActiveFullScreenUIType != 0)
			{
				return "[" + ActiveFullScreenUIType.ToString() + "]";
			}
			return "[UI]";
		}
		if (Type == ContextType.Crash)
		{
			return "[Crash]";
		}
		if (Type == ContextType.ExceptionSpam)
		{
			return "[ExceptionSpam]";
		}
		if (Type == ContextType.Exception)
		{
			return "[Exception]";
		}
		if (Type == ContextType.SpaceCombat)
		{
			return "[SpaceCombat]";
		}
		if (Type == ContextType.SurfaceCombat)
		{
			return "[SurfaceCombat]";
		}
		if (Type == ContextType.Coop)
		{
			return "[Coop]";
		}
		if (Type == ContextType.Desync)
		{
			return "[Desync]";
		}
		string blueprintName = Utilities.GetBlueprintName(currentArea);
		if (blueprintName != null)
		{
			return "[" + blueprintName + "]";
		}
		return "";
	}

	public BugContext(ContextType context)
	{
		Type = context;
		FillAssigneeList();
	}

	public BugContext(BlueprintScriptableObject contextObject)
	{
		ContextObject = contextObject;
		ResetContextType();
		FillAssigneeList();
	}

	public BugContext(BlueprintScriptableObject contextObject, ContextType context)
	{
		Type = context;
		ContextObject = contextObject;
		FillAssigneeList();
	}
}
