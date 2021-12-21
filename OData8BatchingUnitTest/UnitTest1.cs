using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using Xunit;


namespace OData8BatchingUnitTest
{
    public class ControllerUnitTest
    {
        List<string> listOfControllers = new List<string>();
        public string BaseAddress { get; set; }
        public HttpClient Client { get; set; }

        public ControllerUnitTest()
        {

            //Load the assembly
            var assemblies = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory, "ODataBatching8*.dll")
                        .Select(x => Assembly.Load(AssemblyName.GetAssemblyName(x)));
            //.GetTypes().Where(x => x.Name.Contains("Model"));

            foreach (var assembly in assemblies)
            {
                var contextNamespaces = assembly.GetTypes().Where(x=>x.FullName.Contains("Models") && !x.Name.Contains("Context"));

                foreach (var item in contextNamespaces)
                {
                    
                    if (item.IsPublic)
                    {
                        listOfControllers.Add(item.Name);                      
                    }
                }
            }


        }

        [Fact]
        public void TestGetControillers()
        {
            foreach (var getEDM in listOfControllers)
            {
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, BaseAddress + "/odata/" + getEDM);
                HttpResponseMessage response = Client.SendAsync(request).Result;
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            }
        }

    }
}