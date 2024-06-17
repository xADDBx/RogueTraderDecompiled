using System;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Templates;
using Kingmaker.GameCommands;
using Kingmaker.Globalmap.Blueprints.Colonization;
using Kingmaker.Globalmap.Colonization;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.Models.Tooltip.Base;
using Kingmaker.UI.Sound;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.Tooltips;
using UniRx;
using UnityEngine;

namespace Kingmaker.UI.MVVM.VM.Colonization.Events;

public class ColonyEventVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable
{
	public readonly ReactiveProperty<string> Name = new ReactiveProperty<string>();

	public readonly ReactiveProperty<Sprite> Icon = new ReactiveProperty<Sprite>();

	public readonly ReactiveProperty<bool> IsColonyManagement = new ReactiveProperty<bool>();

	public ReactiveProperty<TooltipBaseTemplate> Tooltip = new ReactiveProperty<TooltipBaseTemplate>();

	private Colony m_Colony;

	private BlueprintColonyEvent m_ColonyEvent;

	public ColonyEventVM(Colony colony, BlueprintColonyEvent colonyEvent, bool isColonyManagement)
		: this(colonyEvent, isColonyManagement)
	{
		m_Colony = colony;
		m_ColonyEvent = colonyEvent;
	}

	protected ColonyEventVM(IUIDataProvider colonyEvent, bool isColonyManagement)
	{
		AddDisposable(EventBus.Subscribe(this));
		Name.Value = colonyEvent.Name;
		Icon.Value = colonyEvent.Icon;
		IsColonyManagement.Value = isColonyManagement;
		Tooltip.Value = new TooltipTemplateColonyEvent(colonyEvent.Name, colonyEvent.Description, isColonyManagement);
	}

	protected override void DisposeImplementation()
	{
	}

	public void HandleColonyEvent()
	{
		HandleColonyEventImpl();
	}

	protected virtual void HandleColonyEventImpl()
	{
		if (IsColonyManagement.Value)
		{
			EventBus.RaiseEvent(delegate(IWarningNotificationUIHandler h)
			{
				h.HandleWarning(UIStrings.Instance.ColonyEventsTexts.NeedsVisitWarningMessage);
			});
		}
		else if (m_Colony != null && m_ColonyEvent != null)
		{
			UISounds.Instance.Sounds.SpaceColonization.ColonyEvent.Play();
			Game.Instance.GameCommandQueue.StartColonyEvent(m_Colony.Blueprint, m_ColonyEvent);
		}
	}
}
