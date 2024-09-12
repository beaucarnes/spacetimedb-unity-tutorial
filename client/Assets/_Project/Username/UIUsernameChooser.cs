using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using SpacetimeDB.Types;

public class UIUsernameChooser : Singleton<UIUsernameChooser>
{
    [SerializeField] private GameObject _panel;
    [SerializeField] private TMP_InputField _usernameField;
    [SerializeField] private UIErrorText _error;

    private bool _initialized = false;

    public void Show()
    {
        if (!_initialized)
        {
            _initialized = true;
            _panel.SetActive(true);
            CameraController.AddDisabler(GetHashCode());
        }
    }

    private void OnEnable()
    {
        _panel.SetActive(false);
    }

    public void ShowError(string error) => _error.ShowError(error);

    public void ButtonPressed()
    {
        CameraController.RemoveDisabler(GetHashCode());
        _panel.SetActive(false);

        // Call the SpacetimeDB CreatePlayer reducer
        Reducer.CreatePlayer(_usernameField.text);
    }
}
