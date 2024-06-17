namespace Kingmaker.Networking.Desync;

public struct DesyncMeta
{
	public int Tick;

	public string RoomId;

	public int PlayersCount;

	public DesyncMeta(int tick, string roomId, int playersCount)
	{
		Tick = tick;
		RoomId = roomId;
		PlayersCount = playersCount;
	}
}
