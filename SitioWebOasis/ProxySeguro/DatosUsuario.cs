﻿using OAS_Seguridad.Cliente;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;

namespace SitioWebOasis.ProxySeguro
{
    public class DatosUsuario: WSDatosUsuario.DatosUsuarios
    {
        private OASisLogin login;

        public DatosUsuario()
        {
            this.login = SitioWebOasis.CommonClasses.CacheConfig.Get("OASisLogin") as OASisLogin;
        }

        protected override WebRequest GetWebRequest(Uri uri)
        {
            HttpWebRequest req2 = (HttpWebRequest)base.GetWebRequest(uri);
            if(login != null){
                login.AttachCredentials(req2);
            }

            return req2;
        }
    }
}