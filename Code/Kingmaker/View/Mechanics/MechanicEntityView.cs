using JetBrains.Annotations;
using Kingmaker.Code.View.Mechanics;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Visual.Particles;
using Kingmaker.Visual.Sound;

namespace Kingmaker.View.Mechanics;

public abstract class MechanicEntityView : EntityViewBase
{
	public new MechanicEntity Data => (MechanicEntity)base.Data;

	public MechanicEntity EntityData => Data;

	[CanBeNull]
	public virtual ParticlesSnapMap ParticlesSnapMap => null;

	[CanBeNull]
	public virtual UnitBarksManager Asks => null;

	protected override void OnDrawGizmosSelected()
	{
		base.OnDrawGizmosSelected();
		GizmoHelper.ShowCellsCoveredByMechanicEntity(Data, this);
	}
}
