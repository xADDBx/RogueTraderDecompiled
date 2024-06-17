using System;
using Kingmaker.EntitySystem.Interfaces;

namespace Kingmaker.PubSubSystem.Core;

public interface IItemEntity : IMechanicEntity, IEntity, IDisposable
{
	bool IsIdentified { get; }

	TimeSpan? SellTime { get; }

	int Charges { get; }

	bool IsSpendCharges { get; }

	bool SpendCharges();

	void RestoreCharges();

	void TryIdentify();

	void Identify();
}
