using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.Localization;
using Kingmaker.UI.Models.Tooltip.Base;
using Kingmaker.Utility.Attributes;
using UnityEngine;

namespace Kingmaker.Globalmap.Blueprints.Colonization;

[TypeId("cae39a2d3a2a4164b8d42c8b4180f442")]
public class BlueprintColonyTrait : BlueprintScriptableObject, IUIDataProvider
{
	[Serializable]
	public class Reference : BlueprintReference<BlueprintColonyTrait>
	{
	}

	[SerializeField]
	private LocalizedString m_Name;

	[SerializeField]
	private LocalizedString m_Description;

	[SerializeField]
	private Sprite m_Icon;

	[SerializeField]
	private LocalizedString m_MechanicString;

	public int ContentmentModifier;

	public int EfficiencyModifier;

	public int SecurityModifier;

	public bool IsPermanent;

	[HideIf("IsPermanent")]
	[Tooltip("In segments")]
	public int TraitDuration;

	public bool IsHistorical;

	public ActionList OnStartActions;

	public ActionList OnEndActions;

	public string Name => m_Name.Text;

	public string Description => m_Description.Text;

	public Sprite Icon => m_Icon;

	public string NameForAcronym => m_Name;

	public string MechanicString => m_MechanicString;
}
