using System;
using System.Collections.Generic;
using Kingmaker.Blueprints;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI;
using Kingmaker.UI.Common;
using Owlcat.Runtime.UI.MVVM;
using UniRx;

namespace Kingmaker.Code.UI.MVVM.VM.GroupChanger;

public class GroupChangerContextVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable, IGroupChangerHandler, ISubscriber, IDetachUnitsUIHandler
{
	public readonly ReactiveProperty<GroupChangerVM> GroupChangerVm = new ReactiveProperty<GroupChangerVM>();

	private readonly List<UnitReference> m_LastUnits = new List<UnitReference>();

	private readonly List<BlueprintUnit> m_RequiredUnits = new List<BlueprintUnit>();

	public GroupChangerContextVM()
	{
		AddDisposable(EventBus.Subscribe(this));
	}

	protected override void DisposeImplementation()
	{
		DisposeGroupChanger();
	}

	public void HandleCall(Action goAction, Action closeAction, bool isCapital, bool sameFinishActions = false, bool canCancel = true, bool showRemoteCompanion = false)
	{
		goAction = (Action)Delegate.Combine(goAction, new Action(DisposeGroupChanger));
		closeAction = (Action)Delegate.Combine(closeAction, new Action(DisposeGroupChanger));
		if (GroupChangerVm.Value != null)
		{
			GroupChangerVm.Value.AddActionGo(goAction);
			GroupChangerVm.Value.AddActionClose(closeAction);
		}
		else
		{
			GroupChangerVM disposable = (GroupChangerVm.Value = new GroupChangerCommonVM(goAction, closeAction, m_LastUnits, m_RequiredUnits, isCapital, sameFinishActions, canCancel, showRemoteCompanion));
			AddDisposable(disposable);
		}
		m_LastUnits.Clear();
		m_RequiredUnits.Clear();
	}

	public void HandleSetLastUnits(List<UnitReference> lastUnits)
	{
		m_LastUnits.AddRange(lastUnits);
	}

	public void HandleSetRequiredUnits(List<BlueprintUnit> requiredUnits)
	{
		m_RequiredUnits.AddRange(requiredUnits);
	}

	public void HandleDetachUnits(int maxUnitInParty, ActionList afterDetach)
	{
		if (UINetUtility.IsControlMainCharacter())
		{
			m_LastUnits.Clear();
			m_RequiredUnits.Clear();
			GroupChangerVM disposable = (GroupChangerVm.Value = new GroupChangerDetachVM(Go, null, m_LastUnits, m_RequiredUnits));
			AddDisposable(disposable);
		}
		void Go()
		{
			afterDetach?.Run();
			DisposeGroupChanger();
			UIAccess.SelectionManager.SelectAll();
		}
	}

	private void DisposeGroupChanger()
	{
		GroupChangerVm.Value?.Dispose();
		GroupChangerVm.Value = null;
	}
}
