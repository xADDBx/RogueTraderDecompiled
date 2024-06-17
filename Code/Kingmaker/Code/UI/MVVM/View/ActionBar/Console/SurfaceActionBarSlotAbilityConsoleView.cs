using System.Collections.Generic;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.ActionBar.Surface;
using Kingmaker.Code.UI.MVVM.VM.ContextMenu;
using Kingmaker.PubSubSystem.Core;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.ClickHandlers;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.Tooltips;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.ActionBar.Console;

public class SurfaceActionBarSlotAbilityConsoleView : SurfaceActionBarSlotAbilityView, IConsoleNavigationEntity, IConsoleEntity, IConfirmClickHandler, IHasTooltipTemplate
{
	[Header("ConsoleSlot")]
	[SerializeField]
	private ActionBarSlotConsoleView m_SlotConsoleView;

	public List<ContextMenuCollectionEntity> ContextMenuEntities = new List<ContextMenuCollectionEntity>();

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		m_SlotConsoleView.Bind(base.ViewModel);
		AddDisposable(base.ViewModel.IsEmpty.Subscribe(delegate
		{
			SetContextMenuEntities();
		}));
	}

	private void SetContextMenuEntities()
	{
		ContextMenuEntities.Clear();
		ContextMenuEntities.Add(new ContextMenuCollectionEntity(UIStrings.Instance.CharacterSheet.ChooseActiveAbilityFeatureGroupHint, base.ViewModel.ChooseAbility));
		ContextMenuEntities.Add(new ContextMenuCollectionEntity(UIStrings.Instance.ActionTexts.MoveItem, base.ViewModel.MoveAbility, !base.ViewModel.IsEmpty.Value));
		ContextMenuEntities.Add(new ContextMenuCollectionEntity(UIStrings.Instance.UIBugReport.ClearButtonText, base.ViewModel.ClearSlot, !base.ViewModel.IsEmpty.Value));
	}

	public void SetFocus(bool value)
	{
		m_SlotConsoleView.SetFocus(value);
	}

	public bool IsValid()
	{
		return m_SlotConsoleView.IsValid();
	}

	public bool CanConfirmClick()
	{
		if (!m_SlotConsoleView.CanConfirmClick() && !base.ViewModel.MoveAbilityMode.Value)
		{
			return base.ViewModel.IsInCharScreen;
		}
		return true;
	}

	public void OnConfirmClick()
	{
		BoolReactiveProperty moveAbilityMode = base.ViewModel.MoveAbilityMode;
		if (moveAbilityMode != null && moveAbilityMode.Value)
		{
			EventBus.RaiseEvent(delegate(IActionBarPartAbilitiesHandler h)
			{
				h.SetMoveAbilityMode(on: false);
			});
		}
		else if (base.ViewModel.IsInCharScreen)
		{
			base.ViewModel.ChooseAbility();
		}
		else
		{
			m_SlotConsoleView.OnConfirmClick();
		}
	}

	public string GetConfirmClickHint()
	{
		return string.Empty;
	}

	public TooltipBaseTemplate TooltipTemplate()
	{
		return m_SlotConsoleView.TooltipTemplate();
	}
}
