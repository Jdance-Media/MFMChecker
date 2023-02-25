using Rocket.Core.Plugins;
using SDG.Unturned;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MFMChecker
{
    public class MFMChecker : RocketPlugin
    {
        protected override void Load()
        {
            Level.onPostLevelLoaded += onLoad;
        }

        private void onLoad(int level)
        {
            List<ushort> stupidAssets = new List<ushort>();
            stupidAssets.Add(47181);
            stupidAssets.Add(42291);
            stupidAssets.Add(47228);
            stupidAssets.Add(47358);
            stupidAssets.Add(47359);
            stupidAssets.Add(47357);
            stupidAssets.Add(47356);
            foreach (BarricadeRegion region in BarricadeManager.regions)
            {
                foreach (BarricadeDrop drop in region.drops)
                {
                    if (stupidAssets.Contains(drop.asset.id))
                    {
                        region.drops.Remove(drop);
                    }
                }
            }
        }
    }
}
