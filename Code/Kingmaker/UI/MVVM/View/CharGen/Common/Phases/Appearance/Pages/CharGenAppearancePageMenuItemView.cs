using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.Common;
using Kingmaker.UI.MVVM.View.Pantograph;
using Kingmaker.UI.MVVM.VM.CharGen.Phases.Appearance.Pages;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.SelectionGroup.View;
using Owlcat.Runtime.UI.Utility;
using TMPro;
using UniRx;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.CharGen.Common.Phases.Appearance.Pages;

public class CharGenAppearancePageMenuItemView : SelectionGroupEntityView<CharGenAppearancePageVM>, IWidgetView
{
	[SerializeField]
	private TextMeshProUGUI m_ButtonLabel;

	public PantographConfig PantographConfig { get; private set; }

	public MonoBehaviour MonoBehaviour => this;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		m_ButtonLabel.text = base.ViewModel.PageLabel;
		SetupPantographConfig();
		AddDisposable(base.ViewModel.IsSelected.Subscribe(OnSelected));
	}

	protected override void OnClick()
	{
		if (UINetUtility.IsControlMainCharacter())
		{
			base.OnClick();
		}
	}

	private void SetupPantographConfig()
	{
		PantographConfig = new PantographConfig(base.transform, m_ButtonLabel.text);
	}

	private void OnSelected(bool value)
	{
		if (value && base.ViewModel.IsAvailable.Value && base.ViewModel.IsInDetailedView.Value)
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
		base.ViewModel.SetSelectedFromView(value);
	}

	public void BindWidgetVM(IViewModel vm)
	{
		Bind(vm as CharGenAppearancePageVM);
	}

	public bool CheckType(IViewModel viewModel)
	{
		return viewModel is CharGenAppearancePageVM;
	}
}
