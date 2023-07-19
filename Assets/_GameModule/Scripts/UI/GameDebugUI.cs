using FlatFury.MainModule.Scripts.Controllers;
using FlatFury.MainModule.Scripts.Data;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

namespace FlatFury.GameModule.Scripts.UI
{
    /// <summary>
    /// The <see cref="GameDebugUI"/> class.
    /// This class is used for debug purposes only.
    /// </summary>
    internal sealed class GameDebugUI : MonoBehaviour
    {
        [SerializeField] private Button hostButton;
        [SerializeField] private Button serverButton;
        [SerializeField] private Button clientButton;
        [SerializeField] private Button spawnCoinButton;
        [SerializeField] private Button victoryButton;
        [SerializeField] private Button defeatButton;
        [SerializeField] private Button toLobbyButton;
        [SerializeField] private TextMeshProUGUI statusText;

        public static GameDebugUI Instance { get; private set; }

        private void Awake()
        {
            if (Instance) Destroy(this);
            else Instance = this;
        }

        private void Start()
        {
            if (hostButton) hostButton.onClick.AddListener(() => { hostButton.interactable = false; NetworkManager.Singleton.StartHost(); });
            if (serverButton) serverButton.onClick.AddListener(() => { serverButton.interactable = false; NetworkManager.Singleton.StartServer(); });
            if (clientButton) clientButton.onClick.AddListener(() => { clientButton.interactable = false; NetworkManager.Singleton.StartClient(); });

            if (spawnCoinButton) spawnCoinButton.onClick.AddListener(() => { CoinManager.Instance.SpawnCoin(); });

            //if (victoryButton) victoryButton.onClick.AddListener(() => { GameUI.Instance.ShowVictory(new PlayerData()); });
            //if (defeatButton) defeatButton.onClick.AddListener(() => { GameUI.Instance.ShowDefeat(new PlayerData()); });

            if (toLobbyButton) toLobbyButton.onClick.AddListener(() => { GameMultiplayer.Instance.EndGame(); });
        }

        public static void UpdateStatus(string text) => Instance.statusText.text = text;
    }
}
