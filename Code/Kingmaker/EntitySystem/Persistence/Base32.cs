using System;
using System.Text;

namespace Kingmaker.EntitySystem.Persistence;

public static class Base32
{
	private static readonly char[] Digits = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567".ToCharArray();

	private const int Mask = 31;

	private const int Shift = 5;

	private static int CharToInt(char c)
	{
		switch (c)
		{
		case 'A':
		case 'a':
			return 0;
		case 'B':
		case 'b':
			return 1;
		case 'C':
		case 'c':
			return 2;
		case 'D':
		case 'd':
			return 3;
		case 'E':
		case 'e':
			return 4;
		case 'F':
		case 'f':
			return 5;
		case 'G':
		case 'g':
			return 6;
		case 'H':
		case 'h':
			return 7;
		case 'I':
		case 'i':
			return 8;
		case 'J':
		case 'j':
			return 9;
		case 'K':
		case 'k':
			return 10;
		case 'L':
		case 'l':
			return 11;
		case 'M':
		case 'm':
			return 12;
		case 'N':
		case 'n':
			return 13;
		case 'O':
		case 'o':
			return 14;
		case 'P':
		case 'p':
			return 15;
		case 'Q':
		case 'q':
			return 16;
		case 'R':
		case 'r':
			return 17;
		case 'S':
		case 's':
			return 18;
		case 'T':
		case 't':
			return 19;
		case 'U':
		case 'u':
			return 20;
		case 'V':
		case 'v':
			return 21;
		case 'W':
		case 'w':
			return 22;
		case 'X':
		case 'x':
			return 23;
		case 'Y':
		case 'y':
			return 24;
		case 'Z':
		case 'z':
			return 25;
		case '2':
			return 26;
		case '3':
			return 27;
		case '4':
			return 28;
		case '5':
			return 29;
		case '6':
			return 30;
		case '7':
			return 31;
		default:
			throw new FormatException($"Illegal character: `{c}`");
		}
	}

	public static byte[] FromBase32String(string encoded)
	{
		if (encoded == null)
		{
			throw new ArgumentNullException("encoded");
		}
		encoded = encoded.Trim().TrimEnd('=');
		if (encoded.Length == 0)
		{
			return Array.Empty<byte>();
		}
		byte[] array = new byte[encoded.Length * 5 / 8];
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		int num4 = 0;
		string text = encoded;
		for (int i = 0; i < text.Length; i++)
		{
			char c = text[i];
			num4 = CharToInt(c);
			if (num4 < 0)
			{
				throw new FormatException("Illegal character: `" + c + "`");
			}
			num <<= 5;
			num |= num4 & 0x1F;
			num3 += 5;
			if (num3 >= 8)
			{
				array[num2++] = (byte)(num >> num3 - 8);
				num3 -= 8;
			}
		}
		return array;
	}

	public static string ToBase32String(byte[] data, bool padOutput = false)
	{
		return ToBase32String(data, 0, data.Length, padOutput);
	}

	public static string ToBase32String(byte[] data, int offset, int length, bool padOutput = false)
	{
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		if (offset < 0)
		{
			throw new ArgumentOutOfRangeException("offset");
		}
		if (length < 0)
		{
			throw new ArgumentOutOfRangeException("length");
		}
		if (offset + length > data.Length)
		{
			throw new ArgumentOutOfRangeException();
		}
		if (length == 0)
		{
			return "";
		}
		if (length >= 268435456)
		{
			throw new ArgumentOutOfRangeException("data");
		}
		StringBuilder stringBuilder = new StringBuilder((length * 8 + 5 - 1) / 5);
		int num = offset + length;
		int num2 = data[offset++];
		int num3 = 8;
		while (num3 > 0 || offset < num)
		{
			if (num3 < 5)
			{
				if (offset < num)
				{
					num2 <<= 8;
					num2 |= data[offset++] & 0xFF;
					num3 += 8;
				}
				else
				{
					int num4 = 5 - num3;
					num2 <<= num4;
					num3 += num4;
				}
			}
			int num5 = 0x1F & (num2 >> num3 - 5);
			num3 -= 5;
			stringBuilder.Append(Digits[num5]);
		}
		if (padOutput)
		{
			int num6 = 8 - stringBuilder.Length % 8;
			if (num6 > 0)
			{
				stringBuilder.Append('=', (num6 != 8) ? num6 : 0);
			}
		}
		return stringBuilder.ToString();
	}
}
