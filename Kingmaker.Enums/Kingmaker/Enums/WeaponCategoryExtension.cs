using Kingmaker.Utility.DotNetExtensions;

namespace Kingmaker.Enums;

public static class WeaponCategoryExtension
{
	private class DataItem
	{
		public readonly WeaponCategory Category;

		public readonly WeaponSubCategory[] SubCategories;

		public DataItem(WeaponCategory category, params WeaponSubCategory[] subCategories)
		{
			Category = category;
			SubCategories = subCategories;
		}
	}

	private static readonly DataItem[] Data = new DataItem[0];

	private static DataItem Item(WeaponCategory category, params WeaponSubCategory[] subCategories)
	{
		return new DataItem(category, subCategories);
	}

	public static bool HasSubCategory(this WeaponCategory category, WeaponSubCategory subCategory)
	{
		if (subCategory == WeaponSubCategory.None)
		{
			return true;
		}
		return Data.FirstItem((DataItem i) => i.Category == category)?.SubCategories.HasItem(subCategory) ?? false;
	}

	public static WeaponSubCategory[] GetSubCategories(this WeaponCategory category)
	{
		DataItem dataItem = Data.FirstItem((DataItem i) => i.Category == category);
		if (dataItem == null)
		{
			return new WeaponSubCategory[0];
		}
		return dataItem.SubCategories;
	}

	public static bool ContainsAny(this WeaponCategoryFlags flags, WeaponCategoryFlags otherFlags)
	{
		return (flags & otherFlags) != 0;
	}

	public static bool Contains(this WeaponCategoryFlags flags, WeaponCategory value)
	{
		return ((uint)flags & (uint)(1 << (int)value)) != 0;
	}

	public static bool ContainsAny(this WeaponFamilyFlags flags, WeaponFamilyFlags otherFlags)
	{
		return (flags & otherFlags) != 0;
	}

	public static bool Contains(this WeaponFamilyFlags flags, WeaponFamily value)
	{
		return ((uint)flags & (uint)(1 << (int)value)) != 0;
	}

	public static bool ContainsAny(this WeaponSubCategoryFlags flags, WeaponSubCategoryFlags otherFlags)
	{
		return (flags & otherFlags) != 0;
	}

	public static bool Contains(this WeaponSubCategoryFlags flags, WeaponSubCategory value)
	{
		return ((uint)flags & (uint)(1 << (int)value)) != 0;
	}
}
