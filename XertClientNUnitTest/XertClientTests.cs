using NUnit.Framework;
using System.Net.Http;
using System.Threading.Tasks;
using System.Threading;
using System.Net;
using XertClient;
using System;
using Moq;
using Moq.Protected;

namespace XertClientNUnitTest
{
	public class Tests
	{
		//class MockHandler : HttpMessageHandler
		//{
		//	public virtual HttpResponseMessage Send(HttpRequestMessage request)
		//	{
		//		throw new NotImplementedException("Now we can setup this method with our mocking framework");
		//	}
		//	protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, System.Threading.CancellationToken cancellationToken)
		//	{
		//		return Task.FromResult(Send(request));
		//	}
		//}

		Mock<HttpMessageHandler> handlerMock;// = new Mock<HttpMessageHandler>(MockBehavior.Strict);
		[SetUp]
		public void Setup()
		{
			handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);

			handlerMock
				 .Protected()
				 // Setup the PROTECTED method to mock
				 .Setup<Task<HttpResponseMessage>>(
					  "SendAsync",
					  ItExpr.IsAny<HttpRequestMessage>(),
					  ItExpr.IsAny<CancellationToken>()
				 )
				 // prepare the expected response of the mocked http call
				 .ReturnsAsync(new HttpResponseMessage()
				 {
					   StatusCode = HttpStatusCode.OK,
					   Content = new StringContent("[{'id':1,'value':'1'}]"),
				 })
				 .Verifiable();


		}

		[Test]
		public void TestLogin()
		{
			
			//MockHandler mockHandler = new MockHandler();
			Client _client = new Client(handlerMock.Object);

			//Task<LogEntity> task = Task.Run<LogEntity>(async () => await GetLogAsync());

			Assert.DoesNotThrow(() => Task.Run(async () => await _client.Login("userName", "password")));
			
		}
		
	}
}