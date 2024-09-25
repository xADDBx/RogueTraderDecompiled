using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Area;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.DialogSystem.Blueprints;
using Kingmaker.ElementsSystem;
using Kingmaker.Localization;
using Kingmaker.Utility.Attributes;
using Kingmaker.Utility.UnityExtensions;
using Owlcat.Runtime.Core.Utility.EditorAttributes;
using UnityEngine;

namespace Kingmaker.RandomEncounters.Settings;

[TypeId("4577d42c64f5c0e498f6daca6322e0b6")]
public class BlueprintRandomEncounter : BlueprintScriptableObject, IDialogReference, IAreaEnterPointReference
{
	public static readonly string RootDirectory = PathUtils.BlueprintPath("World/Areas/Random_Encounters");

	public bool ExcludeFromREList;

	public bool IsPeaceful;

	public LocalizedString Name;

	public LocalizedString Description;

	public EncounterAvoidType AvoidType;

	[InfoBox("Skill check Stealth")]
	public int AvoidDC;

	public int EncountersLimit;

	public ConditionsChecker Conditions;

	public EncounterType Type;

	public ActionList OnEnter;

	public bool CanBeCampingEncounter;

	[ShowIf("NeedArea")]
	[SerializeField]
	private BlueprintAreaEnterPointReference m_AreaEntrance;

	[ShowIf("IsBookEvent")]
	[SerializeField]
	private BlueprintDialogReference m_BookEvent;

	public BlueprintAreaEnterPoint AreaEntrance => m_AreaEntrance?.Get();

	public BlueprintDialog BookEvent => m_BookEvent?.Get();

	public bool NeedArea
	{
		get
		{
			if (Type != EncounterType.Custom)
			{
				return Type == EncounterType.RandomizedCombat;
			}
			return true;
		}
	}

	public bool IsBookEvent => Type == EncounterType.BookEvent;

	public bool IsRandomizedCombat => Type == EncounterType.RandomizedCombat;

	public DialogReferenceType GetUsagesFor(BlueprintDialog dialog)
	{
		if (dialog != BookEvent)
		{
			return DialogReferenceType.None;
		}
		return DialogReferenceType.Start;
	}

	public bool GetUsagesFor(BlueprintAreaEnterPoint point)
	{
		return point == AreaEntrance;
	}
}
