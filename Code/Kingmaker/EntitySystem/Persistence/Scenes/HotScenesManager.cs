using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.Utility.CodeTimer;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.Core.Logging;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Kingmaker.EntitySystem.Persistence.Scenes;

public static class HotScenesManager
{
	private class DeactivatedSceneEntry
	{
		[NotNull]
		public readonly List<GameObject> DeactivatedRootObjects = new List<GameObject>();
	}

	private static readonly LogChannel Logger = LogChannelFactory.GetOrCreate("System Loading");

	[NotNull]
	private static readonly Dictionary<string, DeactivatedSceneEntry> s_DeactivatedScenes = new Dictionary<string, DeactivatedSceneEntry>();

	public static IEnumerable<string> DeactivatedScenes => s_DeactivatedScenes.Keys;

	public static bool IsSceneDeactivated(string sceneName)
	{
		return s_DeactivatedScenes.ContainsKey(sceneName);
	}

	public static void DeactivateScene(string sceneName)
	{
		if (s_DeactivatedScenes.ContainsKey(sceneName))
		{
			PFLog.Default.Error("HotScenesManager: " + sceneName + " is already deactivated");
			return;
		}
		Scene sceneByName = SceneManager.GetSceneByName(sceneName);
		if (!sceneByName.IsValid())
		{
			PFLog.Default.Error("HotScenesManager: " + sceneName + " is not loaded");
			return;
		}
		using (CodeTimer.New(Logger, "HotScenesManager: Deactivating scene " + sceneName))
		{
			DeactivatedSceneEntry deactivatedSceneEntry = new DeactivatedSceneEntry();
			s_DeactivatedScenes[sceneName] = deactivatedSceneEntry;
			List<GameObject> list = ListPool<GameObject>.Claim();
			sceneByName.GetRootGameObjects(list);
			foreach (GameObject item in list)
			{
				if (item.activeSelf)
				{
					try
					{
						item.SetActive(value: false);
					}
					catch (Exception ex)
					{
						PFLog.Default.Exception(ex);
					}
					deactivatedSceneEntry.DeactivatedRootObjects.Add(item);
				}
			}
			ListPool<GameObject>.Release(list);
		}
	}

	public static void ActivateScene(string sceneName)
	{
		DeactivatedSceneEntry deactivatedSceneEntry = s_DeactivatedScenes.Get(sceneName);
		if (deactivatedSceneEntry == null)
		{
			PFLog.Default.Error("HotScenesManager: " + sceneName + " was not deactivated");
			return;
		}
		using (CodeTimer.New(Logger, "HotScenesManager: Activating scene " + sceneName))
		{
			s_DeactivatedScenes.Remove(sceneName);
			foreach (GameObject deactivatedRootObject in deactivatedSceneEntry.DeactivatedRootObjects)
			{
				try
				{
					deactivatedRootObject.SetActive(value: true);
				}
				catch (Exception ex)
				{
					PFLog.Default.Exception(ex);
				}
			}
		}
	}

	public static void RemoveScene(string sceneName)
	{
		if (s_DeactivatedScenes.Remove(sceneName))
		{
			PFLog.System.Log("HotScenesManager: Scene " + sceneName + " forgotten");
		}
	}

	public static void ClearScenes(string sceneName)
	{
		s_DeactivatedScenes.Clear();
	}

	private static void CheckForSubscriptions(string sceneName, GameObject rootObject)
	{
		Queue<GameObject> queue = new Queue<GameObject>();
		queue.Enqueue(rootObject);
		while (queue.Count > 0)
		{
			GameObject gameObject = queue.Dequeue();
			Component[] components = gameObject.GetComponents<Component>();
			foreach (Component component in components)
			{
				if (EventBus.IsGloballySubscribed(component))
				{
					PFLog.Default.Error("Component " + component.GetType().Name + " of " + gameObject.name + " in scene " + sceneName + " is still subscribed in EventBus after scene deactivation.");
				}
			}
			foreach (Transform item in gameObject.transform.Children())
			{
				queue.Enqueue(item.gameObject);
			}
		}
	}
}
