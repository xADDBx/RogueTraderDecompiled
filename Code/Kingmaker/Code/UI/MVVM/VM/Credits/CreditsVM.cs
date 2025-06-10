using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Credits;
using Kingmaker.Code.UI.MVVM.View.Credits;
using Kingmaker.Code.UI.MVVM.VM.Slots;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.SelectionGroup;
using UniRx;

namespace Kingmaker.Code.UI.MVVM.VM.Credits;

public class CreditsVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable
{
	public readonly List<BlueprintCreditsGroup> Groups;

	public readonly ReactiveProperty<bool> Pause = new ReactiveProperty<bool>(initialValue: false);

	public readonly SelectionGroupRadioVM<CreditsMenuEntityVM> SelectionGroup;

	public readonly LensSelectorVM Selector;

	public readonly ReactiveCommand<BlueprintCreditsGroup> OnSelectGroup = new ReactiveCommand<BlueprintCreditsGroup>();

	private readonly List<CreditsMenuEntityVM> m_MenuEntitiesList;

	private readonly ReactiveProperty<CreditsMenuEntityVM> m_SelectedMenuEntity = new ReactiveProperty<CreditsMenuEntityVM>();

	private readonly Action m_CloseAction;

	public readonly BoolReactiveProperty InputFieldHasAnySymbol = new BoolReactiveProperty();

	private PageGenerator m_Generator = new PageGenerator();

	public int SelectedMenuIndex => m_MenuEntitiesList.IndexOf(m_SelectedMenuEntity.Value);

	public CreditsVM(Action closeAction, bool onlyBakers = false)
	{
		CreditsVM creditsVM = this;
		m_CloseAction = closeAction;
		Groups = Game.Instance.BlueprintRoot.UIConfig.Credits.Groups.Select((BlueprintCreditsGroupReference g) => g.Get()).ToList();
		Groups = Groups.Where(delegate(BlueprintCreditsGroup g)
		{
			if (!onlyBakers)
			{
				if (g.IsBakers)
				{
					return g.ShowInMainMenuCredits;
				}
				return true;
			}
			return g.IsBakers && g.ShowInGameCredits;
		}).ToList();
		m_MenuEntitiesList = Groups.Select((BlueprintCreditsGroup g) => new CreditsMenuEntityVM(g.PageIcon, g.HeaderText, delegate
		{
			creditsVM.OnSelectGroup?.Execute(g);
		})).ToList();
		AddDisposable(SelectionGroup = new SelectionGroupRadioVM<CreditsMenuEntityVM>(m_MenuEntitiesList, m_SelectedMenuEntity));
		AddDisposable(Selector = new LensSelectorVM());
		m_SelectedMenuEntity.Value = m_MenuEntitiesList.FirstOrDefault();
	}

	protected override void DisposeImplementation()
	{
		m_Generator = null;
		OnSelectGroup.Dispose();
		m_SelectedMenuEntity.Dispose();
		Groups.Clear();
		m_MenuEntitiesList.Clear();
	}

	public void SetSelectedGroup(BlueprintCreditsGroup group)
	{
		m_SelectedMenuEntity.Value = m_MenuEntitiesList.ElementAtOrDefault(Groups.IndexOf(group));
	}

	public void SetPauseState(bool state)
	{
		Pause.Value = state;
	}

	public void TogglePause()
	{
		Pause.Value = !Pause.Value;
	}

	public void CloseCredits()
	{
		m_CloseAction?.Invoke();
	}

	public void CheckInputFieldAnySymbols(string str)
	{
		InputFieldHasAnySymbol.Value = !string.IsNullOrWhiteSpace(str);
	}
}
