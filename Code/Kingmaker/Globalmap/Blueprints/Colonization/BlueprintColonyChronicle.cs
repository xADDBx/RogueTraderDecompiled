using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.DialogSystem.Blueprints;
using Kingmaker.Localization;
using Kingmaker.UI.Models.Tooltip.Base;
using UnityEngine;

namespace Kingmaker.Globalmap.Blueprints.Colonization;

[TypeId("72b026d82361459581a0cb453c0385cb")]
public class BlueprintColonyChronicle : BlueprintScriptableObject, IUIDataProvider
{
	[Serializable]
	public class Reference : BlueprintReference<BlueprintColonyChronicle>
	{
	}

	[SerializeField]
	private BlueprintDialogReference m_BlueprintDialog;

	[SerializeField]
	private LocalizedString m_Name;

	[SerializeField]
	private LocalizedString m_Description;

	public string Name => m_Name;

	public BlueprintDialog BlueprintDialog => m_BlueprintDialog?.Get();

	public string Description => m_Description;

	public Sprite Icon => null;

	public string NameForAcronym => null;
}
