using UnityEditor;

[CustomEditor(typeof(WeatherDataVisualizer))]
public class WeatherDataVisualizerInspector : Editor
{
    [MenuItem("CONTEXT/WeatherDataVisualizer/Bake All")]
    static void BakeAllTextures(MenuCommand command)
    {
        ((WeatherDataVisualizer)command.context).BakeAll();
        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("Operation successful!", "Texture saved in Textures/TestHumidity and Textures/TestTemperature folder.", "Ok");
    }

    [MenuItem("CONTEXT/WeatherDataVisualizer/Test Baking")]
    static void BakingTest(MenuCommand command)
    {
        ((WeatherDataVisualizer)command.context).BakingTest();
        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("Operation successful!", "Texture saved in Textures/TestHumidity and Textures/TestTemperature folder.", "Ok");
    }

    [MenuItem("CONTEXT/WeatherDataVisualizer/Bake Nearest Humidity")]
    static void BakeHumidityNN(MenuCommand command)
    {
        ((WeatherDataVisualizer)command.context).BakeNearestHumidity();
        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("Operation successful!", "Texture saved in Textures/Humidity folder.", "Ok");
    }

    [MenuItem("CONTEXT/WeatherDataVisualizer/Bake Nearest Temperature")]
    static void BakeTemperatureNN(MenuCommand command)
    {
        ((WeatherDataVisualizer)command.context).BakeNearestTemperature();
        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("Operation successful!", "Texture saved in Textures/Temperature folder.", "Ok");
    }

    [MenuItem("CONTEXT/WeatherDataVisualizer/Bake Bilinear Humidity")]
    static void BakeHumidityBilinear(MenuCommand command)
    {
        ((WeatherDataVisualizer)command.context).BakeBilinearHumidity();
        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("Operation successful!", "Texture saved in Textures/Humidity folder.", "Ok");
    }

    [MenuItem("CONTEXT/WeatherDataVisualizer/Bake Bilinear Temperature")]
    static void BakeTemperatureBilinear(MenuCommand command)
    {
        ((WeatherDataVisualizer)command.context).BakeBilinearTemperature();
        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("Operation successful!", "Texture saved in Textures/Temperature folder.", "Ok");
    }
}