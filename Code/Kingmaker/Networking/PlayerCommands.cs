using System.Collections.Generic;
using UnityEngine.Pool;

namespace Kingmaker.Networking;

public readonly struct PlayerCommands<T>
{
	public readonly NetPlayer Player;

	public readonly List<T> Commands;

	public int Count => Commands.Count;

	public T this[int i] => Commands[i];

	public PlayerCommands(NetPlayer player)
	{
		Player = player;
		CollectionPool<List<T>, T>.Get(out Commands);
	}

	public void AddRange(IEnumerable<T> commands)
	{
		Commands.AddRange(commands);
	}

	public void Clear()
	{
		CollectionPool<List<T>, T>.Release(Commands);
	}
}
