using System;
using System.Collections.Generic;
using Code.Package.Runtime.Extensions.Dependencies;
using UnityEngine;

namespace Owlcat.Runtime.UniRx;

public class LastValueOnLateUpdateObservableFabric : MonoBehaviour
{
	private readonly LinkedList<ICanLateUpdate> m_ObservablesToLateUpdate = new LinkedList<ICanLateUpdate>();

	private static LastValueOnLateUpdateObservableFabric s_Instance;

	internal static LastValueOnLateUpdateObservableFabric Instance
	{
		get
		{
			if (s_Instance == null)
			{
				s_Instance = (LastValueOnLateUpdateObservableFabric)UnityEngine.Object.FindObjectOfType(typeof(LastValueOnLateUpdateObservableFabric));
				if (UnityEngine.Object.FindObjectsOfType(typeof(LastValueOnLateUpdateObservableFabric)).Length > 1)
				{
					UniRxLogger.Error("[MonoSingleton] Something went really wrong  - there should never be more than 1 singleton of type " + typeof(LastValueOnLateUpdateObservableFabric).ToString() + "! Reopening the scene might fix it.");
					return s_Instance;
				}
				if (s_Instance == null)
				{
					UniRxLogger.Log("[MonoSingleton] There is no object of type " + typeof(LastValueOnLateUpdateObservableFabric).ToString() + ". Creating new.");
					s_Instance = new GameObject(typeof(LastValueOnLateUpdateObservableFabric).ToString()).AddComponent<LastValueOnLateUpdateObservableFabric>();
					UnityEngine.Object.DontDestroyOnLoad(s_Instance);
				}
			}
			return s_Instance;
		}
	}

	public static IObservable<TSource> CreateObservable<TSource>(IObservable<TSource> source)
	{
		return new LastValueOnLateUpdateObservable<TSource>(source);
	}

	private void LateUpdate()
	{
		LinkedListNode<ICanLateUpdate> linkedListNode = m_ObservablesToLateUpdate.First;
		while (linkedListNode != null)
		{
			LinkedListNode<ICanLateUpdate> next = linkedListNode.Next;
			linkedListNode.Value.OnLateUpdate();
			linkedListNode = next;
		}
	}

	internal void Remove(LinkedListNode<ICanLateUpdate> item)
	{
		m_ObservablesToLateUpdate.Remove(item);
	}

	internal void Add(LinkedListNode<ICanLateUpdate> item)
	{
		m_ObservablesToLateUpdate.AddLast(item);
	}
}
