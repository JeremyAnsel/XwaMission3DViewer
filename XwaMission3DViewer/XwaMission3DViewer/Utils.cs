using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace XwaMission3DViewer
{
    static class Utils
    {
        public static void ComputeHeadingAngles(int positionX, int positionY, int positionZ, out double headingXY, out double headingZ)
        {
            Vector posXY = new Vector(positionX, positionY);
            if (posXY.LengthSquared == 0.0)
            {
                headingXY = 0.0;
            }
            else
            {
                posXY.Normalize();

                if (posXY.X == 0.0)
                {
                    if (posXY.Y > 0.0)
                    {
                        headingXY = 0.0;
                    }
                    else
                    {
                        headingXY = -180.0;
                    }
                }
                else if (posXY.X > 0.0)
                {
                    headingXY = Math.Acos(posXY.Y) * 180.0 / Math.PI;
                }
                else
                {
                    headingXY = -Math.Acos(posXY.Y) * 180.0 / Math.PI;
                }
            }

            Vector posZ = new Vector(positionX == 0 ? positionY : positionX, positionZ);
            if (posZ.LengthSquared == 0.0)
            {
                headingZ = 0.0;
            }
            else
            {
                posZ.Normalize();

                if (posZ.X == 0.0)
                {
                    if (posZ.Y < 0.0)
                    {
                        headingZ = 90.0;
                    }
                    else
                    {
                        headingZ = -90.0;
                    }
                }
                else if (posZ.Y > 0.0)
                {
                    headingZ = Math.Acos(posZ.X) * 180.0 / Math.PI;
                }
                else
                {
                    headingZ = -Math.Acos(posZ.X) * 180.0 / Math.PI;
                }

                if (headingXY >= 0.0)
                {
                    headingZ += 180.0;
                }
            }
        }
    }
}
