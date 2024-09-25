using Kingmaker.Networking;
using UnityEngine;

namespace Kingmaker.Controllers.Net;

public static class SynchronizedDataExtensions
{
	public static int GetMinLag(this PlayerCommandsCollection<SynchronizedData> data, int defaultValue = 0)
	{
		int num = int.MaxValue;
		foreach (PlayerCommands<SynchronizedData> player in data.Players)
		{
			foreach (SynchronizedData command in player.Commands)
			{
				num = Mathf.Min(num, command.maxLag);
			}
		}
		if (num != int.MaxValue)
		{
			return num;
		}
		return defaultValue;
	}

	public static int GetMaxLag(this PlayerCommandsCollection<SynchronizedData> data)
	{
		int num = 0;
		foreach (PlayerCommands<SynchronizedData> player in data.Players)
		{
			foreach (SynchronizedData command in player.Commands)
			{
				num = Mathf.Max(num, command.maxLag);
			}
		}
		return num;
	}
}
