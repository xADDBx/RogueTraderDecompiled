using Kingmaker.Code.UI.MVVM.View.InfoWindow;
using Kingmaker.Code.UI.MVVM.VM.NewGame.Difficulty;
using Kingmaker.Settings;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.Tooltips;
using Owlcat.Runtime.UI.VirtualListSystem;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.NewGame.Base;

public class NewGamePhaseDifficultyBaseView : ViewBase<NewGamePhaseDifficultyVM>
{
	[SerializeField]
	private InfoSectionView m_InfoView;

	[SerializeField]
	protected VirtualListVertical m_VirtualList;

	public readonly ReactiveProperty<TooltipBaseTemplate> ReactiveTooltipTemplate = new ReactiveProperty<TooltipBaseTemplate>();

	public VirtualListVertical VirtualList => m_VirtualList;

	public InfoSectionView InfoView => m_InfoView;

	protected override void BindViewImplementation()
	{
		m_InfoView.Bind(base.ViewModel.InfoVM);
		AddDisposable(base.ViewModel.IsEnabled.Subscribe(delegate(bool value)
		{
			SettingsController.Instance.RevertAllTempValues();
			base.gameObject.SetActive(value);
			m_VirtualList.ScrollController.ForceScrollToTop();
			if (value)
			{
				base.ViewModel.HandleItemChanged(string.Empty);
			}
		}));
		AddDisposable(base.ViewModel.ReactiveTooltipTemplate.Subscribe(delegate(TooltipBaseTemplate value)
		{
			ReactiveTooltipTemplate.Value = value;
		}));
	}

	protected override void DestroyViewImplementation()
	{
	}

	public void StrollInfoViewToTop()
	{
		m_InfoView.Or(null)?.ScrollRectExtended.ScrollToTop();
	}
}
