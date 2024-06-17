using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Localization;
using Kingmaker.ResourceLinks;
using Kingmaker.Utility.Attributes;
using UnityEngine;

namespace Kingmaker.Tutorial;

[AllowedOn(typeof(BlueprintTutorial))]
[AllowMultipleComponents]
[TypeId("44b9a088ef2f4f35bcae462c983decdc")]
public class TutorialPage : BlueprintComponent, ITutorialPage
{
	[SerializeField]
	private VisualSettings m_VisualSettings;

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

	private bool HasTrigger => base.OwnerBlueprint.GetComponent<TutorialTrigger>();

	private bool HasSolver => base.OwnerBlueprint.GetComponent<TutorialSolver>();

	public SpriteLink Picture => m_VisualSettings?.Picture;

	public VideoLink Video => m_VisualSettings?.Video;

	public LocalizedString TitleText => m_TitleText;

	public LocalizedString DescriptionText => m_DescriptionText;

	public LocalizedString TriggerText => m_TriggerText;

	public LocalizedString SolutionFoundText => m_SolutionFoundText;

	public LocalizedString SolutionNotFoundText => m_SolutionNotFoundText;
}
