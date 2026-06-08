using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class PlayerVisuals : MonoBehaviour
{
    [SerializeField] private List<Color> playerColors;
    [SerializeField] private SkinnedMeshRenderer playerLimbsMeshRenderer;

    public void SetPlayerColor(int playerIndex)
    {
        if (playerIndex < 0 || playerIndex >= playerColors.Count)
        {
            Debug.LogWarning($"Player index {playerIndex} is out of range. Defaulting to white.");
            playerLimbsMeshRenderer.materials[0].color = Color.white;
            return;
        }
        playerLimbsMeshRenderer.materials[0].color = playerColors[playerIndex];
    }
}
