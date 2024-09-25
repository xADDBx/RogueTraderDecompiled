using JetBrains.Annotations;
using Kingmaker.Code.UI.MVVM.VM.Other;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.Utility;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View.SurfaceCombat.Console;

public class InitiativeTrackerBuffView : ViewBase<BuffVM>, IWidgetView
{
	[SerializeField]
	[UsedImplicitly]
	private Image m_Icon;

	public MonoBehaviour MonoBehaviour => this;

	public void BindWidgetVM(IViewModel vm)
	{
		Bind((BuffVM)vm);
	}

	protected override void BindViewImplementation()
	{
		AddDisposable(base.ViewModel.Icon.Subscribe(delegate(Sprite icon)
		{
			m_Icon.sprite = icon;
		}));
	}

	protected override void DestroyViewImplementation()
	{
	}

	public bool CheckType(IViewModel viewModel)
	{
		return viewModel is BuffVM;
	}
}
