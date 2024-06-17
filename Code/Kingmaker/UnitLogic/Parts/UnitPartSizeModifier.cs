using System.Collections.Generic;
using Kingmaker.Designers.Mechanics.Buffs;
using Kingmaker.EntitySystem;
using Kingmaker.Enums;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility.DotNetExtensions;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.Parts;

public class UnitPartSizeModifier : BaseUnitPart, IHashable
{
	private readonly List<EntityFact> m_SizeChangeFacts = new List<EntityFact>();

	public void Add(EntityFact fact)
	{
		m_SizeChangeFacts.Add(fact);
		UpdateSize();
	}

	public void Remove(EntityFact fact)
	{
		m_SizeChangeFacts.Remove(fact);
		UpdateSize();
	}

	private void UpdateSize()
	{
		EntityFact entityFact = m_SizeChangeFacts.LastItem();
		if (entityFact == null)
		{
			base.Owner.State.Size = base.Owner.OriginalSize;
			base.Owner.Remove<UnitPartSizeModifier>();
			return;
		}
		Size? size = null;
		foreach (EntityFactComponent component in entityFact.Components)
		{
			size = ((component.SourceBlueprintComponent is Polymorph polymorph) ? new Size?(polymorph.GetUnitSize(component)) : ((component.SourceBlueprintComponent is ChangeUnitSize changeUnitSize) ? new Size?(changeUnitSize.GetUnitSize(component)) : null));
			if (size.HasValue)
			{
				base.Owner.State.Size = size.Value;
				break;
			}
		}
		if (!size.HasValue)
		{
			PFLog.Default.Error(entityFact.Blueprint, $"Invalid fact (has no ChangeUnitSize component): {entityFact.Blueprint}");
			m_SizeChangeFacts.RemoveAt(m_SizeChangeFacts.Count - 1);
			UpdateSize();
		}
	}

	protected override void OnPreSave()
	{
		base.OnPreSave();
		m_SizeChangeFacts.RemoveAll((EntityFact f) => !f.Active);
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
