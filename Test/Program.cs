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
				IXertClient _client = new Client();
				Console.WriteLine("Enter User-name");
				string userName = Console.ReadLine();
				Console.WriteLine("Enter Password");
				ConsoleColor origBG = Console.BackgroundColor; // Store original values
				ConsoleColor origFG = Console.ForegroundColor;
				Console.BackgroundColor = ConsoleColor.White; // Set the block color (could be anything)
				Console.ForegroundColor = ConsoleColor.White;
				string PassWord = Console.ReadLine();
				Console.BackgroundColor = origBG; // revert back to original
				Console.ForegroundColor = origFG;
				Console.WriteLine("One Moment");
				await _client.Login(userName, PassWord);
				Console.WriteLine("working");
				List <XertWorkout> WOs = await _client.GetUsersWorkouts();
				Console.WriteLine("There were {0} workouts returned", WOs.Count);
			}
			catch (Exception excpt)
			{
				Console.WriteLine(excpt.Message);
			}

			Console.WriteLine("\nAny Key to finish");
			Console.ReadKey();
		}
	}
}
