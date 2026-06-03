using System;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Kingmaker.Blueprints.Area;

[Serializable]
public class SceneReference
{
	[SerializeField]
	[ExcludeFieldFromBuild]
	private UnityEngine.Object m_SceneAsset;

	[SerializeField]
	private string m_SceneName = "";

	public static Func<string, UnityEngine.Object> GetSceneByName = (string _) => (UnityEngine.Object)null;

	public bool Enabled { get; private set; }

	public string SceneName => m_SceneName;

	public bool IsDefined => !string.IsNullOrEmpty(m_SceneName);

	public SceneReference()
	{
	}

	public SceneReference(string sceneName)
	{
		m_SceneName = sceneName;
		m_SceneAsset = GetSceneByName(sceneName);
	}

	public void SetEnabled(bool enabled)
	{
		if (enabled == Enabled)
		{
			return;
		}
		Scene sceneByName = SceneManager.GetSceneByName(SceneName);
		if (sceneByName.IsValid() && sceneByName.isLoaded)
		{
			GameObject[] rootGameObjects = sceneByName.GetRootGameObjects();
			foreach (GameObject gameObject in rootGameObjects)
			{
				bool alreadyExists;
				StaticObjectMark staticObjectMark = gameObject.EnsureComponent<StaticObjectMark>(out alreadyExists);
				if (!alreadyExists && !gameObject.activeSelf)
				{
					staticObjectMark.AlwaysDisabled = true;
				}
				if (!staticObjectMark.AlwaysDisabled)
				{
					gameObject.SetActive(enabled);
				}
			}
		}
		Enabled = enabled;
	}

	protected bool Equals(SceneReference other)
	{
		return string.Equals(m_SceneName, other.m_SceneName);
	}

	public override bool Equals(object obj)
	{
		if (obj == null)
		{
			return false;
		}
		if (this == obj)
		{
			return true;
		}
		if (obj.GetType() != GetType())
		{
			return false;
		}
		return Equals((SceneReference)obj);
	}

	public override int GetHashCode()
	{
		if (m_SceneName == null)
		{
			return 0;
		}
		return m_SceneName.GetHashCode();
	}

	public override string ToString()
	{
		return m_SceneName;
	}
}
