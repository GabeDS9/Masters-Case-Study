using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.XR;

public class VR_Movement : MonoBehaviour
{
    public GameObject XRRig;
    public GameObject head;
    public GameObject leftHand;
    public GameObject UI;
    public float flyingSpeed = 1f;

    private InputDevice _targetRHDevice;
    private InputDevice _targetLHDevice;    
    private float triggerFlyLHValue = 0f;
    private bool isMenuDisplayed = false;
    private Stopwatch menuButtonPressedTime = new Stopwatch();
    private long menuTS = 0;
    private bool firstPress = true;

    void Start()
    {
        TryInitializeRH();
        TryInitializeLH();
        UI.SetActive(isMenuDisplayed);
    }

    void TryInitializeRH()
    {
        var inputDevices = new List<InputDevice>();
        InputDeviceCharacteristics rightControllerCharacteristics = InputDeviceCharacteristics.Right | InputDeviceCharacteristics.Controller;
        InputDevices.GetDevicesWithCharacteristics(rightControllerCharacteristics, inputDevices);

        if (inputDevices.Count == 0)
        {
            return;
        }

        _targetRHDevice = inputDevices[0];
    }

    void TryInitializeLH()
    {
        var inputDevices = new List<InputDevice>();
        InputDeviceCharacteristics leftControllerCharacteristics = InputDeviceCharacteristics.Left | InputDeviceCharacteristics.Controller;
        InputDevices.GetDevicesWithCharacteristics(leftControllerCharacteristics, inputDevices);

        if (inputDevices.Count == 0)
        {
            return;
        }
        
        _targetLHDevice = inputDevices[0];
    }

    void Update()
    {
        if (!_targetRHDevice.isValid)
        {
            TryInitializeRH();
        }
        else
        {
            // Do what you would like with _targetDevice here
        }

        if (!_targetLHDevice.isValid)
        {
            TryInitializeLH();
        }
        else
        {
            menuTS = menuButtonPressedTime.ElapsedMilliseconds;
            if(_targetLHDevice.TryGetFeatureValue(CommonUsages.trigger, out float triggerLHValue) && triggerLHValue >= 0.1f)
            {
                Fly();
                triggerFlyLHValue = triggerLHValue;
            }
            if (_targetLHDevice.TryGetFeatureValue(CommonUsages.menuButton, out bool menuPressed) && menuPressed == true && (menuTS > 1000 || firstPress == true))
            {                
                DisplayMenu();
                menuButtonPressedTime.Start();
                firstPress = false;
            }
        }
    }

    private void Fly()
    {
        Vector3 flyDir = leftHand.transform.position - head.transform.position;
        XRRig.transform.position += flyDir * (flyingSpeed * triggerFlyLHValue);
    }

    private void DisplayMenu()
    {
        if (isMenuDisplayed == true)
        {
            isMenuDisplayed = false;
            UI.SetActive(isMenuDisplayed);            
        }
        else if (isMenuDisplayed == false)
        {
            isMenuDisplayed = true;
            UI.SetActive(isMenuDisplayed);
        }
    }
}
