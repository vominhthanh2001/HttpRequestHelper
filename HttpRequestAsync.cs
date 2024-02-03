using HttpRequestHelper.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Policy;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace HttpRequestHelper
{
    public class HttpRequestAsync
    {
        public static Dictionary<string, string> HeaderDefault = new Dictionary<string, string>
        {
            //["Host"] = "www.facebook.com",
            ["sec-ch-ua"] = "\"Not.A/Brand\";v=\"8\", \"Chromium\";v=\"114\", \"Google Chrome\";v=\"114\"",
            ["sec-ch-ua-mobile"] = "?0",
            ["user-agent"] = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/114.0.0.0 Safari/537.36",
            ["sec-ch-ua-platform-version"] = "\"10.0.0\"",
            ["x-asbd-id"] = "129477",
            ["sec-ch-ua-full-version-list"] = "\"Not.A/Brand\";v=\"8.0.0.0\", \"Chromium\";v=\"114.0.5735.134\", \"Google Chrome\";v=\"114.0.5735.134\"",
            ["sec-ch-prefers-color-scheme"] = "light",
            ["sec-ch-ua-platform"] = "\"Windows\"",
            ["accept"] = "*/*",
            //["origin"] = "https://www.facebook.com",
            ["sec-fetch-site"] = "same-origin",
            ["sec-fetch-mode"] = "cors",
            ["sec-fetch-dest"] = "empty",
            //["referer"] = "https://www.facebook.com/",
            ["accept-encoding"] = "gzip, deflate",
            ["accept-language"] = "vi-VN,vi;q=0.9,fr-FR;q=0.8,fr;q=0.7,en-US;q=0.6,en;q=0.5"
        };

        private bool _isRequest;
        private string _cookieTemp;
        private string _proxyTemp;
        private Dictionary<string, string> _headerTemp;

        private CancellationToken _cancellationToken;
        private HttpClient _client;
        private HttpClientHandler _handler;
        private CookieContainer _cookieContainer;
        private HttpResponseMessage _response;
        private TimeSpan _timeout;

        public HttpResponseMessage Response
        {
            get
            {
                return _response;
            }
            set
            {
                _response = value;
            }
        }

        public HttpRequestAsync(TimeSpan timeout = default(TimeSpan))
        {
            Initialize();

            if (timeout != default(TimeSpan))
            {
                SetTimeout(timeout);
            }
        }

        public HttpRequestAsync(string cookie, TimeSpan timeout = default(TimeSpan))
        {
            Initialize();
            SetCookie(cookie, null, null);
            if (timeout != default(TimeSpan))
            {
                SetTimeout(timeout);
            }
        }

        public HttpRequestAsync(string cookie, Dictionary<string, string> headers, TimeSpan timeout = default(TimeSpan))
        {
            Initialize();
            SetCookie(cookie, null, null);
            SetHeader(headers);
            if (timeout != default(TimeSpan))
            {
                SetTimeout(timeout);
            }
        }

        public HttpRequestAsync(string cookie, string path, string domain, Dictionary<string, string> headers, TimeSpan timeout = default(TimeSpan))
        {
            Initialize();
            SetCookie(cookie, path, domain);
            SetHeader(headers);
            if (timeout != default(TimeSpan))
            {
                SetTimeout(timeout);
            }
        }

        private void Initialize()
        {
            _cookieContainer = new CookieContainer();
            _handler = new HttpClientHandler
            {
                AllowAutoRedirect = false,
                AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip,
                CookieContainer = _cookieContainer
            };

            _client = new HttpClient(_handler);
        }

        public void SetTimeout(TimeSpan timeout = default(TimeSpan))
        {
            if (timeout != default(TimeSpan))
            {
                _timeout = timeout;
                _client.Timeout = timeout;
            }
        }

        public void SetCookie(string cookie, string path, string domain)
        {
            IEnumerable<string[]> list = cookie.Split(';').Select(x => x.Split('=')).Where(x => x.Length == 2);

            foreach (string[] info in list)
            {
                SetCookie(info, path, domain);
            }
        }

        public void SetCookie(string[] cookieInfo, string path, string domain)
        {
            string name = cookieInfo[0].Trim();
            string value = cookieInfo[1].Trim();

            Cookie cookieNew = new Cookie(name, value, path, domain);
            _cookieContainer.Add(cookieNew);
        }

        public void SetCookie(CookieModel cookieModel)
        {
            IEnumerable<string[]> cookieList = cookieModel.GetCookieList();

            foreach (string[] cookie in cookieList)
            {
                SetCookie(cookie, cookieModel.Path, cookieModel.Domain);
            }
        }

        public void SetCookieContainer(CookieContainer cookieContainer)
        {
            _cookieContainer = cookieContainer;
        }

        public void SetHeader(Dictionary<string, string> headers, bool isClear = false)
        {
            if (headers == null)
                throw new ArgumentNullException(nameof(headers));

            if (isClear)
                _client.DefaultRequestHeaders.Clear();

            foreach (KeyValuePair<string, string> header in headers)
            {
                _client.DefaultRequestHeaders.TryAddWithoutValidation(header.Key, header.Value);
            }
        }

        public void EditValueHeader(string key)
        {

        }

        public void SetProxy(string ip, string port, string username, string password)
        {
            WebProxy proxy = null;

            if (!string.IsNullOrEmpty(ip) && !string.IsNullOrEmpty(ip))
            {
                proxy = new WebProxy
                {
                    Address = new Uri($"http://{ip}:{port}"),
                    BypassProxyOnLocal = true,
                    UseDefaultCredentials = false,
                };
            }

            if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
            {
                proxy.UseDefaultCredentials = true;
                proxy.Credentials = new NetworkCredential(username, password);
            }

            if (proxy != null)
            {
                _handler.Proxy = proxy;
                _handler.UseProxy = true;
            }
        }

        public void SetProxy(ProxyModel proxy)
        {
            SetProxy(proxy.Ip, proxy.Port, proxy.Username, proxy.Password);
        }

        public void SetCancellationToken(CancellationToken cancellationToken)
        {
            _cancellationToken = cancellationToken;
        }

        private async Task<string> GetTextContent(HttpResponseMessage httpResponseMessage)
        {
            Response = httpResponseMessage;

            byte[] buffer = await httpResponseMessage.Content.ReadAsByteArrayAsync();
            string content = Encoding.UTF8.GetString(buffer, 0, buffer.Length);

            return content;
        }

        public async Task<string> GetAsync(string url)
        {
            var response = _cancellationToken != null ?
                await _client.GetAsync(url, _cancellationToken) : await _client.GetAsync(url);

            string text = await GetTextContent(response);

            return text;
        }

        public async Task<string> PostAsync(string url, string dataContent, string mediaType)
        {
            var content = new StringContent(dataContent, Encoding.UTF8, mediaType);

            var response = _cancellationToken != null ?
                await _client.PostAsync(url, content, _cancellationToken) : await _client.PostAsync(url, content);

            string text = await GetTextContent(response);

            return text;
        }

        public async Task<string> PostAsync(string url, string json)
        {
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = _cancellationToken != null ?
                await _client.PostAsync(url, content, _cancellationToken) : await _client.PostAsync(url, content);

            string text = await GetTextContent(response);

            return text;
        }

        public async Task<string> PostAsync(string url, Dictionary<string, string> dataPost)
        {
            var response = _cancellationToken != null ?
                await _client.PostAsync(url, new FormUrlEncodedContent(dataPost), _cancellationToken) : await _client.PostAsync(url, new FormUrlEncodedContent(dataPost));

            string text = await GetTextContent(response);

            return text;
        }

        public async Task<string> PostAsync(string url, MultipartFormDataContent multipart)
        {
            var response = _cancellationToken != null ?
           await _client.PostAsync(url, multipart, _cancellationToken) : await _client.PostAsync(url, multipart);

            string text = await GetTextContent(response);

            return text;
        }

        public async Task<System.Drawing.Bitmap> DownloadImage(string url)
        {
            System.Drawing.Bitmap image = null;

            var response = _cancellationToken != null ?
            await _client.GetAsync(url, _cancellationToken) : await _client.GetAsync(url);

            response.EnsureSuccessStatusCode();

            var buffer = await response.Content.ReadAsStreamAsync();

            using (MemoryStream memory = new MemoryStream())
            {
                await buffer.CopyToAsync(memory);

                image = new System.Drawing.Bitmap(buffer);

                memory.Close();
            }

            return image;
        }

        public async Task<string> DownloadImageBase64(string url)
        {
            string base64String = string.Empty;

            var response = _cancellationToken != null ?
            await _client.GetAsync(url, _cancellationToken) : await _client.GetAsync(url);

            response.EnsureSuccessStatusCode();

            var buffer = await response.Content.ReadAsStreamAsync();

            using (MemoryStream memory = new MemoryStream())
            {
                await buffer.CopyToAsync(memory);

                // Convert byte[] to Base64 String
                base64String = Convert.ToBase64String(memory.ToArray());

                memory.Close();
            }

            return base64String;
        }

        public async Task DownloadVideoToFolder(string urlVideo, string savePath)
        {
            var response = _cancellationToken != null ?
            await _client.GetAsync(urlVideo, _cancellationToken) : await _client.GetAsync(urlVideo);

            response.EnsureSuccessStatusCode();

            var videoData = await response.Content.ReadAsByteArrayAsync();

            // Lưu dữ liệu vào tệp
            File.WriteAllBytes(savePath, videoData);
        }
    }
}
