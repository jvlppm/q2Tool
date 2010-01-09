namespace q2Tool
{
	public static class Extensions
	{
		public static string NormalizePath(this string path)
		{
			string finalPath = path;
			string[] removeChars = { "*", ":", "\\", "/" };
			foreach (string ch in removeChars)
				finalPath = finalPath.Replace(ch, "");
			return finalPath;
		}
	}
}
