using System.Collections.Generic;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Networking;

public class PlayerCommandsCollection<T> : IHashable
{
	public readonly List<PlayerCommands<T>> Players = new List<PlayerCommands<T>>();

	public PlayerCommands<T> this[NetPlayer player] => GetOrCreate(player);

	public void Clear()
	{
		int i = 0;
		for (int count = Players.Count; i < count; i++)
		{
			Players[i].Clear();
		}
		Players.Clear();
	}

	public void AddCommands(NetPlayer player, IEnumerable<T> commands)
	{
		GetOrCreate(player).AddRange(commands);
	}

	public void Fill(List<T> targetList)
	{
		foreach (PlayerCommands<T> player in Players)
		{
			targetList.AddRange(player.Commands);
		}
	}

	public void Fill(List<(NetPlayer, T)> targetList)
	{
		foreach (PlayerCommands<T> player2 in Players)
		{
			NetPlayer player = player2.Player;
			foreach (T command in player2.Commands)
			{
				targetList.Add((player, command));
			}
		}
	}

	private PlayerCommands<T> GetOrCreate(NetPlayer netPlayer)
	{
		PlayerCommands<T> playerCommands = default(PlayerCommands<T>);
		int num = 0;
		foreach (PlayerCommands<T> player in Players)
		{
			int num2 = player.Player.CompareTo(netPlayer);
			if (num2 == 0)
			{
				playerCommands = player;
				break;
			}
			if (num2 > 0)
			{
				break;
			}
			num++;
		}
		if (playerCommands.Commands == null)
		{
			playerCommands = new PlayerCommands<T>(netPlayer);
			Players.Insert(num, playerCommands);
		}
		return playerCommands;
	}

	public virtual Hash128 GetHash128()
	{
		return default(Hash128);
	}
}
