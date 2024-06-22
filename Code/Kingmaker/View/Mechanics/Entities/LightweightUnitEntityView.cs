using System.Collections.Generic;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.Items;
using Kingmaker.Mechanics.Entities;
using Kingmaker.View.Equipment;
using Kingmaker.View.Mechadendrites;
using Kingmaker.Visual.OcclusionGeometryClip;

namespace Kingmaker.View.Mechanics.Entities;

[KnowledgeDatabaseID("6d7bfae7c43946179659fed18d0333c5")]
public sealed class LightweightUnitEntityView : AbstractUnitEntityView
{
	private List<ItemEntity> m_Mechadendrites = new List<ItemEntity>();

	public new LightweightUnitEntity Data => (LightweightUnitEntity)base.Data;

	public override List<ItemEntity> Mechadendrites => m_Mechadendrites;

	public override Entity CreateEntityData(bool load)
	{
		return Entity.Initialize(new LightweightUnitEntity(UniqueId, base.IsInGameBySettings, base.Blueprint));
	}

	protected override void OnDidAttachToData()
	{
		base.OnDidAttachToData();
		CollectMechadendrites();
		base.MechadendritesEquipment = new UnitViewMechadendritesEquipment(this, base.CharacterAvatar);
		base.MechadendritesEquipment.UpdateAll();
		UnitPartMechadendrites orCreate = Data.GetOrCreate<UnitPartMechadendrites>();
		if (orCreate != null)
		{
			MechadendriteSettings[] componentsInChildren = GetComponentsInChildren<MechadendriteSettings>();
			foreach (MechadendriteSettings settings in componentsInChildren)
			{
				orCreate.RegisterMechadendrite(settings);
			}
			if (!TryGetComponent<OcclusionGeometryClipEntityProxy>(out var _))
			{
				base.gameObject.AddComponent<OcclusionGeometryClipEntityProxy>();
			}
		}
	}

	protected override void OnWillDetachFromData()
	{
		base.OnWillDetachFromData();
		UnitPartMechadendrites optional = Data.GetOptional<UnitPartMechadendrites>();
		if (optional == null)
		{
			return;
		}
		foreach (KeyValuePair<MechadendritesType, MechadendriteSettings> mechadendrite in optional.Mechadendrites)
		{
			optional.UnregisterMechadendrite(mechadendrite.Value);
		}
		Data?.Remove<UnitPartMechadendrites>();
	}

	private void CollectMechadendrites()
	{
		m_Mechadendrites.Clear();
		BlueprintUnit.UnitBody body = Data.OriginalBlueprint.Body;
		for (int i = 0; i < body.Mechadendrites.Length; i++)
		{
			ItemEntity itemEntity = body.Mechadendrites[i]?.CreateEntity();
			if (itemEntity != null)
			{
				m_Mechadendrites.Add(itemEntity);
			}
		}
	}
}
