﻿#nullable enable

using System.Net;
using System.Net.Http.Headers;
using Smartstore.Core.Checkout.Payment;
using Smartstore.Net.Http;

namespace Smartstore.PayPal.Client
{
    public class PayPalResponse : PaymentResponse
    {
        public PayPalResponse(HttpHeaders? headers, HttpStatusCode status)
            : this(status, headers, null)
        {
        }

        public PayPalResponse(HttpStatusCode status, HttpHeaders? headers, object? body)
            : base(status, headers?.ToFlatDictionary(), body)
        {
            HttpHeaders = headers;
        }

        public HttpHeaders? HttpHeaders { get; }
    }

    public class PayPalResponse<TBody> : PayPalResponse
    {
        public PayPalResponse(HttpStatusCode status, HttpHeaders? headers, TBody? body)
            : base(status, headers, body)
        {
        }

        public TBody Body
            => Body<TBody>();
    }
}
