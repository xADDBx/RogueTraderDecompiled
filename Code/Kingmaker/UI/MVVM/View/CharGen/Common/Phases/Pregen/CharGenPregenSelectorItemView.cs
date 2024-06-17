using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.Common;
using Kingmaker.UI.MVVM.View.Pantograph;
using Kingmaker.UI.MVVM.VM.CharGen.Phases.Pregen;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.SelectionGroup.View;
using Owlcat.Runtime.UI.Utility;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.UI.MVVM.View.CharGen.Common.Phases.Pregen;

public class CharGenPregenSelectorItemView : SelectionGroupEntityView<CharGenPregenSelectorItemVM>, IWidgetView
{
	[SerializeField]
	private TextMeshProUGUI m_PregenNameText;

	[SerializeField]
	private GameObject m_PortraitGroup;

	[SerializeField]
	private Image m_PortraitImage;

	[SerializeField]
	private CharGenPregenPhasePantographItemView m_PantographItemView;

	public PantographConfig PantographConfig { get; protected set; }

	public MonoBehaviour MonoBehaviour => this;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		AddDisposable(base.ViewModel.CharacterName.Subscribe(delegate(string e)
		{
			m_PregenNameText.text = e;
		}));
		SetupPantographConfig();
		AddDisposable(base.ViewModel.IsSelected.Subscribe(OnSelected));
		m_PortraitGroup.SetActive(base.ViewModel.Portrait.Value != null);
		m_PortraitImage.sprite = base.ViewModel.Portrait.Value;
	}

	protected override void OnClick()
	{
		if (UINetUtility.IsControlMainCharacter())
		{
			base.OnClick();
		}
	}

	protected virtual void SetupPantographConfig()
	{
		PantographConfig = new PantographConfig(base.transform, m_PantographItemView, base.ViewModel, useLargeView: true);
	}

	private void OnSelected(bool value)
	{
		if (value && base.ViewModel.IsAvailable.Value)
		{
			EventBus.RaiseEvent(delegate(IPantographHandler h)
			{
				h.Bind(PantographConfig);
			});
		}
	}

	public override void SetFocus(bool value)
	{
		base.SetFocus(value);
		if (value)
		{
			base.ViewModel.SetSelectedFromView(state: true);
		}
		EventBus.RaiseEvent(delegate(IPantographHandler h)
		{
			h.SetFocus(value);
		});
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
