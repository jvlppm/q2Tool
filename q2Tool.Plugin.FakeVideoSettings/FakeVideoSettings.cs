using System.Collections.Generic;

namespace q2Tool
{
	public class FakeVideoSettings : Plugin
	{
		readonly Dictionary<string, string> _values;

		public FakeVideoSettings()
		{
			_values = new Dictionary<string, string>
           	{
				{ "$timescale", "1" },
				{ "$gl_driver", "opengl32" },
				{ "$gl_modulate", "1" },
				{ "$gl_lockpvs", "0" },
				{ "$gl_clear", "0" },
				{ "$gl_dynamic", "1" }
           	};
		}

		protected override void OnGameStart()
		{
			Quake.OnStuffText += OnStuffText;
		}

		void OnStuffText(Quake sender, ServerCommandEventArgs<Commands.Server.StuffText> e)
		{
			foreach (string var in _values.Keys)
				e.Command.Message = e.Command.Message.Replace(var, _values[var]);
		}
	}
}
