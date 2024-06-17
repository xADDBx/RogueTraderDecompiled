using System;
using System.Collections.Generic;
using Kingmaker.UI.Models.UnitSettings;
using Owlcat.Runtime.UI.MVVM;

namespace Kingmaker.Code.UI.MVVM.VM.ActionBar;

public class ActionBarConvertedVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable
{
	public readonly List<ActionBarSlotVM> Slots = new List<ActionBarSlotVM>();

	private readonly Action m_OnClose;

	public ActionBarConvertedVM(List<MechanicActionBarSlotSpontaneusConvertedSpell> list, Action onClose)
	{
		m_OnClose = onClose;
		for (int i = 0; i < list.Count; i++)
		{
			Slots.Add(new ActionBarSlotVM(list[i], i));
		}
	}

	protected override void DisposeImplementation()
	{
		Slots.ForEach(delegate(ActionBarSlotVM s)
		{
			s.Dispose();
		});
		Slots.Clear();
	}

	public void Close()
	{
		m_OnClose?.Invoke();
	}
}
