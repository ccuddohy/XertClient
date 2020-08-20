using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using XertClient;


/// <summary>
/// This project is just for quick system level testing of the API
/// </summary>

namespace Test
{
	class Program
	{
		static async Task Main(string[] args)
		{
			try
			{
				Client _client = new Client();
				await _client.Login("chriscuddohy", "kRLvnHGjLC7J");
				List<Client.XertWorkout> WOs = await _client.GetUsersWorkouts();
				Console.WriteLine("There were {0} workouts returned", WOs.Count);
			}
			catch (Exception excpt)
			{
				Console.WriteLine(excpt.Message);
			}

			Console.WriteLine("\nFINISHED");
			Console.ReadKey();
		}
	}
}
