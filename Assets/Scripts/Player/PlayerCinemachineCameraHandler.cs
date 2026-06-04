using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCinemachineCameraHandler : MonoBehaviour
{
    [SerializeField] private CinemachineBrain cinemachineBrain;
    [SerializeField] private CinemachineCamera cinemachineCamera;
    [SerializeField] private CustomInputHandler customInputHandler;

    [SerializeField] private float mouseSensitivity = 1f;

    public void SetCinemachineCamera(int index, OutputChannels channel, PlayerInput input)
    {
        cinemachineBrain.ChannelMask = channel;
        cinemachineCamera.OutputChannel = channel;
    }

    public void OnLook(InputAction.CallbackContext context)
    {
        customInputHandler.SetInputValue(context.ReadValue<Vector2>() * mouseSensitivity);
    }
}
