using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Text;

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("XertClientNUnitTest")]

namespace XertClient
{
	public class Client
	{
		internal Client(HttpMessageHandler handler)
		{
			_Client = new HttpClient(handler);
		}
		public Client()
		{
			_Client = new HttpClient();
		}

		readonly HttpClient _Client;
		private BarrierToken _Token;

		/// <summary>
		/// Gets an access token, available to registered users. The function should throw on any login problem.
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
				if (response.IsSuccessStatusCode)
				{
					string respString = await response.Content.ReadAsStringAsync();
					try
					{
						_Token = JsonConvert.DeserializeObject<BarrierToken>(respString);
					}
					catch(Exception ex)
					{
						int t = 1;
						if (null == _Token)
						{
							_Token = new BarrierToken()
							{
								error = "?",
								error_description = ex.Message
							};
						}
					}
				}
				else
				{
					StringBuilder sBErr = new StringBuilder("Login exception. status code: ");
					sBErr.Append(response.StatusCode.ToString());
					sBErr.Append("ReasonPhrase: ");
					sBErr.Append(response.ReasonPhrase);
					sBErr.Append(" Content: ");
					sBErr.Append( response.Content);
					sBErr.Append(" RequestMessage: ");
					sBErr.Append( response.RequestMessage);
					throw new Exception(sBErr.ToString());		
				}
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

		/// <summary>
		/// Returns a list of workouts. This function requires login'
		/// is required to obtain a token. The curl call is:
		/// curl -X GET "https://www.xertonline.com/oauth/workouts" -H "Authorization: Bearer <token>"
		/// </summary>
		/// <param name="token"></param>
		/// <returns></returns>
		public async Task<List<XertWorkout>> GetUsersWorkouts()
		{
			if (null == _Token)
			{
				throw new Exception("GetUsersWorkouts() Exception! You must Log In before calling this function!");
			}
			using (var request = new HttpRequestMessage(new HttpMethod("GET"), "https://www.xertonline.com/oauth/workouts"))
			{
				request.Headers.TryAddWithoutValidation("Authorization", "Bearer " + _Token.access_token);
				HttpResponseMessage response = await _Client.SendAsync(request);
				string respString = await response.Content.ReadAsStringAsync();
				UserWorkouts userWOs = JsonConvert.DeserializeObject<UserWorkouts>(respString);
				if (userWOs.success)
				{
					return userWOs.workouts;
				}
				else
				{
					Exception execp = new Exception("There was an unknown error in GetUsersWorkouts. There were " +
						Convert.ToString(userWOs.workouts.Count) + " workouts.");
					throw execp;
				}
			}
		}

		public class XertWorkout
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
			public float advisorScore { get; set; }
			public float difficulty { get; set; }
			public string rating { get; set; }
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
				workouts = new List<XertWorkout>();
			}
			public List<XertWorkout> workouts { get; set; }
			public bool success { get; set; }
			
		}

	}

	
}
