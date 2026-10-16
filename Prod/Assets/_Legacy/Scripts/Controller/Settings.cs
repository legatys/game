using UnityEngine;

[CreateAssetMenu(fileName = "GameSettings", menuName = "Game/Settings")]
public class GameSettings : ScriptableObject
{
    [Header("Mouse")]
    public float mouseSensitivity = 0.1f;
    public float minPitch = -80f;
    public float maxPitch = 80f;

    [Header("Movement")]
    public float acceleration  = 15f;
    public float maxSpeed = 5f; 

    [Header("Jump")]
    public int max_jump = 1;
    public float jumpForce = 8f;


}