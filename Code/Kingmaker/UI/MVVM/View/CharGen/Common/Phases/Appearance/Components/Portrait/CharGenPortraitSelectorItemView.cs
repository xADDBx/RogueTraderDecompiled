using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.Common;
using Kingmaker.UI.MVVM.VM.CharGen.Phases.Appearance.Components.Portrait;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.ClickHandlers;
using Owlcat.Runtime.UI.Controls.Other;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.SelectionGroup.View;
using Owlcat.Runtime.UI.Utility;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.UI.MVVM.View.CharGen.Common.Phases.Appearance.Components.Portrait;

public class CharGenPortraitSelectorItemView : SelectionGroupEntityView<CharGenPortraitSelectorItemVM>, IWidgetView, IFunc02ClickHandler, IConsoleEntity
{
	[SerializeField]
	private Image m_Portrait;

	public bool IsSelected => base.ViewModel.IsSelected.Value;

	public MonoBehaviour MonoBehaviour => this;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		if (base.ViewModel.PortraitData.SmallPortraitHandle != null)
		{
			base.ViewModel.PortraitData.SmallPortraitHandle.Request.Loaded += RefreshView;
		}
		AddDisposable(m_Button.OnHoverAsObservable().Subscribe(delegate(bool value)
		{
			if (value)
			{
				EventBus.RaiseEvent(delegate(ICharGenPortraitSelectorHoverHandler h)
				{
					h.HandleHoverStart(base.ViewModel.PortraitData);
				});
			}
			else
			{
				EventBus.RaiseEvent(delegate(ICharGenPortraitSelectorHoverHandler h)
				{
					h.HandleHoverStop();
				});
			}
		}));
	}

	protected override void DestroyViewImplementation()
	{
		base.DestroyViewImplementation();
		if (base.ViewModel.PortraitData.SmallPortraitHandle != null)
		{
			base.ViewModel.PortraitData.SmallPortraitHandle.Request.Loaded -= RefreshView;
		}
	}

	protected override void OnClick()
	{
		if (UINetUtility.IsControlMainCharacter())
		{
			base.OnClick();
		}
	}

	public override void RefreshView()
	{
		m_Portrait.sprite = base.ViewModel.PortraitData.SmallPortrait;
	}

	public void BindWidgetVM(IViewModel vm)
	{
		Bind(vm as CharGenPortraitSelectorItemVM);
	}

	public bool CheckType(IViewModel viewModel)
	{
		if (viewModel is CharGenPortraitSelectorItemVM charGenPortraitSelectorItemVM)
		{
			return !charGenPortraitSelectorItemVM.CustomPortraitCreatorItem;
		}
		return false;
	}

	public override bool IsValid()
	{
		if (base.IsValid())
		{
			return base.gameObject.activeInHierarchy;
		}
		return false;
	}

	public bool CanFunc02Click()
	{
		return base.ViewModel.IsCustom;
	}

	public void OnFunc02Click()
	{
		base.ViewModel.OnCustomPortraitChange();
	}

	public string GetFunc02ClickHint()
	{
		return UIStrings.Instance.CharGen.ChangePortrait;
	}
}
