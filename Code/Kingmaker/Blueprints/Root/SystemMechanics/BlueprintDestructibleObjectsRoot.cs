using System;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.UnitLogic.Mechanics.Blueprints;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.QA.Validation;
using UnityEngine;

namespace Kingmaker.Blueprints.Root.SystemMechanics;

[TypeId("f200acdb25c749babad3f983eee6c13c")]
public class BlueprintDestructibleObjectsRoot : BlueprintScriptableObject
{
	[Serializable]
	public class Reference : BlueprintReference<BlueprintDestructibleObjectsRoot>
	{
	}

	[Serializable]
	private class StandardObjectSettings
	{
		public StandardDestructibleObjectType Type;

		[SerializeField]
		[ValidateNotNull]
		public BlueprintDestructibleObject.Reference m_Blueprint;

		public BlueprintDestructibleObject Blueprint => m_Blueprint;
	}

	[SerializeField]
	private StandardObjectSettings[] m_StandardObjects = new StandardObjectSettings[9]
	{
		new StandardObjectSettings
		{
			Type = StandardDestructibleObjectType.LowHitPointsLowArmor
		},
		new StandardObjectSettings
		{
			Type = StandardDestructibleObjectType.LowHitPointsMediumArmor
		},
		new StandardObjectSettings
		{
			Type = StandardDestructibleObjectType.LowHitPointsHighArmor
		},
		new StandardObjectSettings
		{
			Type = StandardDestructibleObjectType.MediumHitPointsLowArmor
		},
		new StandardObjectSettings
		{
			Type = StandardDestructibleObjectType.MediumHitPointsMediumArmor
		},
		new StandardObjectSettings
		{
			Type = StandardDestructibleObjectType.MediumHitPointsHighArmor
		},
		new StandardObjectSettings
		{
			Type = StandardDestructibleObjectType.HighHitPointsLowArmor
		},
		new StandardObjectSettings
		{
			Type = StandardDestructibleObjectType.HighHitPointsMediumArmor
		},
		new StandardObjectSettings
		{
			Type = StandardDestructibleObjectType.HighHitPointsHighArmor
		}
	};

	public BlueprintDestructibleObject GetStandardObject(StandardDestructibleObjectType type)
	{
		return m_StandardObjects.FirstItem((StandardObjectSettings i) => i.Type == type)?.Blueprint;
	}
}
