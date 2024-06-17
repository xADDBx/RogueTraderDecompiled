using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Localization;
using UnityEngine;

namespace Kingmaker.Blueprints.Items;

[TypeId("28972492b5684446a1f5f4af8cb055d3")]
public class BlueprintItemPattern : BlueprintScriptableObject
{
	[SerializeField]
	private LocalizedString m_DisplayNameText;

	[SerializeField]
	private LocalizedString m_DescriptionText;

	[SerializeField]
	private LocalizedString m_FlavorText;
}
