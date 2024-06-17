using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Faithlife.Utility;

public static class GuidUtility
{
	public static readonly Guid DnsNamespace = new Guid("6ba7b810-9dad-11d1-80b4-00c04fd430c8");

	public static readonly Guid UrlNamespace = new Guid("6ba7b811-9dad-11d1-80b4-00c04fd430c8");

	public static readonly Guid IsoOidNamespace = new Guid("6ba7b812-9dad-11d1-80b4-00c04fd430c8");

	public static bool TryParse(string value, out Guid guid)
	{
		return Guid.TryParse(value, out guid);
	}

	public static string ToLowerNoDashString(this Guid guid)
	{
		return guid.ToString("N");
	}

	public static Guid FromLowerNoDashString(string value)
	{
		return TryFromLowerNoDashString(value) ?? throw new FormatException("The string '" + (value ?? "(null)") + "' is not a no-dash lowercase GUID.");
	}

	public static Guid? TryFromLowerNoDashString(string value)
	{
		if (TryParse(value, out var guid) && !(value != guid.ToLowerNoDashString()))
		{
			return guid;
		}
		return null;
	}

	public static Guid Create(Guid namespaceId, string name)
	{
		return Create(namespaceId, name, 5);
	}

	public static Guid Create(Guid namespaceId, string name, int version)
	{
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		return Create(namespaceId, Encoding.UTF8.GetBytes(name), version);
	}

	public static Guid Create(Guid namespaceId, byte[] nameBytes)
	{
		return Create(namespaceId, nameBytes, 5);
	}

	public static Guid Create(Guid namespaceId, byte[] nameBytes, int version)
	{
		if (version != 3 && version != 5)
		{
			throw new ArgumentOutOfRangeException("version", "version must be either 3 or 5.");
		}
		byte[] array = namespaceId.ToByteArray();
		SwapByteOrder(array);
		byte[] buffer = array.Concat(nameBytes).ToArray();
		byte[] sourceArray;
		using (HashAlgorithm hashAlgorithm = ((version == 3) ? ((HashAlgorithm)MD5.Create()) : ((HashAlgorithm)SHA1.Create())))
		{
			sourceArray = hashAlgorithm.ComputeHash(buffer);
		}
		byte[] array2 = new byte[16];
		Array.Copy(sourceArray, 0, array2, 0, 16);
		array2[6] = (byte)((array2[6] & 0xFu) | (uint)(version << 4));
		array2[8] = (byte)((array2[8] & 0x3Fu) | 0x80u);
		SwapByteOrder(array2);
		return new Guid(array2);
	}

	internal static void SwapByteOrder(byte[] guid)
	{
		SwapBytes(guid, 0, 3);
		SwapBytes(guid, 1, 2);
		SwapBytes(guid, 4, 5);
		SwapBytes(guid, 6, 7);
	}

	private static void SwapBytes(byte[] guid, int left, int right)
	{
		byte b = guid[left];
		guid[left] = guid[right];
		guid[right] = b;
	}
}
