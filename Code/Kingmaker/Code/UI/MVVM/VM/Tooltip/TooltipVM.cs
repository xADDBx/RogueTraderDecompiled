using System;
using System.Collections.Generic;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.InfoWindow;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.Tooltips;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.VM.Tooltip;

public class TooltipVM : InfoBaseVM
{
	public readonly RectTransform OwnerTransform;

	public readonly TooltipBackground Background;

	public readonly InfoCallPCMethod InfoCallPCMethod;

	public readonly InfoCallConsoleMethod InfoCallConsoleMethod;

	public readonly int MaxHeight;

	public readonly int PreferredHeight;

	public readonly int Width;

	public readonly bool IsGlossary;

	public readonly List<Vector2> PriorityPivots;

	public readonly ConsoleNavigationBehaviour OwnerNavigationBehaviour;

	public readonly bool IsComparative;

	public readonly bool ShouldNotHideLittleTooltip;

	protected override TooltipTemplateType TemplateType => TooltipTemplateType.Tooltip;

	public TooltipVM(TooltipData data, bool isComparative = false, bool shouldNotHideLittleTooltip = false)
		: base(data.MainTemplate)
	{
		PriorityPivots = data.Config.PriorityPivots;
		OwnerTransform = data.Config.TooltipPlace;
		Background = data.MainTemplate.Background;
		InfoCallPCMethod = data.Config.InfoCallPCMethod;
		InfoCallConsoleMethod = data.Config.InfoCallConsoleMethod;
		PreferredHeight = data.Config.PreferredHeight;
		MaxHeight = data.Config.MaxHeight;
		Width = data.Config.Width;
		IsComparative = isComparative;
		IsGlossary = data.Config.IsGlossary;
		ShouldNotHideLittleTooltip = shouldNotHideLittleTooltip;
		OwnerNavigationBehaviour = data.OwnerNavigationBehaviour;
		if (Game.Instance.IsControllerMouse && !data.Config.IsEncyclopedia)
		{
			SetupPCInteractionHint();
		}
		AddDisposable(data.CloseCommand?.Subscribe(delegate
		{
			TooltipHelper.HideTooltip();
		}));
	}

	private void SetupPCInteractionHint()
	{
		switch (InfoCallPCMethod)
		{
		case InfoCallPCMethod.LeftMouseButton:
			AddHintBrick(UIStrings.Instance.Tooltips.ShowInfo);
			break;
		case InfoCallPCMethod.RightMouseButton:
			AddHintBrick(UIStrings.Instance.Tooltips.ShowInfo);
			break;
		case InfoCallPCMethod.ShiftRightMouseButton:
			AddHintBrick(UIStrings.Instance.Tooltips.ShowInfo);
			break;
		default:
			throw new ArgumentOutOfRangeException();
		case InfoCallPCMethod.None:
			break;
		}
	}

	private void AddHintBrick(string hintString)
	{
		HintBricks.Add(new TooltipBrickHintVM(hintString));
	}
}
