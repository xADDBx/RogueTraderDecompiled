using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.Common;
using Kingmaker.UI.MVVM.View.Pantograph;
using Kingmaker.UI.MVVM.VM.CharGen.Phases;
using Kingmaker.UI.MVVM.VM.CharGen.Phases.BackgroundBase;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.SelectionGroup.View;
using Owlcat.Runtime.UI.Utility;
using TMPro;
using UniRx;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.CharGen.Common.Phases.BackgroundBase;

public class CharGenBackgroundBasePhaseSelectorItemView<TViewModel> : SelectionGroupEntityView<TViewModel>, IWidgetView where TViewModel : CharGenBackgroundBaseItemVM
{
	[SerializeField]
	private TextMeshProUGUI m_DisplayName;

	protected PantographConfig PantographConfig { get; set; }

	public MonoBehaviour MonoBehaviour => this;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		m_DisplayName.text = base.ViewModel.DisplayName;
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

	protected virtual void SetupPantographConfig()
	{
		PantographConfig = new PantographConfig(base.transform, m_DisplayName.text);
	}

	private void OnSelected(bool value)
	{
		if (!value || !base.ViewModel.IsAvailable.Value)
		{
			return;
		}
		ReactiveProperty<CharGenPhaseBaseVM> currentPhase = base.ViewModel.CurrentPhase;
		if (currentPhase == null || currentPhase.Value.PhaseType != 0)
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
		Bind(vm as TViewModel);
	}

	public bool CheckType(IViewModel viewModel)
	{
		return viewModel is CharGenBackgroundBaseItemVM;
	}
}
