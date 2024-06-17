using System.Collections.Generic;
using Kingmaker.Blueprints.Encyclopedia.Blocks;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Globalmap.Blueprints;
using Kingmaker.Globalmap.SystemMap;
using Kingmaker.Utility.DotNetExtensions;
using UnityEngine;

namespace Kingmaker.Blueprints.Encyclopedia;

[TypeId("946176d009764382a8ee5a482332470d")]
public class BlueprintEncyclopediaAstropathBriefPage : BlueprintEncyclopediaPage, IEncyclopediaPageWithAvailability
{
	public class AstropathBriefBlock : IBlock
	{
		public string MessageLocation;

		public string MessageDate;

		public string MessageSender;

		public string MessageBody;

		public bool IsMessageRead;

		public BlueprintEncyclopediaAstropathBriefPage Entry;

		public AstropathBriefBlock(BlueprintEncyclopediaAstropathBriefPage entry)
		{
			Entry = entry;
		}
	}

	[SerializeField]
	private BlueprintAstropathBrief.Reference m_AstropathBrief;

	public BlueprintAstropathBrief AstropathBrief => m_AstropathBrief?.Get();

	public bool IsAvailable => Game.Instance.Player.StarSystemsState.AstropathBriefs.Any((AstropathBriefInfo info) => info.AstropathBrief == AstropathBrief);

	public override List<IBlock> GetBlocks()
	{
		List<IBlock> blocks = base.GetBlocks();
		AstropathBriefInfo astropathBriefInfo = Game.Instance.Player.StarSystemsState.AstropathBriefs.FirstOrDefault((AstropathBriefInfo info) => info.AstropathBrief == AstropathBrief);
		if (astropathBriefInfo != null)
		{
			AstropathBriefBlock item = new AstropathBriefBlock(this)
			{
				MessageLocation = astropathBriefInfo.MessageLocation,
				MessageDate = astropathBriefInfo.MessageDate,
				MessageSender = astropathBriefInfo.AstropathBrief.MessageSender,
				MessageBody = astropathBriefInfo.AstropathBrief.MessageBody,
				IsMessageRead = true
			};
			blocks.Add(item);
		}
		return blocks;
	}
}
