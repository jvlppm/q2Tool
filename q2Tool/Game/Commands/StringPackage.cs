using q2Tool.Commands;
using Jv.Networking;

namespace q2Tool
{
	public interface IStringPackage : ICommand
	{
		string Message { get; set; }
	}

	public interface IClientStringPackage : IClientCommand, IStringPackage { }
	public interface IServerStringPackage : IServerCommand, IStringPackage { }

	public class StringPackage : IStringPackage
	{
		public string Message { get; set; }

		//[string message]
        public StringPackage(byte code, RawData data)
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

        public void WriteTo(RawData data)
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
