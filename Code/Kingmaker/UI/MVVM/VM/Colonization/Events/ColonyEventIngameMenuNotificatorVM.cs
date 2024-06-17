using System;
using System.Collections.Generic;
using Kingmaker.Globalmap.Blueprints.Colonization;
using Kingmaker.Globalmap.Colonization;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.UI.MVVM;
using UniRx;
using UnityEngine;

namespace Kingmaker.UI.MVVM.VM.Colonization.Events;

public class ColonyEventIngameMenuNotificatorVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable, IColonizationEventHandler, ISubscriber
{
	public readonly ReactiveProperty<Sprite> Icon = new ReactiveProperty<Sprite>();

	public readonly ReactiveProperty<bool> HasEvent = new ReactiveProperty<bool>();

	private List<ColoniesState.ColonyData> Colonies => Game.Instance?.Player?.ColoniesState?.Colonies;

	public ColonyEventIngameMenuNotificatorVM()
	{
		UpdateNotificator();
		AddDisposable(EventBus.Subscribe(this));
	}

	protected override void DisposeImplementation()
	{
	}

	public void HandleEventStarted(Colony colony, BlueprintColonyEvent colonyEvent)
	{
		UpdateNotificator();
	}

	public void HandleEventFinished(Colony colony, BlueprintColonyEvent colonyEvent, BlueprintColonyEventResult result)
	{
		UpdateNotificator();
	}

	private void UpdateNotificator()
	{
		HasEvent.Value = false;
		if (Colonies == null)
		{
			return;
		}
		foreach (ColoniesState.ColonyData colony in Colonies)
		{
			BlueprintColonyEvent blueprintColonyEvent = colony?.Colony?.StartedEvents?.FirstOrDefault();
			if (blueprintColonyEvent != null)
			{
				Icon.Value = blueprintColonyEvent.Icon;
				HasEvent.Value = true;
				break;
			}
		}
	}
}
