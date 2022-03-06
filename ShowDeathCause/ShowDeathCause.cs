using BepInEx;
using RoR2;
using System.Collections.Generic;
//using Zio.FileSystems;

namespace ShowDeathCause
{
    //[NetworkCompatibility(CompatibilityLevel.NoNeedForSync, VersionStrictness.DifferentModVersionsAreOk)]
    //[BepInDependency("com.bepis.r2api")]
    [BepInPlugin("dev.tsunami.ShowDeathCause", "ShowDeathCause", "2.0.2")]
    public class ShowDeathCause : BaseUnityPlugin
    {
        // These strings are added to avoid trying to access a GameObject that doesn't exist
        private DamageReport _damageReport;

        private string _finalAttacker;
        private string _damageTaken;

        //public static FileSystem FileSystem { get; private set; }

        public string GetAttackerName(DamageReport damageReport)
        {
            // Standard code path, GetBestBodyName replaces the need for a check against damageReport.attackerBody
            if (damageReport.attackerMaster)
            {
                return damageReport.attackerMaster.playerCharacterMasterController ? damageReport.attackerMaster.playerCharacterMasterController.networkUser
                            .userName : Util.GetBestBodyName(damageReport.attackerBody.gameObject);
            }
            else if (damageReport.attacker) //for overrides like Suicide() of type VoidDeath
            {
                return Util.GetBestBodyName(damageReport.attacker);
            }
            else return "???";
        }

        public bool AttackerIsVoidFog(DamageReport damageReport)
        {
            var damageInfo = damageReport.damageInfo;
            // Checking done by referencing FogDamageController's EvaluateTeam()
            return damageInfo.damageColorIndex == DamageColorIndex.Void
                && damageInfo.damageType.HasFlag(DamageType.BypassArmor)
                && damageInfo.damageType.HasFlag(DamageType.BypassBlock)
                && damageInfo.attacker == null
                && damageInfo.inflictor == null;
        }

        public void Awake()
        {
            // This function handles printing the death message in chat
            On.RoR2.GlobalEventManager.OnPlayerCharacterDeath += (orig, self, damageReport, networkUser) =>
            {
                // I wanted to remove this initially, but this would cause any mod added before ShowDeathCause
                // in the execution cycle that relied on OnPlayerCharacterDeath to not fire.
                orig(self, damageReport, networkUser);

                if (!networkUser) return;

                _damageReport = damageReport;
                _finalAttacker = GetAttackerName(damageReport);

                string token;
                _damageTaken = $"{damageReport.damageInfo.damage:F2}";
                if (damageReport.isFallDamage)
                {
                    // Fall damage is fatal when HP <=1 or when Artifact of Frailty is active
                    token = "SDC_PLAYER_DEATH_FALLDAMAGE";
                }
                else if (AttackerIsVoidFog(damageReport))
                {
                    token = "SDC_PLAYER_DEATH_VOIDFOG";
                }
                else if (damageReport.isFriendlyFire)
                {
                    // Friendly fire is possible through the Artifact of Chaos or other mods
                    // Compatibility with other mods is untested, but shouldn't break
                    token = damageReport.damageInfo.crit ? $"SDC_PLAYER_DEATH_FRIENDLY_CRIT" : $"SDC_PLAYER_DEATH_FRIENDLY";
                }
                else if ((damageReport.damageInfo.damageType & DamageType.VoidDeath) != DamageType.Generic)
                {
                    token = "SDC_PLAYER_DEATH_VOID";
                }
                else
                {
                    token = damageReport.damageInfo.crit ? $"SDC_PLAYER_DEATH_CRIT" : $"SDC_PLAYER_DEATH";
                }

                Chat.SendBroadcastChat(new Chat.SimpleChatMessage
                {
                    baseToken = token,
                    paramTokens = new string[] { networkUser.userName, _finalAttacker, _damageTaken }
                });
            };

            // This upgrades the game end panel to show damage numbers and be more concise
            On.RoR2.UI.GameEndReportPanelController.SetPlayerInfo += (orig, self, playerInfo) =>
            {
                orig(self, playerInfo);

                // Do nothing if the damage report is unset
                if (_damageReport == null) return;

                // Override the string for killerBodyLabel ("Killed By: <killer>" on the end game panel)
                string token;
                if (_damageReport.isFallDamage)
                {
                    token = "SDC_GENERIC_PREFIX_DEATH";
                    _finalAttacker = Language.GetString("SDC_KILLER_FALLDAMAGE");
                    self.killerBodyPortraitImage.texture = RoR2Content.Artifacts.weakAssKneesArtifactDef.smallIconSelectedSprite.texture;
                }
                else if (AttackerIsVoidFog(_damageReport))
                {
                    token = "SDC_GENERIC_PREFIX_DEATH";
                    _finalAttacker = Language.GetString("SDC_KILLER_VOIDFOG");
                    self.killerBodyPortraitImage.texture = RoR2Content.Buffs.VoidFogMild.iconSprite.texture;
                }
                else if (_damageReport.isFriendlyFire)
                {
                    token = "SDC_GENERIC_PREFIX_DEATH_FRIENDLY";
                }
                else if ((_damageReport.damageInfo.damageType & DamageType.VoidDeath) != DamageType.Generic)
                {
                    token = "SDC_GENERIC_PREFIX_DEATH_VOID";
                }
                else
                {
                    token = $"SDC_GENERIC_PREFIX_DEATH";
                }
                self.killerBodyLabel.text = Language.GetStringFormatted(token, new object[] { _finalAttacker, _damageTaken });
            };
            // This sets up the language

            #region Language

            /*PhysicalFileSystem physicalFileSystem = new PhysicalFileSystem();
            var assemblyDir = System.IO.Path.GetDirectoryName(Info.Location);
            FileSystem = new SubFileSystem(physicalFileSystem, physicalFileSystem.ConvertPathFromInternal(assemblyDir), true);

            if (FileSystem.DirectoryExists("/Language/"))
            {
                Language.collectLanguageRootFolders += delegate (List<string> list)
                {
                    list.Add(System.IO.Path.Combine(assemblyDir, "Language"));
                };
            }*/

            Language.onCurrentLanguageChanged += Language_onCurrentLanguageChanged;
            #endregion Language

        }

        private void Language_onCurrentLanguageChanged()
        {
            if (Language.GetString("SDC_PLAYER_DEATH_FALLDAMAGE") == "SDC_PLAYER_DEATH_FALLDAMAGE") //this always runs now so it can be removed
            {
                List<KeyValuePair<string, string>> list = new List<KeyValuePair<string, string>>()
                {
                    new KeyValuePair<string, string>("SDC_KILLER_FALLDAMAGE", "<color=#964B00>Fall Damage</color>"),
                    new KeyValuePair<string, string>("SDC_KILLER_VOIDFOG", "<color=#753f8a>Void Fog</color>"),

                    new KeyValuePair<string, string>("SDC_GENERIC_PREFIX_DEATH", "<color=#FFFFFF>Killed By:</color> <color=#FFFF80>{0}</color> <color=#FFFFFF>({1} damage)</color>"),
                    new KeyValuePair<string, string>("SDC_GENERIC_PREFIX_DEATH_FRIENDLY", "<color=#FFFFFF>Killed By:</color> <color=#FFFF80>{0}</color> <color=#FFFFFF>({1} damage) <color=#32a852>(FF)</color></color>"),
                    new KeyValuePair<string, string>("SDC_GENERIC_PREFIX_DEATH_VOID", "<color=#FFFFFF>Killed By:</color> <color=#FFFF80>{0}</color> <color=#FFFFFF>({1} damage) <color=#753f8a>(Jail)</color></color>"),

                    new KeyValuePair<string, string>("SDC_PLAYER_DEATH_VOIDFOG", "<color=#00FF80>{0}</color> died to <color=#964B00>void fog</color> ({2} damage taken)."),
                    new KeyValuePair<string, string>("SDC_PLAYER_DEATH_FALLDAMAGE", "<color=#00FF80>{0}</color> died to <color=#964B00>fall damage</color> ({2} damage taken)."),
                    new KeyValuePair<string, string>("SDC_PLAYER_DEATH_FRIENDLY", "<color=#32a852>FRIENDLY FIRE!</color> <color=#00FF80>{0}</color> killed by <color=#FF8000>{1}</color> ({2} damage taken)."),
                    new KeyValuePair<string, string>("SDC_PLAYER_DEATH_FRIENDLY_CRIT", "<color=#32a852>FRIENDLY FIRE!</color> <color=#FF0000>CRITICAL HIT!</color> <color=#00FF80>{0}</color> killed by <color=#FF8000>{1}</color> ({2} damage taken)."),
                    new KeyValuePair<string, string>("SDC_PLAYER_DEATH", "<color=#00FF80>{0}</color> killed by <color=#FF8000>{1}</color> ({2} damage taken)."),
                    new KeyValuePair<string, string>("SDC_PLAYER_DEATH_CRIT", "<color=#FF0000>CRITICAL HIT!</color> <color=#00FF80>{0}</color> killed by <color=#FF8000>{1}</color> ({2} damage taken)."),
                    new KeyValuePair<string, string>("SDC_PLAYER_DEATH_VOID", "<color=#621e7d>JAILED!</color> <color=#00FF80>{0}</color> killed by <color=#FF8000>{1}</color>."),
                };

                Language.currentLanguage.SetStringsByTokens(list);
            }
        }
    }
}