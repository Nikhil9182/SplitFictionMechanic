using System;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Cinemachine;

// This class receives input from a PlayerInput component and disptaches it
// to the appropriate Cinemachine InputAxis.  The playerInput component should
// be on the same GameObject, or specified in the PlayerInput field.
class CustomInputHandler : InputAxisControllerBase<CustomInputHandler.Reader>
{
    public void SetInputValue(Vector2 value)
    {
        for (var i = 0; i < Controllers.Count; i++) Controllers[i].Input.value = value;
    }

    // We process user input on the Update clock
    void Update()
    {
        if (Application.isPlaying)
            UpdateControllers();
    }

    // Controllers will be instances of this class.
    [Serializable]
    public class Reader : IInputAxisReader
    {
        public Vector2 value; // the cached value of the input

        // IInputAxisReader interface: Called by the framework to read the input value
        public float GetValue(UnityEngine.Object context, IInputAxisOwner.AxisDescriptor.Hints hint)
        {
            return (hint == IInputAxisOwner.AxisDescriptor.Hints.Y ? value.y : value.x);
        }
    }
}