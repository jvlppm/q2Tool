using q2Tool.Commands;

namespace q2Tool
{
	public interface IStringPackage : ICommand
	{
		string Message { get; }
	}

	public class StringPackage : IStringPackage
	{
		public string Message { get; set; }

		//[string message]
		public StringPackage(byte code, RawPackage data)
		{
			Message = data.ReadString();
			TypeCode = code;
		}
		protected StringPackage(byte code, string message)
		{
			Message = message;
			TypeCode = code;
		}

		#region ICommand Members
		public int Size()
		{
			if (string.IsNullOrEmpty(Message))
				return 0;
			return Message.Length + 2;
		}

		public void WriteTo(RawPackage data)
		{
			if (string.IsNullOrEmpty(Message))
				return;

			data.WriteByte(TypeCode);
			data.WriteString(Message);
		}

		byte TypeCode { get; set; }
		#endregion
	}
}
