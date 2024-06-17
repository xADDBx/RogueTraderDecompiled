using System;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Localization;
using Kingmaker.UI.Models.Tooltip.Base;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Mechanics.Facts;
using Kingmaker.Utility.Attributes;
using Kingmaker.Utility.DotNetExtensions;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.UnitLogic.Mechanics.Blueprints;

[TypeId("f30b1d10b8dc497180447b3e962d14dc")]
public class BlueprintMechanicEntityFact : BlueprintFact, IUIDataProvider
{
	[Serializable]
	public class Reference : BlueprintReference<BlueprintMechanicEntityFact>
	{
	}

	[SerializeField]
	[FormerlySerializedAs("m_LocalizedName")]
	[ShowIf("ShowDisplayName")]
	private LocalizedString m_DisplayName;

	[SerializeField]
	[FormerlySerializedAs("m_LocalizedDescription")]
	[ShowIf("ShowDescription")]
	private LocalizedString m_Description;

	[SerializeField]
	private Sprite m_Icon;

	public LocalizedString LocalizedName => m_DisplayName;

	public LocalizedString LocalizedDescription => m_Description;

	public virtual string Name => (base.ComponentsArray.FirstOrDefault((BlueprintComponent p) => p is AddStringToFactName) as AddStringToFactName)?.NewString(m_DisplayName) ?? ((string)m_DisplayName);

	public virtual string Description => (base.ComponentsArray.FirstOrDefault((BlueprintComponent p) => p is AddStringToFactDescription) as AddStringToFactDescription)?.NewString(m_Description) ?? ((string)m_Description);

	public virtual Sprite Icon => m_Icon;

	public string NameForAcronym => name;

	protected virtual bool ShowDisplayName => true;

	protected virtual bool ShowDescription => true;

	protected override Type GetFactType()
	{
		return typeof(UnitFact);
	}

	public virtual MechanicEntityFact CreateFact([CanBeNull] MechanicsContext parentContext, MechanicEntity owner, BuffDuration duration)
	{
		return new MechanicEntityFact(this);
	}
}
