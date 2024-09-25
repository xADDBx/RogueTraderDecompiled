using System.Collections.Generic;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Owlcat.Runtime.UI.Tooltips;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.ServiceWindows.CharacterInfo.Sections.Abilities;

public class StatusEffectPCView : StatusEffectBaseView
{
	[SerializeField]
	private List<Vector2> m_LeftSideTooltipPivot = new List<Vector2>
	{
		new Vector2(1f, 0.5f)
	};

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		SetupTooltip();
	}

	private void SetupTooltip()
	{
		if (base.ViewModel.Tooltip != null)
		{
			AddDisposable(this.SetTooltip(base.ViewModel.Tooltip, new TooltipConfig(InfoCallPCMethod.RightMouseButton, InfoCallConsoleMethod.LongRightStickButton, isGlossary: false, isEncyclopedia: false, null, 0, 0, 0, m_LeftSideTooltipPivot)));
		}
	}
}
