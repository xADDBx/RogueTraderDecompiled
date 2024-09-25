using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.MVVM.View.Pantograph;
using Kingmaker.UI.MVVM.VM.CharGen.Phases.Ship;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.SelectionGroup.View;
using Owlcat.Runtime.UI.Utility;
using TMPro;
using UniRx;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.CharGen.Common.Phases.Ship;

public class CharGenShipPhaseSelectorItemView : SelectionGroupEntityView<CharGenShipItemVM>, IWidgetView
{
	[SerializeField]
	private TextMeshProUGUI m_DisplayName;

	public PantographConfig PantographConfig { get; protected set; }

	public MonoBehaviour MonoBehaviour => this;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		m_DisplayName.text = base.ViewModel.Title;
		SetupPantographConfig();
		AddDisposable(base.ViewModel.IsSelected.Subscribe(OnSelected));
	}

	protected virtual void SetupPantographConfig()
	{
		PantographConfig = new PantographConfig(base.transform, m_DisplayName.text);
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
		Bind(vm as CharGenShipItemVM);
	}

	public bool CheckType(IViewModel viewModel)
	{
		return viewModel is CharGenShipItemVM;
	}
}
