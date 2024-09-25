using Kingmaker.Code.UI.MVVM.VM.EscMenu;
using Owlcat.Runtime.UI.MVVM;
using UniRx;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.EscMenu.Console;

public class EscMenuContextConsoleView : ViewBase<EscMenuContextVM>
{
	[SerializeField]
	private EscMenuConsoleView m_EscMenuConsoleView;

	public void Initialize()
	{
		m_EscMenuConsoleView.Initialize();
	}

	protected override void BindViewImplementation()
	{
		AddDisposable(base.ViewModel.EscMenu.Subscribe(m_EscMenuConsoleView.Bind));
	}

	protected override void DestroyViewImplementation()
	{
	}
}
