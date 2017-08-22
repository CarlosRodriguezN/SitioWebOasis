﻿using OAS_Seguridad.Cliente;
using System;
using System.Net;

namespace SitioWebOasis.ProxySeguro
{
    public class NotasEstudiante: WSNotasEstudiante.NotasEstudiante
    {
        private OASisLogin login;

        public NotasEstudiante()
        {
            this.login = SitioWebOasis.CommonClasses.CacheConfig.Get("OASisLogin") as OASisLogin;
        }

        protected override WebRequest GetWebRequest(Uri uri)
        {
            HttpWebRequest req2 = (HttpWebRequest)base.GetWebRequest(uri);
            if (login != null)
            {
                login.AttachCredentials(req2);
            }

            return req2;
        }
    }
}