using Kingmaker.Code.UI.MVVM.View.ServiceWindows.CargoManagement.Components;
using Kingmaker.Code.UI.MVVM.VM.Loot;
using Owlcat.Runtime.UI.MVVM;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.Loot.PC;

public class LootContextPCView : ViewBase<LootContextVM>
{
	[SerializeField]
	private LootPCView m_LootPCView;

	[SerializeField]
	private CargoRewardsPCView m_CargoPCView;

	private bool m_IsInit;

	public void Initialize()
	{
		if (!m_IsInit)
		{
			m_LootPCView.Initialize();
			m_CargoPCView.Initialize();
			m_IsInit = true;
		}
	}

	protected override void BindViewImplementation()
	{
		AddDisposable(base.ViewModel.LootVM.Subscribe(m_LootPCView.Bind));
		AddDisposable(base.ViewModel.CargoVM.Subscribe(m_CargoPCView.Bind));
	}

	protected override void DestroyViewImplementation()
	{
	}
}
