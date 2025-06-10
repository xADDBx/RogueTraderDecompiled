using Kingmaker.UI.MVVM.VM.CharGen.Phases.Appearance.Components.Base;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.GamepadInput;
using Owlcat.Runtime.UI.ConsoleTools.HintTool;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.Controls.Selectable;
using Owlcat.Runtime.UI.MVVM;
using UniRx;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.CharGen.Common.Phases.Appearance.Components.Base;

public abstract class BaseCharGenAppearancePageComponentView<TViewModel> : VirtualListElementViewBase<TViewModel>, ICharGenAppearancePageComponent, IConsoleNavigationEntity, IConsoleEntity where TViewModel : BaseCharGenAppearancePageComponentVM
{
	[SerializeField]
	private OwlcatMultiSelectable m_Selectable;

	protected readonly ReactiveProperty<bool> IsFocused = new ReactiveProperty<bool>();

	protected override void BindViewImplementation()
	{
	}

	protected override void DestroyViewImplementation()
	{
		IsFocused.Value = false;
	}

	public virtual void AddInput(ref InputLayer inputLayer, ConsoleHintsWidget hintsWidget)
	{
	}

	public virtual void RemoveInput()
	{
	}

	public virtual void SetFocus(bool value)
	{
		if (m_Selectable != null)
		{
			m_Selectable.SetFocus(value);
		}
		IsFocused.Value = value;
		if (value)
		{
			base.ViewModel.Focused();
		}
	}

	public virtual bool IsValid()
	{
		return base.ViewModel.IsAvailable.Value;
	}

	protected void Show()
	{
		base.gameObject.SetActive(value: true);
	}

	protected void Hide()
	{
		base.gameObject.SetActive(value: false);
	}
}
