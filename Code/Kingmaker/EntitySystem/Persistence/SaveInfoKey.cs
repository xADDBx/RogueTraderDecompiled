using System;
using JetBrains.Annotations;
using Kingmaker.Networking;
using Newtonsoft.Json;

namespace Kingmaker.EntitySystem.Persistence;

[JsonObject(IsReference = false)]
public readonly struct SaveInfoKey : IEquatable<SaveInfoKey>
{
	public static SaveInfoKey Empty;

	[JsonProperty]
	public readonly string Id;

	[JsonProperty]
	public readonly string Name;

	[JsonProperty]
	public readonly NetPlayer Player;

	public bool IsEmpty => Id == null;

	public SaveInfoKey([NotNull] string id, [NotNull] string name, NetPlayer player)
	{
		Id = id;
		Name = name;
		Player = player;
	}

	public SaveInfoKey(SaveInfo saveInfo)
		: this(saveInfo.SaveId, saveInfo.Name, NetworkingManager.LocalNetPlayer)
	{
	}

	public bool IsFit(SaveInfo saveInfo)
	{
		if (!IsEmpty && Id.Equals(saveInfo.SaveId, StringComparison.Ordinal))
		{
			return Name.Equals(saveInfo.Name, StringComparison.Ordinal);
		}
		return false;
	}

	public override string ToString()
	{
		return $"SaveInfoKey[{Id}, '{Name}', {Player}]";
	}

	public static explicit operator SaveInfoKey(SaveInfo saveInfo)
	{
		return new SaveInfoKey(saveInfo);
	}

	public bool Equals(SaveInfoKey other)
	{
		if (Id.Equals(other.Id, StringComparison.Ordinal) && Name.Equals(other.Name, StringComparison.Ordinal))
		{
			return Player.Equals(other.Player);
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		if (obj is SaveInfoKey other)
		{
			return Equals(other);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(Id, Name, Player);
	}
}
