using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using WPM; //Asset WorldPoliticalMapGlobeEdition

public class WeatherDataVisualizer : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] long unixTimestamp;
    SortedDictionary<long, Dictionary<Vector2, WeatherDataReader.WeatherData>> weatherData;

    [Header("Gradients")]
    [SerializeField] private Gradient temperatureGradient;
    [SerializeField] private Gradient humudityGradient;

    [Header("Temperatur")] //celsius
    [SerializeField] float minTemperature = -30;
    [SerializeField] float maxTemperature = 40;

    [Header("Humidity")] //percent
    [SerializeField] float minHumidity = 0;
    [SerializeField] float maxHumidity = 100;

    public void BakeAll()
    {
        BakeBilinearTemperature(); 
        BakeBilinearHumidity();
        BakeNearestTemperature(); 
        BakeNearestHumidity();
    }

    public void BakeNearestHumidity()
    {
        BakeTextures("/Textures/Humidity/Nearest/", WeatherType.Humidity, Interpolation.Nearest);
    }

    public void BakeNearestTemperature()
    {
        BakeTextures("/Textures/Temperature/Nearest/", WeatherType.Temperature, Interpolation.Nearest);
    }

    public void BakeBilinearHumidity()
    {
        BakeTextures("/Textures/Temperature/Bilinear/", WeatherType.Humidity, Interpolation.Bilinear);
    }

    public void BakeBilinearTemperature()
    {
        BakeTextures("/Textures/Temperature/Bilinear/", WeatherType.Temperature, Interpolation.Bilinear);
    }

    public void BakingTest()
    {
        weatherData = WeatherDataReader.GetWeatherData();
        BakeTexture(WeatherType.Humidity, Interpolation.Bilinear, unixTimestamp, weatherData);
        BakeTexture(WeatherType.Temperature, Interpolation.Bilinear, unixTimestamp, weatherData);
    }

    public void BakeTextures(string outputPath, WeatherType weather, Interpolation interpolation)
    {
        int counter = 0;
        weatherData = WeatherDataReader.GetWeatherData();
        foreach (KeyValuePair<long, Dictionary<Vector2, WeatherDataReader.WeatherData>> timestampData in weatherData)
        {
            if (counter++ % 24 == 0) //each day
            {
                BakeTexture(weather, interpolation, timestampData.Key, weatherData);
            }
        }
    }

    public Texture2D BakeTexture(WeatherType weather, Interpolation interpolation, long timestamp, 
        SortedDictionary<long, Dictionary<Vector2, WeatherDataReader.WeatherData>> weatherData)
    {
        string outputPath = weather switch
        {
            WeatherType.Temperature => interpolation switch
            {
                Interpolation.Nearest => "/Resources/Textures/Temperature/Nearest/",
                Interpolation.Bilinear => "/Resources/Textures/Temperature/Bilinear/",
                _ => "",
            },
            WeatherType.Humidity => interpolation switch
            {
                Interpolation.Nearest => "/Resources/Textures/Humidity/Nearest/",
                Interpolation.Bilinear => "/Resources/Textures/Humidity/Bilinear/",
                _ => "",
            },
            _ => "",
        };

        string outputFile = Application.dataPath + outputPath + timestamp + ".png";
        if (File.Exists(outputFile))
            return null;

        WorldMapGlobe.instance.ReloadEarthTexture();
        Texture2D texture = Instantiate(WorldMapGlobe.instance.earthTexture);
        Color32[] colors = texture.GetPixels32();

        float temperatureDelta = maxTemperature - minTemperature;
        float humidityDelta = maxHumidity - minHumidity;

        List<DataPoint> dataPoints = new List<DataPoint>();
        foreach (KeyValuePair<Vector2, WeatherDataReader.WeatherData> kvp in weatherData[timestamp])
        {
            Vector2 latLon = kvp.Key;
            WeatherDataReader.WeatherData weatherDataAtGeo = kvp.Value;
            Vector2 uv = Conversion.GetUVFromLatLon(latLon.x, latLon.y);

            int x = (int)(uv.x * texture.width);
            int y = (int)(uv.y * texture.height);

            DataPoint currentPoint = null;
            switch (weather)
            {
                case WeatherType.Temperature:
                    currentPoint = new DataPoint { x = x, y = y, data = weatherDataAtGeo.main.temp };
                    dataPoints.Add(currentPoint);
                    break;

                case WeatherType.Humidity:
                    currentPoint = new DataPoint { x = x, y = y, data = weatherDataAtGeo.main.humidity };
                    dataPoints.Add(currentPoint);
                    break;

                default: break;
            }
        }

        var billinear = new BilinearInterpolation(dataPoints, texture.width);
        for (int y = 0; y < texture.height; y++)
        {
            for (int x = 0; x < texture.width; x++)
            {
                if (texture.GetPixel(x, y).a == 0) // Mask for oceans
                {
                    float data = interpolation switch
                    {
                        Interpolation.Nearest => FindNearestNeighbor
                        (dataPoints, new DataPoint { x = x, y = y }, texture.width, texture.height).data,
                        Interpolation.Bilinear => billinear.InterpolateBillinear(new DataPoint { x = x, y = y }),
                        _ => 0,
                    };

                    float gradientTime = weather switch
                    {
                        WeatherType.Temperature => (data - minTemperature) / temperatureDelta,
                        WeatherType.Humidity => (data - minHumidity) / humidityDelta,
                        _ => 0,
                    };

                    Color color = weather switch
                    {
                        WeatherType.Temperature => temperatureGradient.Evaluate(gradientTime),
                        WeatherType.Humidity => humudityGradient.Evaluate(gradientTime),
                        _ => Color.black,
                    };

                    DrawTexture(colors, color, x, y, texture.width);
                }
            }
        }
        texture.SetPixels32(colors);
        texture.Apply();

        File.WriteAllBytes(outputFile, texture.EncodeToPNG());
        return texture;
    }

    DataPoint FindNearestNeighbor(List<DataPoint> points, DataPoint currentPoint, int width, int height)
    {
        float minDistance = float.MaxValue;
        DataPoint nearestNeighbor = null;

        foreach (DataPoint point in points)
        {
            float distance = CalcDistance(point, currentPoint, width, height);
            if (distance < minDistance)
            {
                minDistance = distance;
                nearestNeighbor = point;
            }
        }
        return nearestNeighbor;
    }

    public static float CalcDistance(DataPoint p1, DataPoint p2, int width, int height)
    {
        float dx = p2.x - p1.x;
        //Zylinder so dick wie die Erde auf der mitteleren Höhe der Punkte wäre
        dx = Mathf.Min(Mathf.Abs(dx), width - Mathf.Abs(dx));
        dx *= Mathf.Sin((p1.y + p2.y) / 2 * Mathf.PI / height);
        float dy = p2.y - p1.y;

        return Mathf.Sqrt(dx * dx + dy * dy);
    }

    void DrawTexture(Color32[] colors, Color32 color, int x, int y, int width)
    {
        int j = y * width;
        int colorIndex = j + x;
        if (colorIndex < 0 || colorIndex >= colors.Length) return;

        colors[colorIndex].r = color.r;
        colors[colorIndex].g = color.g;
        colors[colorIndex].b = color.b;
        colors[colorIndex].a = color.a;
    }

    public class BilinearInterpolation
    {
        private SortedList<int, SortedList<int, DataPoint>> lookupTable;
        private int width;

        public BilinearInterpolation(List<DataPoint> points, int width)
        {
            this.width = width;
            lookupTable = new SortedList<int, SortedList<int, DataPoint>>();
            foreach (DataPoint point in points)
            {
                if (!lookupTable.ContainsKey(point.y))
                    lookupTable.Add(point.y, new SortedList<int, DataPoint>());
                //if (!lookupTable[point.y].ContainsKey(point.x))
                    lookupTable[point.y].Add(point.x, point);
            }
        }
        
        public float InterpolateBillinear(DataPoint point)
        {
            var lowerY = lookupTable.First().Value;
            var upperY = lowerY;
            foreach (KeyValuePair<int, SortedList<int, DataPoint>> entry in lookupTable)
            {
                lowerY = upperY;
                upperY = entry.Value;
                if (entry.Key >= point.y) break;
            }

            var (lowerLeft, lowerRight) = FindDataPointsInList(lowerY, point.x);
            var (upperLeft, upperRight) = FindDataPointsInList(upperY, point.x);

            var lowerWeight = CalcWeight(point.x, lowerLeft.x, lowerRight.x, width);
            var upperWeight = CalcWeight(point.x, upperLeft.x, upperRight.x, width);

            var lowerInterpolation = Mathf.Lerp(lowerLeft.data, lowerRight.data, lowerWeight);
            var upperInterpolation = Mathf.Lerp(upperLeft.data, upperRight.data, upperWeight);

            return Mathf.Lerp(lowerInterpolation, upperInterpolation, CalcWeight(point.y, lowerLeft.y, upperLeft.y));
        }

        private (DataPoint, DataPoint) FindDataPointsInList(SortedList<int, DataPoint> list, int x)
        {
            var lowerX = list.Last().Value;
            var upperX = list.First().Value;
            if (lowerX.x >= x && upperX.x < x)
            {
                foreach (KeyValuePair<int, DataPoint> entry in list)
                {
                    lowerX = upperX;
                    upperX = entry.Value;
                    if (entry.Key >= x) break;
                }
            }
            return (lowerX, upperX);
        }

        private float CalcWeight(float target, float lower, float upper, int wrapAt = int.MaxValue)
        {
            return Mathf.Min(WrappedDistance(target, lower, wrapAt) / WrappedDistance(upper, lower, wrapAt), 1);
        }

        private static float WrappedDistance(float a, float b, int wrapAt)
        {
            var dist = Mathf.Abs(b - a);
            return Mathf.Min(dist, wrapAt - dist);
        }
    }

    public class DataPoint
    {
        public int x { get; set; }
        public int y { get; set; }
        public float data { get; set; } //for Temperature or Humidity
    }

    public enum WeatherType
    {
        Temperature,
        Humidity
    }

    public enum Interpolation
    {
        Nearest,
        Bilinear
    }
}