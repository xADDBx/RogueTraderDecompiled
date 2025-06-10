using System;
using System.Collections.Generic;
using System.Reflection;
using Kingmaker.Blueprints.JsonSystem;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using UnityEngine;

namespace Kingmaker.QA.Arbiter.GameCore.AreaChecker;

[Serializable]
public class ArbiterElementList
{
	[SerializeReference]
	public List<ArbiterElement> ElementList = new List<ArbiterElement>();

	[SerializeField]
	private string m_ElementTypeGuid;

	public Type ListElementType => ((GuidClassBinder)Json.Serializer.SerializationBinder).BindToType("", m_ElementTypeGuid);

	public ArbiterElementList()
	{
	}

	public ArbiterElementList(Type listElementType)
	{
		m_ElementTypeGuid = listElementType.GetCustomAttribute<TypeIdAttribute>().Guid.ToString("N");
	}
}
