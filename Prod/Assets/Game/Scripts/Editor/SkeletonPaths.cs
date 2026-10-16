namespace Game.EditorTools
{
    /// <summary>Пути к генерируемым ассетам. Всё сгенерированное лежит в Assets/Game/Generated.</summary>
    internal static class SkeletonPaths
    {
        public const string Root = "Assets/Game";
        public const string Generated = Root + "/Generated";

        public const string Configs = Generated + "/Configs";
        public const string Materials = Generated + "/Materials";
        public const string Animation = Generated + "/Animation";
        public const string Weapons = Generated + "/Weapons";
        public const string WeaponViews = Generated + "/Weapons/Views";
        public const string Effects = Generated + "/Prefabs/Effects";
        public const string Level = Generated + "/Prefabs/Level";
        public const string Characters = Generated + "/Prefabs/Characters";
        public const string Scenes = Root + "/Scenes";

        public const string MovementConfig = Configs + "/MovementConfig.asset";
        public const string CameraConfig = Configs + "/CameraConfig.asset";

        public const string AnimationSet = Animation + "/PlayerAnimationSet.asset";
        public const string AnimatorController = Animation + "/PlayerAnimator.controller";
        public const string UpperBodyMask = Animation + "/UpperBody.mask";
        public const string MannequinAvatar = Animation + "/MannequinAvatar.asset";

        public const string PlayerPrefab = Characters + "/Player.prefab";
        public const string DemoScene = Scenes + "/Demo.unity";

        /// <summary>Папка с Mixamo-клипами из исходного проекта (используются по умолчанию).</summary>
        public const string MixamoModels = "Assets/Models";
    }
}
