using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using WPM;

public class TextureController : MonoBehaviour
{
    
    SliderInt sliderDate;
    DropdownField dropdownInterpolation;
    public DropdownField dropdownWeatherType;
    Toggle toggleVisialization;

    Texture2D[] textures;
    private Texture2D[] temperatureNearestTextures, temperatureBilinearTextures,
                        humidityNearestTextures, humidityBilinearTextures;

    private static readonly DateTime unixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    public static DateTime UnixTimestampToDateTime(double unixTimestamp)
    {
        return unixEpoch.AddSeconds(unixTimestamp).ToLocalTime();
    }

    public void InitializeTextureList(VisualElement root)
    {
        temperatureNearestTextures = Resources.LoadAll<Texture2D>("Textures/Temperature/Nearest/");
        temperatureBilinearTextures = Resources.LoadAll<Texture2D>("Textures/Temperature/Bilinear/");
        humidityNearestTextures = Resources.LoadAll<Texture2D>("Textures/Humidity/Nearest/");
        humidityBilinearTextures = Resources.LoadAll<Texture2D>("Textures/Humidity/Bilinear/");

        textures = temperatureBilinearTextures;
        WorldMapGlobe.instance.earthMaterial.mainTexture = textures[0];

        sliderDate = root.Q<SliderInt>("SliderDate");
        sliderDate.lowValue = 0;
        sliderDate.highValue = textures.Length - 1;
        sliderDate.value = 0;
        sliderDate.label = UnixTimestampToDateTime(double.Parse(textures[sliderDate.value].name, System.Globalization.CultureInfo.InvariantCulture)).ToString();
        sliderDate.RegisterValueChangedCallback(evt =>
        {
            WorldMapGlobe.instance.earthMaterial.mainTexture = textures[evt.newValue];
            sliderDate.label = UnixTimestampToDateTime(double.Parse(textures[evt.newValue].name, System.Globalization.CultureInfo.InvariantCulture)).ToString();
        });

        dropdownWeatherType = root.Q<DropdownField>("DropdownWeatherType");
        dropdownWeatherType.choices = new List<string> { "Temperature", "Humidity" };
        dropdownWeatherType.index = 0;
        dropdownWeatherType.RegisterValueChangedCallback(evt =>
        {
            switch (evt.newValue)
            {
                case "Temperature":
                    if (dropdownInterpolation.index == 0) textures = temperatureBilinearTextures;
                    if (dropdownInterpolation.index == 1) textures = temperatureNearestTextures;
                    break;

                case "Humidity":
                    if (dropdownInterpolation.index == 0) textures = humidityBilinearTextures;
                    if (dropdownInterpolation.index == 1) textures = humidityNearestTextures;
                    break;

                default: break;
            }
            
            sliderDate.highValue = textures.Length - 1;
            WorldMapGlobe.instance.earthMaterial.mainTexture = textures[sliderDate.value];
        });

        dropdownInterpolation = root.Q<DropdownField>("DropdownInterpolation");
        dropdownInterpolation.choices = new List<string> { "Bilinear", "Nearest" };
        dropdownInterpolation.index = 0;
        dropdownInterpolation.RegisterValueChangedCallback(evt =>
        {
            switch (evt.newValue)
            {
                case "Bilinear":
                    if (dropdownWeatherType.index == 0) textures = temperatureBilinearTextures;
                    if (dropdownWeatherType.index == 1) textures = humidityBilinearTextures;
                    break;

                case "Nearest":
                    if (dropdownWeatherType.index == 0) textures = temperatureNearestTextures;
                    if (dropdownWeatherType.index == 1) textures = humidityNearestTextures;
                    break;

                default: break;
            }

            sliderDate.highValue = textures.Length - 1;
            WorldMapGlobe.instance.earthMaterial.mainTexture = textures[sliderDate.value];
        });

        toggleVisialization = root.Q<Toggle>("VisualizationToggle");
        toggleVisialization.value = true;
        toggleVisialization.RegisterValueChangedCallback(evt =>
        {
            if (toggleVisialization.value)
            {
                sliderDate.SetEnabled(true);
                dropdownWeatherType.SetEnabled(true);
                dropdownInterpolation.SetEnabled(true);
                WorldMapGlobe.instance.earthMaterial.mainTexture = textures[sliderDate.value];
            }
            else
            {
                sliderDate.SetEnabled(false);
                dropdownWeatherType.SetEnabled(false);
                dropdownInterpolation.SetEnabled(false);
                WorldMapGlobe.instance.ReloadEarthTexture();
            }
        });
    }
}
