using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Horizon
{
    class Request
    {
        public static async Task<PlanetRaw> getPlanet(string name)
        {
            System.Diagnostics.Debug.WriteLine("\n\n\n--"+name+"--\n\n\n");
            var client = new HttpClient();
            client.BaseAddress = new Uri("https://wgc2.jpl.nasa.gov:8443");

            DateTime UtcNow = DateTime.UtcNow;
            String time = UtcNow.Year + "-" + UtcNow.Month + "-" + UtcNow.Day + " " + UtcNow.Hour + ":" + UtcNow.Minute + ":" + UtcNow.Second + ".000000 UTC";

            string jsonData =
            @"{
                ""calculationType"": ""STATE_VECTOR"",
                ""kernels"": 
                [
                    {
                            ""type"": ""KERNEL_SET"",
                            ""id"": 1
                    }
                ],
                ""timeSystem"": ""UTC"",
                ""timeFormat"": ""CALENDAR"",
                ""intervals"": 
                [
                    {
                        ""startTime"": """ + time + @""",
                        ""endTime"": """ + time + @"""
                    }
                ],
                ""timeStep"": 30,
                ""timeStepUnits"": ""MINUTES"",
                ""targetType"": ""OBJECT"",
                ""target"": """ + name + @""",
                ""observerType"": ""OBJECT"",
                ""observer"": ""EARTH"",
                ""referenceFrame"": ""J2000"",
                ""aberrationCorrection"": ""NONE"",
                ""stateRepresentation"": ""RA_DEC""
            }";


            //richiesta POST del nuovo calcolo
            var content = new StringContent(jsonData, Encoding.UTF8, "application/json");
            HttpResponseMessage response = await client.PostAsync("https://wgc2.jpl.nasa.gov:8443/webgeocalc/api/calculation/new", content);
            var result = await response.Content.ReadAsStringAsync();

            Results results = JsonConvert.DeserializeObject<Results>(result);

            //richiesta GET per il risultato del calcolo
            response = await client.GetAsync("https://wgc2.jpl.nasa.gov:8443/webgeocalc/api/calculation/" + results.calculationId + "/results");
            result = await response.Content.ReadAsStringAsync();

            PlanetRaw pr = JsonConvert.DeserializeObject<PlanetRaw>(result);


            return pr;
        }

    }
}

