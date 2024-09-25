using System;
using System.Collections.Generic;
using Owlcat.Runtime.UI.MVVM;

namespace Kingmaker.Visual.CharacterSystem.Dismemberment.UI;

public class DismembermentSetListVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable
{
	private List<DismembermentSetVM> m_Sets = new List<DismembermentSetVM>();

	public Action<DismembermentSetVM> SetSelected;

	public Action<DismembermentSetVM> SetRemoveCallback;

	public Action ResetSelectedSet;

	public DismembermentSetVM[] Sets => m_Sets.ToArray();

	public DismembermentSetListVM()
	{
		SetSelected = (Action<DismembermentSetVM>)Delegate.Combine(SetSelected, new Action<DismembermentSetVM>(OnSetSelected));
		SetRemoveCallback = (Action<DismembermentSetVM>)Delegate.Combine(SetRemoveCallback, new Action<DismembermentSetVM>(OnSetRemoved));
	}

	private void OnSetSelected(DismembermentSetVM set)
	{
	}

	private void OnSetRemoved(DismembermentSetVM set)
	{
		m_Sets.Remove(set);
	}

	public void AddSet(DismembermentSetVM set)
	{
		m_Sets.Add(set);
		set.SelectionCallback = SetSelected;
		set.RemoveCallback = SetRemoveCallback;
	}

	public void ClearSets()
	{
		ResetSelectedSet?.Invoke();
		m_Sets.Clear();
	}

	protected override void DisposeImplementation()
	{
	}
}
