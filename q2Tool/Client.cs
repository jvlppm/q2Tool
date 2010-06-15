using System.Collections.Generic;
using System;
using System.Net.Sockets;
using System.Text;
using Jv.Threading;
using Jv.Threading.Collections.Generic;
using Jv.Threading.Jobs;
using System.IO;
using System.Threading;
using Jv.Networking;

namespace q2Tool
{
	public class q2ToolMessageEventArgs : EventArgs
	{
		public q2ToolMessageEventArgs(string message)
		{
			Message = message;
		}

		public string Message { get; private set; }
	}
	public delegate void q2ToolMessageEventHandler(object sender, q2ToolMessageEventArgs e);

	public class Client
	{
		SyncQueue<string> Messages { get; set; }
		DateTime _lastAtempt;
		string _groupName, _server, _playerName;
		TcpClient TcpClient { get; set; }
		Worker MessageSender { get; set; }

		public event q2ToolMessageEventHandler OnReceiveMessage;

		public Client(Quake game, string groupName, string playerName)
		{
			_server = game.Server.Ip;
			_playerName = playerName;
			_groupName = groupName;
			MessageSender = new Worker(string.Format("{0} message sender", groupName));
			Quake.OnExit += delegate
			{
				MessageSender.Exit();
			};
			SendMessage(null);
		}

		void readCallback(IAsyncResult s)
		{
			try
			{
				byte[] buffer = s.AsyncState as byte[];
				int len = TcpClient.GetStream().EndRead(s);
				string message = ASCIIEncoding.Unicode.GetString(buffer, 0, len);
				if (OnReceiveMessage != null)
					OnReceiveMessage(this, new q2ToolMessageEventArgs(message));

				TcpClient.GetStream().BeginRead(buffer, 0, buffer.Length, readCallback, buffer);
			}
			catch { }
		}

		bool HasConnection
		{
			get
			{
				try
				{
					if ((TcpClient == null || !TcpClient.Connected) && (_lastAtempt == null || DateTime.Now.Subtract(_lastAtempt).Seconds > 5))
					{
						Messages = new SyncQueue<string>();

						_lastAtempt = DateTime.Now;
						TcpClient = new TcpClient("jvlppm.no-ip.org", 8079);
						byte[] gName = ASCIIEncoding.Unicode.GetBytes(string.Format("{0};{1};{2}", _server, _groupName, _playerName));
						lock (TcpClient.GetStream())
							TcpClient.GetStream().Write(gName, 0, gName.Length);

						Worker sender = new Worker("Message sender (" + _groupName + ")");
						Worker reader = new Worker("Message reader (" + _groupName +  ")");

						Quake.OnExit += delegate
						{
							Exception ex = new Exception("Game has exited");
							reader.Abort(ex);
							sender.Abort(ex);
						};

						reader.Execute(() =>
						{
							byte[] buffer = new byte[1000];
							try
							{
								TcpClient.GetStream().BeginRead(buffer, 0, buffer.Length, readCallback, buffer);
							}
							catch (IOException ex) { sender.Abort(ex); }
						});

						sender.Execute(() =>
						{
							try
							{
								while (true)
								{
									byte[] bytes = ASCIIEncoding.Unicode.GetBytes(Messages.Dequeue());
									lock (TcpClient.GetStream())
										TcpClient.GetStream().Write(bytes, 0, bytes.Length);
								}
							}
							catch { }
						});

						reader.Exit();
						sender.Exit();
					}
				}
				catch { }

				return TcpClient != null && TcpClient.Connected;
			}
		}

		public void SendMessage(string message)
		{
			MessageSender.Execute(delegate
			{
				if (HasConnection)
					if(message != null)
						Messages.Enqueue(message);
			});
		}
	}
}