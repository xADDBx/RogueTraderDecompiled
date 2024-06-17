using System;
using JetBrains.Annotations;
using Kingmaker.ElementsSystem;
using UnityEngine;

namespace Kingmaker.View.Mechanics.Destructible;

[Serializable]
public class DestructionStagesActionsSettings
{
	[SerializeField]
	[CanBeNull]
	private ActionsReference m_OnBecameDamaged;

	[SerializeField]
	[CanBeNull]
	private ActionsReference m_OnBecameDestroyed;

	[CanBeNull]
	public ActionsHolder OnBecameDamaged => m_OnBecameDamaged;

	[CanBeNull]
	public ActionsHolder OnBecameDestroyed => m_OnBecameDestroyed;
}
