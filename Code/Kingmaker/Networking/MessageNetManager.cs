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

		public const byte SaveMetaAcknowledge = 4;

		public const byte Commands = 7;

		public const byte Lock = 8;

		public const byte Kick = 9;

		public const byte ContinueLoading = 11;

		public const byte BugReport = 12;

		public const byte AvatarMeta = 21;

		public const byte ScreenshotMeta = 22;

		public const byte CustomPortraitMeta = 23;

		public const byte SaveMetaDataSender = 24;

		public const byte Ack = 30;

		public const byte DataSend = 31;

		public const byte CancelDataSend = 32;

		public const byte SyncPortraitCommand = 40;

		public const byte SyncPortraitAck = 41;

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
				PhotonManager.Save.OnRequestSave(photonActorNumber);
				break;
			case 3:
				PhotonManager.Save.OnSaveMetaReceived(photonActorNumber, bytes);
				break;
			case 4:
				PhotonManager.Save.OnSaveMetaAcknowledge(player, photonActorNumber, bytes);
				break;
			case 7:
				PhotonManager.Command.OnCommandsReceived(player, bytes);
				break;
			case 8:
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
			case 40:
			case 41:
				PhotonManager.Instance.PortraitSyncer.OnMessage(code, photonActorNumber, bytes);
				break;
			case 21:
			case 22:
			case 23:
			case 24:
			case 30:
			case 31:
			case 32:
				PhotonManager.Instance.DataTransporter.OnMessage(code, photonActorNumber, bytes);
				break;
			case 12:
				PhotonManager.BugReport.OnMessage(bytes);
				break;
			default:
				PFLog.Net.Error($"Unexpected code={code}!");
				break;
			}
		}
	}
}
