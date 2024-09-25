using System.Collections.Generic;
using System.Text;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.Dialog.BookEvent;
using Kingmaker.Code.UI.MVVM.VM.Dialog.Dialog;
using Kingmaker.Controllers.Dialog;
using Kingmaker.DialogSystem.Blueprints;
using Kingmaker.ResourceLinks;
using Kingmaker.UnitLogic.Alignments;
using UniRx;
using UnityEngine;
using UnityEngine.Video;

namespace Kingmaker.Code.UI.MVVM.VM.Dialog.Epilog;

public class EpilogVM : BookEventVM
{
	public ReactiveProperty<VideoClip> BackgroundClip { get; } = new ReactiveProperty<VideoClip>();


	public ReactiveProperty<Sprite> BackgroundSprite { get; } = new ReactiveProperty<Sprite>();


	public ReactiveProperty<Sprite> Portrait { get; } = new ReactiveProperty<Sprite>();


	public ReactiveProperty<string> Title { get; } = new ReactiveProperty<string>();


	public ReactiveProperty<string> SoundStart { get; } = new ReactiveProperty<string>();


	public ReactiveProperty<string> SoundStop { get; } = new ReactiveProperty<string>();


	public EpilogVM()
	{
		Portrait.Value = null;
		Title.Value = UIStrings.Instance.Epilogues.EpiloguesPortraitTitle;
	}

	protected override void SetPage(BlueprintBookPage page, List<CueShowData> cues, List<BlueprintAnswer> answers)
	{
		base.SetPage(page, cues, answers);
		SetPortrait(page);
		SetBackground(page);
	}

	private void SetTitle(BlueprintBookPage page)
	{
		Title.Value = (page.ShowMainCharacterName ? Game.Instance.Player.MainCharacter.Entity.CharacterName : (page.Companion?.Get()?.CharacterName ?? string.Empty));
	}

	protected override void SetCues(List<CueShowData> cues)
	{
		StringBuilder stringBuilder = new StringBuilder();
		foreach (CueShowData cue in cues)
		{
			stringBuilder.AppendLine();
			stringBuilder.AppendLine(cue.Cue.DisplayText);
		}
		Cues.Add(new CueVM(stringBuilder.ToString(), new List<SkillCheckResult>(), new List<SoulMarkShift>()));
	}

	private void SetPortrait(BlueprintBookPage page)
	{
		if (page.ShowMainCharacter)
		{
			Portrait.Value = Game.Instance.Player.MainCharacterEntity.UISettings.Portrait.FullLengthPortrait;
			return;
		}
		BlueprintPortraitReference portrait = page.Portrait;
		if (portrait?.Get() != null)
		{
			Portrait.Value = (portrait.Get().FullLengthPortrait ? portrait.Get().FullLengthPortrait : portrait.Get().HalfLengthPortrait);
		}
		else
		{
			Portrait.Value = Game.Instance.Player.MainCharacterEntity.UISettings.Portrait.FullLengthPortrait;
		}
	}

	private void SetBackground(BlueprintBookPage page)
	{
		if (page.UseSound)
		{
			if (page.SoundStartEvent == null || !page.SoundStartEvent.IsValid() || page.SoundStopEvent == null || !page.SoundStopEvent.IsValid())
			{
				Debug.LogError($"{page} has Null or Invalid sound events");
			}
			SoundStart.Value = ((page.SoundStartEvent != null && page.SoundStartEvent.IsValid()) ? page.SoundStartEvent.Name : null);
			SoundStop.Value = ((page.SoundStopEvent != null && page.SoundStopEvent.IsValid()) ? page.SoundStopEvent.Name : null);
		}
		else
		{
			SoundStart.Value = null;
			SoundStop.Value = null;
		}
		if (page.UseBackgroundVideo)
		{
			VideoLink backgroundVideoLink = page.BackgroundVideoLink;
			BackgroundSprite.Value = null;
			BackgroundClip.Value = ((backgroundVideoLink != null && backgroundVideoLink.Exists()) ? backgroundVideoLink.Load() : null);
		}
		else
		{
			SpriteLink backgroundImageLink = page.BackgroundImageLink;
			BackgroundClip.Value = null;
			BackgroundSprite.Value = ((backgroundImageLink != null && backgroundImageLink.Exists()) ? backgroundImageLink.Load() : null);
		}
	}
}
