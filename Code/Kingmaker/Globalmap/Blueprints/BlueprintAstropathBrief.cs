using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Localization;
using UnityEngine;

namespace Kingmaker.Globalmap.Blueprints;

[TypeId("5d94d4b44fdf47b5a447f8ff0f048061")]
public class BlueprintAstropathBrief : BlueprintScriptableObject
{
	[Serializable]
	public class Reference : BlueprintReference<BlueprintAstropathBrief>
	{
	}

	[SerializeField]
	private LocalizedString m_MessageSender;

	[SerializeField]
	private LocalizedString m_MessageBody;

	[SerializeField]
	private string m_EncyclopediaLink;

	public string MessageSender => m_MessageSender.Text;

	public string MessageBody => m_MessageBody.Text;

	public string EncyclopediaLink => m_EncyclopediaLink;
}
