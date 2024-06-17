using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.Localization;
using Kingmaker.UI.Models.Tooltip.Base;
using MemoryPack;
using UnityEngine;

namespace Kingmaker.Globalmap.Blueprints.Exploration;

[TypeId("d59d45d820894ab4aca196931b82fa71")]
[MemoryPackable(GenerateType.NoGenerate)]
public class BlueprintPointOfInterest : BlueprintScriptableObject, IUIDataProvider
{
	[SerializeField]
	private LocalizedString m_Name;

	[SerializeField]
	private LocalizedString m_Description;

	[SerializeField]
	private Sprite m_Icon;

	[SerializeField]
	[CanBeNull]
	public ConditionsReference VisibleConditions;

	[SerializeField]
	[CanBeNull]
	public ConditionsReference InteractableConditions;

	[SerializeField]
	public ActionList OnInteractedActions;

	public string Name => m_Name;

	public string Description => m_Description;

	public Sprite Icon => m_Icon;

	public string NameForAcronym => m_Name;

	public bool IsVisible()
	{
		return VisibleConditions?.Get()?.Check() ?? true;
	}

	public bool IsInteractable()
	{
		return InteractableConditions?.Get()?.Check() ?? true;
	}
}
