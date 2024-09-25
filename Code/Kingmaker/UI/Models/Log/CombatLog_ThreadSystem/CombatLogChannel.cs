using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.UI.Models.Log.Events;
using Owlcat.Runtime.UI.MVVM;
using UniRx;

namespace Kingmaker.UI.Models.Log.CombatLog_ThreadSystem;

public class CombatLogChannel : BaseDisposable
{
	public string ChannelName;

	public ReactiveCollection<CombatLogMessage> Messages;

	private readonly List<CombatLogMessage> m_MessagesQueue = new List<CombatLogMessage>();

	public CombatLogChannel(List<LogThreadBase> channelThreads, string channelName)
	{
		ChannelName = channelName;
		List<CombatLogMessage> list = channelThreads.SelectMany((LogThreadBase thread) => thread.AllMessages).ToList();
		for (int i = 0; i < list.Count - 1; i++)
		{
			CombatLogMessage combatLogMessage = list[i];
			if (combatLogMessage.IsSeparator && combatLogMessage.SeparatorState != GameLogEventAddSeparator.States.Break && list[i + 1].IsSeparator)
			{
				list.RemoveAt(i + 1);
				i--;
			}
		}
		Messages = new ReactiveCollection<CombatLogMessage>(list.OrderBy((CombatLogMessage z) => z.Received));
		foreach (LogThreadBase channelThread in channelThreads)
		{
			AddDisposable(channelThread.ObserveAdd().Subscribe(delegate(CollectionAddEvent<CombatLogMessage> z)
			{
				AddNewMessage(z.Value);
			}));
			AddDisposable(channelThread.ObserveRemove().Subscribe(delegate(CollectionRemoveEvent<CombatLogMessage> z)
			{
				RemoveMessage(z.Value);
			}));
		}
	}

	public void AddNewMessage(CombatLogMessage newMessage)
	{
		if (newMessage.IsSeparator)
		{
			switch (newMessage.SeparatorState)
			{
			case GameLogEventAddSeparator.States.Start:
				if (m_MessagesQueue.Count > 0)
				{
					SendQueueMessages();
				}
				m_MessagesQueue.Add(newMessage);
				break;
			case GameLogEventAddSeparator.States.Break:
				SendQueueMessages();
				break;
			case GameLogEventAddSeparator.States.Finish:
				if (m_MessagesQueue.Count == 1)
				{
					m_MessagesQueue.Clear();
				}
				else if (m_MessagesQueue.Count == 2)
				{
					Messages.Add(m_MessagesQueue[1]);
					m_MessagesQueue.Clear();
				}
				else
				{
					m_MessagesQueue.Add(newMessage);
					SendQueueMessages();
				}
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
		}
		else if (m_MessagesQueue.Count == 0)
		{
			Messages.Add(newMessage);
		}
		else
		{
			m_MessagesQueue.Add(newMessage);
		}
	}

	private void SendQueueMessages()
	{
		CombatLogMessage combatLogMessage = Messages.LastOrDefault();
		if (combatLogMessage != null && combatLogMessage.IsSeparator && m_MessagesQueue.Count > 0)
		{
			m_MessagesQueue[0] = null;
		}
		for (int j = 0; j < m_MessagesQueue.Count; j++)
		{
			combatLogMessage = m_MessagesQueue[j];
			if (combatLogMessage != null)
			{
				Messages.Add(combatLogMessage);
				m_MessagesQueue[j] = null;
			}
		}
		m_MessagesQueue.RemoveAll((CombatLogMessage i) => i == null);
	}

	private void RemoveMessage(CombatLogMessage message)
	{
		Messages.Remove(message);
	}

	protected override void DisposeImplementation()
	{
	}
}
