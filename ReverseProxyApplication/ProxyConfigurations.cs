using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace ReverseProxyApplication
{
    public class ProxyConfigurations : IProxyConfigurations
    {
        Dictionary<string, X509Certificate2> _certificates;
        X509Certificate2 _fallbackCert;

        public ProxyConfigurations()
        {
            _certificates = new Dictionary<string, X509Certificate2>(StringComparer.OrdinalIgnoreCase);

            _certificates["metrics.test"] = new X509Certificate2("D:\\dev\\RootCA.pfx", "password");
            _fallbackCert = new X509Certificate2("D:\\dev\\RootCA2.pfx", "password2");
            _certificates["metrics2.test"] = _fallbackCert;
        }

        public X509Certificate2 GetCertificate(string name)
        {
            if (name != null && _certificates.TryGetValue(name, out var cert))
            {
                return cert;
            }

            return _fallbackCert;
        }
    }
}
