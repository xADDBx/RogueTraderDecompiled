using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Kingmaker.EntitySystem.Persistence;
using Kingmaker.EntitySystem.Persistence.JsonUtility;
using Kingmaker.EntitySystem.Persistence.SavesStorage;
using Kingmaker.EntitySystem.Persistence.Scenes;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Kingmaker.QA;

public class SavesSmokeTest : MonoBehaviour
{
	private class SaveEntry
	{
		public readonly int Id;

		private readonly string m_Chapter;

		private readonly string m_Zone;

		private readonly string m_Version;

		public SaveEntry(int id, string dataJson)
		{
			Id = id;
			JToken jToken = JToken.Parse(dataJson);
			m_Chapter = jToken.SelectToken("..chapter")?.Value<string>();
			m_Zone = jToken.SelectToken("..zone").Value<string>();
			m_Version = jToken.SelectToken("..version").Value<string>();
		}

		public override string ToString()
		{
			return $"[{Id}] {m_Chapter}: {m_Zone} ({m_Version})";
		}
	}

	public static bool IsActive { get; private set; }

	public static void StartTest()
	{
		CreateTester();
	}

	private static void CreateTester()
	{
		new GameObject("[smokeTestRunner]").AddComponent<SavesSmokeTest>();
	}

	private void Start()
	{
		IsActive = true;
		StartCoroutine(SmokeTestCoroutine());
		UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
	}

	private IEnumerator SmokeTestCoroutine()
	{
		PFLog.SmokeTest.Log("Fetching smoke saves list...");
		yield return null;
		Task<List<int>> generateTask = SavesStorageAccess.GetSmokeTestSaveIds();
		while (!generateTask.IsCompleted)
		{
			yield return null;
		}
		List<int> result = generateTask.Result;
		List<SaveEntry> saves = new List<SaveEntry>();
		foreach (int saveId in result)
		{
			Task<string> dataTask = SavesStorageAccess.GetSaveData(saveId);
			while (!dataTask.IsCompleted)
			{
				yield return null;
			}
			SaveEntry saveEntry = new SaveEntry(saveId, dataTask.Result);
			saves.Add(saveEntry);
			PFLog.SmokeTest.Log("FETCHED: " + saveEntry);
			yield return null;
		}
		PFLog.SmokeTest.Log("Starting smoke test");
		yield return null;
		SceneLoader.LoadObligatoryScenes();
		foreach (SaveEntry save in saves)
		{
			Runner.ClearError();
			LoadingProcess.Instance.StartLoadingProcess(LoadProcess(save.Id));
			while (LoadingProcess.Instance.IsLoadingInProcess)
			{
				yield return null;
			}
			if (Runner.LastError != null)
			{
				Exception lastError = Runner.LastError;
				PFLog.SmokeTest.Error($"FAIL: {save} | {lastError.Message}");
			}
			else
			{
				PFLog.SmokeTest.Log($"OK: {save}");
			}
			yield return null;
		}
		PFLog.SmokeTest.Log("Smoke test finished");
		yield return null;
		UnityEngine.Object.Destroy(base.gameObject);
	}

	private static IEnumerator LoadProcess(int saveId)
	{
		string filePath = Path.Combine(SaveManager.SavePath, "remote-save.zks");
		Task<bool> task = Task.Run(() => SavesStorageAccess.Download(saveId.ToString(), filePath));
		while (!task.IsCompleted && !task.IsCanceled && !task.IsFaulted)
		{
			yield return null;
		}
		if (task.Result)
		{
			SaveInfo saveInfo = SaveManager.LoadZipSave(SaveSystemJsonSerializer.Serializer, filePath);
			if (saveInfo != null)
			{
				Game.Instance.LoadGameForSmokeTest(saveInfo);
			}
		}
	}
}
