using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Blueprints.Root.Strings.GameLog;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.Models.Log.CombatLog_ThreadSystem;
using Kingmaker.UI.Models.Log.Events;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.UI.MVVM;
using UniRx;
using UnityEngine;

namespace Kingmaker.UI.MVVM.VM.CombatLog;

public class CombatLogVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable, ICombatLogChangeStateHandler, ISubscriber, ICombatLogForceDeactivateControlsHandler, IUnitGetAbilityPush
{
	public class PushAction
	{
		public int DistanceInCells;

		public MechanicEntity Caster;

		public MechanicEntity Target;

		public Vector3 FromPoint;

		public PushAction(int distanceInCells, MechanicEntity caster, MechanicEntity target, Vector3 fromPoint)
		{
			DistanceInCells = distanceInCells;
			Caster = caster;
			Target = target;
		}
	}

	private readonly List<CombatLogChannel> m_Channels = new List<CombatLogChannel>();

	private readonly ReactiveProperty<CombatLogChannel> m_CurrentChannel = new ReactiveProperty<CombatLogChannel>();

	public readonly ReactiveCollection<CombatLogBaseVM> Items = new ReactiveCollection<CombatLogBaseVM>();

	private readonly Dictionary<CombatLogChannel, CombatLogMessage> m_LastVisibleMessageInChannel = new Dictionary<CombatLogChannel, CombatLogMessage>();

	private IDisposable m_AddSubscription;

	private IDisposable m_RemoveSubscription;

	private CombatLogChannel m_CommonChannel;

	private CombatLogChannel m_CombatChannel;

	private PushAction m_PushAction;

	protected readonly ReactiveProperty<bool> m_IsActive = new ReactiveProperty<bool>();

	protected readonly ReactiveProperty<bool> m_IsControlActive = new ReactiveProperty<bool>();

	public readonly ReactiveProperty<int> CurrentSizeIndex = new ReactiveProperty<int>(Game.Instance.Player.UISettings.CombatLogSizeIndex);

	private static LogThreadService LogThreadService => LogThreadService.Instance;

	public IReadOnlyReactiveProperty<CombatLogChannel> CurrentChannel => m_CurrentChannel;

	public IReactiveProperty<bool> IsActive => m_IsActive;

	public IReactiveProperty<bool> IsControlActive => m_IsControlActive;

	public CombatLogVM(CombatLogChannelsType createType = CombatLogChannelsType.InGame)
	{
		if (createType == CombatLogChannelsType.InGame)
		{
			CreateInGameChannels();
		}
		AddDisposable(EventBus.Subscribe(this));
	}

	protected override void DisposeImplementation()
	{
		m_AddSubscription?.Dispose();
		m_RemoveSubscription?.Dispose();
		foreach (CombatLogChannel channel in m_Channels)
		{
			channel.Dispose();
		}
		m_Channels.Clear();
		Items.Dispose();
	}

	private void CreateInGameChannels()
	{
		m_CommonChannel = new CombatLogChannel(LogThreadService.GetThreadsByChannelType(LogChannelType.Common, LogChannelType.Dialog, LogChannelType.LifeEvents, LogChannelType.DialogAndLife, LogChannelType.AnyCombat, LogChannelType.InGameCombat), UIStrings.Instance.InventoryScreen.FilterTextAll);
		m_Channels.Add(m_CommonChannel);
		m_Channels.Add(new CombatLogChannel(LogThreadService.GetThreadsByChannelType(LogChannelType.LifeEvents, LogChannelType.DialogAndLife), UIStrings.Instance.CombatTexts.CombatLogEventsFilter));
		m_Channels.Add(new CombatLogChannel(LogThreadService.GetThreadsByChannelType(LogChannelType.Dialog, LogChannelType.DialogAndLife), UIStrings.Instance.CombatTexts.CombatLogDialogueFilter));
		m_CombatChannel = new CombatLogChannel(LogThreadService.GetThreadsByChannelType(LogChannelType.AnyCombat, LogChannelType.InGameCombat), UIStrings.Instance.CombatTexts.CombatLogCombatFilter);
		m_Channels.Add(m_CombatChannel);
		SetCurrentChannel(m_CommonChannel);
	}

	public string GetChannelName(int id)
	{
		if (id <= m_Channels.Count)
		{
			return m_Channels[id].ChannelName;
		}
		return string.Empty;
	}

	private void SetCurrentChannel(CombatLogChannel channel)
	{
		if (m_CurrentChannel.Value == channel)
		{
			return;
		}
		if (m_CurrentChannel.Value != null)
		{
			CombatLogMessage combatLogMessage = Items.LastOrDefault((CombatLogBaseVM vm) => vm.HasView)?.Message;
			if (combatLogMessage != null)
			{
				m_LastVisibleMessageInChannel[m_CurrentChannel.Value] = combatLogMessage;
			}
		}
		Items.Clear();
		foreach (CombatLogMessage message in channel.Messages)
		{
			AddNewMessage(message);
		}
		m_CurrentChannel.Value = channel;
		m_AddSubscription?.Dispose();
		m_RemoveSubscription?.Dispose();
		m_AddSubscription = m_CurrentChannel.Value.Messages.ObserveAdd().Subscribe(delegate(CollectionAddEvent<CombatLogMessage> z)
		{
			AddNewMessage(z.Value);
		});
		m_RemoveSubscription = m_CurrentChannel.Value.Messages.ObserveRemove().Subscribe(delegate(CollectionRemoveEvent<CombatLogMessage> z)
		{
			RemoveNewMessage(z.Value);
		});
	}

	public void SetCurrentChannelById(int id)
	{
		if (id <= m_Channels.Count)
		{
			SetCurrentChannel(m_Channels[id]);
		}
	}

	[CanBeNull]
	public CombatLogBaseVM GetLastVisibleItemForChannel(CombatLogChannel channel)
	{
		if (m_LastVisibleMessageInChannel.ContainsKey(channel))
		{
			return Items.FindOrDefault((CombatLogBaseVM item) => item.Message == m_LastVisibleMessageInChannel[channel]);
		}
		return null;
	}

	private void AddNewMessage(CombatLogMessage message)
	{
		if (message.IsSeparator && message.SeparatorState != GameLogEventAddSeparator.States.Break)
		{
			Items.Add(new CombatLogSeparatorVM(message));
			return;
		}
		if (m_PushAction != null && message.IsPerformAttackMessage)
		{
			m_PushAction = null;
			message.ReplaceMessage(message.Message + GameLogStrings.Instance.TooltipBrickStrings.TriggersPush.Text);
		}
		Items.Add(new CombatLogItemVM(message));
	}

	private void RemoveNewMessage(CombatLogMessage message)
	{
		Items.Remove(Items.FirstOrDefault((CombatLogBaseVM z) => z.Message == message));
	}

	public void HandleUnitResultPush(int distanceInCells, MechanicEntity caster, MechanicEntity target, Vector3 fromPoint)
	{
		m_PushAction = new PushAction(distanceInCells, caster, target, fromPoint);
	}

	public void HandleUnitAbilityPushDidActed(int distanceInCells)
	{
	}

	public void HandleUnitResultPush(int distanceInCells, Vector3 targetPoint, MechanicEntity target, MechanicEntity caster)
	{
	}

	public void CombatLogChangeState()
	{
		if (m_IsControlActive.Value)
		{
			DeactivateControls();
			return;
		}
		if (!m_IsActive.Value)
		{
			Activate();
		}
		ActivateControls();
	}

	public void HandleCombatLogForceDeactivateControls()
	{
		DeactivateControls();
	}

	public void Activate()
	{
		m_IsActive.Value = true;
	}

	public void Deactivate()
	{
		if (m_IsControlActive.Value)
		{
			DeactivateControls();
		}
		m_IsActive.Value = false;
	}

	private void ActivateControls()
	{
		if (!m_IsControlActive.Value)
		{
			m_IsControlActive.Value = true;
		}
	}

	private void DeactivateControls()
	{
		if (m_IsControlActive.Value)
		{
			m_IsControlActive.Value = false;
		}
	}
}
