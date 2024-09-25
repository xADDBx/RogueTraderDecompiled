using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Kingmaker.EntitySystem.Persistence.JsonUtility;
using Kingmaker.QA.Analytics;
using Kingmaker.Settings;
using Kingmaker.Utility.CodeTimer;
using Kingmaker.Utility.CommandLineArgs;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Utility.Serialization;
using Newtonsoft.Json;
using UnityEngine;

namespace Kingmaker.EntitySystem.Persistence.SavesStorage;

public class SavesStorageAccess
{
	private static int s_UploadingCount;

	private const float UploadDelay = 3600f;

	private static float s_NextUploadTime;

	public static bool UploadEnabled;

	[NotNull]
	private static readonly HttpClient s_HttpClient;

	public const string ProdPublicUri = "http://188.93.59.251:8876/api/savespublic/";

	public static bool IsUploading => s_UploadingCount > 0;

	static SavesStorageAccess()
	{
		s_UploadingCount = 0;
		s_NextUploadTime = 600f;
		string text = CommandLineArguments.Parse().Get("savesapi");
		if (string.IsNullOrWhiteSpace(text))
		{
			text = "http://188.93.59.251:8876/api/savespublic/";
		}
		UploadEnabled = SettingsRoot.Game.Main.SendGameStatistic;
		s_HttpClient = new HttpClient
		{
			BaseAddress = new Uri(text),
			Timeout = 30.Minutes()
		};
	}

	[Conditional("SAVE_ENABLE_UPLOAD")]
	public static async void Upload([NotNull] SaveInfo saveInfo, [NotNull] SaveCreateDTO dto)
	{
		try
		{
			await UploadImpl(saveInfo, dto);
		}
		catch (Exception ex)
		{
			PFLog.Default.Exception(ex);
		}
	}

	private static Task<string> UploadImpl([NotNull] SaveInfo saveInfo, [NotNull] SaveCreateDTO dto)
	{
		if (!UploadEnabled)
		{
			return null;
		}
		if (OwlcatAnalytics.Instance.IsOptInConsentShown && OwlcatAnalytics.Instance.IsOptIn)
		{
			return null;
		}
		if (!SettingsRoot.Game.Main.SendSaves)
		{
			return null;
		}
		if (saveInfo.Type == SaveInfo.SaveType.Remote || saveInfo.Type == SaveInfo.SaveType.Bugreport)
		{
			return null;
		}
		if (IsUploading)
		{
			return null;
		}
		if (s_NextUploadTime > Time.unscaledTime)
		{
			return null;
		}
		s_NextUploadTime = Time.unscaledTime + 3600f;
		using (ProfileScope.New("Start Upload Task"))
		{
			return StartUploadTask(saveInfo, "", dto);
		}
	}

	public static void Load(string remotePath)
	{
		if (Application.isPlaying)
		{
			LoadingProcess.Instance.StartLoadingProcess(LoadProcess(remotePath));
		}
	}

	public static Task<string> StartUploadTask(SaveInfo saveInfo, string description, SaveCreateDTO dto)
	{
		if (!saveInfo.IsActuallySaved)
		{
			PFLog.Default.Error("SaveStorage: Cannot upload save, it is not yet completed");
			return Task.FromResult("");
		}
		string folderName = saveInfo.FolderName;
		if (!File.Exists(folderName))
		{
			PFLog.Default.Error("SaveStorage: Cannot upload save, file doesnt exist: " + folderName);
			return Task.FromResult("");
		}
		dto.Save.Description = description;
		string dtoJson = SaveSystemJsonSerializer.Serializer.SerializeObject(dto);
		return Task.Run(() => Upload(dtoJson, saveInfo));
	}

	private static IEnumerator LoadProcess(string remotePath)
	{
		yield break;
	}

	private static async Task<string> Upload([NotNull] string dtoJson, SaveInfo saveInfo)
	{
		if (s_UploadingCount > 0)
		{
			PFLog.Default.Warning("SaveStorage: Skipping upload, another upload is currently active");
			return "";
		}
		try
		{
			if (Interlocked.Increment(ref s_UploadingCount) != 1)
			{
				PFLog.Default.Warning("SaveStorage: Skipping upload, another upload is currently active (race)");
				return "";
			}
			StringContent content = new StringContent(dtoJson, null, "application/json");
			HttpResponseMessage createResponse = await s_HttpClient.PostAsync("new", content);
			string text = await createResponse.Content.ReadAsStringAsync();
			if (!createResponse.IsSuccessStatusCode)
			{
				PFLog.Default.Error("SaveStorage: Error while creating save metadata: " + text);
				return "";
			}
			if (!int.TryParse(text, out var saveId))
			{
				PFLog.Default.Error("SaveStorage: Could not parse saveId: " + text);
				return "";
			}
			if (saveId <= 0)
			{
				PFLog.Default.Log("SaveStorage: Save rejected by chances");
				return "";
			}
			PFLog.Default.Log("SaveStorage: Successfully created save metadata via api: " + saveId);
			HttpResponseMessage uploadResponse;
			using (saveInfo.GetReadScope())
			{
				using FileStream content2 = new FileStream(saveInfo.FolderName, FileMode.Open);
				Task<HttpResponseMessage> task = s_HttpClient.PutAsync(saveId.ToString(), new StreamContent(content2));
				task.Wait();
				uploadResponse = task.Result;
			}
			string text2 = await uploadResponse.Content.ReadAsStringAsync();
			if (uploadResponse.IsSuccessStatusCode)
			{
				PFLog.SmartConsole.Log($"Uploaded: {saveId}: {text2}");
				PFLog.Default.Log("SaveStorage", $"Successfully uploaded save file via api: {text2} ({saveId})");
				return text2;
			}
			PFLog.Default.Error("SaveStorage: Error while uploading save file: " + text2);
			return "";
		}
		catch (Exception ex)
		{
			for (Exception innerException = ex.InnerException; innerException != null; innerException = innerException.InnerException)
			{
				PFLog.Default.Exception(innerException);
			}
			PFLog.Default.Exception(ex);
			return "";
		}
		finally
		{
			Interlocked.Decrement(ref s_UploadingCount);
		}
	}

	public static async Task<bool> Download(string remotePath, string filePath)
	{
		_ = 2;
		try
		{
			Stream stream = await s_HttpClient.GetStreamAsync(remotePath);
			bool result;
			await using (FileStream fs = new FileStream(filePath, FileMode.Create))
			{
				await stream.CopyToAsync(fs);
				result = true;
			}
			return result;
		}
		catch (Exception ex)
		{
			PFLog.Default.Exception(ex);
			return false;
		}
	}

	public static async Task<List<int>> GetSmokeTestSaveIds()
	{
		return JsonConvert.DeserializeObject<List<int>>(await s_HttpClient.GetStringAsync("smoke_test_saves"));
	}

	public static async Task<string> GetSaveData(int saveId)
	{
		return await s_HttpClient.GetStringAsync($"data?id={saveId}");
	}

	public static void SetApiUri(string uri)
	{
		s_HttpClient.BaseAddress = new Uri(uri);
	}
}
