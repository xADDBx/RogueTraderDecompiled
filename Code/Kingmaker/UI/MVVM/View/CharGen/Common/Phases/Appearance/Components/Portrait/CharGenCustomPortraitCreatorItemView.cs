using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.UI.Common;
using Kingmaker.UI.MVVM.VM.CharGen.Phases.Appearance.Components.Portrait;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.Controls.Other;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.Utility;
using UniRx;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.CharGen.Common.Phases.Appearance.Components.Portrait;

public class CharGenCustomPortraitCreatorItemView : ViewBase<CharGenPortraitSelectorItemVM>, IWidgetView, IConsoleEntityProxy, IConsoleEntity
{
	[SerializeField]
	private OwlcatButton m_Button;

	public IConsoleEntity ConsoleEntityProxy => m_Button;

	public MonoBehaviour MonoBehaviour => this;

	protected override void BindViewImplementation()
	{
		m_Button.ConfirmClickHint = UIStrings.Instance.CharGen.AddPortrait;
		bool flag = UINetUtility.IsControlMainCharacter();
		m_Button.SetInteractable(flag);
		if (flag)
		{
			AddDisposable(m_Button.OnLeftClickAsObservable().Subscribe(delegate
			{
				base.ViewModel.OnCustomPortraitCreate();
			}));
			AddDisposable(m_Button.OnConfirmClickAsObservable().Subscribe(delegate
			{
				base.ViewModel.OnCustomPortraitCreate();
			}));
		}
	}

	protected override void DestroyViewImplementation()
	{
	}

	public void BindWidgetVM(IViewModel vm)
	{
		Bind(vm as CharGenPortraitSelectorItemVM);
	}

	public bool CheckType(IViewModel viewModel)
	{
		if (viewModel is CharGenPortraitSelectorItemVM charGenPortraitSelectorItemVM)
		{
			return charGenPortraitSelectorItemVM.CustomPortraitCreatorItem;
		}
		return false;
	}
}
