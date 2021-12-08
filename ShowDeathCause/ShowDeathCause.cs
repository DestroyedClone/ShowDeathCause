using BepInEx;
using R2API.Utils;
using RoR2;
using System.Collections.Generic;
using Zio;
using Zio.FileSystems;

namespace ShowDeathCause
{
    [NetworkCompatibility(CompatibilityLevel.NoNeedForSync, VersionStrictness.DifferentModVersionsAreOk)]
    [BepInDependency("com.bepis.r2api")]
    [BepInPlugin("dev.tsunami.ShowDeathCause", "ShowDeathCause", "2.0.2")]
    public class ShowDeathCause : BaseUnityPlugin
    {
        // These strings are added to avoid trying to access a GameObject that doesn't exist
        private DamageReport _damageReport;
        private string _finalAttacker;
        private string _damageTaken;

        public static FileSystem FileSystem { get; private set; }

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

                string token;
                _damageTaken = $"{damageReport.damageInfo.damage:F2}";
                if (damageReport.isFallDamage)
                {
                    // Fall damage is fatal when HP <=1 or when Artifact of Frailty is active
                    token = "SDC_PLAYER_DEATH_FALLDAMAGE";
                }
                else if (damageReport.isFriendlyFire)
                {
                    // Friendly fire is possible through the Artifact of Chaos or other mods
                    // Compatibility with other mods is untested, but shouldn't break
                    _finalAttacker = damageReport.attackerMaster.playerCharacterMasterController.networkUser
                        .userName;
                    token = damageReport.damageInfo.crit ? $"SDC_PLAYER_DEATH_FRIENDLY_CRIT" : $"SDC_PLAYER_DEATH_FRIENDLY";
                }
                else
                {
                    // Standard code path, GetBestBodyName replaces the need for a check against damageReport.attackerBody
                    _finalAttacker = Util.GetBestBodyName(damageReport.attackerBody.gameObject);
                    token = "SDC_PLAYER_DEATH";
                }

                Chat.SendBroadcastChat(new Chat.SimpleChatMessage {
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
                    token = "FALLDAMAGE_PREFIX_DEATH";
                }
                else if (_damageReport.isFriendlyFire)
                {
                    token = $"GENERIC_PREFIX_DEATH";
                }
                else
                {
                    token = $"GENERIC_PREFIX_DEATH";
                }
                self.killerBodyLabel.text = Language.GetStringFormatted(token, new object[] { _finalAttacker, _damageTaken });
            };

            // This sets up the language
            #region Language
            PhysicalFileSystem physicalFileSystem = new PhysicalFileSystem();
            var assemblyDir = System.IO.Path.GetDirectoryName(Info.Location);
            FileSystem = new SubFileSystem(physicalFileSystem, physicalFileSystem.ConvertPathFromInternal(assemblyDir), true);

            if (FileSystem.DirectoryExists("/Language/"))
            {
                Language.collectLanguageRootFolders += delegate (List<DirectoryEntry> list)
                {
                    list.Add(FileSystem.GetDirectoryEntry("/Language/"));
                };
            }
            #endregion
        }
    }
}
