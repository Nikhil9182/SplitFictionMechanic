using NUnit.Framework;
using System;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerManager : MonoBehaviour
{
    [SerializeField] private List<PlayerInput> players = new List<PlayerInput>();
    [SerializeField] private List<Transform> spawnPoints;
    [SerializeField] private List<OutputChannels> playerChannels;

    private PlayerInputManager inputManager;

    private void Awake()
    {
        inputManager = GetComponent<PlayerInputManager>();
    }

    private void OnEnable()
    {
        inputManager.onPlayerJoined += HandlePlayerJoined;
        inputManager.onPlayerLeft += HandlePlayerLeft;
    }

    private void OnDisable()
    {
        inputManager.onPlayerJoined -= HandlePlayerJoined;
        inputManager.onPlayerLeft -= HandlePlayerLeft;
    }

    private void HandlePlayerJoined(PlayerInput input)
    {
        players.Add(input);

        var playerController = input.GetComponent<PlayerController>();
        playerController.SetSpawnPoint(spawnPoints[players.Count - 1]);

        int index = players.Count - 1;
        var playerCameraHandler = input.GetComponent<PlayerCinemachineCameraHandler>();
        playerCameraHandler.SetCinemachineCamera(index, playerChannels[index], input);
    }

    private void HandlePlayerLeft(PlayerInput input)
    {
        players.Remove(input);
    }
}
