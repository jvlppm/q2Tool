using System;

namespace q2Tool
{
	//public class RoundBeginEventArgs : EventArgs { }
	//public class RoundEndEventArgs : EventArgs { }

	public delegate void RoundBeginEventHandler(Action sender, EventArgs e);
	public delegate void RoundEndEventHandler(Action sender, EventArgs e);
}
