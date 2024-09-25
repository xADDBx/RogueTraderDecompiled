using System;
using Kingmaker.UI.MVVM.VM.CharGen.Portrait;
using Owlcat.Runtime.UI.MVVM;
using UniRx;

namespace Kingmaker.UI.MVVM.VM.CharGen.Phases.Appearance.Components.Portrait;

public class CharGenCustomPortraitCreatorVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable
{
	public readonly IReadOnlyReactiveProperty<CharGenPortraitVM> Portrait;

	private readonly Action m_OnOpenFolder;

	private readonly Action m_OnRefreshPortrait;

	private readonly Action m_OnClose;

	public CharGenCustomPortraitCreatorVM(IReadOnlyReactiveProperty<CharGenPortraitVM> portraitVM, Action onOpenFolder, Action onRefreshPortrait, Action onClose)
	{
		Portrait = portraitVM.Where((CharGenPortraitVM p) => (p?.PortraitData?.IsCustom).GetValueOrDefault() && p.PortraitData.EnsureImages()).ToReactiveProperty();
		m_OnOpenFolder = onOpenFolder;
		m_OnRefreshPortrait = onRefreshPortrait;
		m_OnClose = onClose;
	}

	public void OnOpenFolderClick()
	{
		m_OnOpenFolder?.Invoke();
	}

	public void OnRefreshPortraitClick()
	{
		m_OnRefreshPortrait?.Invoke();
	}

	public void OnClose()
	{
		m_OnClose?.Invoke();
	}

	protected override void DisposeImplementation()
	{
	}
}
