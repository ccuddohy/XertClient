using System.Collections.Generic;
using System.Threading.Tasks;

namespace XertClient
{
	public interface IClient
	{
		Task<List<Client.XertWorkout>> GetUsersWorkouts();
		Task Login(string userName, string password);
	}
}