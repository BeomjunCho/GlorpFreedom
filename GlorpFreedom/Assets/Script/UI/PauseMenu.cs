using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    [Header("Pause Menu UI")]
    [Tooltip("Assign the panel (or root GameObject) for the pause menu here.")]
    [SerializeField] private GameObject pausePanel;

    private bool isPaused = false;

    private void Awake()
    {
        // Ensure pause menu is hidden at start
        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }
        else
        {
            Debug.LogWarning("PauseMenu: pausePanel is not assigned in the Inspector.");
        }
    }

    private void Update()
    {
        // Toggle pause when Tab key is pressed
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (isPaused)
                ResumeGame();
            else
                PauseGame();
        }
    }

    /// <summary>
    /// Activates pause menu, stops game time, and unlocks cursor.
    /// </summary>
    public void PauseGame()
    {
        if (pausePanel != null)
            pausePanel.SetActive(true);

        Time.timeScale = 0f;                   // Freeze game logic and animations
        isPaused = true;

        Cursor.lockState = CursorLockMode.None; // Unlock cursor so player can click UI
        Cursor.visible = true;                  // Make cursor visible
    }

    /// <summary>
    /// Hides pause menu, resumes game time, and locks cursor.
    /// </summary>
    public void ResumeGame()
    {
        if (pausePanel != null)
            pausePanel.SetActive(false);

        Time.timeScale = 1f;                   // Resume normal time scale
        isPaused = false;

        Cursor.lockState = CursorLockMode.Locked; // Lock cursor back to center (if your game uses locked cursor)
        Cursor.visible = false;                   // Hide cursor during gameplay
    }

    /// <summary>
    /// Quits the application. In the Editor, stops Play mode.
    /// </summary>
    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
