using System;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.Utility;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.VM.Bark;

public class SpaceBarksHolderVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable, IBarkHandler, ISubscriber<IEntity>, ISubscriber
{
	public readonly AutoDisposingReactiveCollection<SpaceBarkVM> BarksVMs = new AutoDisposingReactiveCollection<SpaceBarkVM>();

	public readonly ReactiveCommand ClearBarks = new ReactiveCommand();

	private int m_LastBarkIndex;

	private bool m_IsHidden;

	public SpaceBarksHolderVM()
	{
		AddDisposable(EventBus.Subscribe(this));
	}

	protected override void DisposeImplementation()
	{
		Clear();
	}

	public void HandleOnShowBark(string text)
	{
		if (!m_IsHidden && EventInvokerExtensions.Entity is BaseUnitEntity baseUnitEntity)
		{
			AddBarkVM(baseUnitEntity, text);
		}
	}

	public void HandleOnShowBarkWithName(string text, string name, Color nameColor)
	{
	}

	public void HandleOnShowLinkedBark(string text, string encyclopediaLink)
	{
	}

	public void HandleOnHideBark()
	{
		if (!m_IsHidden && m_LastBarkIndex < BarksVMs.Count)
		{
			BarksVMs[m_LastBarkIndex].HideBark();
			m_LastBarkIndex++;
		}
	}

	public void Show()
	{
		m_IsHidden = false;
	}

	public void Hide()
	{
		Clear();
		m_IsHidden = true;
	}

	private void Clear()
	{
		BarksVMs.Clear();
		m_LastBarkIndex = 0;
		ClearBarks.Execute();
	}

	private void AddBarkVM(BaseUnitEntity baseUnitEntity, string text)
	{
		SpaceBarkVM spaceBarkVM = new SpaceBarkVM(baseUnitEntity, text);
		AddDisposable(spaceBarkVM);
		BarksVMs.Add(spaceBarkVM);
	}
}
