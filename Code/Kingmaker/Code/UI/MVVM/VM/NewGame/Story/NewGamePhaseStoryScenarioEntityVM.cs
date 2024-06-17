using System;
using Kingmaker.Blueprints.Root;
using Owlcat.Runtime.UI.SelectionGroup;

namespace Kingmaker.Code.UI.MVVM.VM.NewGame.Story;

public class NewGamePhaseStoryScenarioEntityVM : SelectionGroupEntityVM
{
	private readonly NewGameRoot.StoryEntity m_StoryEntity;

	private Action m_SetStory;

	public string Title => m_StoryEntity.Title;

	public NewGamePhaseStoryScenarioEntityVM(NewGameRoot.StoryEntity storyEntity, Action setStory)
		: base(allowSwitchOff: false)
	{
		m_StoryEntity = storyEntity;
		m_SetStory = setStory;
	}

	protected override void DoSelectMe()
	{
		m_SetStory?.Invoke();
	}

	protected override void DisposeImplementation()
	{
		m_SetStory = null;
	}
}
