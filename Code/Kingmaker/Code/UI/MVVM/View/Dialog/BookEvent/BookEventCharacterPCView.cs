using Kingmaker.Code.UI.MVVM.VM.Dialog.BookEvent;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.Utility;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View.Dialog.BookEvent;

public class BookEventCharacterPCView : ViewBase<BookEventCharacterVM>, IWidgetView
{
	[SerializeField]
	private Image m_Portrait;

	[SerializeField]
	private Toggle m_Toggle;

	private bool m_IsInit;

	public MonoBehaviour MonoBehaviour => this;

	public void Initialize()
	{
		if (!m_IsInit)
		{
			m_IsInit = true;
		}
	}

	protected override void BindViewImplementation()
	{
		m_Toggle.group = base.transform.parent.EnsureComponent<ToggleGroup>();
		SetPortrait();
		AddDisposable(m_Toggle.OnValueChangedAsObservable().Subscribe(OnChoose));
	}

	private void SetPortrait()
	{
		m_Portrait.gameObject.SetActive(base.ViewModel.Portrait != null);
		m_Portrait.sprite = base.ViewModel.Portrait;
	}

	private void OnChoose(bool value)
	{
		base.ViewModel.OnChooseUnit(value);
	}

	protected override void DestroyViewImplementation()
	{
	}

	public void BindWidgetVM(IViewModel vm)
	{
		if (vm == null)
		{
			SetPortrait();
		}
		else
		{
			Bind((BookEventCharacterVM)vm);
		}
	}

	public bool CheckType(IViewModel viewModel)
	{
		return viewModel is BookEventCharacterVM;
	}
}
