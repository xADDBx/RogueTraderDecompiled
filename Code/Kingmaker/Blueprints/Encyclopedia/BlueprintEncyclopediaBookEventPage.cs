using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.Blueprints.Encyclopedia.Blocks;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.DialogSystem.Blueprints;
using Kingmaker.ResourceLinks;

namespace Kingmaker.Blueprints.Encyclopedia;

[TypeId("512480542acb49bd823c823f3b4cfb04")]
public class BlueprintEncyclopediaBookEventPage : BlueprintEncyclopediaPage, IEncyclopediaPageWithAvailability
{
	public class BookEventLogBlock : IBlock
	{
		public readonly BlueprintScriptableObject Entry;

		public bool IsBookPage => Entry is BlueprintBookPage;

		public BookEventLogBlock(BlueprintScriptableObject entry)
		{
			Entry = entry;
		}
	}

	[UsedImplicitly]
	public BlueprintDialogReference BookEvent;

	private List<SpriteLink> m_Images;

	private List<IBlock> m_Blocks;

	private bool m_IsInitialized;

	public bool IsAvailable => Game.Instance.Player.Dialog.BookEventLog.ContainsKey(BookEvent);

	private void Initialize()
	{
		if (m_IsInitialized)
		{
			return;
		}
		Dictionary<BlueprintDialog, List<BlueprintScriptableObject>> bookEventLog = Game.Instance.Player.Dialog.BookEventLog;
		if (!bookEventLog.ContainsKey(BookEvent))
		{
			return;
		}
		m_Images = base.GetImages();
		m_Blocks = base.GetBlocks();
		bookEventLog[BookEvent].ForEach(delegate(BlueprintScriptableObject entry)
		{
			if (entry is BlueprintBookPage blueprintBookPage)
			{
				if (!m_Images.Contains(blueprintBookPage.ImageLink))
				{
					m_Images.Add(blueprintBookPage.ImageLink);
				}
			}
			else
			{
				m_Blocks.Add(new BookEventLogBlock(entry));
			}
		});
		m_IsInitialized = true;
	}

	public override List<IBlock> GetBlocks()
	{
		Initialize();
		return m_Blocks;
	}

	public override List<SpriteLink> GetImages()
	{
		Initialize();
		return m_Images;
	}
}
