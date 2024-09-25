using Kingmaker.UI.MVVM.VM.CharGen.Phases.Pregen;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.SelectionGroup.View;
using Owlcat.Runtime.UI.Utility;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View.NewGame.Common;

public class TempPregenSelectorItemCommonView : SelectionGroupEntityView<CharGenPregenSelectorItemVM>, IWidgetView
{
	[SerializeField]
	private Image m_PortraitImage;

	[SerializeField]
	private TextMeshProUGUI m_PregenNameText;

	[SerializeField]
	private TextMeshProUGUI m_ClassText;

	public MonoBehaviour MonoBehaviour => this;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		AddDisposable(base.ViewModel.Portrait.Subscribe(delegate(Sprite e)
		{
			m_PortraitImage.sprite = e;
		}));
		AddDisposable(base.ViewModel.CharacterName.Subscribe(delegate(string e)
		{
			m_PregenNameText.text = e;
		}));
		AddDisposable(base.ViewModel.Class.Subscribe(delegate(string e)
		{
			m_ClassText.text = e;
		}));
	}

	protected override void ClearView()
	{
	}

	public void BindWidgetVM(IViewModel vm)
	{
		Bind(vm as CharGenPregenSelectorItemVM);
	}

	public bool CheckType(IViewModel viewModel)
	{
		return viewModel is CharGenPregenSelectorItemVM;
	}
}
