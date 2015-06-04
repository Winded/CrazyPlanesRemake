using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GTA;
using GTA.Native;
using GTA.Math;

namespace HelixMod
{
    public class CrazyPlanes : Script
    {
        public class CrazyPlane
        {
            public Vehicle Vehicle;
            public Ped Pilot;
            public Blip Blip;
            public int SpawnTime;
            public int LastRocketFire;
        }

        public CrazyPlanes()
        {
            mHashes = new VehicleHash[]
            {
                VehicleHash.CargoPlane,
                VehicleHash.Cuban800,
                VehicleHash.Dodo,
                VehicleHash.Duster,
                VehicleHash.Jet,
                VehicleHash.Luxor,
                VehicleHash.Shamal,
                VehicleHash.Vestra,
            };
            mPlanes = new List<CrazyPlane>();
            Tick += OnTick;
            KeyDown += OnKeyDown;
            Interval = 100;
        }

        ~CrazyPlanes()
        {
            foreach (var cp in mPlanes)
            {
                RemovePlane(cp);
            }
            mPlanes.Clear();
        }

        private void OnKeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            if (e.KeyCode == System.Windows.Forms.Keys.B)
            {
                mEnabled = !mEnabled;
                UI.ShowSubtitle(mEnabled ? "Angry Planes ON" : "Angry Planes OFF");
                if (!mEnabled)
                {
                    foreach(var cp in mPlanes)
                    {
                        RemovePlane(cp);
                    }
                    mPlanes.Clear();
                }
            }
        }

        private bool mEnabled = false;
        private List<CrazyPlane> mPlanes;
        private VehicleHash[] mHashes;

        private void OnTick(object sender, EventArgs e)
        {
            var player = Game.Player.Character;

            if (!mEnabled)
                return;

            int i = 0;
            while(i < mPlanes.Count)
            {
                var pl = mPlanes[i];
                if(pl.Vehicle == null || pl.Vehicle.IsDead)
                {
                    pl.Vehicle.MarkAsNoLongerNeeded();
                    if(pl.Pilot != null)
                    {
                        pl.Pilot.MarkAsNoLongerNeeded();
                    }
                    if(pl.Blip != null)
                    {
                        pl.Blip.Remove();
                        pl.Blip = null;
                    }
                    continue;
                }
                if(pl.Pilot == null)
                {
                    var ped = pl.Vehicle.CreatePedOnSeat(VehicleSeat.Driver, new Model(PedHash.Jesus01));
                    ped.Task.FightAgainst(player);
                    //var blip = ped.AddBlip();
                    //blip.SetAsHostile();
                    pl.Pilot = ped;
                    //pl.Blip = blip;
                }
                i++;
            }

            FireRockets();

            if(mPlanes.Count < 40)
            {
                var spawnPos = player.Position + Vector3.RandomXYZ() * 500f;
                spawnPos.Z = player.Position.Z + 300f;

                var plane = World.CreateVehicle(new Model(RandomPlane()), spawnPos);
                if (plane == null)
                    return;
                plane.EngineRunning = true;
                plane.ApplyForceRelative(Vector3.RelativeFront * 10f);

                var cp = new CrazyPlane()
                {
                    Vehicle = plane,
                    SpawnTime = Game.GameTime
                };
                mPlanes.Add(cp);
            }
        }

        private void FireRockets()
        {
            var rocketHash = Function.Call<int>(Hash.GET_HASH_KEY, "WEAPON_VEHICLE_ROCKET");
            if (Function.Call<bool>(Hash.HAS_WEAPON_ASSET_LOADED, rocketHash))
            {
                Function.Call(Hash.REQUEST_WEAPON_ASSET, rocketHash, 31, 0);
                return;
            }

            var player = Game.Player.Character;
            foreach(var cp in mPlanes)
            {
                if(Game.GameTime - cp.LastRocketFire >= 5000 && cp.Vehicle.Position.DistanceTo(player.Position) <= 250f)
                {
                    var dir = player.Position - cp.Vehicle.Position;
                    dir.Normalize();
                    var start = cp.Vehicle.Position + dir * 10f;
                    var end = player.Position;
                    Function.Call(Hash.SHOOT_SINGLE_BULLET_BETWEEN_COORDS, start.X, start.Y, start.Z,
                        end.X, end.Y, end.Z, 250, 1, rocketHash, cp.Pilot, 1, 0, -1.0f);
                    cp.LastRocketFire = Game.GameTime;
                }
            }
        }

        private VehicleHash RandomPlane()
        {
            var rnd = new Random(Game.GameTime);
            var idx = rnd.Next(mHashes.Length);
            return mHashes[idx];
        }

        private void RemovePlane(CrazyPlane cp)
        {
            mPlanes.Remove(cp);
            if (cp.Pilot != null)
            {
                cp.Pilot.Delete();
            }
            if (cp.Blip != null)
            {
                cp.Blip.Remove();
            }
            if (cp.Vehicle != null)
            {
                cp.Vehicle.Delete();
            }
        }
    }
}
