using System.Net;
using System.Text;
using System.Web;

namespace OAuthStart.OAuth2.AuthorizationCodeFlow
{
    internal class OAuthCallbackHandler : IDisposable
    {
        private readonly HttpListener _listener;
        private readonly AuthorizationCodeFlowService _authService;
        private readonly string _callbackPath;
        private bool _disposed;

        internal OAuthCallbackHandler(AuthorizationCodeFlowService authService, string listenerPrefix, string callbackPath = "/callback")
        {
            _authService = authService;
            _callbackPath = callbackPath;
            _listener = new HttpListener();
            _listener.Prefixes.Add(listenerPrefix);
        }

        internal async Task<TokenResponse> StartAndWaitForCallbackAsync(CancellationToken cancellationToken = default)
        {
            _listener.Start();
            Console.WriteLine($"DEBUG --- OAuthCallbackHandler listening for callback...");

            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    HttpListenerContext context = await _listener.GetContextAsync();
                    HttpListenerRequest request = context.Request;
                    HttpListenerResponse response = context.Response;

                    if (request.Url?.AbsolutePath == _callbackPath)
                    {
                        System.Collections.Specialized.NameValueCollection queryParams = HttpUtility.ParseQueryString(request.Url.Query);
                        string? code = queryParams["code"];
                        string? state = queryParams["state"];
                        string? error = queryParams["error"];
                        string? errorDescription = queryParams["error_description"];

                        if (!string.IsNullOrEmpty(error))
                        {
                            await SendResponseAsync(response, $"Authorization failed: {error} - {errorDescription}", HttpStatusCode.BadRequest);
                            throw new InvalidOperationException($"Authorization failed: {error} - {errorDescription}");
                        }

                        if (string.IsNullOrEmpty(code) || string.IsNullOrEmpty(state))
                        {
                            await SendResponseAsync(response, "Missing code or state parameter.", HttpStatusCode.BadRequest);
                            continue;
                        }

                        try
                        {
                            TokenResponse tokenResponse = await _authService.HandleCallbackAsync(code, state);
                            await SendResponseAsync(response, "Authorization successful! You can close this window.", HttpStatusCode.OK);
                            return tokenResponse;
                        }
                        catch (Exception ex)
                        {
                            await SendResponseAsync(response, $"Error: {ex.Message}", HttpStatusCode.InternalServerError);
                            throw;
                        }
                    }
                    else
                    {
                        await SendResponseAsync(response, "Not Found", HttpStatusCode.NotFound);
                    }
                }

                throw new OperationCanceledException("Callback wait was cancelled.");
            }
            finally
            {
                _listener.Stop();
            }
        }

        private static async Task SendResponseAsync(HttpListenerResponse response, string content, HttpStatusCode statusCode)
        {
            string html = $"""
                <!DOCTYPE html>
                <html>
                <head><title>OAuth Callback</title></head>
                <body style="font-family: Arial, sans-serif; text-align: center; padding-top: 50px;">
                    <h2>{content}</h2>
                </body>
                </html>
                """;

            byte[] buffer = Encoding.UTF8.GetBytes(html);
            response.StatusCode = (int)statusCode;
            response.ContentType = "text/html";
            response.ContentLength64 = buffer.Length;
            await response.OutputStream.WriteAsync(buffer);
            response.Close();
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _listener.Close();
                _disposed = true;
            }
        }
    }
}