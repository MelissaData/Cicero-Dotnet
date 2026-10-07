using Newtonsoft.Json;

namespace CiceroDotnet
{
  /// <summary>
  /// Cicero matches a location to its legislative districts and returns the elected
  /// officials who represent it, along with their contact information.
  ///
  /// <para>High-level flow of this sample:</para>
  /// <list type="number">
  ///   <item><description>ARGS    - ParseArguments reads any --flag values off the command line.</description></item>
  ///   <item><description>INPUT   - CallAPI fills in whatever wasn't supplied via interactive prompts.</description></item>
  ///   <item><description>REQUEST - CallAPI builds the REST query string (license + input fields).</description></item>
  ///   <item><description>CALL    - GetContents issues the GET request and pretty-prints the JSON response.</description></item>
  /// </list>
  ///
  /// <para>This sample is a thin HTTP client: it builds a query string, sends a GET
  /// request to the Cicero Cloud API (official endpoint), and prints the JSON response.</para>
  ///
  /// <para>Reference:</para>
  /// <list type="bullet">
  ///   <item><description>Documentation: https://docs.melissa.com/cloud-api/cicero/cicero-index.html</description></item>
  ///   <item><description>Release notes: https://releasenotes.melissa.com/cloud-api/cicero-api/</description></item>
  ///   <item><description>Result codes: https://docs.melissa.com/melissa/result-codes/result-codes-index.html</description></item>
  /// </list>
  /// </summary>
  static class Program
  {
    /// <summary>
    /// Entry point. Reads the optional command-line arguments, then hands control to
    /// CallAPI, which performs the actual request/response cycle.
    /// </summary>
    /// <param name="args">The raw command-line arguments.</param>
    static void Main(string[] args)
    {
      string baseServiceUrl = @"https://app.cicerodata.com/";
      string serviceEndpoint = @"v3.1/official";
      string license = "";
      string latitude = "";
      string longitude = "";
      string location = "";
      string max = "";

      // Populate any values passed on the command line, then run the lookup.
      ParseArguments(ref license, ref latitude, ref longitude, ref location, ref max, args);
      CallAPI(baseServiceUrl, serviceEndpoint, license, latitude, longitude, location, max);
    }

    /// <summary>
    /// Reads the supported command-line options and writes each recognized value into
    /// its matching by-ref parameter. Any parameter left unset here falls back to an
    /// interactive prompt later in <see cref="CallAPI"/>.
    ///
    /// <para>Recognized flags (each followed by its value, e.g. "--max 5"):
    /// --license/-l, --lat, --long, --location, --max.</para>
    /// </summary>
    /// <param name="license">Receives the Melissa license string, if supplied.</param>
    /// <param name="latitude">Receives the latitude, if supplied.</param>
    /// <param name="longitude">Receives the longitude, if supplied.</param>
    /// <param name="location">Receives the address or place to search, if supplied.</param>
    /// <param name="max">Receives the maximum number of results to return, if supplied.</param>
    /// <param name="args">The raw command-line arguments to parse.</param>
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

    /// <summary>
    /// Issues the GET request against the Cicero endpoint and
    /// pretty-prints the API call and the JSON response to the console.
    /// </summary>
    /// <param name="baseServiceUrl">The Cicero Cloud API base URL.</param>
    /// <param name="requestQuery">The endpoint path plus query string built by <see cref="CallAPI"/>.</param>
    public static async Task GetContents(string baseServiceUrl, string requestQuery)
    {
      HttpClient client = new HttpClient();
      client.BaseAddress = new Uri(baseServiceUrl);
      HttpResponseMessage response = await client.GetAsync(requestQuery);

      string text = await response.Content.ReadAsStringAsync();

      // Re-serialize with indentation so the raw response is easier to read.
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
    
    /// <summary>
    /// Drives the interactive/CLI loop: gathers the input fields, builds and
    /// submits the REST query, prints the result, and optionally repeats for another record.
    ///
    /// <para>In interactive mode (no input arguments supplied) it loops, asking for a new record each pass
    /// until the user answers "N". In one-shot mode (any input argument supplied) it runs a single
    /// pass and exits.</para>
    /// </summary>
    /// <param name="baseServiceUrl">The Cicero Cloud API base URL.</param>
    /// <param name="serviceEndPoint">The specific Cicero endpoint path to call.</param>
    /// <param name="license">The Melissa license string sent with every request.</param>
    /// <param name="latitude">Latitude of the location; if empty, the program prompts for it.</param>
    /// <param name="longitude">Longitude of the location; if empty, the program prompts for it.</param>
    /// <param name="location">An address or place to search; if empty, the program prompts for it.</param>
    /// <param name="max">The maximum number of results to return; if empty, the program prompts for it.</param>
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

        // No input fields were supplied via command line, so prompt for every field.
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
          // At least one field was supplied via command line; use those values as-is.
          inputLatitude = latitude;
          inputLongitude = longitude;
          inputLocation = location;
          inputMax = max;
        }

        // All four fields are required by this sample, so keep prompting for any
        // that are still missing.
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

        // Map input fields to the API's expected query parameter names and
        // request a JSON response.
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

        // If any field came from the command line, treat this as a one-shot
        // run rather than looping for additional records.
        bool isValid = false;
        if (!string.IsNullOrEmpty(latitude + longitude + location + max))
        {
          isValid = true;
          shouldContinueRunning = false;
        }

        // Otherwise ask whether to test another record. Keep prompting until we get a
        // valid Y/N. "N" ends the program; "Y" falls through to another pass.
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
