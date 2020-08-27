using NUnit.Framework;
using System.Net.Http;
using System.Threading.Tasks;
using System.Net;
using XertClient;
using System;
using System.Collections.Generic;

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
			protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, System.Threading.CancellationToken cancellationToken)
			{
				if(ThrowSendAsync)
				{
					throw new Exception("No such host");
				}
				return Task.FromResult(new HttpResponseMessage()
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


		//public async Task<List<XertWorkout>> GetUsersWorkouts()
		//List<Client.XertWorkout> WOs = await _client.GetUsersWorkouts();
		[Test]
		public void GetWorkoutsTestWorking()
		{
			//_Token
			MyMockHandler mockHandler = new MyMockHandler();
			mockHandler.SetAsyncReturnContent(
				"{\"StatusCode\":\"200," +
				 "\"ReasonPhrase\":\"'OK'\"," +
				 "\"Version\":\"1.1," +
				 "\"Content\":\"System.Net.Http.HttpConnectionResponseContent\"," +
				 "{\"Headers\":"+
				  "\"Server\":\"nginx / 1.14.0,\"" +
				  "\"Server\":\"(Ubuntu),\"" +
				  "\"Transfer-Encoding\":\"chunked,\"" +
				  "\"Connection\":\"keep-alive\", +" +
				  "\"Content-Type\":\"application/json,\"" +
				  "}}"
				);

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
			try
			{
				Task<List<Client.XertWorkout>> taskWOs = _client.GetUsersWorkouts();
				taskWOs.Wait();
			}
			catch (Exception ex)
			{
				int t = 1;
				//exceptionThrown = true;
				//Assert.AreEqual(expectedExceptionMessage, ex.Message);
			}


		}


	}
}