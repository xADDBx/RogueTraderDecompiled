using Kingmaker.Blueprints.Root;
using Kingmaker.Code.UI.MVVM.VM.SpaceCombat.Components;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UniRx;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Warhammer.SpaceCombat.Blueprints;

namespace Kingmaker.UI.MVVM.View.SpaceCombat.PC;

public class ShipCrewDollModulePCView : ViewBase<ShipCrewDollModuleVM>
{
	[SerializeField]
	private ShipModuleType m_ModuleType;

	[SerializeField]
	private Image m_Image;

	public ShipModuleType ModuleType => m_ModuleType;

	protected override void BindViewImplementation()
	{
		AddDisposable(base.ViewModel.ModuleState.Subscribe(UpdateState));
		m_Image.alphaHitTestMinimumThreshold = 0.5f;
		m_Image.OnPointerClickAsObservable().Subscribe(delegate
		{
			base.ViewModel.HandleClick();
		});
	}

	protected override void DestroyViewImplementation()
	{
	}

	private void UpdateState(ShipCrewModuleState state)
	{
		m_Image.color = UIConfig.Instance.SpaceCombat.GetShipDollModuleColor(state);
	}
}
