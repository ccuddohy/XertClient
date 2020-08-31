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
				IClient _client = new Client();
				Console.WriteLine("Enter User-name");
				string userName = Console.ReadLine();
				Console.WriteLine("Enter Password");
				string PassWord = Console.ReadLine();
				await _client.Login(userName, PassWord);
				Console.WriteLine("working");
				List <Client.XertWorkout> WOs = await _client.GetUsersWorkouts();
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
