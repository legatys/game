namespace Game.EditorTools
{
    /// <summary>Пути к генерируемым ассетам. Всё сгенерированное лежит в Assets/Game/Generated.</summary>
    internal static class SkeletonPaths
    {
        /// <summary>Корень проекта игры.</summary>
        public const string Root = "Assets/Game";
        /// <summary>Папка для всего сгенерированного.</summary>
        public const string Generated = Root + "/Generated";

        /// <summary>Папка конфигов (ScriptableObject).</summary>
        public const string Configs = Generated + "/Configs";
        /// <summary>Папка материалов.</summary>
        public const string Materials = Generated + "/Materials";
        /// <summary>Папка анимационных ассетов (контроллер, маска, Avatar).</summary>
        public const string Animation = Generated + "/Animation";
        /// <summary>Папка WeaponDefinition-ов.</summary>
        public const string Weapons = Generated + "/Weapons";
        /// <summary>Папка префабов моделей оружия.</summary>
        public const string WeaponViews = Generated + "/Weapons/Views";
        /// <summary>Папка префабов эффектов.</summary>
        public const string Effects = Generated + "/Prefabs/Effects";
        /// <summary>Папка префабов объектов уровня.</summary>
        public const string Level = Generated + "/Prefabs/Level";
        /// <summary>Папка префабов персонажей.</summary>
        public const string Characters = Generated + "/Prefabs/Characters";
        /// <summary>Папка сцен.</summary>
        public const string Scenes = Root + "/Scenes";

        /// <summary>Ассет настроек передвижения.</summary>
        public const string MovementConfig = Configs + "/MovementConfig.asset";
        /// <summary>Ассет настроек камеры.</summary>
        public const string CameraConfig = Configs + "/CameraConfig.asset";

        /// <summary>Набор анимаций игрока для генератора.</summary>
        public const string AnimationSet = Animation + "/PlayerAnimationSet.asset";
        /// <summary>Сгенерированный Animator Controller игрока.</summary>
        public const string AnimatorController = Animation + "/PlayerAnimator.controller";
        /// <summary>Маска верхней части тела для слоя UpperBody.</summary>
        public const string UpperBodyMask = Animation + "/UpperBody.mask";
        /// <summary>Humanoid Avatar манекена.</summary>
        public const string MannequinAvatar = Animation + "/MannequinAvatar.asset";

        /// <summary>Префаб игрока.</summary>
        public const string PlayerPrefab = Characters + "/Player.prefab";
        /// <summary>Сцена Demo, собранная генератором.</summary>
        public const string DemoScene = Scenes + "/Demo.unity";

        /// <summary>Папка с Mixamo-клипами из исходного проекта (используются по умолчанию).</summary>
        public const string MixamoModels = "Assets/Models";
    }
}
