using System;
using Jv.Plugins;
using q2Tool.Commands.Server;
using System.IO;
using HttpServer;
using System.Text;

namespace q2Tool
{
	public class Test : Plugin
	{
		const int BufferSize = 40;
		string[] console_buffer = new string[BufferSize];
		int initial = 0, last = 0;

		protected override void OnGameStart()
		{
			HttpListener listener = HttpListener.Create(System.Net.IPAddress.Any, 8080);
			listener.RequestReceived += OnRequest;
			listener.Start(5);

			Quake.OnServerPrint += Quake_OnServerPrint;
			Quake.OnServerCenterPrint += Quake_OnServerCenterPrint;
		}

		private void OnRequest(object sender, RequestEventArgs e)
		{
			e.Response.Connection.Type = HttpServer.Headers.ConnectionType.Close;

			StringBuilder html = new StringBuilder();

			switch (e.Request.Uri.LocalPath)
			{
				case "/get":
					html.Append("[");
					bool first = true;
					int i = last;
					while (i != initial)
					{
						i = (i <= 0 ? BufferSize : i) - 1;
						if (first) first = false; else html.Append(",");
						html.Append("\"" + console_buffer[i].Replace("\"", "\\\"").Replace("\n", "<br>").Replace("\r", "<p>") + "\"");
					}
					html.Append("]");
					break;
				case "/send":
					Quake.ExecuteCommand(e.Request.Parameters["cmd"]);
					break;
				default:
					html.Append("<html><head><script language=\"javascript\" src=\"http://code.jquery.com/jquery-1.4.2.min.js\"></script><script language=\"javascript\" src=\"http://www.evanbot.com/bin/2009/06/jquery-timer/jquery.timer.js\"></script><script language=\"javascript\">function update(){$.ajax({ url: \"get\", dataType:\"json\", success: function(data){var output = \'\';for(var i = 0; i <= data.length; i++)if(data[i])output += data[i];$(\"#content\").html(output);$.timer(1000,update);}});}function enable(enabled){if(enabled){$(\"#message\").val(\"\");$(\"#say\").removeAttr(\"disabled\");$(\"#execute\").removeAttr(\"disabled\");}else{$(\"#say\").attr(\"disabled\", \"disabled\");$(\"#execute\").attr(\"disabled\", \"disabled\");}}function send(cmd){enable(false);$.ajax({url: \"send\",data: {cmd: cmd},success: function(data){enable(true);}});}$(document).ready(function() {enable(true);update();$(\"#say\").click(function(){var cmd = $(\"#message\").val();if(!$(\"#anonymous\").is(\":checked\")) cmd = \'");
					html.Append("say " + e.Request.Parameters["nick"] + ": ");
					html.Append("\' + $(\"#message\").val();send(cmd);});$(\"#execute\").click(function(){var cmd = $(\"#message\").val();if(!$(\"#anonymous\").is(\":checked\")) cmd = \'");
					html.Append("echo " + e.Request.Parameters["nick"] + "; ");
					html.Append("\' + $(\"#message\").val();send(cmd);});});</script></head><body><input id=\"message\" type=\"text\"></input><input id=\"say\" type=\"button\" value=\"say\"></input><input id=\"execute\" type=\"button\" value=\"execute\"></input><input type=checkbox id=\"anonymous\" ");
					if(string.IsNullOrEmpty(e.Request.Parameters["nick"]))
						html.Append("disabled=\"disabled\" checked=\"checked\"");
					html.Append("/>Anonymous<br><div id=\"content\"></div></body></html>");
					break;
			}

			byte[] buffer = Encoding.UTF8.GetBytes(html.ToString());
			e.Response.Body.Write(buffer, 0, buffer.Length);
		}

		void AddToBuffer(string text)
		{
			console_buffer[last] = text;
			int next = (last + 1) % BufferSize;
			if (next == initial)
				initial = (initial + 1) % BufferSize;
			last = next;
		}

		void Quake_OnServerCenterPrint(Quake sender, ServerCommandEventArgs<CenterPrint> e)
		{
			AddToBuffer("<b>" + e.Command.Message + "</b>");
		}

		void Quake_OnServerPrint(Quake sender, ServerCommandEventArgs<Print> e)
		{
			switch (e.Command.Level)
			{
				case Print.PrintLevel.Chat:
					AddToBuffer(e.Command.Message);
					break;
				case Print.PrintLevel.High:
					AddToBuffer("<font color=\"gray\">" + e.Command.Message + "</font>");
					break;
			}
		}
	}
}
