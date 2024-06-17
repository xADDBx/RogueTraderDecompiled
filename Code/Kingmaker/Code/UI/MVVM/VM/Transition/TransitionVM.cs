using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.GameCommands;
using Kingmaker.Globalmap.Blueprints;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.Common;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.Utility;

namespace Kingmaker.Code.UI.MVVM.VM.Transition;

public class TransitionVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable, IAreaTransitionHandler, ISubscriber, IScreenUIHandler
{
	public readonly BlueprintMultiEntrance.BlueprintMultiEntranceMap Map;

	public readonly string Name;

	public readonly List<TransitionEntryVM> EntryVms = new List<TransitionEntryVM>();

	public readonly AutoDisposingList<TransitionLegendButtonVM> ObjectsList = new AutoDisposingList<TransitionLegendButtonVM>();

	private readonly Action m_Close;

	public TransitionVM(BlueprintMultiEntrance entrance, Action close)
	{
		m_Close = close;
		Map = entrance.Map;
		Name = entrance.Name;
		entrance.Entries.ForEach(delegate(BlueprintMultiEntranceEntry e)
		{
			EntryVms.Add(new TransitionEntryVM(e, Close));
		});
		AddDisposable(EventBus.Subscribe(this));
	}

	public void AddObjectsInfo(List<Action> hoverAction, List<Action> unHoverAction)
	{
		ObjectsList.Clear();
		List<TransitionEntryVM> list = EntryVms.ToList();
		foreach (TransitionEntryVM item in list)
		{
			ObjectsList.Add(new TransitionLegendButtonVM(item, hoverAction[list.IndexOf(item)], unHoverAction[list.IndexOf(item)]));
		}
	}

	public void EnterLocation()
	{
		ObjectsList.FirstOrDefault((TransitionLegendButtonVM o) => o.IsHover.Value)?.OnClick();
	}

	protected override void DisposeImplementation()
	{
		EntryVms.ForEach(delegate(TransitionEntryVM vm)
		{
			vm.Dispose();
		});
		EntryVms.Clear();
		ObjectsList.Clear();
	}

	public void Close()
	{
		bool flag = Game.Instance.CurrentlyLoadedArea.IsShipArea || (Game.Instance.LoadedAreaState?.Settings.CapitalPartyMode).Value;
		if (!flag || UINetUtility.IsControlMainCharacterWithWarning())
		{
			Game.Instance.GameCommandQueue.CloseScreen(IScreenUIHandler.ScreenType.Transition, flag);
		}
	}

	void IScreenUIHandler.CloseScreen(IScreenUIHandler.ScreenType screen)
	{
		m_Close?.Invoke();
	}

	void IAreaTransitionHandler.HandleAreaTransition()
	{
		m_Close?.Invoke();
	}
}
