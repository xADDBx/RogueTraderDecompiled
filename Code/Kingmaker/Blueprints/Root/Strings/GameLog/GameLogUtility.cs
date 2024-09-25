using System.Text;
using JetBrains.Annotations;
using Kingmaker.UI.Models.Log.Enums;
using UnityEngine;

namespace Kingmaker.Blueprints.Root.Strings.GameLog;

public static class GameLogUtility
{
	[NotNull]
	private static readonly StringBuilder s_StringBuilder = new StringBuilder();

	public static StringBuilder StringBuilder
	{
		get
		{
			s_StringBuilder.Clear();
			return s_StringBuilder;
		}
	}

	public static Sprite GetIcon(PrefixIcon prefix)
	{
		return prefix switch
		{
			PrefixIcon.LeftArrow => GameLogStrings.Instance.LeftArrowSprite, 
			PrefixIcon.RightArrow => GameLogStrings.Instance.RightArrowSprite, 
			PrefixIcon.RightGreyArrow => GameLogStrings.Instance.RightArrowSprite, 
			PrefixIcon.Dies => GameLogStrings.Instance.DiesSprite, 
			PrefixIcon.Momentum => GameLogStrings.Instance.MomentumSprite, 
			PrefixIcon.VeilThickness => GameLogStrings.Instance.VeilThicknessSprite, 
			PrefixIcon.Buff => GameLogStrings.Instance.BuffSprite, 
			PrefixIcon.Trauma => GameLogStrings.Instance.TraumaSprite, 
			PrefixIcon.Folder => GameLogStrings.Instance.FolderSprite, 
			PrefixIcon.Empty => GameLogStrings.Instance.EmptySprite, 
			PrefixIcon.Invisible => null, 
			PrefixIcon.None => null, 
			_ => null, 
		};
	}
}
