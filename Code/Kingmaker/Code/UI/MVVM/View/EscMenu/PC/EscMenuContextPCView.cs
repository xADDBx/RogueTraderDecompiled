using Kingmaker.Code.UI.MVVM.VM.EscMenu;
using Kingmaker.UI.MVVM.View.EscMenu.PC;
using Owlcat.Runtime.UI.MVVM;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.EscMenu.PC;

public class EscMenuContextPCView : ViewBase<EscMenuContextVM>
{
	[SerializeField]
	private EscMenuPCView m_EscMenuPCView;

	private bool m_IsInit;

	public void Initialize()
	{
		if (!m_IsInit)
		{
			m_EscMenuPCView.Initialize();
			m_IsInit = true;
		}
	}

	protected override void BindViewImplementation()
	{
		AddDisposable(base.ViewModel.EscMenu.Subscribe(m_EscMenuPCView.Bind));
	}

	protected override void DestroyViewImplementation()
	{
	}
}
