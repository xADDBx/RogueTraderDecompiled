using Kingmaker.UI.MVVM.View.Colonization.Base;
using Owlcat.Runtime.UI.Controls.Other;
using UniRx;

namespace Kingmaker.UI.MVVM.View.Colonization.PC;

public class ColonyProjectsBuiltListAddElemPCView : ColonyProjectsBuiltListAddElemBaseView
{
	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		AddDisposable(m_Button.OnLeftClickAsObservable().Subscribe(delegate
		{
			HandleClick();
		}));
	}

	private void HandleClick()
	{
		base.ViewModel.OpenColonyProjects();
	}
}
