namespace q2Tool.Commands.Server
{
	public class ConfigStringPlayerInfo
	{
		public int Id
		{
			get
			{
				Update(_configString);
				return _configString.SubCode;
			}
			set { _configString.SubCode = value; }
		}

		string _name, _model, _skin;

		public string Name
		{
			get
			{
				Update(_configString); 
				return _name;
			}
			set
			{
				_name = value;
				_configString.Message = string.Format(@"{0}\{1}/{2}", _name, _model, _skin);
			}
		}

		public string Model
		{
			get
			{
				Update(_configString);
				return _model;
			}
			set
			{
				_model = value;
				_configString.Message = string.Format(@"{0}\{1}/{2}", _name, _model, _skin);
			}
		}

		public string Skin
		{
			get
			{
				Update(_configString);
				return _skin;
			}
			set
			{
				_skin = value;
				_configString.Message = string.Format(@"{0}\{1}/{2}", _name, _model, _skin);
			}
		}

		readonly ConfigString _configString;

		public ConfigStringPlayerInfo(ConfigString configString)
		{
			_configString = configString;
			Update(configString);
		}

		void Update(ConfigString configString)
		{
			string[] words = configString.Message.Split('\\');
			string[] look = words[1].Split('/');
			_name = words[0];
			_model = look[0];
			_skin = look[1];
		}
	}
}
