using Kingmaker.UI.MVVM.View.Bark.PC;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.SystemMap.PC;

public class SystemMapPCView : SystemMapView
{
	[SerializeField]
	private StarSystemSpaceBarksHolderPCView m_StarSystemSpaceBarksHolderPCView;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		m_StarSystemSpaceBarksHolderPCView.Bind(base.ViewModel.StarSystemSpaceBarksHolderVM);
	}
}
