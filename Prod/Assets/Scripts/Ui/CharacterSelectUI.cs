using UnityEngine;
using UnityEngine.SceneManagement;

public class CharacterSelectUI : MonoBehaviour
{
    [SerializeField] private GameObject humanPrefab;
    [SerializeField] private GameObject molePrefab;
    [SerializeField] private string gameSceneName = "MainGame";

    public void SelectHuman()
    {
        GameSelection.SelectedCharacterPrefab = humanPrefab;
        SceneManager.LoadScene(gameSceneName);
    }

    public void SelectMole()
    {
        GameSelection.SelectedCharacterPrefab = molePrefab;
        SceneManager.LoadScene(gameSceneName);
    }
}