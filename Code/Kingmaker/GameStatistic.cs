using System;
using System.Security.Cryptography;
using System.Text;
using Core.Async;
using Kingmaker.EntitySystem.Persistence;
using Kingmaker.EntitySystem.Persistence.JsonUtility;
using Kingmaker.GameInfo;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.Utility.Reporting.Base;
using Kingmaker.Utility.Serialization;
using Newtonsoft.Json;

namespace Kingmaker;

[JsonObject]
public class GameStatistic : IDisposable
{
	public enum AppType
	{
		Default,
		NonDefault
	}

	public class AppStatus
	{
		private string appStatus = string.Empty;

		public void Add(string data)
		{
			if (data == null)
			{
				return;
			}
			byte[] bytes = Encoding.Unicode.GetBytes(data);
			using SHA1 sHA = SHA1.Create();
			byte[] array = sHA.ComputeHash(bytes);
			appStatus = BitConverter.ToString(array);
			appStatus = appStatus.Replace("-", "").ToLower();
		}

		public string Get()
		{
			return appStatus;
		}

		public string Randomize()
		{
			char[] array = new char[16]
			{
				'0', '1', '2', '3', '4', '5', '6', '7', '8', '9',
				'a', 'b', 'c', 'd', 'e', 'f'
			};
			Random random = new Random(Environment.TickCount);
			StringBuilder stringBuilder = new StringBuilder("");
			stringBuilder.Append(array[random.Next(1, array.Length)]);
			for (int i = 1; i < 40; i++)
			{
				stringBuilder.Append(array[random.Next(0, array.Length)]);
			}
			appStatus = stringBuilder.ToString();
			return appStatus;
		}
	}

	private static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
	{
		Formatting = Formatting.None,
		Converters = { (JsonConverter)new DictionaryConverter() }
	};

	private static readonly JsonSerializer MainThreadSerializer = JsonSerializer.Create(Settings);

	[JsonProperty]
	public GameMetaData Meta;

	private static GameMetaData s_CachedMeta;

	[JsonProperty]
	public string m_gameSessionGUID = Guid.NewGuid().ToString();

	[JsonProperty]
	public string m_AppStatus = string.Empty;

	[JsonIgnore]
	public AppType m_AppType;

	[JsonProperty]
	public string m_DeviceUniqueIdentifier = string.Empty;

	[JsonProperty]
	public int m_CurrentLevel = -1;

	public static JsonSerializer Serializer
	{
		get
		{
			if (!UnitySyncContextHolder.IsInUnity)
			{
				return JsonSerializer.Create(Settings);
			}
			return MainThreadSerializer;
		}
	}

	public void Dispose()
	{
		EventBus.Unsubscribe(this);
	}

	private void UpdateAppStatus(AppStatus appStatus)
	{
		if (string.Equals(m_AppStatus, appStatus.Get(), StringComparison.Ordinal))
		{
			m_AppType = AppType.Default;
		}
		else
		{
			m_AppType = AppType.NonDefault;
		}
	}

	private void CreateAppStatus(AppStatus appStatus)
	{
		if (m_AppType == AppType.Default)
		{
			m_AppStatus = appStatus.Get();
		}
		else
		{
			m_AppStatus = appStatus.Randomize();
		}
	}

	private void PostLoad(GameStatistic old, AppStatus appStatus, Game gameInstance)
	{
		UpdateAppStatus(appStatus);
	}

	public void PreSave(AppStatus appStatus)
	{
		m_DeviceUniqueIdentifier = GameVersion.DeviceUniqueIdentifier;
		CreateAppStatus(appStatus);
		if (s_CachedMeta == null)
		{
			s_CachedMeta = GameMetaData.Create(ReportDllChecksumManager.GetDllCRC(), ReportDllChecksumManager.IsUnityModManagerActive());
		}
		Meta = s_CachedMeta;
	}

	public void Quit()
	{
	}

	public void Reset()
	{
	}

	public static string Serialize(SaveInfo saveInfo, AppStatus appStatus)
	{
		return Serializer.SerializeObject(Game.Instance.Statistic);
	}

	public static void Deserialize(SaveInfo saveInfo, string data, AppStatus appStatus)
	{
		GameStatistic statistic = Game.Instance.Statistic;
		EventBus.Unsubscribe(statistic);
		try
		{
			Game.Instance.Statistic = Serializer.DeserializeObject<GameStatistic>(data) ?? new GameStatistic();
		}
		catch (Exception arg)
		{
			PFLog.GameStatistics.Error($"Failed to read game statistic from save, creating new. Error: {arg}");
			Game.Instance.Statistic = new GameStatistic();
		}
		Game.Instance.Statistic.PostLoad(statistic, appStatus, Game.Instance);
	}
}
