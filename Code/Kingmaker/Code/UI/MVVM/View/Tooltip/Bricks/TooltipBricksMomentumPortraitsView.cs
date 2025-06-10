using System.Collections.Generic;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.Tooltip.Bricks;

public class TooltipBricksMomentumPortraitsView : TooltipBaseBrickView<TooltipBricksMomentumPortraitsVM>
{
	[SerializeField]
	private List<TooltipBrickMomentumPortraitView> MomentumPortraitViews = new List<TooltipBrickMomentumPortraitView>();

	protected override void BindViewImplementation()
	{
		int num = Mathf.Min(base.ViewModel.MomentumPortraits.Count, MomentumPortraitViews?.Count ?? 0);
		for (int i = 0; i < num; i++)
		{
			if (base.ViewModel.MomentumPortraits[i] != null)
			{
				MomentumPortraitViews[i].Bind(base.ViewModel.MomentumPortraits[i]?.GetVM() as TooltipBrickMomentumPortraitVM);
			}
			else
			{
				MomentumPortraitViews[i].gameObject.SetActive(value: false);
			}
		}
		for (int j = num; j < MomentumPortraitViews.Count; j++)
		{
			MomentumPortraitViews[j].gameObject.SetActive(value: false);
		}
	}

	protected override void DestroyViewImplementation()
	{
		for (int i = 0; i < MomentumPortraitViews.Count; i++)
		{
			MomentumPortraitViews[i].GetViewModel()?.Dispose();
		}
	}
}
