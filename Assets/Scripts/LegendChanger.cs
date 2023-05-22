using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class LegendChanger : MonoBehaviour
{
    [SerializeField] private UIDocument uiDocument;
    private DropdownField dropdownWeatherType;
    public GameObject temperatureGradient;
    public GameObject humidityGradient;

    void Start()
    {
        var root = uiDocument.rootVisualElement;
        dropdownWeatherType = root.Q<DropdownField>("DropdownWeatherType");
    }

    void Update()
    {
        if (dropdownWeatherType.value == "Temperature")
        {
            humidityGradient.SetActive(false);
            temperatureGradient.SetActive(true);
        }

        if (dropdownWeatherType.value == "Humidity")
        {
            temperatureGradient.SetActive(false);
            humidityGradient.SetActive(true);
        }
    }
}
