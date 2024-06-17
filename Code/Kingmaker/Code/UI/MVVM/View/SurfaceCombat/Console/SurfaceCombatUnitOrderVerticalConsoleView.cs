using System;
using System.Collections.Generic;
using DG.Tweening;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Kingmaker.GameModes;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.MVVM.VM.Tooltip.Templates;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.Tooltips;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.SurfaceCombat.Console;

public class SurfaceCombatUnitOrderVerticalConsoleView : SurfaceCombatUnitOrderVerticalView, IConsoleNavigationEntity, IConsoleEntity
{
	[Header("Console")]
	[SerializeField]
	private OwlcatMultiButton m_Button;

	public void SetFocus(bool value)
	{
		m_Button.SetFocus(value);
		SetSelected(value);
		if (base.ViewModel != null)
		{
			base.ViewModel.UnitAsBaseUnitEntity?.View.HandleHoverChange(value);
		}
	}

	protected override void SetAlphaAndScale()
	{
		base.CanvasGroup.alpha = 0f;
		base.RectTransform.localScale = new Vector3(1.2f, 1.2f, 1.2f);
	}

	protected override Sequence GetHideAnimationInternal(Action completeAction)
	{
		Sequence hideAnimationInternal = base.GetHideAnimationInternal(completeAction);
		hideAnimationInternal.OnStart(delegate
		{
			m_Button.SetFocus(value: false);
		});
		return hideAnimationInternal;
	}

	public void ShowInspect()
	{
		EventBus.RaiseEvent(delegate(IUnitClickUIHandler h)
		{
			h.HandleUnitConsoleInvoke(base.ViewModel.UnitAsBaseUnitEntity);
		});
	}

	public void ShowTooltip(RectTransform upperRightAnchor)
	{
		TooltipBaseTemplate template = ((Game.Instance.CurrentMode == GameModeType.SpaceCombat) ? ((TooltipBaseTemplate)new TooltipTemplateSpaceUnitInspect(base.ViewModel.UnitAsBaseUnitEntity)) : ((TooltipBaseTemplate)new TooltipTemplateUnitInspectShort(base.ViewModel.UnitAsBaseUnitEntity)));
		m_Button.ShowConsoleTooltip(template, null, new TooltipConfig
		{
			InfoCallConsoleMethod = InfoCallConsoleMethod.None,
			TooltipPlace = upperRightAnchor,
			PriorityPivots = new List<Vector2>
			{
				new Vector2(1f, 1f)
			}
		});
	}

	public void ToggleSquad()
	{
		base.ViewModel.HandleShow();
	}
}
