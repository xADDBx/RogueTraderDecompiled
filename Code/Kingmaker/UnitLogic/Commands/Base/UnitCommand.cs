using System;
using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Mechanics.Entities;

namespace Kingmaker.UnitLogic.Commands.Base;

public abstract class UnitCommand : AbstractUnitCommand
{
	public new BaseUnitEntity Executor => (BaseUnitEntity)base.Executor;

	protected UnitCommand([NotNull] UnitCommandParams @params)
		: base(@params)
	{
	}

	public sealed override void Init(AbstractUnitEntity executor)
	{
		if (!(executor is BaseUnitEntity executor2))
		{
			throw new ArgumentException("Only BaseUnitEntity can execute " + GetType().Name);
		}
		base.Init(executor2);
		OnInit(executor2);
	}

	protected virtual void OnInit(AbstractUnitEntity executor)
	{
	}
}
public abstract class UnitCommand<TParams> : UnitCommand where TParams : UnitCommandParams
{
	public new TParams Params => (TParams)base.Params;

	protected UnitCommand([NotNull] TParams @params)
		: base(@params)
	{
	}
}
