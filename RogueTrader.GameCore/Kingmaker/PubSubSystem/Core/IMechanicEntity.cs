using System;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Enums;
using Pathfinding;
using UnityEngine;

namespace Kingmaker.PubSubSystem.Core;

public interface IMechanicEntity : IEntity, IDisposable
{
	bool IsPlayerFaction { get; }

	bool IsNeutral { get; }

	new Vector3 Position { get; set; }

	IntRect SizeRect { get; }

	Size Size { get; }

	bool IsDirectlyControllable { get; }

	Vector3 Forward { get; }

	bool IsDead { get; }

	bool IsConscious { get; }

	bool IsDeadOrUnconscious { get; }

	bool IsAnimating { get; }

	bool IsAble { get; }

	bool CanRotate { get; }

	bool CanMove { get; }

	bool CanAct { get; }

	bool CanActInTurnBased { get; }

	bool CanCast { get; }

	bool IsInCombat { get; }

	bool IsInPlayerParty { get; }

	bool IsInLockControlCutscene { get; }

	bool IsEnemy(IMechanicEntity entity);
}
