﻿namespace GitVersion
{
    using System;
    using System.Net;
    using System.Text;
#if !NETDESKTOP
    using System.Net.Http;
    using System.Threading.Tasks;
    using System.Net.Http.Headers;
    using Newtonsoft.Json;
    using System.IO;
#endif

    public class AppVeyor : BuildServerBase
    {
        public const string EnvironmentVariableName = "APPVEYOR";

        public override bool CanApplyToCurrentContext()
        {
            return !string.IsNullOrEmpty(Environment.GetEnvironmentVariable(EnvironmentVariableName));
        }

#if !NETDESKTOP


        public override string GenerateSetVersionMessage(VersionVariables variables)
        {

            var buildNumber = Environment.GetEnvironmentVariable("APPVEYOR_BUILD_NUMBER");
            var restBase = Environment.GetEnvironmentVariable("APPVEYOR_API_URL");

            var request = (HttpWebRequest)WebRequest.Create(restBase + "api/build");            
            request.Method = "PUT";


            var data = string.Format("{{ \"version\": \"{0}.build.{1}\" }}", variables.FullSemVer, buildNumber);
            var bytes = Encoding.UTF8.GetBytes(data);
            if (request.Headers == null)
            {
                request.Headers = new WebHeaderCollection();
            }
            
            var bytesLength = bytes.Length;
            //request.Headers["Content-Length"] = bytesLength.ToString();
            // request.ContentLength = bytes.Length;
            request.ContentType = "application/json";

            using (var writeStream = request.GetRequestStreamAsync().Result)
            {
                writeStream.Write(bytes, 0, bytes.Length);
            }

            //var result = request.BeginGetRequestStream((asyncResult) =>
            // {
            //     using (var writeStream = request.EndGetRequestStream(asyncResult))
            //     {
            //         writeStream.Write(bytes, 0, bytes.Length);
            //     }

            // }, null);

            // result.AsyncWaitHandle.WaitOne(new TimeSpan(0, 3, 0));

            using (var response = (HttpWebResponse)request.GetResponseAsync().Result)
            {
                if (response.StatusCode != HttpStatusCode.OK && response.StatusCode != HttpStatusCode.NoContent)
                {
                    var message = string.Format("Request failed. Received HTTP {0}", response.StatusCode);
                    return message;
                }
            }

            return string.Format("Set AppVeyor build number to '{0}'.", variables.FullSemVer);   

        }


        public override string[] GenerateSetParameterMessage(string name, string value)
        {

          //  var buildNumber = Environment.GetEnvironmentVariable("APPVEYOR_BUILD_NUMBER");
            var restBase = Environment.GetEnvironmentVariable("APPVEYOR_API_URL");

            var request = (HttpWebRequest)WebRequest.Create(restBase + "api/build/variables");
            request.Method = "POST";
            request.ContentType = "application/json";
            request.Accept = "application/json";

            var data = string.Format("{{ \"name\": \"GitVersion_{0}\", \"value\": \"{1}\" }}", name, value);           
            var bytes = Encoding.UTF8.GetBytes(data);
            if (request.Headers == null)
            {
                request.Headers = new WebHeaderCollection();
            }
            var bytesLength = bytes.Length;
            // No property for content-length - and no Add() method on header collection? WHAAAAT
            // request.ContentLength = bytes.Length;            
            //request.Headers["Content-Length"] = bytesLength.ToString();                

            using (var writeStream = request.GetRequestStreamAsync().Result)
            {
                writeStream.Write(bytes, 0, bytes.Length);
            }         

            return new[]
            {
                string.Format("Adding Environment Variable. name='GitVersion_{0}' value='{1}']", name, value)
            };
        }


#else

        public override string GenerateSetVersionMessage(VersionVariables variables)
        {
            var buildNumber = Environment.GetEnvironmentVariable("APPVEYOR_BUILD_NUMBER");
            var restBase = Environment.GetEnvironmentVariable("APPVEYOR_API_URL");

            var request = (HttpWebRequest)WebRequest.Create(restBase + "api/build");
            request.Method = "PUT";

            var data = string.Format("{{ \"version\": \"{0}.build.{1}\" }}", variables.FullSemVer, buildNumber);
            var bytes = Encoding.UTF8.GetBytes(data);
            if (request.Headers == null)
            {
                request.Headers = new WebHeaderCollection();
            }
            var bytesLength = bytes.Length;
            // request.Headers["Content-Length"] = bytesLength.ToString();

            // request.ContentLength = bytes.Length;
            request.ContentType = "application/json";

            using (var writeStream = request.GetRequestStream())
            {
                writeStream.Write(bytes, 0, bytes.Length);
            }

            using (var response = (HttpWebResponse)request.GetResponse())
            {
                if (response.StatusCode != HttpStatusCode.OK && response.StatusCode != HttpStatusCode.NoContent)
                {
                    var message = string.Format("Request failed. Received HTTP {0}", response.StatusCode);
                    return message;
                }
            }

            return string.Format("Set AppVeyor build number to '{0}'.", variables.FullSemVer);
        }

        public override string[] GenerateSetParameterMessage(string name, string value)
        {
            using (var wc = new WebClient())
            {
                wc.BaseAddress = Environment.GetEnvironmentVariable("APPVEYOR_API_URL");
                wc.Headers["Accept"] = "application/json";
                wc.Headers["Content-type"] = "application/json";

                var body = string.Format("{{ \"name\": \"GitVersion_{0}\", \"value\": \"{1}\" }}", name, value);
                wc.UploadData("api/build/variables", "POST", Encoding.UTF8.GetBytes(body));
            }

            return new[]
            {
                string.Format("Adding Environment Variable. name='GitVersion_{0}' value='{1}']", name, value)
            };
        }


#endif

    }
}