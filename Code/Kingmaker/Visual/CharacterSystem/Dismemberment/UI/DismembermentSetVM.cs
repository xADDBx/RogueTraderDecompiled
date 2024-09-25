using System;
using Owlcat.Runtime.UI.MVVM;

namespace Kingmaker.Visual.CharacterSystem.Dismemberment.UI;

public class DismembermentSetVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable
{
	public UnitDismembermentManager.DismembermentSet Set;

	public Action<DismembermentSetVM> SelectionCallback;

	public Action<DismembermentSetVM> RemoveCallback;

	public DismembermentSetVM(UnitDismembermentManager.DismembermentSet set)
	{
		Set = set;
	}

	public void OnSelectedChanged(bool value)
	{
		if (value)
		{
			SelectionCallback?.Invoke(this);
		}
	}

	public void OnRemove()
	{
		RemoveCallback?.Invoke(this);
	}

	protected override void DisposeImplementation()
	{
	}

	internal void OnTypeChanged(int value)
	{
		Set.Type = (DismembermentLimbsApartType)value;
	}
}
