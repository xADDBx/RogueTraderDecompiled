using System;
using JetBrains.Annotations;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Blueprints.NodeLink;

[TypeId("05f446aeb54b469c8d7f72df6734d364")]
public class BlueprintNodeLinkRoot : BlueprintScriptableObject
{
	[Serializable]
	public class TraverseSettings
	{
		[SerializeField]
		private float m_MaxTraverseInDuration;

		[SerializeField]
		private float m_MaxTraverseOutDuration;

		public float MaxTraverseInDuration => m_MaxTraverseInDuration;

		public float MaxTraverseOutDuration => m_MaxTraverseOutDuration;
	}

	[Serializable]
	[HashRoot]
	public class Reference : BlueprintReference<BlueprintNodeLinkRoot>
	{
	}

	[SerializeField]
	[NotNull]
	private TraverseSettings m_TraverseSettingsUp = new TraverseSettings();

	[SerializeField]
	[NotNull]
	private TraverseSettings m_TraverseSettingsDown = new TraverseSettings();

	public TraverseSettings GetDefaultTraverseSettings(bool isUp)
	{
		if (!isUp)
		{
			return m_TraverseSettingsDown;
		}
		return m_TraverseSettingsUp;
	}
}
