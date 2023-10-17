using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace HttpRequestHelper.Model
{
    public class CookieModel
    {
        private string _cookie;

        public string Cookie
        {
            get { return _cookie; }
            set { _cookie = value; }
        }

        private string _path;

        public string Path
        {
            get { return _path; }
            set { _path = value; }
        }

        private string _domain;

        public string Domain
        {
            get { return _domain; }
            set { _domain = value; }
        }

        public IEnumerable<string[]> GetCookieList()
        {
            IEnumerable<string[]> cookieArgs = _cookie?.Split(';')?.Select(x => x.Split('='))?.Where(x => x.Length == 2);

            return cookieArgs;
        }
    }
}
