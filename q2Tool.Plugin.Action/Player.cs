namespace q2Tool
{
	public class Player
	{
		public Player(string name, int id)
		{
			Id = id;
			Name = name;
		}
		public string Name { get; set; }
		public int Id { get; private set; }
	}
}
