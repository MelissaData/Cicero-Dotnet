using Newtonsoft.Json;

namespace CiceroDotnet
{
  static class Program
  {
    static void Main(string[] args)
    {
      string baseServiceUrl = @"https://app.cicerodata.com/";
      string serviceEndpoint = @"v3.1/official";
      string license = "";
      string latitude = "";
      string longitude = "";
      string location = "";
      string max = "";

      ParseArguments(ref license, ref latitude, ref longitude, ref location, ref max, args);
      CallAPI(baseServiceUrl, serviceEndpoint, license, latitude, longitude, location, max);
    }

    static void ParseArguments(ref string license, ref string latitude, ref string longitude, ref string location, ref string max, string[] args)
    {
      for (int i = 0; i < args.Length; i++)
      {
        if (args[i].Equals("--license") || args[i].Equals("-l"))
        {
          if (args[i + 1] != null)
          {
            license = args[i + 1];
          }
        }
        if (args[i].Equals("--lat"))
        {
          if (args[i + 1] != null)
          {
            latitude = args[i + 1];
          }
        }
        if (args[i].Equals("--long"))
        {
          if (args[i + 1] != null)
          {
            longitude = args[i + 1];
          }
        }
        if (args[i].Equals("--location"))
        {
          if (args[i + 1] != null)
          {
            location = args[i + 1];
          }
        }
        if (args[i].Equals("--max"))
        {
          if (args[i + 1] != null)
          {
            max = args[i + 1];
          }
        }
      }
    }

    public static async Task GetContents(string baseServiceUrl, string requestQuery)
    {
      HttpClient client = new HttpClient();
      client.BaseAddress = new Uri(baseServiceUrl);
      HttpResponseMessage response = await client.GetAsync(requestQuery);

      string text = await response.Content.ReadAsStringAsync();

      var obj = JsonConvert.DeserializeObject(text);
      var prettyResponse = JsonConvert.SerializeObject(obj, Newtonsoft.Json.Formatting.Indented);

      // Print output
      Console.WriteLine("\n============================================= OUTPUT =============================================\n");
      
      Console.WriteLine("API Call: ");
      string APICall = Path.Combine(baseServiceUrl, requestQuery);
      for (int i = 0; i < APICall.Length; i += 70)
      {
        if (i + 70 < APICall.Length)
        {
          Console.WriteLine(APICall.Substring(i, 70));
        }
        else
        {
          Console.WriteLine(APICall.Substring(i, APICall.Length - i));
        }
      }

      Console.WriteLine("\nAPI Response:");
      Console.WriteLine(prettyResponse);
    }
    
    static void CallAPI(string baseServiceUrl, string serviceEndPoint, string license, string latitude, string longitude, string location, string max)
    {
      Console.WriteLine("\n=============================== WELCOME TO MELISSA CICERO CLOUD API ==============================\n");
      
      bool shouldContinueRunning = true;
      while (shouldContinueRunning)
      {
        string inputLatitude = "";
        string inputLongitude = "";
        string inputLocation = "";
        string inputMax = "";

        if (string.IsNullOrEmpty(latitude) && string.IsNullOrEmpty(longitude) && string.IsNullOrEmpty(location) && string.IsNullOrEmpty(max))
        {
          Console.WriteLine("\nFill in each value to see results");

          Console.Write("Latitude: ");
          inputLatitude = Console.ReadLine();

          Console.Write("Longitude: ");
          inputLongitude = Console.ReadLine();

          Console.Write("Search Location: ");
          inputLocation = Console.ReadLine();

          Console.Write("Max: ");
          inputMax = Console.ReadLine();
        }
        else
        {
          inputLatitude = latitude;
          inputLongitude = longitude;
          inputLocation = location;
          inputMax = max;
        }

        while (string.IsNullOrEmpty(inputLatitude) || string.IsNullOrEmpty(inputLongitude) || string.IsNullOrEmpty(inputLocation) || string.IsNullOrEmpty(inputMax))
        {
          Console.WriteLine("\nFill in missing required parameter");

          if (string.IsNullOrEmpty(inputLatitude))
          {
            Console.Write("Latitude: ");
            inputLatitude = Console.ReadLine();
          }

          if (string.IsNullOrEmpty(inputLongitude))
          {
            Console.Write("Longitude: ");
            inputLongitude = Console.ReadLine();
          }

          if (string.IsNullOrEmpty(inputLocation))
          {
            Console.Write("Search Location: ");
            inputLocation = Console.ReadLine();
          }

          if (string.IsNullOrEmpty(inputMax))
          {
            Console.Write("Max: ");
            inputMax = Console.ReadLine();
          }
        }

        Dictionary<string, string> inputs = new Dictionary<string, string>()
        {
            { "format", "json"},
            { "max", inputMax },
            { "lat", inputLatitude},
            { "lon", inputLongitude},
            { "search_loc", inputLocation}
        };

        Console.WriteLine("\n============================================= INPUTS =============================================\n");
        Console.WriteLine($"\t   Base Service Url: {baseServiceUrl}");
        Console.WriteLine($"\t  Service End Point: {serviceEndPoint}");
        Console.WriteLine($"\t           Latitude: {inputLatitude}");
        Console.WriteLine($"\t          Longitude: {inputLongitude}");
        Console.WriteLine($"\t    Search Location: {inputLocation}");
        Console.WriteLine($"\t                Max: {inputMax}");

        // Create Service Call
        // Set the License String in the Request
        string RESTRequest = "";

        RESTRequest += @"&key=" + Uri.EscapeDataString(license);

        // Set the Input Parameters
        foreach (KeyValuePair<string, string> kvp in inputs)
          RESTRequest += @"&" + kvp.Key + "=" + Uri.EscapeDataString(kvp.Value);

        // Build the final REST String Query
        RESTRequest = serviceEndPoint + @"?" + RESTRequest;

        // Submit to the Web Service. 
        bool success = false;
        int retryCounter = 0;

        do
        {
          try //retry just in case of network failure
          {
            GetContents(baseServiceUrl, $"{RESTRequest}").Wait();
            Console.WriteLine();
            success = true;
          }
          catch (Exception ex)
          {
            retryCounter++;
            Console.WriteLine(ex.ToString());
            return;
          }
        } while ((success != true) && (retryCounter < 5));

        bool isValid = false;
        if (!string.IsNullOrEmpty(latitude + longitude + location + max))
        {
          isValid = true;
          shouldContinueRunning = false;
        }

        while (!isValid)
        {
          Console.WriteLine("\nTest another record? (Y/N)");
          string testAnotherResponse = Console.ReadLine();

          if (!string.IsNullOrEmpty(testAnotherResponse))
          {
            testAnotherResponse = testAnotherResponse.ToLower();
            if (testAnotherResponse == "y")
            {
              isValid = true;
            }
            else if (testAnotherResponse == "n")
            {
              isValid = true;
              shouldContinueRunning = false;
            }
            else
            {
              Console.Write("Invalid Response, please respond 'Y' or 'N'");
            }
          }
        }
      }
      
      Console.WriteLine("\n============================= THANK YOU FOR USING MELISSA CLOUD API ==============================\n");
    }
  }
}
