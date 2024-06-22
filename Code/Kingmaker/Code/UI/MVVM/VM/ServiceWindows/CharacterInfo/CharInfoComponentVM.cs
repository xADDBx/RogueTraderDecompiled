using System;
using Kingmaker.Code.UI.MVVM.VM.Common.UnitState;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Levelup;
using Owlcat.Runtime.UI.MVVM;
using UniRx;

namespace Kingmaker.Code.UI.MVVM.VM.ServiceWindows.CharacterInfo;

public abstract class CharInfoComponentVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable, ILevelUpManagerUIHandler, ISubscriber
{
	public readonly IReadOnlyReactiveProperty<BaseUnitEntity> Unit;

	public MechanicEntityUIWrapper UnitUIWrapper;

	protected CharInfoComponentVM(IReadOnlyReactiveProperty<BaseUnitEntity> unit)
	{
		AddDisposable(EventBus.Subscribe(this));
		Unit = unit;
		AddDisposable(Unit?.Subscribe(delegate(BaseUnitEntity descriptor)
		{
			if (descriptor != null)
			{
				UnitUIWrapper = new MechanicEntityUIWrapper(descriptor);
				RefreshData();
			}
		}));
	}

	protected override void DisposeImplementation()
	{
	}

	protected virtual void RefreshData()
	{
	}

	public void HandleCreateLevelUpManager(LevelUpManager manager)
	{
	}

	public void HandleDestroyLevelUpManager()
	{
	}

	public void HandleUISelectCareerPath()
	{
	}

	public virtual void HandleUICommitChanges()
	{
		RefreshData();
	}

	public void HandleUISelectionChanged()
	{
	}
}
