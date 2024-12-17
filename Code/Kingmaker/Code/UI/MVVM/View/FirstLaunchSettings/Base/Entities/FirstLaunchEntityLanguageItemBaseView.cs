using Kingmaker.Code.UI.MVVM.VM.FirstLaunchSettings.Entities;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.Controls.Other;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UniRx;
using TMPro;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.FirstLaunchSettings.Base.Entities;

public abstract class FirstLaunchEntityLanguageItemBaseView : ViewBase<FirstLaunchEntityLanguageItemVM>, IConsoleEntityProxy, IConsoleEntity
{
	[SerializeField]
	private TextMeshProUGUI m_Title;

	[SerializeField]
	protected OwlcatMultiButton m_Button;

	public IConsoleEntity ConsoleEntityProxy => m_Button;

	public bool IsSelected => base.ViewModel.IsSelected.Value;

	public OwlcatMultiButton EntityButton => m_Button;

	protected override void BindViewImplementation()
	{
		m_Title.text = base.ViewModel.Title;
		AddDisposable(base.ViewModel.IsSelected.Subscribe(SetValueFromSettings));
		AddDisposable(m_Button.OnLeftClick.AsObservable().Subscribe(base.ViewModel.SetSelected));
		AddDisposable(m_Button.OnConfirmClickAsObservable().Subscribe(base.ViewModel.SetSelected));
	}

	protected override void DestroyViewImplementation()
	{
	}

	private void SetValueFromSettings(bool value)
	{
		m_Button.SetActiveLayer(value ? "On" : "Off");
	}
}
