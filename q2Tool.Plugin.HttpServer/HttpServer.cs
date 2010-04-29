using System;
using Jv.Plugins;
using q2Tool.Commands.Server;
using System.IO;
using HttpServer;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using HttpServer.Headers;

namespace q2Tool
{
	public class HttpServer : Plugin
	{
		const int BufferSize = 40;
		EventArgs[] console_buffer = new EventArgs[BufferSize];
		int initial = 0, last = 0;
		readonly Dictionary<Player, Dictionary<string, int>> _binds;
		int PlayerNum;

		public HttpServer()
		{
			_binds = new Dictionary<Player, Dictionary<string, int>>();
		}

		protected override void OnGameStart()
		{
			HttpListener listener = HttpListener.Create(System.Net.IPAddress.Any, 8080);
			listener.RequestReceived += OnRequest;
			listener.Start(5);

			Quake.OnServerData += (s, e) => PlayerNum = e.Command.PlayerNum;

			GetPlugin<PAction>().OnPlayerMessage += PlayerMessage;
			GetPlugin<PAction>().OnServerMessage += (s, e) => AddToBuffer(e);
			Quake.OnServerCenterPrint += (s, e) => AddToBuffer(e);
		}

		void PlayerMessage(PAction sender, PlayerMessageEventArgs e)
		{
			if (!_binds.ContainsKey(e.Player))
				_binds.Add(e.Player, new Dictionary<string, int>());

			if (_binds[e.Player].ContainsKey(e.CodedMessage))
				_binds[e.Player][e.CodedMessage]++;
			else
				_binds[e.Player].Add(e.CodedMessage, 1);

			AddToBuffer(e);
		}

		private string DecodeEvent(EventArgs eventArgs)
		{
			try
			{
				string text = "", color = "black";
				bool bold = false;

				if (eventArgs is ServerMessageEventArgs)
				{
					var evnt = (ServerMessageEventArgs)eventArgs;
					text = highlightNames(evnt.Message);
					color = "gray";
					switch (evnt.Level)
					{
						case Print.PrintLevel.Low:
							color = "red";
							break;
						case Print.PrintLevel.Medium:
							color = "green";
							break;
						case Print.PrintLevel.High:
							color = "#aaaaaa";
							break;
					}
				}
				else if (eventArgs is PlayerMessageEventArgs)
				{
					var evnt = (PlayerMessageEventArgs)eventArgs;
					if (_binds[evnt.Player][evnt.CodedMessage] <= 1)
						color = "blue";

					if (evnt.Dead)
						text += "[DEAD] ";
					text += "<b>" + evnt.Player.Name + "</b>: " + highlightNames(findEmoticons(evnt.Message)).Replace("\\", "\\\\") + "\n";
				}
				else if (eventArgs is ServerCommandEventArgs<CenterPrint>)
				{
					var evnt = (ServerCommandEventArgs<CenterPrint>)eventArgs;
					bold = true;
					text = evnt.Command.Message.TrimEnd('\n', '\r') + "\n";
				}

				if (bold) text = "<b>" + text + "</b>";
				text = "<font color='" + color + "'>" + text + "</font>";

				return text.Replace("\"", "\\\"").Replace("\n", "<br>").Replace("\r", "<p>");
			}
			catch (Exception ex)
			{
				return "<font color='red'>Error: " + ex.Message + "</font><br/>";
			}
		}

		private string highlightNames(string message)
		{
			foreach (var player in GetPlugin<PAction>().Players)
				message = message.Replace(player.Name, string.Format("<b>{0}</b>", player.Name));

			return message;
		}

		private string findEmoticons(string msg)
		{
			var emoticons = new Dictionary<string, string>
			{
				{"=D", "http://sites.google.com/site/jvlppm2/a34.png"},
				{"xD", "http://sites.google.com/site/jvlppm2/a32.png"},
				{":)", "http://sites.google.com/site/jvlppm2/a21.png"},
				{":(", "http://sites.google.com/site/jvlppm2/a41.png"},
				{"(:", "http://sites.google.com/site/jvlppm2/a22.png"},
				{"):", "http://sites.google.com/site/jvlppm2/a44.png"}
			};

			foreach(var emoticon in emoticons)
				msg = msg.Replace(emoticon.Key, "<img src='" + emoticon.Value + "'></img>");

			return msg;
		}

		void AddToBuffer(EventArgs text)
		{
			int prev = last - 1;
			if (prev < 0)
				prev += BufferSize;

			if (DecodeEvent(console_buffer[prev]) != DecodeEvent(text))
			{
				console_buffer[last] = text;
				int next = (last + 1) % BufferSize;
				if (next == initial)
					initial = (initial + 1) % BufferSize;
				last = next;
			}
		}

		private void OnRequest(object sender, RequestEventArgs e)
		{
			e.Response.Connection.Type = ConnectionType.Close;

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
						html.Append("\"" + DecodeEvent(console_buffer[i]) + "\"");
					}
					html.Append("]");
					break;
				case "/send":
					Quake.ExecuteCommand(e.Request.Parameters["cmd"]);
					break;
				default:
					html.Append(@"
<html>
	<head>
		<script language='javascript' src='http://code.jquery.com/jquery-1.4.2.min.js'></script>
		<script language='javascript' src='http://www.evanbot.com/bin/2009/06/jquery-timer/jquery.timer.js'></script>
		<script language='javascript'>
			function update(){
				$.ajax({
					url: 'get',
					dataType: 'json',
					error: function(a,b,c){
						$('#controls').attr('disabled', 'disables');
						$.timer(1000,update);
					},
					timeout: 3000,
					success: function(data){
						$('#controls').removeAttr('disabled');
						var output = '';
						for(var i = 0; i <= data.length; i++)
							if(data[i]) output += data[i];
						$('#content').html(output);
						$.timer(1000,update);
					}
				});
			}
			function checkLogin()
			{
				if($('#nick').val() != '')
				{
					$('#lblNick').html($('#nick').val() + ': ');
					$('#controls').show();
					$('#login').hide();
				}
				else
				{
					$('#controls').hide();
					$('#login').show();
				}
			}
			function send(cmd){
				$('#controls').hide('fast');
				$.ajax({
					url: 'send',
					data: {cmd: cmd},
					success: function(data){
						$('#message').val('');
						$('#controls').show('fast');
					}
				});
			}
			var defaultSay = true;
			function sendDefault() {
				if(defaultSay)
					send('say ' + $('#nick').val() + ': ' + $('#message').val());
				else
					send('echo ' + $('#nick').val() + '; ' + $('#message').val());
			}
			$(document).ready(function() {
				update();
				checkLogin();
				$('#do_login').click(function() { checkLogin(); });
				$('#say').click(function(){
					defaultSay = true;
					sendDefault();
				});
				$('#execute').click(function(){
					defaultSay = false;
					sendDefault();
				});
			});
		</script>
	</head>
	<body>
		<div id='login'>
			<label for='nick'>Nick: </label> <input type='text' id='nick' name='nick' value='" + e.Request.Parameters["nick"] + @"' onkeydown='if(event.keyCode == 13)checkLogin()'></input>
			<input type='button' id='do_login' value='OK'></input>
		</div>
		<div id='controls'>
			<label id='lblNick' for='message'></label>
			<input id='message' type='text' onkeydown='if(event.keyCode == 13)sendDefault()'></input>
			<input id='say' type='button' value='say'></input>
			<input id='execute' type='button' value='execute'></input>
			<!-- <input type='checkbox' id='anonymous' disabled='disabled' checked='checked'></input>Anonymous<br> -->
		</div>
		<div id='content'></div>
	</body>
</html>");
					break;
			}

			byte[] buffer = Encoding.UTF8.GetBytes(html.ToString());
			e.Response.Body.Write(buffer, 0, buffer.Length);
		}
	}
}
