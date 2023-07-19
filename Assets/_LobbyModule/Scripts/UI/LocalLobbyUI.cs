using FlatFury.MainModule.Scripts.Controllers;
using UnityEngine;
using UnityEngine.UI;

namespace FlatFury.LobbyModule
{
    /// <summary>
    /// The <see cref="LocalLobbyUI"/> class.
    /// This class is for testing purposes.
    /// </summary>
    internal sealed class LocalLobbyUI : MonoBehaviour
    {
        [SerializeField] private Button _createGameButton;
        [SerializeField] private Button _joinGameButton;

        private void Awake()
        {
            if (_createGameButton) _createGameButton.onClick.AddListener(() =>
            {
                _createGameButton.interactable = false;
                _joinGameButton.gameObject.SetActive(false);
                CreateGame();
            });

            if (_joinGameButton) _joinGameButton.onClick.AddListener(() =>
            {
                _createGameButton.gameObject.SetActive(false);
                _joinGameButton.interactable = false;
                StartClient();
            });
        }

        private void CreateGame()
        {
            GameMultiplayer.Instance.StartHost();
        }

        private void StartClient()
        {
            GameMultiplayer.Instance.StartClient();
        }
    }
}
