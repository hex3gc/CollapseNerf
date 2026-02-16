using BepInEx;
using R2API.Utils;
using RoR2;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System.Reflection;
using System;
using BepInEx.Configuration;

namespace CollapseNerf
{
    [BepInPlugin(COLLAPSENERF_GUID, COLLAPSENERF_NAME, COLLAPSENERF_VER)]
    [BepInDependency("com.bepis.r2api", BepInDependency.DependencyFlags.HardDependency)]
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.EveryoneNeedSameModVersion)]
    public class Main : BaseUnityPlugin
    {
        public const string COLLAPSENERF_GUID = "com.Hex3.CollapseNerf";
        public const string COLLAPSENERF_NAME = "CollapseNerf";
        public const string COLLAPSENERF_VER = "1.0.1";
        public static Main Instance;

        public void Awake()
        {
            Log.Init(Logger);
            Log.Info($"Init {COLLAPSENERF_NAME} {COLLAPSENERF_VER}");

            Instance = this;

            Log.Info($"Creating hooks...");

            ConfigEntry<float> Collapse_MaxPercent = Instance.Config.Bind(new ConfigDefinition("Collapse nerf", "Max damage percentage per stack"), 20f, new ConfigDescription("Max percentage of an ally's health that a single stack of collapse can deal. Must be above 0.", null, Array.Empty<object>()));

            On.RoR2.DotController.InflictDot_refInflictDotInfo += (orig, ref self) =>
            {
                if 
                (
                    self.dotIndex == DotController.DotIndex.Fracture
                    && self.attackerObject 
                    && self.attackerObject.TryGetComponent(out CharacterBody attackerBody)
                    && self.victimObject.TryGetComponent(out CharacterBody victimBody)
                    && victimBody.healthComponent
                    && victimBody.teamComponent
                    && victimBody.teamComponent.teamIndex == TeamIndex.Player
                    && attackerBody.teamComponent 
                    && (attackerBody.teamComponent.teamIndex == TeamIndex.Monster || attackerBody.teamComponent.teamIndex == TeamIndex.Void)
                    && attackerBody.baseDamage > 0f
                )
                {
                    float fractureDamage = attackerBody.baseDamage * 4f;
                    float fracturePercentOfMaxHealth = fractureDamage / victimBody.healthComponent.fullCombinedHealth;
                    float maxDamageFraction = Collapse_MaxPercent.Value / 100f;
                    float finalDamageMultiplier = maxDamageFraction / fracturePercentOfMaxHealth;
                    
                    self.damageMultiplier = Math.Min(1f, finalDamageMultiplier);
                }

                orig(ref self);
            };

            Log.Info($"Done");
        }
    }
}
