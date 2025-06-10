using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.UI.SurfaceCombatHUD;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities.Components.Patterns;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.UnitLogic.Progression.Features;
using Kingmaker.Utility.Attributes;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Designers.Mechanics.Facts.Shields;

[Obsolete]
[AllowedOn(typeof(BlueprintFeature))]
[TypeId("b8bdda256eb7470a8fd6d0e3e607956f")]
public class ShieldBonusDefenceFromSide : UnitFactComponentDelegate, IHashable
{
	[SerializeField]
	[Tooltip("Сторона с которой работает паттерн защиты")]
	private WarhammerCombatSide m_DefenceSide;

	[SerializeField]
	[Tooltip("Паттерн защиты, выставляется с выбранной стороны")]
	private AoEPattern m_DefencePattern;

	[SerializeField]
	[Tooltip("Заменить базовую кавер магнитуду из блюпринта щита")]
	private bool m_OverrideCoverMagnitude;

	[SerializeField]
	[ShowIf("m_OverrideCoverMagnitude")]
	[Tooltip("Вероятность попасть в кавер")]
	[Range(0f, 100f)]
	private int m_CoverMagnitude;

	[SerializeField]
	private Texture2D m_PersistentAreaTexture2D;

	[SerializeField]
	private CombatHudMaterialRemapAsset m_PersistentAreaMaterialRemap;

	[SerializeField]
	[Tooltip("Маркер для бонусной стороны щита")]
	private GameObject m_BonusDefenceMarker;

	[SerializeField]
	[Tooltip("Экшены на успешное поглощение удара кавером щита")]
	private ActionList m_ActionsOnHitOnCover;

	[SerializeField]
	[Tooltip("Экшены на успешное паррирование щитом")]
	private ActionList m_ActionsOnParryHit;

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
