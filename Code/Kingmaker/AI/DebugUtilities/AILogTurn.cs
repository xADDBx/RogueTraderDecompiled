using System;
using Kingmaker.EntitySystem.Entities;

namespace Kingmaker.AI.DebugUtilities;

public class AILogTurn : AILogObject
{
	private enum Type
	{
		Start,
		End
	}

	private readonly Type type;

	private readonly MechanicEntity entity;

	public static AILogTurn StartTurn(MechanicEntity entity)
	{
		return new AILogTurn(Type.Start, entity);
	}

	public static AILogTurn EndTurn(MechanicEntity entity)
	{
		return new AILogTurn(Type.End, entity);
	}

	private AILogTurn(Type type, MechanicEntity entity)
	{
		this.type = type;
		this.entity = entity;
	}

	public override string GetLogString()
	{
		return type switch
		{
			Type.Start => $"[ START TURN ]: {entity}", 
			Type.End => $"[ END TURN ]: {entity}", 
			_ => throw new ArgumentOutOfRangeException(), 
		};
	}
}
