using Daybreak.Common.Features.Hooks;
using JetBrains.Annotations;
using Terraria.Achievements;
using Terraria.Audio;
using Terraria.GameContent.Achievements;
using Terraria.GameContent.UI.Chat;
using Terraria.Initializers;
using Terraria.Localization;

namespace WaterMod.Content.Achievements;

[UsedImplicitly]
internal class ReefsAchievement : ModAchievement {
    public override string TextureName => Assets.Textures.UI.Achievements.SetSail.KEY;
    public CustomFlagCondition SubworldEnteredCondition { get; private set; } = null!;

    public override void SetStaticDefaults() {
        Achievement.SetCategory(AchievementCategory.Explorer);
        SubworldEnteredCondition = AddCondition("EnteredReefsSubworld");
    }

    public override Position GetDefaultPosition() => new Before("THE_GREAT_SOUTHERN_PLANTKILL");

    [OnLoad]
    [UsedImplicitly]
    static void ReplaceAchievementSound() {
        On_AchievementInitializer.OnAchievementCompleted += (orig, achievement) => {
            if (achievement.ModAchievement is SetSailAchievement) {
                var sound = Assets.Sound.KelpForestAchievement.Asset;
                
                Main.NewText(Language.GetTextValue("Achievements.Completed", AchievementTagHandler.GenerateTag(achievement)));
                if (SoundEngine.FindActiveSound(sound) == null)
                    SoundEngine.PlayTrackedSound(sound);
            }
            else {
                orig(achievement);
            }
        };
    }
}