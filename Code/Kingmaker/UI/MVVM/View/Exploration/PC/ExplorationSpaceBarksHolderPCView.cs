using Kingmaker.Code.UI.MVVM.VM.Exploration;
using Kingmaker.UI.MVVM.View.Bark.PC;
using Kingmaker.UI.MVVM.View.Exploration.Base;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.Exploration.PC;

public class ExplorationSpaceBarksHolderPCView : ExplorationComponentBaseView<ExplorationSpaceBarksHolderVM>
{
	[SerializeField]
	private SpaceBarksHolderPCView m_SpaceBarksHolderPCView;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		m_SpaceBarksHolderPCView.Bind(base.ViewModel.SpaceBarksHolderVM);
	}

	protected override void DestroyViewImplementation()
	{
	}
}
