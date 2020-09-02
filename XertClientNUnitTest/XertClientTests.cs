using NUnit.Framework;
using System.Net.Http;
using System.Threading.Tasks;
using System.Net;
using XertClient;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework.Constraints;

namespace XertClientNUnitTest
{
	public class Tests
	{
		/// <summary>
		/// A Mock of the HttpMessageHandler for unit testing the client.
		/// </summary>
		class MyMockHandler : HttpMessageHandler
		{
			/// <summary>
			/// Customize the return response message from SendAsync. 
			/// </summary>
			public string SendAsyncReturnContent { get; private set; }
			
			public HttpStatusCode SendAsyncStatusCode {get; private set;}
			public bool ThrowSendAsync { get; private set; }
			public void SetToThrowSendAsync()
			{
				ThrowSendAsync = true;
			}

			public MyMockHandler()
			{
				ThrowSendAsync = false;
			}

			public void SetAsyncReturnContent(string newContent)
			{
				SendAsyncReturnContent = newContent;
			}
			public void SetAsyncStatusCode(HttpStatusCode code)
			{
				SendAsyncStatusCode = code;
			}
			protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, System.Threading.CancellationToken cancellationToken)
			{
				if(ThrowSendAsync)
				{
					throw new Exception("No such host");
				}
				return await Task.FromResult(new HttpResponseMessage()
				{
					StatusCode = SendAsyncStatusCode,
					Content = new StringContent(SendAsyncReturnContent)
				});
			}
		}

		/// <summary>
		/// Test Login works, without error, when the response is as expected.
		/// </summary>
		[Test]
		public void TestLoginWorking()
		{
			MyMockHandler mockHandler = new MyMockHandler();

			mockHandler.SetAsyncReturnContent(
				"{\"access_token\":\"a4a4a4\"," +
				"\"expires_in\":604800," +
				"\"token_type\":\"Bearer\"," +
				"\"scope\":\"basic\"," +
				"\"refresh_token\":\"b9b9b9\"}");

			mockHandler.SetAsyncStatusCode(HttpStatusCode.OK);
			Client _client = new Client(mockHandler);
			Assert.DoesNotThrowAsync(() => _client.Login("userName", "password"));

			Assert.AreEqual("a4a4a4", _client._Token.access_token);

			Assert.AreEqual("a4a4a4", _client._Token.access_token);
			Assert.AreEqual("b9b9b9", _client._Token.refresh_token);
			Assert.AreEqual("Bearer", _client._Token.token_type);
			Assert.AreEqual(604800, _client._Token.expires_in);
			Assert.AreEqual("basic", _client._Token.scope);
		}

		/// <summary>
		/// This test is just an example, for my reference, of testing for an exception without using Assert.ThrowsAsync.
		/// Before I had experience with using Assert,ThrowsAsync 
		/// </summary>
		[Test]
		public void TestLoginRaw()
		{
			MyMockHandler mockHandler = new MyMockHandler();
			mockHandler.SetAsyncReturnContent("[{'id':1,'value':'1'}]");
			mockHandler.SetAsyncStatusCode(HttpStatusCode.OK);
			Client _client = new Client(mockHandler);
			bool exceptionThrown = false;
			string expectedExceptionMessage = "One or more errors occurred. (XertClient LogIn failed! Error: ?. " +
				"Error description: Cannot deserialize the current JSON array (e.g. [1,2,3]) into type " +
				"'XertClient.Client+BarrierToken' because the type requires a JSON object (e.g. {\"name\":\"value\"}) " +
				"to deserialize correctly.\r\nTo fix this error either change the JSON to a JSON object (e.g. " +
				"{\"name\":\"value\"}) or change the deserialized type to an array or a type that implements a collection" +
				" interface (e.g. ICollection, IList) like List<T> that can be deserialized from a JSON array. " +
				"JsonArrayAttribute can also be added to the type to force it to deserialize from a JSON array.\r\nPath '', " +
				"line 1, position 1.)";

			try
			{
				Task task = _client.Login("userName", "password");
				task.Wait();
			}
			catch (Exception ex)
			{
				exceptionThrown = true;
				Assert.AreEqual(expectedExceptionMessage, ex.Message);
			}
			Assert.IsTrue(exceptionThrown);
		}

		/// <summary>
		/// Testing what happens when there is a problem with the deserializing to the BarrierToken object.
		/// </summary>
		[Test]
		public void TestLoginJsonConvertExcept()
		{
			MyMockHandler mockHandler = new MyMockHandler();
			mockHandler.SetAsyncReturnContent("[{'id':1,'value':'1'}]");
			mockHandler.SetAsyncStatusCode(HttpStatusCode.OK);

			Client _client = new Client(mockHandler);
		
			string expectedMessage = "XertClient LogIn failed! Error: ?. Error description: Cannot deserialize the current JSON array " +
				"(e.g. [1,2,3]) into type 'XertClient.Client+BarrierToken' because the type requires a JSON object (e.g. {\"name\":\"value\"}) " +
				"to deserialize correctly.\r\nTo fix this error either change the JSON to a JSON object (e.g. {\"name\":\"value\"}) or change " +
				"the deserialized type to an array or a type that implements a collection interface (e.g. ICollection, IList) like List<T> that " + "" +
				"can be deserialized from a JSON array. JsonArrayAttribute can also be added to the type to force it to deserialize from a JSON " +
				"array.\r\nPath '', line 1, position 1.";

			Exception ex = Assert.ThrowsAsync<Exception>(() => _client.Login("userName", "password"));
			Assert.That(ex.Message, Is.EqualTo(expectedMessage));
		}

		/// <summary>
		/// Testing different HttpStatusCode errors from the SendAsync call and is an example of Overkill.
		/// </summary>
		[Test]
		public void TestLoginProblemResponses()
		{
			MyMockHandler mockHandler = new MyMockHandler();
			mockHandler.SetAsyncReturnContent("[{'id':1,'value':'1'}]");

			mockHandler.SetAsyncStatusCode(HttpStatusCode.BadRequest);
			Client _client = new Client(mockHandler);
			string expectedMessage = "Login exception. status code: BadRequestReasonPhrase: Bad Request Content: System.Net.Http.StringContent RequestMessage: ";
			Exception ex = Assert.ThrowsAsync<Exception>(() => _client.Login("userName", "password"));
			Assert.That(ex.Message, Is.EqualTo(expectedMessage));
		
			
			mockHandler.SetAsyncStatusCode(HttpStatusCode.NotFound);
			_client = new Client(mockHandler);
			expectedMessage = "Login exception. status code: NotFoundReasonPhrase: Not Found Content: System.Net.Http.StringContent RequestMessage: ";
			ex = Assert.ThrowsAsync<Exception>(() => _client.Login("userName", "password"));
			Assert.That(ex.Message, Is.EqualTo(expectedMessage));

			mockHandler.SetAsyncStatusCode(HttpStatusCode.InternalServerError);
			_client = new Client(mockHandler);
			expectedMessage = "Login exception. status code: InternalServerErrorReasonPhrase: Internal Server Error Content: System.Net.Http.StringContent RequestMessage: ";
			ex = Assert.ThrowsAsync<Exception>(() => _client.Login("userName", "password"));
			Assert.That(ex.Message, Is.EqualTo(expectedMessage));

			mockHandler.SetAsyncStatusCode(HttpStatusCode.ServiceUnavailable);
			_client = new Client(mockHandler);
			expectedMessage = "Login exception. status code: ServiceUnavailableReasonPhrase: Service Unavailable Content: System.Net.Http.StringContent RequestMessage: ";
			ex = Assert.ThrowsAsync<Exception>(() => _client.Login("userName", "password"));
			Assert.That(ex.Message, Is.EqualTo(expectedMessage));
		}

		/// <summary>
		/// Checking what the error would look like if bad user name password combination. The response is what the actual API returns as of 8/2020
		/// </summary>
		[Test]
		public void TestLoginBadPW()
		{
			MyMockHandler mockHandler = new MyMockHandler();
			mockHandler.SetAsyncReturnContent("[{'id':1,'value':'1'}]");
			mockHandler.SetAsyncStatusCode(HttpStatusCode.Unauthorized);
			Client _client = new Client(mockHandler);
			string expectedMessage = "Login exception. status code: UnauthorizedReasonPhrase: Unauthorized Content: System.Net.Http.StringContent RequestMessage: ";
			Exception ex = Assert.ThrowsAsync<Exception>(() => _client.Login("userName", "password"));
			Assert.That(ex.Message, Is.EqualTo(expectedMessage));
		}

		//"User name and password must to be entered to log in"
		[Test]
		public void TestLoginEmpty()
		{
			MyMockHandler mockHandler = new MyMockHandler();
			mockHandler.SetAsyncReturnContent("[{'id':1,'value':'1'}]");
			mockHandler.SetAsyncStatusCode(HttpStatusCode.Unauthorized);
			Client _client = new Client(mockHandler);
			string expectedMessage = "User name and password must to be entered to log in.";
			Exception ex = Assert.ThrowsAsync<Exception>(() => _client.Login("x", ""));
			Assert.That(ex.Message, Is.EqualTo(expectedMessage));

			_client = new Client(mockHandler);
			ex = Assert.ThrowsAsync<Exception>(() => _client.Login("", "x"));
			Assert.That(ex.Message, Is.EqualTo(expectedMessage));

			_client = new Client(mockHandler);
			ex = Assert.ThrowsAsync<Exception>(() => _client.Login("", ""));
			Assert.That(ex.Message, Is.EqualTo(expectedMessage));
		}

		/// <summary>
		/// Playing with no internet connection response
		/// </summary>
		[Test]
		public void TestLoginNonetwork()
		{
			MyMockHandler mockHandler = new MyMockHandler();
			mockHandler.SetAsyncReturnContent("[{'id':1,'value':'1'}]");
			mockHandler.SetAsyncStatusCode(HttpStatusCode.OK);
			mockHandler.SetToThrowSendAsync();
			Client _client = new Client(mockHandler);

			string expectedMessage = "No such host";
			Exception ex = Assert.ThrowsAsync<Exception>(() => _client.Login("userName", "password"));
			Assert.That(ex.Message, Is.EqualTo(expectedMessage));
		}

		/// <summary>
		/// Response if the token is missing. The token will be needed for some other functions. This is probably an
		/// unlikely failure mode but worth a couple of minutes.
		/// </summary>
		[Test]
		public void TestLoginTokenEmpty()
		{
			MyMockHandler mockHandler = new MyMockHandler();

			mockHandler.SetAsyncReturnContent(
				"{\"access_token\":\"\"," +
				"\"expires_in\":6," +
				"\"token_type\":\"B\"," +
				"\"scope\":\"b\"," +
				"\"refresh_token\":\"b\"}");

			mockHandler.SetAsyncStatusCode(HttpStatusCode.OK);
			Client _client = new Client(mockHandler);

			string expectedMessage = "XertClient LogIn failed! The access token is empty";

		   Exception ex = Assert.ThrowsAsync<Exception>(() => _client.Login("userName", "password"));
			Assert.That(ex.Message, Is.EqualTo(expectedMessage));
		}


		/// <summary>
		/// Gets all the workouts and checks the first index for the parameters. Does not look at all the parameters for the sets.
		/// </summary>
		[Test]
		public void GetWorkoutsTestNormalIndxZero()
		{
			MyMockHandler mockHandler = new MyMockHandler();
			//the properties of the file are Set in the solution, to always copy to output directory (build action none).
			string workOutStr = File.ReadAllText(TestContext.CurrentContext.TestDirectory + "\\FiveWorkouts.txt");
			mockHandler.SetAsyncReturnContent(workOutStr);
			mockHandler.SetAsyncStatusCode(HttpStatusCode.OK);
			Client _client = new Client(mockHandler);
			_client._Token = new Client.BarrierToken()
			{
				access_token = "abc123",
				expires_in = 994999,
				token_type = "Barer",
				scope = "basic",
				refresh_token = "999999"
			};
			
			Assert.DoesNotThrowAsync(() => _client.GetUsersWorkouts());
			Task<List<XertWorkout>> taskWOs = _client.GetUsersWorkouts();
			taskWOs.Wait();
			List<XertWorkout> xertWorkouts = taskWOs.Result;
			Assert.AreEqual(5, xertWorkouts.Count);
			Assert.AreEqual(xertWorkouts.ElementAt(0).advisorScore, 120.5041, 0.00001);
			Assert.IsNull(xertWorkouts.ElementAt(0).coach);
			Assert.AreEqual(xertWorkouts.ElementAt(0).description, "Tabata's are a type of HIIT workout designed with a 2:1 work:recovery" +
				" ratio,\r\n meaning you only get 10 seconds of rest between each 20 second bout of exercise. That short interval wont allow" +
				" you to fully recover,\r\n which is one reason it's great for building endurance.  Notice the high difficulty score of this" +
				" workout - d to complete this workout.");
			Assert.AreEqual(xertWorkouts.ElementAt(0).difficulty, 100.496933, 0.0000001);
			Assert.AreEqual(xertWorkouts.ElementAt(0).duration, "01:31:51");
			Assert.AreEqual(xertWorkouts.ElementAt(0).focus, "Puncheur");
			Assert.AreEqual(xertWorkouts.ElementAt(0).name, "SMART - Tabata's 300");
			Assert.AreEqual(xertWorkouts.ElementAt(0).owner, "Admin");
			Assert.AreEqual(xertWorkouts.ElementAt(0).path, "0uw1x5xz5gwsyb4x");
			Assert.AreEqual(xertWorkouts.ElementAt(0).rating, "\u2666\u2666\u2666\u00bd - Difficult");
			Assert.IsFalse(xertWorkouts.ElementAt(0).recommended);
			Assert.AreEqual(xertWorkouts.ElementAt(0).sets.Count, 13);
			Assert.AreEqual(xertWorkouts.ElementAt(0).workout, "Tempo");
			Assert.AreEqual(xertWorkouts.ElementAt(0).xss, 134, 0.1);
			Assert.AreEqual(xertWorkouts.ElementAt(0)._id, "5b195559ba614c73458b456c");
		}

		[Test]
		public void GetWorkoutsTestNormalIndxX()
		{
			MyMockHandler mockHandler = new MyMockHandler();
			//the properties of the file are Set in the solution, to always copy to output directory (build action none).
			string workOutStr = File.ReadAllText(TestContext.CurrentContext.TestDirectory + "\\FiveWorkouts.txt");
			mockHandler.SetAsyncReturnContent(workOutStr);
			mockHandler.SetAsyncStatusCode(HttpStatusCode.OK);
			Client _client = new Client(mockHandler);
			_client._Token = new Client.BarrierToken()
			{
				access_token = "abc123",
				expires_in = 994999,
				token_type = "Barer",
				scope = "basic",
				refresh_token = "999999"
			};

			Assert.DoesNotThrowAsync(() => _client.GetUsersWorkouts());
			Task<List<XertWorkout>> taskWOs = _client.GetUsersWorkouts();
			taskWOs.Wait();
			List<XertWorkout> xertWorkouts = taskWOs.Result;

			Assert.AreEqual(xertWorkouts.ElementAt(4).name, "VO2max Target MPA - to 40% Reserve");
			Assert.AreEqual(xertWorkouts.ElementAt(4).owner, "Admin");
			Assert.AreEqual(xertWorkouts.ElementAt(4).path, "2vkxjituk8uj2i5n");
			Assert.AreEqual(xertWorkouts.ElementAt(4).rating, "\u2666\u2666\u2666\u2666 - Tough");
			Assert.IsFalse(xertWorkouts.ElementAt(4).recommended);
			Assert.AreEqual(xertWorkouts.ElementAt(4).sets.Count, 4);
			Assert.IsNull(xertWorkouts.ElementAt(4).workout);
			Assert.AreEqual(xertWorkouts.ElementAt(4).xss, 132, 0.1);
			Assert.AreEqual(xertWorkouts.ElementAt(4)._id, "589faf94ba614c30668b45d8");

			// check a couple of sets
			Assert.AreEqual(xertWorkouts.ElementAt(4).sets.ElementAt(0).DT_RowId, "ilmj1nei");
			Assert.AreEqual(xertWorkouts.ElementAt(4).sets.ElementAt(0).duration.type, "absolute");
			Assert.AreEqual(xertWorkouts.ElementAt(4).sets.ElementAt(0).duration.value, "01:00");
			Assert.AreEqual(xertWorkouts.ElementAt(4).sets.ElementAt(0).interval_count, "5");
			Assert.AreEqual(xertWorkouts.ElementAt(4).sets.ElementAt(0).name, "Warmup");
			Assert.AreEqual(xertWorkouts.ElementAt(4).sets.ElementAt(0).power.type, "relative_ltp");
			Assert.AreEqual(xertWorkouts.ElementAt(4).sets.ElementAt(0).power.value, 70);
			Assert.AreEqual(xertWorkouts.ElementAt(4).sets.ElementAt(0).rib_duration.type, "absolute");
			Assert.AreEqual(xertWorkouts.ElementAt(4).sets.ElementAt(0).rib_duration.value, "01:00");
			Assert.AreEqual(xertWorkouts.ElementAt(4).sets.ElementAt(0).rib_power.type, "relative_ltp");
			Assert.AreEqual(xertWorkouts.ElementAt(4).sets.ElementAt(0).rib_power.value, 90);
			Assert.AreEqual(xertWorkouts.ElementAt(4).sets.ElementAt(0).sequence, "0");

			Assert.AreEqual(xertWorkouts.ElementAt(4).sets.ElementAt(3).DT_RowId, "ilmj1oaz");
			Assert.AreEqual(xertWorkouts.ElementAt(4).sets.ElementAt(3).duration.type, "absolute");
			Assert.AreEqual(xertWorkouts.ElementAt(4).sets.ElementAt(3).duration.value, "05:00");
			Assert.AreEqual(xertWorkouts.ElementAt(4).sets.ElementAt(3).interval_count, "1");
			Assert.AreEqual(xertWorkouts.ElementAt(4).sets.ElementAt(3).name, "Cooldown");
			Assert.AreEqual(xertWorkouts.ElementAt(4).sets.ElementAt(3).power.type, "relative_ftp");
			Assert.AreEqual(xertWorkouts.ElementAt(4).sets.ElementAt(3).power.value, 50);
			Assert.AreEqual(xertWorkouts.ElementAt(4).sets.ElementAt(3).rib_duration.type, "absolute");
			Assert.AreEqual(xertWorkouts.ElementAt(4).sets.ElementAt(3).rib_duration.value, "00:00");
			Assert.AreEqual(xertWorkouts.ElementAt(4).sets.ElementAt(3).rib_power.type, "absolute");
			Assert.AreEqual(xertWorkouts.ElementAt(4).sets.ElementAt(3).rib_power.value, 0);
			Assert.AreEqual(xertWorkouts.ElementAt(4).sets.ElementAt(3).sequence, "3");
		}

		[Test]
		public void GetWorkoutsTestWorkingClasic()
		{
			//_Token
			MyMockHandler mockHandler = new MyMockHandler();
			//the properties of the file are Set in the solution, to always copy to output directory (build action none).
			string workOutStr = File.ReadAllText(TestContext.CurrentContext.TestDirectory + "\\FiveWorkouts.txt");
			mockHandler.SetAsyncReturnContent(workOutStr);
			mockHandler.SetAsyncStatusCode(HttpStatusCode.OK);
			Client _client = new Client(mockHandler);
			_client._Token = new Client.BarrierToken()
			{
				access_token = "abc123",
				expires_in = 994999,
				token_type = "Barer",
				scope = "basic",
				refresh_token = "999999"
			};
			bool exceptionThrown = false;
			try
			{
				Task<List<XertWorkout>> taskWOs = _client.GetUsersWorkouts();
				taskWOs.Wait();
				List<XertWorkout> xertWorkouts = taskWOs.Result;
				Assert.AreEqual(5, xertWorkouts.Count);
			}
			catch (Exception)
			{
				exceptionThrown = true;
			}
			Assert.AreEqual(false, exceptionThrown);
		}
	}


}