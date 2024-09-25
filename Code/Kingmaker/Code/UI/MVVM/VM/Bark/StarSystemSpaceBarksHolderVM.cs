using System;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.GameModes;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.Utility;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.VM.Bark;

public class StarSystemSpaceBarksHolderVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable, IBarkHandler, ISubscriber<IEntity>, ISubscriber, IGameModeHandler
{
	public readonly AutoDisposingReactiveCollection<StarSystemSpaceBarkVM> BarksVMs = new AutoDisposingReactiveCollection<StarSystemSpaceBarkVM>();

	public readonly ReactiveProperty<bool> IsVisible = new ReactiveProperty<bool>(initialValue: false);

	private int m_LastBarkIndex;

	public StarSystemSpaceBarksHolderVM()
	{
		AddDisposable(EventBus.Subscribe(this));
	}

	protected override void DisposeImplementation()
	{
		Clear();
	}

	public void HandleOnShowBark(string text)
	{
		IsVisible.Value = true;
		if (EventInvokerExtensions.Entity is BaseUnitEntity baseUnitEntity)
		{
			AddBarkVM(baseUnitEntity, text, null);
		}
	}

	public void HandleOnShowBarkWithName(string text, string name, Color nameColor)
	{
	}

	public void HandleOnShowLinkedBark(string text, string encyclopediaLink)
	{
		IsVisible.Value = true;
		if (EventInvokerExtensions.Entity is BaseUnitEntity baseUnitEntity)
		{
			AddBarkVM(baseUnitEntity, text, encyclopediaLink);
		}
	}

	public void HandleOnHideBark()
	{
		if (m_LastBarkIndex < BarksVMs.Count)
		{
			BarksVMs[m_LastBarkIndex].HideBark();
			m_LastBarkIndex++;
		}
	}

	private void Clear()
	{
		BarksVMs.Clear();
		m_LastBarkIndex = 0;
	}

	private void AddBarkVM(BaseUnitEntity baseUnitEntity, string text, string encyclopediaLink)
	{
		StarSystemSpaceBarkVM starSystemSpaceBarkVM = new StarSystemSpaceBarkVM(baseUnitEntity, text, encyclopediaLink);
		AddDisposable(starSystemSpaceBarkVM);
		BarksVMs.Add(starSystemSpaceBarkVM);
	}

	public void OnGameModeStart(GameModeType gameMode)
	{
		IsVisible.Value = false;
	}

	public void OnGameModeStop(GameModeType gameMode)
	{
	}
}
