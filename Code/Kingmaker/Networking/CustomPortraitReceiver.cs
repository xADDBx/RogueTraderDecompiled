using System;
using JetBrains.Annotations;

namespace Kingmaker.Networking;

public class CustomPortraitReceiver : DataReceiver
{
	public readonly struct Result
	{
		public readonly PhotonActorNumber PlayerSource;

		public readonly ArraySegment<byte> SmallPortraitBytes;

		public readonly ArraySegment<byte> HalfPortraitBytes;

		public readonly ArraySegment<byte> FullPortraitBytes;

		public readonly string CustomPortraitId;

		public readonly Guid PortraitGuid;

		public Result(PhotonActorNumber playerSource, CustomPortraitMetaData metaData, byte[] bytes)
		{
			PlayerSource = playerSource;
			CustomPortraitId = metaData.CustomPortraitId;
			PortraitGuid = metaData.PortraitGuid;
			SmallPortraitBytes = new ArraySegment<byte>(bytes, 0, metaData.LengthSmallPortrait);
			HalfPortraitBytes = new ArraySegment<byte>(bytes, metaData.LengthSmallPortrait, metaData.LengthHalfPortrait);
			FullPortraitBytes = new ArraySegment<byte>(bytes, metaData.LengthSmallPortrait + metaData.LengthHalfPortrait, metaData.LengthFullPortrait);
		}
	}

	private readonly Action<Result> m_OnReceived;

	private readonly Action<Guid> m_OnCancel;

	private CustomPortraitMetaData m_MetaData;

	protected override int MainPartLength => m_MetaData.LengthSmallPortrait + m_MetaData.LengthHalfPortrait + m_MetaData.LengthFullPortrait;

	protected override int SenderUniqueNumber => m_MetaData.SenderUniqueNumber;

	public CustomPortraitReceiver(PhotonActorNumber playerSource, [NotNull] PhotonManager sender, [NotNull] Action<Result> onReceived, [NotNull] Action<Guid> onCancel, IProgress<DataTransferProgressInfo> progress)
		: base(playerSource, sender, progress)
	{
		m_OnReceived = onReceived;
		m_OnCancel = onCancel;
	}

	protected override void DeserializeMeta(ReadOnlySpan<byte> bytes)
	{
		m_MetaData = NetMessageSerializer.DeserializeFromSpan<CustomPortraitMetaData>(bytes);
	}

	protected override void OnMainPartReceiveCompleted(PhotonActorNumber playerSource, byte[] bytes)
	{
		m_OnReceived(new Result(playerSource, m_MetaData, bytes));
	}

	public override void OnCancel()
	{
		base.OnCancel();
		m_OnCancel(m_MetaData.PortraitGuid);
	}
}
