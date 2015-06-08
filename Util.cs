using GTA.Math;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrazyPlanes
{
    static class Util
    {
        public static Vector3 RotationToDirection(Vector3 rotation)
        {
            var ret = new Vector3(0, 0, 0);
            ret.X = (float)(Math.Cos(rotation.Y) * Math.Cos(rotation.X));
            ret.Y = (float)(Math.Sin(rotation.Y) * Math.Cos(rotation.X));
            ret.Z = (float)Math.Sin(rotation.X);
            return ret;
        }

        public static Vector3 HeadingToDirection(float heading)
        {
            var ret = new Vector3(0, 0, 0);
            ret.X = (float)Math.Cos(heading);
            ret.Y = (float)Math.Sin(heading);
            return ret;
        }
    }
}
