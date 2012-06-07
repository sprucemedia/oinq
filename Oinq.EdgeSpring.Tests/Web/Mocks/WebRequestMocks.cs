using System;
using System.IO;
using System.Net;
using System.Text;

namespace Oinq.EdgeSpring.Tests.Web
{
    public class WebRequestCreateMock : IWebRequestCreate
    {
        private static WebRequest _nextRequest;
        private static Object _lockObject = new Object();

        protected static WebRequest NextRequest
        {
            get { return _nextRequest; }
            set
            {
                lock (_lockObject)
                {
                    _nextRequest = value;
                }
            }
        }

        public WebRequest Create(Uri uri)
        {
            return NextRequest;
        }

        public static WebRequestMock CreateWebRequestMock(String response)
        {
            WebRequestMock request = new WebRequestMock(response);
            NextRequest = request;
            return request;
        }

        public static WebRequestMock CreateErrorWebRequestMock(String response)
        {
            WebRequestMock request = new ErrorWebRequestMock(response);
            NextRequest = request;
            return request;
        }
    }

    public class WebRequestMock : WebRequest
    {
        private MemoryStream _requestStream = new MemoryStream();
        private MemoryStream _responseStream;

        public WebRequestMock(String response)
        {
            _responseStream = new MemoryStream(Encoding.UTF8.GetBytes(response));
        }

        public override String Method { get; set; }
        public override String ContentType { get; set; }
        public override Int64 ContentLength { get; set; }

        public String ContentAsString()
        {
            return Encoding.UTF8.GetString(_requestStream.ToArray());
        }

        public override Stream GetRequestStream()
        {
            return _requestStream;
        }

        public override WebResponse GetResponse()
        {
            return new WebResponseMock(_responseStream);
        }  
    }

    public class ErrorWebRequestMock : WebRequestMock
    {
        public ErrorWebRequestMock(String response)
            : base(response)
        {
        }

        public override WebResponse GetResponse()
        {
            var wex = new WebException("Mock WebException", null, WebExceptionStatus.UnknownError, null);
            throw wex;
        }
    }

    public class WebResponseMock : WebResponse
    {
        private Stream _responseStream;

        public WebResponseMock(Stream responseStream)
        {
            _responseStream = responseStream;
        }

        public override Stream GetResponseStream()
        {
 	        return _responseStream;
        }
    }
}
