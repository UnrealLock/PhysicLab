using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;

public class PcManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    // Reference to the PC camera to switch
    public Camera PcCamera;
    // Reference to the app window
    GameObject app;
    // Flag to check if the app is open
    bool isOpen = false;
    void Start()
    {
        // We start with the app opened
        OpenApp();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OpenApp()
    {
        // Do not open if already opened
        if (isOpen) return;
        // Load the app prefab
        app = Resources.Load<GameObject>("Objects/Pc/App/App");
        // Save RectTransform data of all child objects
        var objectsData = new List<RectTransformData>();
        for (int i = 0; i < app.transform.childCount; i++)
        {
            objectsData.Add(new RectTransformData());
            objectsData[i].CopyRectTransform(app.transform.GetChild(i));
        }
        // Instantiate the app
        app = Instantiate(app, new Vector3(0, 0, -0.15f), Quaternion.identity);
        // Set the camera for the app movement script
        app.GetComponent<MoveAppScreen>().MainCamera = PcCamera;
        // Parent the app to the screen and set its position and scale
        var screen = GameObject.Find("Screen");
        app.transform.SetParent(screen.transform);
        app.transform.localPosition = new Vector3(0, 0, -0.15f);
        app.transform.localScale = new Vector3(60, 60, 1);
        app.transform.localRotation = Quaternion.identity;
        // Restore RectTransform data for all child objects
        for (int i = 0; i < app.transform.childCount; i++)
        {
            objectsData[i].PasteRectTransform(app.transform.GetChild(i));
            if (i == 0)
                app.transform.GetChild(i).localPosition = new Vector3(0, 0.02f, 0.01f);
            else
                app.transform.GetChild(i).localPosition = new Vector3(0, 0, -0.01f);
        }
        // Set up buttons for the app
        SetFrameButtons();
        // Mark the app as opened
        isOpen = true;
    }

    void SetFrameButtons()
    {
        // Get buttons objects
        var buttons = app.transform.GetChild(0).transform.GetChild(0);
        var fullScreenButton = buttons.GetChild(1).GetComponent<UnityEngine.UI.Button>();
        var hideButton = buttons.GetChild(2).GetComponent<UnityEngine.UI.Button>();
        var closeButton = buttons.GetChild(0).GetComponent<UnityEngine.UI.Button>();
        // Set buttons positions and methods
        buttons.localPosition = Vector3.zero;
        buttons.localScale = new Vector3(1, 1, 1);
        buttons.GetChild(0).transform.localPosition = new Vector3(0.4695f, 0.4781f, -0.02f);
        buttons.GetChild(1).transform.localPosition = new Vector3(0.4205f, 0.4781f, -0.02f);
        buttons.GetChild(2).transform.localPosition = new Vector3(0.3707f, 0.4781f, -0.02f);
        fullScreenButton.onClick.AddListener(MakeFullScreen);
        hideButton.onClick.AddListener(HideApp);
        closeButton.onClick.AddListener(CloseApp);
    }
    public void CloseApp()
    {
        // Destroy the app and mark it as closed
        Destroy(app);
        app = null;
        isOpen = false;
    }

    public void HideApp()
    {
        // Hide the app and show the show button
        app.gameObject.SetActive(false);
        var screen = GameObject.Find("Screen");
        var showButton = screen.transform.GetChild(4);
        showButton.gameObject.SetActive(true);
    }

    public void MakeFullScreen()
    {
        // Set the app to full screen size and position
        app.transform.localScale = new Vector3(100, 95, 1);
        app.transform.localPosition = new Vector3(0, -1, -0.15f);
        // Change the full screen button method to make normal
        var buttons = app.transform.GetChild(0).transform.GetChild(0);
        var fullScreenButton = buttons.GetChild(1).GetComponent<UnityEngine.UI.Button>();
        // Disable app movement when in full screen
        app.GetComponent<MoveAppScreen>().CanBeDriven = false;
        fullScreenButton.onClick.RemoveAllListeners();
        fullScreenButton.onClick.AddListener(MakeNormal);
    }

    public void ShowApp() {
        // Show the app and hide the show button
        app.gameObject.SetActive(true);
        var screen = GameObject.Find("Screen");
        var showButton = screen.transform.GetChild(4);
        showButton.gameObject.SetActive(false);
    }

    public void MakeNormal()
    {
        // Set the app to normal size and position
        app.transform.localScale = new Vector3(60, 60, 1);
        app.transform.localPosition = new Vector3(0, 0, -0.15f);
        var buttons = app.transform.GetChild(0).transform.GetChild(0);
        var fullScreenButton = buttons.GetChild(1).GetComponent<UnityEngine.UI.Button>();
        // Enable app movement when in normal screen
        app.GetComponent<MoveAppScreen>().CanBeDriven = true;
        // Change the full screen button method to make full screen
        fullScreenButton.onClick.RemoveAllListeners();
        fullScreenButton.onClick.AddListener(MakeFullScreen);
    }
}

class RectTransformData
{
    // RectTransform properties to copy
    Vector2 anchorMin;
    Vector2 anchorMax;
    Vector2 anchoredPosition;
    Vector2 sizeDelta;
    Vector2 pivot;
    Vector3 localScale;
    Quaternion localRotation;
    Quaternion rotation;

    public void CopyRectTransform(Transform objectTransform)
    {
        // Save the RectTransform component
        var rectTransform = objectTransform.GetComponent<RectTransform>();
        anchorMin = rectTransform.anchorMin;
        anchorMax = rectTransform.anchorMax;
        anchoredPosition = rectTransform.anchoredPosition;
        sizeDelta = rectTransform.sizeDelta;
        pivot = rectTransform.pivot;
        localScale = rectTransform.localScale;
    }

    public void PasteRectTransform(Transform objectTransform)
    {
        // Restore the RectTransform component
        var rectTransform = objectTransform.GetComponent<RectTransform>();
        rectTransform.anchorMin = anchorMin;
        rectTransform.anchorMax = anchorMax;
        rectTransform.anchoredPosition = anchoredPosition;
        rectTransform.sizeDelta = sizeDelta;
        rectTransform.pivot = pivot;
        rectTransform.localScale = localScale;
        rectTransform.localPosition = new Vector3(0, 0, 0);
    }
}
