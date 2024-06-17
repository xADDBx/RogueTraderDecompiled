using System;
using Kingmaker.Blueprints;
using Owlcat.Runtime.UI.MVVM;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.Stories;

public class CharInfoCompanionStoryVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable
{
	public readonly string Title;

	public readonly string StoryText;

	public readonly Sprite Picture;

	public CharInfoCompanionStoryVM(BlueprintCompanionStory story)
	{
		Title = story?.Title;
		StoryText = story?.Description;
		Picture = story?.Image;
	}

	protected override void DisposeImplementation()
	{
	}
}
