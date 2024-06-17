using System.Collections.Generic;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.Utility;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.ServiceWindows.Encyclopedia.Blocks;

public abstract class EncyclopediaPageBlockPCView<TBlockVM> : ViewBase<TBlockVM>, IWidgetView where TBlockVM : class, IViewModel
{
	private bool m_IsInit;

	public MonoBehaviour MonoBehaviour => this;

	public void Initialize()
	{
		if (!m_IsInit)
		{
			m_IsInit = true;
			DoInitialize();
		}
	}

	public virtual void DoInitialize()
	{
	}

	protected override void BindViewImplementation()
	{
		base.gameObject.SetActive(value: true);
	}

	protected override void DestroyViewImplementation()
	{
		base.gameObject.SetActive(value: false);
	}

	public abstract List<TextMeshProUGUI> GetLinksTexts();

	public void BindWidgetVM(IViewModel vm)
	{
		Bind((TBlockVM)vm);
	}

	public bool CheckType(IViewModel viewModel)
	{
		return viewModel is TBlockVM;
	}
}
