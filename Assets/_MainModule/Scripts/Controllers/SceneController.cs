using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace FlatFury.MainModule.Scripts.Controllers
{
    /// <summary>
    /// The <see cref="SceneController"/> class.
    /// </summary>
    internal sealed class SceneController
    {
        #region Scene Enum

        /// <summary>
        /// The <see cref="Scene"/> enumeration of the available scenes.
        /// </summary>
        public enum Scene { Lobby, Loading, Game, LocalLobby }

        #endregion

        #region Consts

        private const string SCENE_LOCAL_LOBBY = "LocalLobbyScene";
        private const string SCENE_LOBBY = "LobbyScene";
        private const string SCENE_LOADING = "LoadingScene";
        private const string SCENE_GAME = "GameScene";

        #endregion

        #region Properties

        public static Scene Current { get; private set; }

        #endregion

        #region Methods
         
        /// <summary>
        /// Gets the scene name by the specified scene enum.
        /// </summary>
        /// <param name="scene">scene</param>
        /// <returns>scene name</returns>
        public static string GetSceneName(Scene scene)
        {
            switch (scene)
            {
                case Scene.Game: return SCENE_GAME;
                case Scene.Loading: return SCENE_LOADING;
                case Scene.Lobby: return SCENE_LOBBY;
                case Scene.LocalLobby: return SCENE_LOCAL_LOBBY;
                default: return SCENE_LOBBY;
            }
        }

        /// <summary>
        /// Loads the specified scene.
        /// </summary>
        /// <param name="scene">scene</param>
        public static void Load(Scene scene)
        {
            Current = scene;
            SceneManager.LoadScene(GetSceneName(Current));
        }

        /// <summary>
        /// Loads the specified network scene.
        /// </summary>
        /// <param name="scene">scene</param>
        public static void LoadNetwork(Scene scene)
        {
            Current = scene;
            NetworkManager.Singleton.SceneManager.LoadScene(GetSceneName(Current), LoadSceneMode.Single);
        }

        /// <summary>
        /// Proceeds to the lobby.
        /// </summary>
        public static void ToLobby()
        {
            Load(Scene.Lobby);
            //Load(Scene.LocalLobby);
        }

        #endregion
    }
}
