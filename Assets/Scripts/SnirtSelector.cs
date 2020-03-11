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
    public Text deathTex;
    public GameObject indicator;

    private Camera cam;
    private Snirt currentSnirt;

    void Start()
    {
        overallUI.gameObject.SetActive(false);
        indicator.gameObject.SetActive(false);
        cam = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                if (hit.transform.gameObject.CompareTag("Boid"))
                {
                    SnirtStats(hit.transform.gameObject.GetComponent<Snirt>());
                }
            }
        }

        if (currentSnirt != null && overallUI.enabled)
        {
            // Set the sliders accordingly.
            foodSlider.value = currentSnirt.hunger;
            staminaSlider.value = currentSnirt.stamina;

            // Move the indicator to focus over the Snirt.
            indicator.transform.position = currentSnirt.transform.position;

            if (currentSnirt.amAlive) deathTex.gameObject.SetActive(false);
            else deathTex.gameObject.SetActive(true);
        }
    }

    public void SnirtStats(Snirt snirt)
    {
        currentSnirt = snirt;

        // Set the sliders' max value.
        foodSlider.maxValue = currentSnirt.hungerMax;
        staminaSlider.maxValue = currentSnirt.staminaMax;

        // Set the name accordingly.
        nameTex.text = currentSnirt.snirtName;

        overallUI.gameObject.SetActive(true);
        indicator.gameObject.SetActive(true);
    }

    public void CloseUI()
    {
        overallUI.gameObject.SetActive(false);
        indicator.gameObject.SetActive(false);
    }
}
