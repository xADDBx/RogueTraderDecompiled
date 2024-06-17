using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;
using Core.Async;
using Kingmaker.EntitySystem.Persistence.JsonUtility.Core;
using Kingmaker.Utility.BuildModeUtils;
using Kingmaker.Utility.UnityExtensions;
using Newtonsoft.Json;
using Owlcat.Runtime.Core.Logging;
using UnityEngine;

namespace Kingmaker.Utility;

public static class AssigneeContainer
{
	private static readonly LogChannel Logger = LogChannelFactory.GetOrCreate("AssigneeContainer");

	private static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
	{
		Converters = new List<JsonConverter>
		{
			new DictionaryJsonConverter()
		}
	};

	public static string AssigneeLocalCacheFileName { get; private set; }

	public static async Task<AssigneeModelRoot> LoadAssigneesAsync(string project, CancellationToken token)
	{
		_ = 6;
		try
		{
			await Awaiters.UnityThread;
			string assigneeFileGlobal = Path.Combine(Application.dataPath.Substring(0, Application.dataPath.LastIndexOf('/')), "assignee.json");
			AssigneeLocalCacheFileName = Path.Combine(ApplicationPaths.persistentDataPath, "assignee.json");
			await Awaiters.ThreadPool;
			Task storeTask = Task.CompletedTask;
			string assignees = null;
			if (await ShouldLoadAssigneesFromServer())
			{
				try
				{
					assignees = await LoadAssigneesFromServerAsync(project, token);
					storeTask = StoreAssigneesLocally(AssigneeLocalCacheFileName, assignees);
				}
				catch (Exception ex)
				{
					Logger.Exception(ex);
				}
			}
			if (assignees == null)
			{
				try
				{
					if (File.Exists(AssigneeLocalCacheFileName))
					{
						assignees = await File.ReadAllTextAsync(AssigneeLocalCacheFileName);
					}
				}
				catch (Exception ex2)
				{
					Logger.Exception(ex2);
				}
			}
			if (assignees == null)
			{
				assignees = await File.ReadAllTextAsync(assigneeFileGlobal);
				storeTask = StoreAssigneesLocally(AssigneeLocalCacheFileName, assignees);
			}
			AssigneeModelRoot assigneeContainer = JsonConvert.DeserializeObject<AssigneeModelRoot>(assignees, Settings);
			await storeTask;
			return assigneeContainer;
		}
		catch (Exception ex3)
		{
			Logger.Exception(ex3);
			throw;
		}
	}

	private static async Task StoreAssigneesLocally(string path, string data)
	{
		try
		{
			await File.WriteAllTextAsync(path, data);
		}
		catch (Exception ex)
		{
			Logger.Exception(ex);
		}
	}

	private static async Task<bool> ShouldLoadAssigneesFromServer()
	{
		bool flag = await IsEditor() || BuildModeUtility.Data.LoadAssignees;
		if (!flag)
		{
			flag = await IsConnectVPN();
		}
		return flag || IsDomainUser();
	}

	private static async Task<bool> IsEditor()
	{
		await Awaiters.UnityThread;
		return Application.isEditor;
	}

	private static async Task<string> LoadAssigneesFromServerAsync(string project, CancellationToken token)
	{
		Uri requestUri = new Uri(SirenAddressFinder.GetSirenAddress() + "/api/report/assignee?project=" + project);
		using HttpRequestMessage request = new HttpRequestMessage
		{
			Method = HttpMethod.Post,
			RequestUri = requestUri
		};
		using HttpClient client = new HttpClient();
		using HttpResponseMessage response = await client.SendAsync(request, token);
		Logger.Log($"Response: {response.IsSuccessStatusCode}");
		response.EnsureSuccessStatusCode();
		string result = await response.Content.ReadAsStringAsync();
		Logger.Log("Assignee list has been downloaded");
		return result;
	}

	private static async Task<bool> IsConnectVPN()
	{
		try
		{
			using System.Net.NetworkInformation.Ping ping = new System.Net.NetworkInformation.Ping();
			return (await ping.SendPingAsync("dc.owlcat.local", 2000)).Status == IPStatus.Success;
		}
		catch (Exception)
		{
			return false;
		}
	}

	private static bool IsDomainUser()
	{
		try
		{
			return Environment.UserDomainName != Environment.MachineName && Environment.UserDomainName.Contains("OWLCAT");
		}
		catch
		{
			return false;
		}
	}
}
