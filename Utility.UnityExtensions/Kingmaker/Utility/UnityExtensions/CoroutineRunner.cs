using System.Collections;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Utility.UnityExtensions;

public class CoroutineRunner : MonoSingleton<CoroutineRunner>
{
	public static Coroutine Start(IEnumerator coroutine)
	{
		return MonoSingleton<CoroutineRunner>.Instance.StartCoroutine(coroutine);
	}

	public static void Stop(Coroutine coroutine)
	{
		MonoSingleton<CoroutineRunner>.Instance.StopCoroutine(coroutine);
	}
}
