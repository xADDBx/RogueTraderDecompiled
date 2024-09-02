using System;
using System.Collections.Generic;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.View.ServiceWindows.CharacterInfo;
using Kingmaker.Code.UI.MVVM.VM.MessageBox;
using Kingmaker.Code.UI.MVVM.VM.SystemMap;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.SelectionGroup;
using Owlcat.Runtime.UniRx;
using UniRx;

namespace Kingmaker.Code.UI.MVVM.VM.Retrain;

public class RespecVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable
{
	public readonly IntReactiveProperty RespecCost = new IntReactiveProperty();

	public readonly BoolReactiveProperty CanRespec = new BoolReactiveProperty();

	public readonly SelectionGroupRadioVM<RespecCharacterVM> CharacterSelectionGroupRadioVM;

	public readonly ReactiveProperty<RespecCharacterVM> CurrentCharacterVM = new ReactiveProperty<RespecCharacterVM>();

	public readonly SystemMapSpaceResourcesVM SystemMapSpaceResourcesVM;

	private readonly List<RespecCharacterVM> m_Characters = new List<RespecCharacterVM>();

	private readonly Action<BaseUnitEntity> m_SuccessAction;

	private readonly Action m_CloseAction;

	public RespecVM(List<BaseUnitEntity> characters, Action<BaseUnitEntity> successAction, Action closeAction)
	{
		m_SuccessAction = successAction;
		m_CloseAction = closeAction;
		foreach (BaseUnitEntity character in characters)
		{
			if (!(character is StarshipEntity))
			{
				m_Characters.Add(AddDisposableAndReturn(new RespecCharacterVM(character)));
			}
		}
		CharacterSelectionGroupRadioVM = AddDisposableAndReturn(new SelectionGroupRadioVM<RespecCharacterVM>(m_Characters, CurrentCharacterVM));
		CharacterSelectionGroupRadioVM.TrySelectFirstValidEntity();
		AddDisposable(SystemMapSpaceResourcesVM = new SystemMapSpaceResourcesVM());
		AddDisposable(CurrentCharacterVM.Subscribe(delegate(RespecCharacterVM ch)
		{
			RespecCost.Value = ch.Unit.Progression.GetRespecCost();
		}));
		AddDisposable(RespecCost.Subscribe(delegate(int cost)
		{
			SystemMapSpaceResourcesVM.SetAdditionalProfitFactor(-1 * cost);
			CanRespec.Value = (float)cost <= Game.Instance.Player.ProfitFactor.Total;
		}));
		AddDisposable(CanRespec.Subscribe(delegate(bool value)
		{
			SystemMapSpaceResourcesVM.JournalOrderProfitFactorVM.IsNegative.Value = !value;
		}));
		AddDisposable(EventBus.Subscribe(this));
	}

	protected override void DisposeImplementation()
	{
	}

	public void OnConfirm()
	{
		BaseUnitEntity unit = CurrentCharacterVM.Value?.Unit;
		if (unit == null)
		{
			return;
		}
		EventBus.RaiseEvent(delegate(IDialogMessageBoxUIHandler h)
		{
			h.HandleOpen(UIStrings.Instance.CharGen.RespecSelectCharacter, DialogMessageBoxBase.BoxType.Dialog, delegate(DialogMessageBoxBase.BoxButton button)
			{
				if (button == DialogMessageBoxBase.BoxButton.Yes)
				{
					m_SuccessAction(unit);
					DelayedInvoker.InvokeInFrames(delegate
					{
						OnClose();
						OnAfterSuccess();
					}, 1);
				}
			});
		});
	}

	public void OnClose()
	{
		m_CloseAction();
	}

	private void OnAfterSuccess()
	{
		Game.Instance.Player.UISettings.SavedUnitProgressionWindowData.CareerPath = null;
		EventBus.RaiseEvent(delegate(INewServiceWindowUIHandler h)
		{
			h.HandleOpenCharacterInfoPage(CharInfoPageType.LevelProgression, CurrentCharacterVM.Value?.Unit);
		});
	}
}
