using System;
using Kingmaker.Items;
using Owlcat.Runtime.UI.MVVM;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.VM.CounterWindow;

public class CounterWindowVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable
{
	public int MaxValue;

	public int CurrentValue;

	public string ItemName;

	public Sprite ItemIcon;

	public string ItemCount;

	public CounterWindowType OperationType;

	private readonly Action<int> m_OnAccept;

	private readonly Action m_OnDecline;

	public CounterWindowVM(CounterWindowType type, ItemEntity item, Action<int> onAccept, Action onDecline)
	{
		if (item != null)
		{
			ItemName = item.Name;
			ItemIcon = item.Icon;
			ItemCount = item.Count.ToString();
			OperationType = type;
			switch (OperationType)
			{
			case CounterWindowType.Split:
				MaxValue = item.Count - 1;
				CurrentValue = Mathf.Max(item.Count / 2, 1);
				break;
			case CounterWindowType.Drop:
			case CounterWindowType.Move:
				MaxValue = item.Count;
				CurrentValue = 1;
				break;
			}
			m_OnAccept = onAccept;
			m_OnDecline = onDecline;
		}
	}

	public void Accept()
	{
		m_OnAccept?.Invoke(CurrentValue);
		Close();
	}

	public void Close()
	{
		m_OnDecline?.Invoke();
	}

	protected override void DisposeImplementation()
	{
	}
}
