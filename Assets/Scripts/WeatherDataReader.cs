using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class WeatherDataReader
{
    public static SortedDictionary<long, Dictionary<Vector2, WeatherData>> GetWeatherData()
    {
        //Key: Timestamp  Value: Dictionaroy (Key: Geo-Koordinaten Value: weatherData)
        SortedDictionary<long, Dictionary<Vector2, WeatherData>> weatherData = 
            new SortedDictionary<long, Dictionary<Vector2, WeatherData>>();

        DirectoryInfo directoryInfo = new DirectoryInfo("Data");
        int directoryCount = directoryInfo.GetDirectories().Length;

        for (int directoryIndex = 0; directoryIndex < directoryCount; directoryIndex++)
        {
            string currentPath = directoryInfo.GetDirectories()[directoryIndex].FullName;
            string[] files = Directory.GetFiles(currentPath);
            int filesCount = files.Length;

            for (int fileIndex = 0; fileIndex < filesCount; fileIndex++)
            {
                WeatherDataObject weatherDataObject = JsonUtility.FromJson<WeatherDataObject>
                                                      (File.ReadAllText(files[fileIndex]));
                Vector2 geos = GetLatLon(files[fileIndex]);
                
                if (weatherDataObject.list == null) continue;
                foreach (WeatherData element in weatherDataObject.list)
                {
                    if (!weatherData.ContainsKey(element.dt))
                        weatherData.Add(element.dt, new Dictionary<Vector2, WeatherData>());
                    weatherData[element.dt].Add(geos, element);
                }
            }
        }
        return weatherData;
    }

    private static Vector2 GetLatLon(string filePath)
    {
        int lastIndexOfSlash = filePath.LastIndexOf('\\');
        string fileName = "";

        if (lastIndexOfSlash >= 0)
            fileName = filePath.Substring(lastIndexOfSlash + 1);

        string[] seperators = new string[] { ".json", "_" };
        string[] LatLon = fileName.Split(seperators, 0);
        return new Vector2(float.Parse(LatLon[0], System.Globalization.CultureInfo.InvariantCulture),
                           float.Parse(LatLon[1], System.Globalization.CultureInfo.InvariantCulture)); //ignores Komma 
    }

    [System.Serializable]
    public class WeatherDataObject
    {
        public WeatherData[] list;
    }

    [System.Serializable]
    public class WeatherData
    {
        public long dt;
        public MainData main;
        public WindData wind;
    }

    [System.Serializable]
    public class MainData
    {
        public float temp;
        public float pressure;
        public float humidity;
    }

    [System.Serializable]
    public class WindData
    {
        public float speed;
        public float deg;
    }
}