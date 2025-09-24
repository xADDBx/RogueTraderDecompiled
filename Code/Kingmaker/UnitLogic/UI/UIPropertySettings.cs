using System;
using JetBrains.Annotations;
using Kingmaker.Enums;
using Kingmaker.Localization;
using Kingmaker.UnitLogic.Mechanics.Blueprints;
using Kingmaker.Utility.Attributes;
using UnityEngine;

namespace Kingmaker.UnitLogic.UI;

[Serializable]
public class UIPropertySettings
{
	public UIPropertyName NameType;

	[SerializeField]
	[ShowIf("NameIsCustom")]
	private LocalizedString m_Name;

	public LocalizedString Description;

	[SerializeField]
	private BlueprintMechanicEntityFact.Reference m_DescriptionFact;

	[SerializeField]
	private bool m_LinkProperty;

	[SerializeField]
	[ShowIf("m_LinkProperty")]
	private ContextPropertyName m_PropertyName;

	[SerializeField]
	[ShowIf("m_LinkProperty")]
	private BlueprintMechanicEntityFact.Reference m_PropertySource;

	[SerializeField]
	private string m_LinkKey;

	public bool Main;

	public string LinkKey => m_LinkKey;

	private bool NameIsCustom => NameType == UIPropertyName.Custom;

	public string Name
	{
		get
		{
			if (!NameIsCustom)
			{
				return NameType.GetLocalizedName();
			}
			return m_Name;
		}
	}

	[CanBeNull]
	public BlueprintMechanicEntityFact DescriptionFact => m_DescriptionFact;

	public ContextPropertyName? PropertyName
	{
		get
		{
			if (!m_LinkProperty)
			{
				return null;
			}
			return m_PropertyName;
		}
	}

	[CanBeNull]
	public BlueprintMechanicEntityFact PropertySource => m_LinkProperty ? m_PropertySource : null;
}
