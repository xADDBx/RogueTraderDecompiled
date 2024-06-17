using UnityEngine;
using UnityEngine.SceneManagement;

namespace Kingmaker.EntitySystem.Persistence.Scenes;

public static class SceneManagerWrapper
{
	public delegate AsyncOperation LoadAsyncDelegate(string sceneName, LoadSceneMode mode);

	public delegate void LoadSceneDelegate(string sceneName, LoadSceneMode mode);

	public static LoadAsyncDelegate LoadSceneAsync = SceneManager.LoadSceneAsync;

	public static LoadSceneDelegate LoadScene = SceneManager.LoadScene;
}
