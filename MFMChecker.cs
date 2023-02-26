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
            stupidAssets.Add(47228);
            stupidAssets.Add(47356);
            stupidAssets.Add(47357);
            stupidAssets.Add(47358);
            stupidAssets.Add(47359);
            stupidAssets.Add(47181);
            stupidAssets.Add(47352);
            stupidAssets.Add(47353);
            stupidAssets.Add(47354);
            stupidAssets.Add(47355);
            stupidAssets.Add(42291);
            stupidAssets.Add(47242);
            stupidAssets.Add(47243);

            List<RemovalQueue> removals = new List<RemovalQueue>();
            foreach (BarricadeRegion region in BarricadeManager.regions)
            {
                foreach (BarricadeDrop drop in region.drops)
                {
                    if (stupidAssets.Contains(drop.asset.id))
                    {
                        removals.Add(new RemovalQueue(drop, region));
                    }
                }
            }

            foreach (RemovalQueue removal in removals)
            {
                removal.Region.barricades.Remove(removal.Drop.GetServersideData());
                removal.Region.drops.Remove(removal.Drop);
            }
            Level.onPostLevelLoaded -= onLoad;
        }

        public class RemovalQueue
        {
            public BarricadeDrop Drop { get; set; }
            public BarricadeRegion Region { get; set; }

            public RemovalQueue(BarricadeDrop drop, BarricadeRegion region)
            {
                Drop = drop;
                Region = region;
            }
        }
    }
}
