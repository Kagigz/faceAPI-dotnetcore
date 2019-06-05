using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace FaceAPI
{
    public class RequestHelpers
    {

        private static readonly HttpClient client = new HttpClient();
        private static string subscriptionKey = "YOUR_SUBSCRIPTION_KEY";

        public static async Task<string> PostRequest(string uri, JsonContent body)
        {

            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", subscriptionKey);

            HttpResponseMessage response = null;
            string contents = null;

            try
            {
                response = await client.PostAsync(uri, body);
            }
            catch (Exception e)
            {
                Console.WriteLine($"\n{e.GetType().Name}: Error sending POST request.");
                throw;
            }

            if (response != null)
            {
                try
                {
                    contents = await response.Content.ReadAsStringAsync();
                    Console.Write('\n');
                    Console.Write(contents);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"\n{e.GetType().Name}: Error parsing response from POST request.");
                    throw;
                }
            }

            return contents;
        }

        public static async Task<string> GetRequest(string uri)
        {

            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", subscriptionKey);

            HttpResponseMessage response = null;
            string contents = null;

            try
            {
                response = await client.GetAsync(uri);
            }
            catch (Exception e)
            {
                Console.WriteLine($"\n{e.GetType().Name}: Error sending GET request.");
                throw;
            }

            if (response != null)
            {
                try
                {
                    contents = await response.Content.ReadAsStringAsync();
                    Console.Write('\n');
                    Console.Write(contents);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"\n{e.GetType().Name}: Error parsing response from GET request.");
                    throw;
                }
            }

            return contents;
        }

        public static async Task<string> PutRequest(string uri, JsonContent body)
        {

            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", subscriptionKey);

            HttpResponseMessage response = null;
            string contents = null;

            try
            {
                response = await client.PutAsync(uri, body);
            }
            catch (Exception e)
            {
                Console.WriteLine($"\n{e.GetType().Name}: Error sending PUT request.");
                throw;
            }

            if (response != null)
            {
                try
                {
                    contents = await response.Content.ReadAsStringAsync();
                    Console.Write('\n');
                    Console.Write(contents);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"\n{e.GetType().Name}: Error parsing response from PUT request.");
                    throw;
                }
            }

            return contents;
        }
    }
}
