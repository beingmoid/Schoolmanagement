using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace SchoolManagement.Services
{
    public class ServiceException : Exception
    {
        public ServiceException(params string[] errors)
        {
            this.Errros = errors;
        }
        public ServiceException(HttpStatusCode httpStatusCode, params string[] errors)
            : this(errors)
        {
            this.HttpStatusCode = httpStatusCode;
        }

        public HttpStatusCode HttpStatusCode { get; } = HttpStatusCode.BadRequest;
        public string[] Errros { get; }
    }
}
