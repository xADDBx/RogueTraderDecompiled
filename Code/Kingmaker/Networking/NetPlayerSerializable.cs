using Newtonsoft.Json;

namespace Kingmaker.Networking;

[JsonObject(IsReference = false)]
public readonly struct NetPlayerSerializable
{
	private const short EmptyIndex = -1;

	[JsonProperty(PropertyName = "i")]
	public readonly short Index;

	private NetPlayerSerializable(short index)
	{
		Index = index;
	}

	public NetPlayerSerializable(NetPlayer player)
		: this((short)(player.IsEmpty ? (-1) : ((short)player.Index)))
	{
	}

	public static explicit operator NetPlayer(NetPlayerSerializable value)
	{
		if (value.Index == -1)
		{
			return NetPlayer.Empty;
		}
		if (value.Index == NetPlayer.Offline.Index)
		{
			return NetPlayer.Offline;
		}
		bool isLocal = value.Index == NetworkingManager.LocalNetPlayer.Index;
		return new NetPlayer(value.Index, isLocal);
	}

	public static explicit operator NetPlayerSerializable(NetPlayer value)
	{
		return new NetPlayerSerializable(value);
	}
}
