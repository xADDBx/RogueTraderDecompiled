using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Localization;
using Kingmaker.UI.Models.Tooltip.Base;
using UnityEngine;

namespace Kingmaker.Globalmap.Blueprints.Colonization;

[TypeId("e25c25115634449ca0cfb2919700e882")]
public class BlueprintResource : BlueprintScriptableObject, IUIDataProvider
{
	[SerializeField]
	private LocalizedString m_Name;

	[SerializeField]
	private LocalizedString m_Description;

	[SerializeField]
	private Sprite m_Icon;

	[SerializeField]
	public float ProfitFactorPenalty;

	public string Name => m_Name.Text;

	public string Description => m_Description.Text;

	public Sprite Icon => m_Icon;

	public string NameForAcronym => m_Name;
}
