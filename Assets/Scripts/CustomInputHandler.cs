using System;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Cinemachine;

class CustomInputHandler : InputAxisControllerBase<CustomInputHandler.Reader>
{
    public void SetInputValue(Vector2 value)
    {
        for (var i = 0; i < Controllers.Count; i++) Controllers[i].Input.SetValue(value);
    }

    void Update()
    {
        if (Application.isPlaying)
            UpdateControllers();
    }

    [Serializable]
    public class Reader : IInputAxisReader
    {
        private Vector2 value;

        public void SetValue(Vector2 newValue)
        {
            value = newValue;
        }

        public float GetValue(UnityEngine.Object context, IInputAxisOwner.AxisDescriptor.Hints hint)
        {
            return (hint == IInputAxisOwner.AxisDescriptor.Hints.Y ? value.y : value.x);
        }
    }
}