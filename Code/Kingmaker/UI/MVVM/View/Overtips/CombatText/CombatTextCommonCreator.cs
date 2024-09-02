using System;
using System.Collections.Generic;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.MVVM.VM.Overtips.CombatText;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.Overtips.CombatText;

[Serializable]
public class CombatTextCommonCreator : CombatTextCreator<CombatTextCommonView, CombatMessageBase>, ITurnStartHandler, ISubscriber<IMechanicEntity>, ISubscriber
{
	[SerializeField]
	public bool m_HideDuplicateMessages;

	private const int Capacity = 4;

	private readonly List<string> m_PreviousCombatTexts = new List<string>(5);

	private IDisposable m_EventBusDispose;

	private MechanicEntity m_MechanicEntity;

	public void Initialize(MechanicEntity mechanicEntity)
	{
		if (m_HideDuplicateMessages)
		{
			m_MechanicEntity = mechanicEntity;
			m_EventBusDispose = EventBus.Subscribe(this);
		}
	}

	public void Dispose()
	{
		if (m_HideDuplicateMessages)
		{
			m_MechanicEntity = null;
			m_EventBusDispose?.Dispose();
			m_EventBusDispose = null;
		}
	}

	protected override CombatTextCommonView DoCreate(CombatMessageBase message)
	{
		if (m_HideDuplicateMessages)
		{
			string text = message.GetText();
			if (m_PreviousCombatTexts.Contains(text))
			{
				return null;
			}
			m_PreviousCombatTexts.Add(text);
			if (m_PreviousCombatTexts.Count > 4)
			{
				m_PreviousCombatTexts.RemoveAt(0);
			}
		}
		return base.DoCreate(message);
	}

	protected override void DoDisposeCombatText(CombatTextEntityBaseView<CombatMessageBase> combatText)
	{
		string text = combatText.GetText();
		if (m_PreviousCombatTexts.Contains(text))
		{
			m_PreviousCombatTexts.Remove(text);
		}
	}

	protected override void DoClear()
	{
		if (m_HideDuplicateMessages)
		{
			m_PreviousCombatTexts.Clear();
		}
		base.DoClear();
	}

	public void HandleUnitStartTurn(bool isTurnBased)
	{
		if (EventInvokerExtensions.MechanicEntity == m_MechanicEntity && m_HideDuplicateMessages)
		{
			m_PreviousCombatTexts.Clear();
		}
	}
}
