﻿using Anotar.Log4Net;
using LCU.NET;
using LCU.NET.API_Models;
using LCU.NET.Plugins.LoL;
using Legendary_Rune_Maker.Game;
using Legendary_Rune_Maker.Locale;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Legendary_Rune_Maker.Data
{
    public class RunePage
    {
        public int[] RuneIDs { get; set; }
        public int PrimaryTree { get; set; }
        public int SecondaryTree { get; set; }

        public string Name { get; set; }

        public int ChampionID { get; set; }
        public Position Position { get; set; }

        public RunePage()
        {
        }

        public RunePage(int[] runeIDs, int primaryTree, int secondaryTree, int championId, Position position)
        {
            this.RuneIDs = runeIDs;
            this.PrimaryTree = primaryTree;
            this.SecondaryTree = secondaryTree;
            this.ChampionID = championId;
            this.Position = position;
        }

        private void VerifyRunes()
        {
            this.RuneIDs = this.RuneIDs.Select(o => Riot.Runes.Single(i => i.Key == o).Key).ToArray();
        }

        public async Task UploadToClient(IPerks perks)
        {
            if (!GameState.CanUpload)
                return;

            VerifyRunes();

            var page = new LolPerksPerkPageResource
            {
                primaryStyleId = PrimaryTree,
                subStyleId = SecondaryTree,
                selectedPerkIds = RuneIDs,
                name = this.Name ?? Riot.GetChampion(ChampionID).Name + " - " + Enum.GetName(typeof(Position), Position)
            };

            LogTo.Debug("Uploading rune page with name '{0}'");

            if (Config.Default.LastRunePageId != default)
            {
                try
                {
                    await perks.DeletePageAsync(Config.Default.LastRunePageId);
                }
                catch
                {
                }
            }

            try
            {
                var pageRet = await perks.PostPageAsync(page);
                Config.Default.LastRunePageId = pageRet.id;
            }
            catch (APIErrorException ex) when (ex.Message == "Max pages reached")
            {
                LogTo.Info("Max number of rune pages reached, deleting current page and trying again");

                var currentPage = await perks.GetCurrentPageAsync();

                if (currentPage.isDeletable)
                {
                    await perks.DeletePageAsync(currentPage.id);
                    await UploadToClient(perks);
                }
                else
                {
                    MainWindow.ShowNotification(Text.CantUploadPageTitle, Text.CantUploadPageMessage);
                    return;
                }
            }
        }

        public static async Task<RunePage> GetActivePageFromClient(IPerks perks)
        {
            var page = await perks.GetCurrentPageAsync();

            return new RunePage(page.selectedPerkIds, page.primaryStyleId, page.subStyleId, 0, Position.Fill)
            {
                Name = page.name
            };
        }
    }
}
