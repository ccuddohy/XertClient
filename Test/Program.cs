using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using XertClient;
using System.Text.Json;
//using System.Linq;

/// <summary>
/// This project is just for quick system level testing of the API and playing with the collection.
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
				List <IXertWorkout> WOs = await _client.GetUsersWorkouts();
				Console.WriteLine("There were {0} workouts returned", WOs.Count);

				do
				{
					Console.WriteLine("\nW to serialize workout to file, R to deserialize file to a list of workouts, E to end");
					ConsoleKeyInfo k = Console.ReadKey();
					if (k.KeyChar == 'R' || k.KeyChar == 'r')
					{
						List<XertWorkout> readWos = GetWorkouts("workouts.json");
						Console.WriteLine("\nThere were {0} workouts read from file", readWos.Count);
						break;
					}
					if (k.KeyChar == 'W' || k.KeyChar == 'w')
					{
						WriteWorkoutsToFileAsync(WOs, "workouts.json");
						break;
					}
					else if (k.KeyChar == 'E' || k.KeyChar == 'e')
					{
						break;
					}
				} while (true);
			}
			catch (Exception excpt)
			{
				Console.WriteLine("\n" +excpt.Message);
			}
			Console.WriteLine("\nany key to close");
			Console.ReadKey();

		}

		static void WriteWorkoutsToFileAsync(List<IXertWorkout> workouts, string path)
		{
			string json = JsonSerializer.Serialize(workouts);
			using (StreamWriter file = new StreamWriter(path, false))
			{
				file.Write(json);
			}
		}

		static List<XertWorkout> GetWorkouts(string path)
		{
			String JSONtxt = File.ReadAllText(path);
			List<XertWorkout> wkouts = JsonSerializer.Deserialize<List<XertWorkout>>(JSONtxt);//JsonSerializer.DeserializeObject<List<XertWorkout>>(JSONtxt);
			return wkouts;
		}

	}
}
