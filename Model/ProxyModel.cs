using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HttpRequestHelper.Model
{
    public class ProxyModel
    {
        private string _ip;

        public string Ip
        {
            get { return _ip; }
            set { _ip = value; }
        }

        private string _port;

        public string Port
        {
            get { return _port; }
            set { _port = value; }
        }

        private string _username;

        public string Username
        {
            get { return _username; }
            set { _username = value; }
        }

        private string _password;

        public string Password
        {
            get { return _password; }
            set { _password = value; }
        }

        public override string ToString()
        {
            if (!string.IsNullOrEmpty(_ip) && !string.IsNullOrEmpty(_port) && !string.IsNullOrEmpty(_username) && !string.IsNullOrEmpty(_password))
            {
                return $"{_ip}:{_port}:{_username}:{_password}";
            }
            else if (!string.IsNullOrEmpty(_ip) && !string.IsNullOrEmpty(_port))
            {
                return $"{_ip}:{_port}";
            }

            return base.ToString();
        }
    }
}
