using Kingmaker.Blueprints.Attributes;
using Kingmaker.Localization;
using Kingmaker.UI.Models.Tooltip.Base;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.UI.Common;

[CreateAssetMenu(menuName = "Blueprints/UI/UIDataProvider")]
public class UIDataProvider : ScriptableObject, IUIDataProvider
{
	[SerializeField]
	[FormerlySerializedAs("m_LocalizedName")]
	[AssetPathFilter(new string[] { "_DisplayName" })]
	private LocalizedString m_DisplayName;

	[SerializeField]
	[FormerlySerializedAs("m_LocalizedDescription")]
	[AssetPathFilter(new string[] { "_Description" })]
	private LocalizedString m_Description;

	[SerializeField]
	private Sprite m_Icon;

	public string Name => m_DisplayName;

	public string Description => m_Description;

	public Sprite Icon => m_Icon;

	public string NameForAcronym => base.name;
}
