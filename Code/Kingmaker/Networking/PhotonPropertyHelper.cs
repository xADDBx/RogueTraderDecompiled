using ExitGames.Client.Photon;
using Photon.Realtime;

namespace Kingmaker.Networking;

public static class PhotonPropertyHelper
{
	public static void SetProperty(this Photon.Realtime.Player player, object key, object value)
	{
		Hashtable propertiesToSet = new Hashtable { [key] = value };
		player.SetCustomProperties(propertiesToSet);
	}

	public static bool TryGetProperty<T>(this Photon.Realtime.Player player, object key, out T value)
	{
		if (player.CustomProperties.TryGetValue(key, out var value2))
		{
			value = (T)value2;
			return true;
		}
		value = default(T);
		return false;
	}
}
