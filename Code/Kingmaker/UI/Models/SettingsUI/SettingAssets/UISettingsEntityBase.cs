using System.Collections.Generic;
using Kingmaker.Blueprints;
using Kingmaker.Localization;
using Kingmaker.Utility.Attributes;
using Owlcat.Runtime.Core.Utility.EditorAttributes;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.UI.Models.SettingsUI.SettingAssets;

public abstract class UISettingsEntityBase : ScriptableObject, IUISettingsEntityBase
{
	public enum UISettingsPlatform
	{
		Everywhere,
		PC,
		Console,
		GamepadAndPC,
		PCMouseOnly,
		Hide,
		PCAndNotMSStore
	}

	[SerializeField]
	[FormerlySerializedAs("Description")]
	private LocalizedString m_Description;

	[SerializeField]
	[FormerlySerializedAs("TooltipDescription")]
	private LocalizedString m_TooltipDescription;

	[SerializeField]
	private LocalizedString m_ConsoleTooltipDescription;

	[SerializeField]
	private List<BlueprintEncyclopediaPageReference> m_EncyclopediaDescription;

	[Header("View setting")]
	[SerializeField]
	[FormerlySerializedAs("ShowVisualConnection")]
	private bool m_ShowVisualConnection;

	[ConditionalShow("ShowVisualConnection")]
	[SerializeField]
	[FormerlySerializedAs("IAmSetHandler")]
	private bool m_IAmSetHandler;

	[SerializeField]
	[InfoBox("If you change the 'Settings Platform' value for graphics related setting, then change the corresponding 'Console User Controlled Values' value in 'Assets/Mechanics/Blueprints/Root/Settings/GraphicsPresetsList'")]
	private UISettingsPlatform m_SettingsPlatform;

	[SerializeField]
	private bool m_HasCoopTooltipDescription;

	[SerializeField]
	private bool m_NoDefaultReset;

	[SerializeField]
	private bool m_IsTrinityModeOnly;

	public LocalizedString Description => m_Description;

	public LocalizedString TooltipDescription => m_TooltipDescription;

	public List<BlueprintEncyclopediaPageReference> EncyclopediaDescription => m_EncyclopediaDescription;

	public bool IsCustom => Type == SettingsListItemType.Custom;

	public bool IsSeparator => Type == SettingsListItemType.Separator;

	public virtual SettingsListItemType? Type => null;

	public bool ShowVisualConnection => m_ShowVisualConnection;

	public bool IAmSetHandler => m_IAmSetHandler;

	public UISettingsPlatform SettingsPlatform => m_SettingsPlatform;

	public bool HasCoopTooltipDescription => m_HasCoopTooltipDescription;

	public bool NoDefaultReset => m_NoDefaultReset;

	public bool IsTrinityModeOnly => m_IsTrinityModeOnly;
}
