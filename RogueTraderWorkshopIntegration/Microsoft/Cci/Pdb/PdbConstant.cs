namespace Microsoft.Cci.Pdb;

internal class PdbConstant
{
	internal string name;

	internal uint token;

	internal object value;

	internal PdbConstant(string name, uint token, object value)
	{
		this.name = name;
		this.token = token;
		this.value = value;
	}

	internal PdbConstant(BitAccess bits)
	{
		bits.ReadUInt32(out token);
		bits.ReadUInt8(out var b);
		bits.ReadUInt8(out var b2);
		switch (b2)
		{
		case 0:
			value = b;
			break;
		case 128:
			switch (b)
			{
			case 0:
			{
				bits.ReadInt8(out var b3);
				value = b3;
				break;
			}
			case 1:
			{
				bits.ReadInt16(out var num6);
				value = num6;
				break;
			}
			case 2:
			{
				bits.ReadUInt16(out var num5);
				value = num5;
				break;
			}
			case 3:
			{
				bits.ReadInt32(out var num4);
				value = num4;
				break;
			}
			case 4:
			{
				bits.ReadUInt32(out var num3);
				value = num3;
				break;
			}
			case 5:
				value = bits.ReadFloat();
				break;
			case 6:
				value = bits.ReadDouble();
				break;
			case 9:
			{
				bits.ReadInt64(out var num2);
				value = num2;
				break;
			}
			case 10:
			{
				bits.ReadUInt64(out var num);
				value = num;
				break;
			}
			case 16:
			{
				bits.ReadBString(out var text);
				value = text;
				break;
			}
			case 25:
				value = bits.ReadDecimal();
				break;
			}
			break;
		}
		bits.ReadCString(out name);
	}
}
