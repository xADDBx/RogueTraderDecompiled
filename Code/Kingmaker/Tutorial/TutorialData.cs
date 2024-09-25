using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.Items;
using Kingmaker.ResourceLinks;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.Utility.UnityExtensions;

namespace Kingmaker.Tutorial;

public class TutorialData
{
	public class Page
	{
		public string Title { get; set; }

		public string Description { get; set; }

		[CanBeNull]
		public string TriggerText { get; set; }

		[CanBeNull]
		public string SolutionText { get; set; }

		[CanBeNull]
		public SpriteLink Picture { get; set; }

		public VideoLink Video { get; set; }
	}

	public readonly BlueprintTutorial Blueprint;

	[CanBeNull]
	public readonly TutorialTrigger Trigger;

	public readonly bool SolutionFound;

	public readonly EntityRef<BaseUnitEntity> RevealUnitInfo;

	public readonly List<Page> Pages = new List<Page>();

	[CanBeNull]
	public BaseUnitEntity SourceUnit { get; set; }

	[CanBeNull]
	public BaseUnitEntity SolutionUnit { get; set; }

	[CanBeNull]
	public ItemEntity SolutionItem { get; set; }

	[CanBeNull]
	public AbilityData SolutionAbility { get; set; }

	public TutorialData(BlueprintTutorial blueprint, [CanBeNull] TutorialTrigger trigger, EntityRef<BaseUnitEntity> targetUnit, bool solutionFound)
	{
		Blueprint = blueprint;
		Trigger = trigger;
		SolutionFound = solutionFound;
		RevealUnitInfo = targetUnit;
	}

	public void AddPage(Page page)
	{
		Pages.Add(page);
	}

	public void AddPage(ITutorialPage pageData)
	{
		string text = ((Trigger != null) ? pageData.TriggerText.Text : null);
		text = (text.IsNullOrEmpty() ? null : text);
		string text2 = (SolutionFound ? pageData.SolutionFoundText.Text : pageData.SolutionNotFoundText.Text);
		text2 = (text2.IsNullOrEmpty() ? null : text2);
		Page page = new Page
		{
			Title = pageData.TitleText.Text,
			Description = pageData.DescriptionText.Text,
			TriggerText = text,
			SolutionText = text2,
			Picture = pageData.Picture,
			Video = pageData.Video
		};
		AddPage(page);
	}
}
