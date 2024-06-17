using System;
using JetBrains.Annotations;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Designers.Mechanics.Facts.Restrictions;
using Kingmaker.EntitySystem.Properties;
using UnityEngine;

namespace Kingmaker.Blueprints;

[TypeId("f73264d322814b5f90d973478a501cff")]
public class RestrictionsHolder : BlueprintScriptableObject
{
	[Serializable]
	public class Reference : BlueprintReference<RestrictionsHolder>
	{
	}

	[SerializeField]
	[NotNull]
	private RestrictionCalculator m_Restrictions = new RestrictionCalculator();

	public bool IsPassed(PropertyContext context)
	{
		return m_Restrictions.IsPassed(context);
	}
}
