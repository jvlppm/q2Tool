using q2Tool.Commands;

namespace q2Tool
{
	public class RawPackage
	{
		public int CurrentPosition { get; set; }

		public byte[] Data { get; protected set; }
		public int Id { get; protected set; }
		public int Ack { get; protected set; }

		public RawPackage(byte[] data)
		{
			CurrentPosition = 0;
			Data = data;

			Id = ReadInt();
			//if (Id != -1)
				Ack = ReadInt();
		}

		public RawPackage(int size)
		{
			CurrentPosition = 0;
			Data = new byte[size];
		}

		public RawPackage(int id, int ack, ICommand package)
		{
			CurrentPosition = 0;
			Data = new byte[8 + package.Size()];
			Id = id;
			Ack = ack;
			WriteInt(id);
			WriteInt(ack);
			package.WriteTo(this);
		}

		public bool EndOfData { get { return CurrentPosition >= Data.Length; } }

		#region Read Data
		public byte ReadByte()
		{
			return Data[CurrentPosition++];
		}

		public short ReadShort()
		{
			return (short)((ReadByte()) + (((uint)ReadByte()) << 8));
		}

		public int ReadInt()
		{
			return (int)((ReadByte()) + (((uint)ReadByte()) << 8) + (((uint)ReadByte()) << 16) + (((uint)ReadByte()) << 24));
		}

		public string ReadString()
		{
			string text = string.Empty;
			char ch;
			do
			{
				ch = (char)ReadByte();
				if (ch != '\0')
					text += ch;
			} while (ch != '\0' && CurrentPosition < Data.Length);

			if(Data.Length < CurrentPosition)
			{
				
			}

			return text;
		}
		#endregion

		#region WriteData
		public void WriteByte(byte value)
		{
			Data[CurrentPosition++] = value;
		}
		public void WriteShort(short value)
		{
			WriteByte((byte)(value & 0xff));
			WriteByte((byte)(value >> 8));
		}
		public void WriteInt(int value)
		{
			WriteByte((byte)(value & 0xff));
			WriteByte((byte)((value >> 8) & 0xff));
			WriteByte((byte)((value >> 16) & 0xff));
			WriteByte((byte)(value >> 24));
		}
		public void WriteString(string text)
		{
			foreach (char ch in text)
				WriteByte((byte)ch);
			WriteByte(0x00);
		}
		public void WriteBytes(byte[] bytes)
		{
			bytes.CopyTo(Data, CurrentPosition);
			CurrentPosition += bytes.Length;
		}
		#endregion
	}
}
