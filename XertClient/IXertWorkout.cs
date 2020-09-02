using System.Collections.Generic;

namespace XertClient
{
	public interface IXertWorkout
	{
		string _id { get; set; }
		float advisorScore { get; set; }
		string coach { get; set; }
		string description { get; set; }
		float difficulty { get; set; }
		string duration { get; set; }
		string focus { get; set; }
		string name { get; set; }
		string owner { get; set; }
		string path { get; set; }
		string rating { get; set; }
		bool recommended { get; set; }
		List<XertWorkout.Set> sets { get; set; }
		string workout { get; set; }
		float xss { get; set; }
	}
}