using System.Collections.Generic;
using System.Threading.Tasks;

namespace XertClient
{
	public interface IXertClient
	{
		Task<List<XertWorkout>> GetUsersWorkouts();
		Task Login(string userName, string password);
	}
}