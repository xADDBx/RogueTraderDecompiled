using Kingmaker.UI.MVVM.VM.Colonization.Projects;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.Utility;
using TMPro;
using UniRx;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.Colonization.Base;

public class ColonyProjectsHeaderElementBaseView : ViewBase<ColonyProjectsHeaderElementVM>, IWidgetView, IConsoleNavigationEntity, IConsoleEntity
{
	[SerializeField]
	private TextMeshProUGUI m_Header;

	public MonoBehaviour MonoBehaviour => this;

	protected override void BindViewImplementation()
	{
		AddDisposable(base.ViewModel.Header.Subscribe(delegate(string h)
		{
			m_Header.text = h;
		}));
	}

	protected override void DestroyViewImplementation()
	{
	}

	public void BindWidgetVM(IViewModel vm)
	{
		Bind((ColonyProjectsHeaderElementVM)vm);
	}

	public bool CheckType(IViewModel viewModel)
	{
		return viewModel is ColonyProjectsHeaderElementVM;
	}

	public void SetFocus(bool value)
	{
	}

	public bool IsValid()
	{
		return false;
	}
}
