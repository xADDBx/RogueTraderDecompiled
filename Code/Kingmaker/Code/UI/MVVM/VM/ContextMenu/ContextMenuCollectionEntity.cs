using System;
using Kingmaker.Localization;
using Kingmaker.UI.Sound;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.VM.ContextMenu;

public class ContextMenuCollectionEntity
{
	public readonly bool IsHeader;

	public Sprite Icon;

	public LocalizedString Title { get; }

	public string TitleText { get; private set; }

	public string SubTitle { get; private set; }

	public Action Command { get; }

	public Func<bool> Condition { get; private set; }

	public bool IsInteractable { get; private set; } = true;


	public UISounds.ButtonSoundsEnum ClickSoundType { get; }

	public UISounds.ButtonSoundsEnum HoverSoundType { get; }

	public bool IsEmpty => Command == null;

	public bool IsEnabled => Condition?.Invoke() ?? true;

	public bool IsValid
	{
		get
		{
			if (IsEmpty || !IsEnabled)
			{
				return IsHeader;
			}
			return true;
		}
	}

	public ContextMenuCollectionEntity(UISounds.ButtonSoundsEnum clickSoundType = UISounds.ButtonSoundsEnum.NormalSound, UISounds.ButtonSoundsEnum hoverSoundType = UISounds.ButtonSoundsEnum.NormalSound)
	{
		ClickSoundType = clickSoundType;
		HoverSoundType = hoverSoundType;
	}

	public ContextMenuCollectionEntity(LocalizedString title, Action command, Func<bool> condition = null, UISounds.ButtonSoundsEnum clickSoundType = UISounds.ButtonSoundsEnum.NormalSound, UISounds.ButtonSoundsEnum hoverSoundType = UISounds.ButtonSoundsEnum.NormalSound)
	{
		Title = title;
		Command = command;
		Condition = condition;
		ClickSoundType = clickSoundType;
		HoverSoundType = hoverSoundType;
	}

	public ContextMenuCollectionEntity(string title, string subTitle, bool isHeader = false, UISounds.ButtonSoundsEnum clickSoundType = UISounds.ButtonSoundsEnum.NormalSound, UISounds.ButtonSoundsEnum hoverSoundType = UISounds.ButtonSoundsEnum.NormalSound)
	{
		TitleText = title;
		IsHeader = isHeader;
		SubTitle = subTitle;
		ClickSoundType = clickSoundType;
		HoverSoundType = hoverSoundType;
	}

	public ContextMenuCollectionEntity(LocalizedString title, Action command, bool condition, bool isInteractable = true, UISounds.ButtonSoundsEnum clickSoundType = UISounds.ButtonSoundsEnum.NormalSound, UISounds.ButtonSoundsEnum hoverSoundType = UISounds.ButtonSoundsEnum.NormalSound)
	{
		Title = title;
		Command = command;
		Condition = () => condition;
		IsInteractable = isInteractable;
		ClickSoundType = clickSoundType;
		HoverSoundType = hoverSoundType;
	}

	public ContextMenuCollectionEntity(string title, Action command, bool condition, bool isInteractable = true, Sprite icon = null, UISounds.ButtonSoundsEnum clickSoundType = UISounds.ButtonSoundsEnum.NormalSound, UISounds.ButtonSoundsEnum hoverSoundType = UISounds.ButtonSoundsEnum.NormalSound)
	{
		TitleText = title;
		Command = command;
		Condition = () => condition;
		IsInteractable = isInteractable;
		Icon = icon;
		ClickSoundType = clickSoundType;
		HoverSoundType = hoverSoundType;
	}

	public void SetNewTitleText(string text)
	{
		TitleText = text;
	}

	public void SetNewIcon(Sprite icon)
	{
		Icon = icon;
	}

	public void SetSubtitleText(string text)
	{
		SubTitle = text;
	}

	public void ForceUpdateEnabling(bool value)
	{
		Condition = () => value;
	}

	public void ForceUpdateInteractive(bool value)
	{
		IsInteractable = value;
	}

	public void Execute()
	{
		Command?.Invoke();
	}
}
