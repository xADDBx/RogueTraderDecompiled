using Kingmaker.UI.MVVM.VM.Colonization.Events;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.Utility;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.UI.MVVM.View.Colonization;

public class ColonyEventNotificatorPCView : ViewBase<ColonyEventVM>, IWidgetView
{
	[SerializeField]
	private Image m_Icon;

	public MonoBehaviour MonoBehaviour => this;

	protected override void BindViewImplementation()
	{
		AddDisposable(base.ViewModel.Icon.Subscribe(delegate(Sprite val)
		{
			m_Icon.sprite = val;
		}));
	}

	protected override void DestroyViewImplementation()
	{
	}

	public void BindWidgetVM(IViewModel vm)
	{
		Bind(vm as ColonyEventVM);
	}

	public bool CheckType(IViewModel viewModel)
	{
		return viewModel is ColonyEventVM;
	}
}
