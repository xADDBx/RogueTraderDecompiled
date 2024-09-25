using Kingmaker.PubSubSystem.Core;

namespace Kingmaker.Items;

public static class ItemEntityExtensions
{
	public static ItemEntity ToItemEntity(this IItemEntity itemEntity)
	{
		return (ItemEntity)itemEntity;
	}
}
