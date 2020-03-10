using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SnirtSelector : MonoBehaviour
{
    public Image overallUI;
    public Slider foodSlider;
    public Slider staminaSlider;
    public Text nameTex;
    public GameObject indicator;

    private Snirt currentSnirt;

    void Start()
    {
        overallUI.gameObject.SetActive(false);
        indicator.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (currentSnirt != null && overallUI.enabled)
        {
            // Set the sliders accordingly.
            foodSlider.maxValue = currentSnirt.hungerMax;
            foodSlider.value = currentSnirt.hunger;

            staminaSlider.maxValue = currentSnirt.staminaMax;
            staminaSlider.value = currentSnirt.stamina;

            // Set the name accordingly.
            nameTex.text = currentSnirt.snirtName;

            // Move the indicator to focus over the Snirt.
            indicator.transform.position = currentSnirt.transform.position;
        }
    }

    public void SnirtStats(Snirt snirt)
    {
        currentSnirt = snirt;
        overallUI.gameObject.SetActive(true);
        indicator.gameObject.SetActive(true);
    }

    public void CloseUI()
    {
        overallUI.gameObject.SetActive(false);
        indicator.gameObject.SetActive(false);
    }
}
