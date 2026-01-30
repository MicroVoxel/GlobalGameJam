namespace Core.Data
{
    /// <summary>
    /// Centralized constants to avoid magic strings throughout the project.
    /// </summary>
    public static class GameConstants
    {
        // Scene Names
        public const string SCENE_MAIN_MENU = "MainMenuScene";
        public const string SCENE_GAMEPLAY = "GameplayScene";

        // PlayerPrefs Keys
        public const string KEY_SETTINGS = "GameSettings";

        // Animation Parameters
        public const float UI_FADE_DURATION = 0.4f;
    }
}