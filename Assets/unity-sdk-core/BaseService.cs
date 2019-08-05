﻿/**
* Copyright 2019 IBM Corp. All Rights Reserved.
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
*      http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
*
*/

using IBM.Cloud.SDK.Utilities;
using IBM.Cloud.SDK.Authentication;
using System;
using System.Collections.Generic;

namespace IBM.Cloud.SDK
{
    public class BaseService
    {
        protected Credentials credentials;
        protected string url;
        protected Dictionary<string, string> customRequestHeaders = new Dictionary<string, string>();

        public BaseService(string serviceId)
        {
            var credentialsPaths = Utility.GetCredentialsPaths();
            if (credentialsPaths.Count > 0)
            {
                foreach (string path in credentialsPaths)
                {
                    if (Utility.LoadEnvFile(path))
                    {
                        break;
                    }
                }

                string ApiKey = Environment.GetEnvironmentVariable(serviceId.ToUpper() + "_IAM_APIKEY");
                // check for old IAM API key name as well
                if (string.IsNullOrEmpty(ApiKey)) {
                    ApiKey = Environment.GetEnvironmentVariable(serviceId.ToUpper() + "_APIKEY");
                }
                string Username = Environment.GetEnvironmentVariable(serviceId.ToUpper() + "_USERNAME");
                string Password = Environment.GetEnvironmentVariable(serviceId.ToUpper() + "_PASSWORD");
                string ServiceUrl = Environment.GetEnvironmentVariable(serviceId.ToUpper() + "_URL");
                string AuthenticationType = Environment.GetEnvironmentVariable(serviceId.ToUpper() + "_AUTHENTICATION_TYPE");
                string Icp4dAccessToken = Environment.GetEnvironmentVariable(serviceId.ToUpper() + "_ICP4D_ACCESS_TOKEN");
                string Icp4dUrl = Environment.GetEnvironmentVariable(serviceId.ToUpper() + "_ICP4D_URL");

                if (string.IsNullOrEmpty(ApiKey) && (string.IsNullOrEmpty(Username) || string.IsNullOrEmpty(Password)))
                {
                    throw new NullReferenceException(string.Format("Either {0}_APIKEY or {0}_USERNAME and {0}_PASSWORD did not exist. Please add credentials with this key in ibm-credentials.env.", serviceId.ToUpper()));
                }

                if (!string.IsNullOrEmpty(ApiKey) || AuthenticationType == "iam")
                {
                    IamTokenOptions tokenOptions = new IamTokenOptions()
                    {
                        IamApiKey = ApiKey
                    };
                    credentials = new Credentials(tokenOptions, ServiceUrl);
                }

                if (!string.IsNullOrEmpty(Icp4dAccessToken) || AuthenticationType == "icp4d")
                {
                    Icp4dTokenOptions tokenOptions = new Icp4dTokenOptions()
                    {
                        Username = Username,
                        Password = Password,
                        AccessToken = Icp4dAccessToken,
                        Url = Icp4dUrl
                    };

                    credentials = new Credentials(tokenOptions, ServiceUrl);

                    if (string.IsNullOrEmpty(credentials.Url))
                    {
                        credentials.Url = url;
                    }
                }
                else if (!string.IsNullOrEmpty(Username) && !string.IsNullOrEmpty(Password))
                {
                    credentials = new Credentials(Username, Password, url);
                }
            }
        }

        public BaseService(string versionDate, string serviceId) : this(serviceId) { }

        public BaseService(string versionDate, Credentials credentials, string serviceId) { }

        public BaseService(Credentials credentials, string serviceId) { }

        public void WithHeader(string name, string value)
        {
            if (!customRequestHeaders.ContainsKey(name))
            {
                customRequestHeaders.Add(name, value);
            }
            else
            {
                customRequestHeaders[name] = value;
            }
        }

        public void WithHeaders(Dictionary<string, string> headers)
        {
            foreach (KeyValuePair<string, string> kvp in headers)
            {
                if (!customRequestHeaders.ContainsKey(kvp.Key))
                {
                    customRequestHeaders.Add(kvp.Key, kvp.Value);
                }
                else
                {
                    customRequestHeaders[kvp.Key] = kvp.Value;
                }
            }
        }

        protected void ClearCustomRequestHeaders()
        {
            customRequestHeaders = new Dictionary<string, string>();
        }
    }
}
