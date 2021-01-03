using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using BitTorrentAnonymizer.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Logging;

namespace BitTorrentAnonymizer.Middleware
{
    public class ProxyMiddleware
    {
        private readonly RequestDelegate _nextMiddleware;

        public ProxyMiddleware(RequestDelegate nextMiddleware)
        {
            _nextMiddleware = nextMiddleware;
        }

        public async Task Invoke(HttpContext pipelineHttpContext,
            ILogger<ProxyMiddleware> logger,
            AnonymizerHttpClient anonymizerHttpClient)
        {
            string BuildTargetUri(HttpRequest request)
            {
                // assume ?targetUri={...} as target uri
                var resultUri = request.Query.TryGetValue("targetUri", out var result)
                    ? result.FirstOrDefault()
                    : null;

                if (resultUri == null || request.QueryString.Value == null)
                {
                    return null;
                }

                // append &param1={...}&param2={...} to target uri
                var indexOfAmpersand = request.QueryString.Value.IndexOf('&');
                return indexOfAmpersand > 0
                    ? resultUri + request.QueryString.Value.Substring(indexOfAmpersand)
                    : resultUri;
            }

            void CopyResponseHeaders(HttpContext ctx, HttpResponseMessage responseMessage)
            {
                foreach (var header in responseMessage.Headers)
                {
                    ctx.Response.Headers[header.Key] = header.Value.ToArray();
                }

                ctx.Response.Headers.Remove("transfer-encoding");
            }

            async Task CopyResponseContent(HttpContext ctx, HttpResponseMessage responseMessage)
            {
                var content = await responseMessage.Content.ReadAsByteArrayAsync();
                await ctx.Response.Body.WriteAsync(content);
            }

            HttpRequestMessage CreateTargetRequest(HttpContext context, Uri uri)
            {
                HttpMethod GetMethod(string method)
                {
                    return method switch
                    {
                        _ when HttpMethods.IsDelete(method) => HttpMethod.Delete,
                        _ when HttpMethods.IsGet(method) => HttpMethod.Get,
                        _ when HttpMethods.IsHead(method) => HttpMethod.Head,
                        _ when HttpMethods.IsOptions(method) => HttpMethod.Options,
                        _ when HttpMethods.IsPatch(method) => HttpMethod.Patch,
                        _ when HttpMethods.IsPost(method) => HttpMethod.Post,
                        _ when HttpMethods.IsPut(method) => HttpMethod.Put,
                        _ when HttpMethods.IsTrace(method) => HttpMethod.Trace,
                        _ => new HttpMethod(method),
                    };
                }

                void CopyContentAndHeaders(HttpContext ctx, HttpRequestMessage targetRequestMessage)
                {
                    var requestMethod = ctx.Request.Method;

                    if (!HttpMethods.IsGet(requestMethod) &&
                        !HttpMethods.IsHead(requestMethod) &&
                        !HttpMethods.IsDelete(requestMethod) &&
                        !HttpMethods.IsTrace(requestMethod))
                    {
                        var streamContent = new StreamContent(ctx.Request.Body);
                        targetRequestMessage.Content = streamContent;
                    }

                    foreach (var header in ctx.Request.Headers)
                    {
                        targetRequestMessage.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray());
                    }
                }

                var requestMessage = new HttpRequestMessage();
                CopyContentAndHeaders(context, requestMessage);

                requestMessage.RequestUri = uri;
                requestMessage.Headers.Host = uri.Host;
                requestMessage.Method = GetMethod(context.Request.Method);

                return requestMessage;
            }

            logger.LogTrace($@"[USER=>ANONYMIZER] Request:
{pipelineHttpContext.Request.GetDisplayUrl()}
{string.Join(Environment.NewLine, pipelineHttpContext.Request.Headers.Select(h => h.Key + " " + string.Join("|", h.Value)))}");

            var targetUri = new Uri(BuildTargetUri(pipelineHttpContext.Request));

            if (targetUri != null)
            {
                var targetRequestMessage = CreateTargetRequest(pipelineHttpContext, targetUri);

                logger.LogTrace($@"[ANONYMIZER=>TARGET] Request:
{targetUri}
{string.Join(Environment.NewLine, targetRequestMessage.Headers.Select(h => h.Key + " " + string.Join("|", h.Value)))}");

                using var responseMessage = await anonymizerHttpClient.Client.SendAsync(targetRequestMessage,
                    HttpCompletionOption.ResponseHeadersRead, pipelineHttpContext.RequestAborted);

                logger.LogTrace($@"[TARGET=>ANONYMIZER] Response:
{targetRequestMessage.RequestUri} {responseMessage.StatusCode}
{string.Join(Environment.NewLine, responseMessage.Headers.Select(h => h.Key + " " + string.Join("|", h.Value)))}");

                pipelineHttpContext.Response.StatusCode = (int) responseMessage.StatusCode;
                CopyResponseHeaders(pipelineHttpContext, responseMessage);
                await CopyResponseContent(pipelineHttpContext, responseMessage);
                return;
            }

            await _nextMiddleware(pipelineHttpContext);
        }
    }
}
