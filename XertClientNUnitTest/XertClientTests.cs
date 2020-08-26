using NUnit.Framework;
using System.Net.Http;
using System.Threading.Tasks;
using System.Net;
using XertClient;
using System;


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

			//try
			//{
			//	Task task = _client.Login("userName", "password");
			//	task.Wait();

			//	int x = 1;
			//}
			//catch (Exception exA)
			//{
			//	int t = 1;
			//}
		}

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

			//try
			//{
			//	Task t = _client.Login("userName", "password");
			//	t.Wait();
			//}
			//catch (Exception e)
			//{
			//	int t = 1;
			//}
		}

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

			//try
			//{
			//	Task t = _client.Login("userName", "password");
			//	t.Wait();
			//}
			//catch(Exception e)
			//{
			//	int t = 1;
			//}
		}


	}
}