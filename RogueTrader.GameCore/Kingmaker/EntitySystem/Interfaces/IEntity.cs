using System;
using Kingmaker.EntitySystem.Entities.Base;
using UnityEngine;

namespace Kingmaker.EntitySystem.Interfaces;

public interface IEntity : IDisposable
{
	string UniqueId { get; }

	EntityServiceProxy Proxy { get; set; }

	bool IsDisposed { get; }

	bool IsInGame { get; set; }

	bool Suppressed { get; set; }

	bool IsInFogOfWar { get; set; }

	Vector3 Position { get; set; }

	IEntityViewBase View { get; }

	bool IsInState { get; }

	bool IsRevealed { get; set; }

	bool WillBeDestroyed { get; set; }

	bool Destroyed { get; }

	bool IsInitialized { get; }

	bool IsDisposingNow { get; }

	bool IsPostLoadExecuted { get; }

	EntityRef Ref { get; }

	new string ToString();

	void DetachView();
}
