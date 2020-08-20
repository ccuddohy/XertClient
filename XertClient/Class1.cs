using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace XertClient
{
	class Client
	{
		readonly HttpClient _Client;
		private BarrierToken _Token;
		public Client()
		{
			_Client = new HttpClient();
			_Token = null;
		}
		

		/// <summary>
		/// Returns an access token, available to registered users. The function should throw on any login problem.
		/// The curl message is:
		/// curl -u xert_public:xert_public -POST "https://www.xertonline.com/oauth/token" -d 'grant_type=refresh_token' -d 'refresh_token=1badfdee0f72b847dc91d1baf9e5c095c774c14a'
		/// </summary>
		/// <returns>BarrierTokenObject/returns>
		public async Task Login(string userName, string password)
		{
			using (var request = new HttpRequestMessage(new HttpMethod("POST"), "https://www.xertonline.com/oauth/token"))
			{
				var base64authorization = Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes("xert_public:xert_public"));
				request.Headers.TryAddWithoutValidation("Authorization", $"Basic {base64authorization}");

				var contentList = new List<string>();
				contentList.Add("grant_type=password");
				contentList.Add("username=" + userName);
				contentList.Add("password=" + password);
				request.Content = new StringContent(string.Join("&", contentList));
				request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/x-www-form-urlencoded");
				HttpResponseMessage response = await _Client.SendAsync(request);
				string respString = await response.Content.ReadAsStringAsync();
				_Token = JsonConvert.DeserializeObject<BarrierToken>(respString);
			}
			if (null != _Token)
			{
				if (!string.IsNullOrEmpty(_Token.error) && string.IsNullOrEmpty(_Token.access_token)) //error not empty and access token is empty
				{
					throw new Exception("XertClient LogIn failed! Error: " + _Token.error + ". Error description: " + _Token.error_description);
				}
				else if (string.IsNullOrEmpty(_Token.access_token))//access token empty 
				{
					throw new Exception("XertClient LogIn failed! The cause is unknown"  );
				}
			}
			else
			{
				throw new Exception("XertClient LogIn failed! The login token is null but the cause is unknown");
			}
		}

		class BarrierToken
		{
			public string access_token { get; set; }
			public int expires_in { get; set; }
			public string token_type { get; set; }
			public string scope { get; set; }
			public string refresh_token { get; set; }
			public string error { get; set; }
			public string error_description { get; set; }
		}

		public class UserWorkouts
		{
			public UserWorkouts()
			{
				success = false;
				workouts = new List<WorkoutObj>();
			}
			public List<WorkoutObj> workouts { get; set; }
			public bool success { get; set; }
			public class WorkoutObj
			{
				public class set
				{
					public class ValuePairIntString
					{
						public string type { get; set; }
						public float value { get; set; }
					}
					public class ValuePairStringString
					{
						public string type { get; set; }
						public string value { get; set; }
					}
					public string DT_RowId { get; set; }
					public string sequence { get; set; }
					public string name { get; set; }
					public ValuePairIntString power { get; set; }
					public ValuePairStringString duration { get; set; }
					public ValuePairIntString rib_power { get; set; }
					public ValuePairStringString rib_duration { get; set; }
					public string interval_count { get; set; }
				}
				public string _id { get; set; }
				public string path { get; set; }
				//public string end { get; set; }
				public string name { get; set; }
				public string description { get; set; }
				public string workout { get; set; }
				public List<set> sets { get; set; }
				public string coach { get; set; }
				public bool recommended { get; set; }
				public string owner { get; set; }
				public string focus { get; set; }
				public float xss { get; set; }
				public string duration { get; set; }
				public float difficulity { get; set; }
				public string rating { get; set; }
			}
		}

	}

	
}
