using System;

namespace Kingmaker.Networking;

public class MessageNetManager
{
	public static class PacketType
	{
		public const byte None = 0;

		public const byte LoadSave = 1;

		public const byte RequestSave = 2;

		public const byte SaveMeta = 3;

		public const byte Save = 4;

		public const byte SaveAcknowledge = 5;

		public const byte Commands = 6;

		public const byte Lock = 7;

		public const byte Kick = 9;

		public const byte ContinueLoading = 11;

		public const byte AvatarMeta = 21;

		public const byte ScreenshotMeta = 22;

		public const byte Ack = 23;

		public const byte DataSend = 24;

		public const byte CancelDataSend = 25;

		public const byte Max = 200;
	}

	private const int MaxPacketBytes = 3145728;

	public static readonly CustomArrayBufferWriter<byte> SendBytes = new CustomArrayBufferWriter<byte>(3145728);

	public void OnMessage(byte code, int actorNumber, ReadOnlySpan<byte> bytes)
	{
		if (PhotonManager.Initialized)
		{
			PhotonActorNumber photonActorNumber = new PhotonActorNumber(actorNumber);
			NetPlayer player = photonActorNumber.ToNetPlayer(NetPlayer.Empty);
			switch (code)
			{
			case 2:
				PhotonManager.Save.OnRequestSave(player);
				break;
			case 3:
				PhotonManager.Save.OnSaveMetaReceived(photonActorNumber, bytes);
				break;
			case 4:
				PhotonManager.Save.OnSaveReceived(photonActorNumber, bytes);
				break;
			case 5:
				PhotonManager.Save.OnSaveAcknowledge(player, bytes);
				break;
			case 6:
				PhotonManager.Command.OnCommandsReceived(player, bytes);
				break;
			case 7:
				PhotonManager.Lock.OnLockReceived(player, bytes);
				break;
			case 1:
				PhotonManager.NetGame.OnSaveReceived(photonActorNumber);
				break;
			case 9:
				PhotonManager.Instance.Kick();
				break;
			case 11:
				PhotonManager.Instance.OnContinueLoadingReceived();
				break;
			case 21:
			case 22:
			case 23:
			case 24:
			case 25:
				PhotonManager.Instance.DataTransporter.OnMessage(code, photonActorNumber, bytes);
				break;
			default:
				PFLog.Net.Error($"Unexpected code={code}!");
				break;
			}
		}
	}
}
