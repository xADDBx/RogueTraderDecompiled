using System.Collections;
using Code.Package.Runtime.Extensions.Dependencies;
using UnityEngine;

namespace Owlcat.Runtime.UniRx;

public class DelayedInvokerCoroutineRunner : MonoBehaviour
{
	private static DelayedInvokerCoroutineRunner s_Instance;

	private static DelayedInvokerCoroutineRunner Instance
	{
		get
		{
			if (s_Instance == null)
			{
				s_Instance = (DelayedInvokerCoroutineRunner)Object.FindObjectOfType(typeof(DelayedInvokerCoroutineRunner));
				if (Object.FindObjectsOfType(typeof(DelayedInvokerCoroutineRunner)).Length > 1)
				{
					UniRxLogger.Error("[MonoSingleton] Something went really wrong  - there should never be more than 1 singleton of type " + typeof(DelayedInvokerCoroutineRunner).ToString() + "! Reopening the scene might fix it.");
					return s_Instance;
				}
				if (s_Instance == null)
				{
					UniRxLogger.Log("[MonoSingleton] There is no object of type " + typeof(DelayedInvokerCoroutineRunner).ToString() + ". Creating new.");
					s_Instance = new GameObject(typeof(DelayedInvokerCoroutineRunner).ToString()).AddComponent<DelayedInvokerCoroutineRunner>();
					Object.DontDestroyOnLoad(s_Instance);
				}
			}
			return s_Instance;
		}
	}

	public static Coroutine Start(IEnumerator coroutine)
	{
		return Instance.StartCoroutine(coroutine);
	}

	public static void Stop(Coroutine coroutine)
	{
		Instance.StopCoroutine(coroutine);
	}
}
