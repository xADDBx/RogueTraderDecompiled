using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Kingmaker.EntitySystem.Persistence.Scenes;

public class RootGroup
{
	private readonly string m_Name;

	private Scene m_Scene;

	private readonly List<Renderer> m_EnabledRenderers = new List<Renderer>();

	public string Name => m_Name;

	public GameObject[] Children
	{
		get
		{
			if (!m_Scene.isLoaded)
			{
				return Array.Empty<GameObject>();
			}
			return m_Scene.GetRootGameObjects();
		}
	}

	public bool IsEmpty
	{
		get
		{
			if (m_Scene.isLoaded)
			{
				return m_Scene.rootCount == 0;
			}
			return true;
		}
	}

	public RootGroup(string name)
	{
		m_Name = name;
	}

	public void Add(Transform t)
	{
		if (!m_Scene.isLoaded)
		{
			m_Scene = SceneManager.CreateScene(m_Name);
		}
		SceneManager.MoveGameObjectToScene(t.gameObject, m_Scene);
	}

	public AsyncOperation Clear()
	{
		if (!m_Scene.isLoaded)
		{
			return null;
		}
		return SceneManager.UnloadSceneAsync(m_Scene);
	}

	public void Hide()
	{
		m_EnabledRenderers.Clear();
		if (!m_Scene.isLoaded)
		{
			return;
		}
		GameObject[] rootGameObjects = m_Scene.GetRootGameObjects();
		foreach (GameObject gameObject in rootGameObjects)
		{
			if (!gameObject.activeSelf)
			{
				continue;
			}
			Renderer[] componentsInChildren = gameObject.GetComponentsInChildren<Renderer>();
			foreach (Renderer renderer in componentsInChildren)
			{
				if (renderer.enabled)
				{
					m_EnabledRenderers.Add(renderer);
					renderer.enabled = false;
				}
			}
		}
	}

	public void Unhide()
	{
		if (m_Scene.isLoaded)
		{
			foreach (Renderer enabledRenderer in m_EnabledRenderers)
			{
				if ((bool)enabledRenderer)
				{
					enabledRenderer.enabled = true;
				}
			}
		}
		m_EnabledRenderers.Clear();
	}
}
