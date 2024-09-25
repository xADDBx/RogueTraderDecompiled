using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Localization;
using Kingmaker.ResourceLinks;
using Kingmaker.UI.Models.SettingsUI.SettingAssets;
using Kingmaker.Utility.Attributes;
using UnityEngine;

namespace Kingmaker.Tutorial;

[TypeId("14881005377545c48c592b6bc8151c9a")]
public class BlueprintTutorial : BlueprintFact, ITutorialPage
{
	[Serializable]
	public class Reference : BlueprintReference<BlueprintTutorial>
	{
	}

	[SerializeField]
	private VisualSettings m_VisualSettings;

	[SerializeField]
	private Reference m_BlueprintTutorialConsoleRef;

	[SerializeField]
	private LocalizedString m_TitleText;

	[SerializeField]
	[ShowIf("HasTrigger")]
	private LocalizedString m_TriggerText;

	[SerializeField]
	private LocalizedString m_DescriptionText;

	[SerializeField]
	[ShowIf("HasSolver")]
	private LocalizedString m_SolutionFoundText;

	[SerializeField]
	private LocalizedString m_SolutionNotFoundText;

	public TutorialTag Tag;

	public int Priority;

	public int Limit = 1;

	public int Frequency;

	public bool SetCooldown;

	public bool IgnoreCooldown;

	public bool Windowed;

	public bool DisableAnalyticsTracking;

	public UISettingsEntityBase.UISettingsPlatform VisibilitySetting;

	public BlueprintEncyclopediaPageReference EncyclopediaReference;

	private bool HasTrigger => this.GetComponent<TutorialTrigger>();

	private bool HasSolver => this.GetComponent<TutorialSolver>();

	public Reference BlueprintTutorialConsoleRef => m_BlueprintTutorialConsoleRef;

	public SpriteLink Picture => m_VisualSettings?.Picture;

	public VideoLink Video => m_VisualSettings?.Video;

	public VisualOverride XBox => m_VisualSettings?.XBox;

	public VisualOverride PS4 => m_VisualSettings?.PS4;

	public LocalizedString TitleText => m_TitleText;

	public LocalizedString DescriptionText => m_DescriptionText;

	public LocalizedString TriggerText => m_TriggerText;

	public LocalizedString SolutionFoundText => m_SolutionFoundText;

	public LocalizedString SolutionNotFoundText => m_SolutionNotFoundText;

	protected override Type GetFactType()
	{
		return typeof(Tutorial);
	}

	public BlueprintTutorial GetTutorial()
	{
		if (!Application.isPlaying)
		{
			return null;
		}
		if (Game.Instance.IsControllerGamepad)
		{
			return m_BlueprintTutorialConsoleRef?.Get() ?? this;
		}
		return this;
	}
}
