using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.UI.MVVM.VM.Overtips.CombatText;
using Owlcat.Runtime.UI.Utility;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.Overtips.CombatText;

[Serializable]
public abstract class CombatTextCreator<TCombatTextView, TCombatMessage> where TCombatTextView : CombatTextEntityBaseView<TCombatMessage> where TCombatMessage : CombatMessageBase
{
	public readonly List<CombatTextEntityBaseView<TCombatMessage>> ActiveViews = new List<CombatTextEntityBaseView<TCombatMessage>>();

	private Action m_CollectionUpdated;

	[SerializeField]
	public RectTransform ContainerRect;

	[SerializeField]
	public TCombatTextView m_PrefabView;

	public void SetCallback(Action collectionUpdated)
	{
		m_CollectionUpdated = collectionUpdated;
	}

	public TCombatTextView Create(TCombatMessage message)
	{
		return DoCreate(message);
	}

	protected virtual TCombatTextView DoCreate(TCombatMessage message)
	{
		TCombatTextView combatTextView = WidgetFactory.GetWidget(m_PrefabView);
		combatTextView.transform.SetParent(ContainerRect, worldPositionStays: false);
		combatTextView.SetData(message, delegate
		{
			DisposeCombatText(combatTextView);
		});
		ActiveViews.Add(combatTextView);
		return combatTextView;
	}

	private void DisposeCombatText(CombatTextEntityBaseView<TCombatMessage> combatText)
	{
		WidgetFactory.DisposeWidget(combatText);
		ActiveViews.Remove(combatText);
		m_CollectionUpdated?.Invoke();
		DoDisposeCombatText(combatText);
	}

	protected virtual void DoDisposeCombatText(CombatTextEntityBaseView<TCombatMessage> combatText)
	{
	}

	public void Clear()
	{
		DoClear();
	}

	protected virtual void DoClear()
	{
		ActiveViews.ToList().ForEach(DisposeCombatText);
		ActiveViews.Clear();
		m_CollectionUpdated?.Invoke();
	}
}
