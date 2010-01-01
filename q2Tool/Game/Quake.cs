using System;
using System.Diagnostics;
using System.Collections.Generic;
using q2Tool.Commands;
using Jv.Networking;

namespace q2Tool
{
	public partial class Quake
	{
		#region Constants
		public const int MaxClients = 256;
		public const int MaxModels = 256;
		public const int MaxSounds = 256;
		public const int MaxImages = 256;
		public const int MaxLights = 256;
		public const int MaxItems = 256;
		public const int MaxGeneral = MaxClients * 2;
		#endregion

		public static string Path { get; private set; }
		public string Name { get; private set; }
		public string CFG { get; set; }

		public Quake(string path) : this(path, string.Empty) { }

		public Quake(string path, string name)
		{
			if (string.IsNullOrEmpty(path))
				throw new Exception("Empty game path");

			Path = path.Replace("\\", "/");

			if (name == string.Empty)
				Name = Path.Substring(Path.LastIndexOf('/') + 1, Path.Length - 5 - Path.LastIndexOf('/'));
			else
				Name = name;

			#region CreateProxy
			_fakeServerCommands = new Queue<IServerCommand>();
			_fakeClientCommands = new Queue<IClientCommand>();

			_proxy = new UdpProxy();
			_proxy.OnDestinationMessage += ParseServerData;
			_proxy.OnSourceMessage += ParseClientData;
			#endregion
		}

		public string Ip { get; private set; }
		public int Port { get; private set; }

		public void Run(string server)
		{
			int serverPort = 27910;
			string[] address = server.Split(':');

			if (address[0] == string.Empty)
				throw new Exception("Empty server address");

			if (address.Length > 2)
				throw new Exception(string.Format("Bad server address: \"{0}\"", server));

			if (address.Length > 1)
			{
				try
				{
					serverPort = int.Parse(address[1]);
				}
				catch (Exception ex)
				{
					throw new Exception(string.Format("Bad server port: \"{0}\"", address[1]), ex);
				}
			}
			Port = serverPort;
			Ip = address[0];
			Run(Ip, Port);
		}

		public void Run(string serverAddress, int serverPort)
		{
			StartProxy(27910, serverAddress, serverPort);
			var processList = Process.GetProcessesByName(Name);
			if (processList.Length < 1)
			{
				var launchEventArgs = new GameLaunchEventArgs();

				string customCFG = Settings.ReadValue("Game", "CustomCFG");
				var pi = new ProcessStartInfo(Path,
				                              "+set game action " + launchEventArgs.CustomArgs +
				                              (CFG != string.Empty ? " +exec " + CFG : "") + " +connect localhost:27910")
				         	{WorkingDirectory = Path};
				Process.Start(pi);
			}
		}
	}
}
