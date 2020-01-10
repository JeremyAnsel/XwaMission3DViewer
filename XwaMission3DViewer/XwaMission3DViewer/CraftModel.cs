using JeremyAnsel.Xwa.Mission;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XwaMission3DViewer
{
    public sealed class CraftModel
    {
        public CraftModel()
        {
        }

        public CraftModel(TieFlightGroup flightGroup)
        {
            if (flightGroup == null)
            {
                throw new ArgumentNullException(nameof(flightGroup));
            }

            this.Region = flightGroup.StartPointRegions[0] + 1;
            this.CraftId = flightGroup.CraftId;
            this.CraftName = ExeConverter.CraftIdToName(flightGroup.CraftId);
            this.Name = flightGroup.Name;
            this.Markings = flightGroup.Markings;
            this.PositionX = flightGroup.StartPoints[0].PositionX;
            this.PositionY = flightGroup.StartPoints[0].PositionY;
            this.PositionZ = flightGroup.StartPoints[0].PositionZ;
            this.Yaw = flightGroup.Yaw;
            this.Pitch = flightGroup.Pitch;
            this.Roll = flightGroup.Roll;

            if (this.CraftId == 183)
            {
                Utils.ComputeHeadingAngles(this.PositionX, -this.PositionY, this.PositionZ, out double headingXY, out double headingZ);
                this.HeadingXY = headingXY;
                this.HeadingZ = headingZ;
            }
            else
            {
                int region = this.Region - 1;
                TieOrder order = flightGroup.Orders[region * 4 + 0];
                TieWaypoint waypoint = order.Waypoints[0];

                if (waypoint.M06 != 0)
                {
                    this.UseStartWaypoint = true;

                    int offsetX = waypoint.PositionX - this.PositionX;
                    int offsetY = waypoint.PositionY - this.PositionY;
                    int offsetZ = waypoint.PositionZ - this.PositionZ;

                    Utils.ComputeHeadingAngles(offsetX, -offsetY, offsetZ, out double headingXY, out double headingZ);
                    this.HeadingXY = headingXY;
                    this.HeadingZ = headingZ + 180.0;
                }
            }

            this.PlanetId = flightGroup.PlanetId;

            if (this.CraftId == 183 && this.PlanetId != 0)
            {
                var planet = AppSettings.ExePlanets?.ElementAtOrDefault(this.PlanetId);

                if (planet != null && planet.ModelIndex != 0)
                {
                    if (planet.DataIndex2 != 0)
                    {
                        this.CraftName = planet.DataIndex1 + ", " + planet.DataIndex2;
                    }
                    else if (planet.DataIndex1 == 6250)
                    {
                        this.CraftName = planet.DataIndex1 + ", 0";
                    }
                    else
                    {
                        this.CraftName = planet.DataIndex1 + ", " + flightGroup.GlobalCargoIndex;
                    }
                }
            }
        }

        public int Region { get; set; }

        public int CraftId { get; set; }

        public string CraftName { get; set; }

        public string Name { get; set; }

        public int Markings { get; set; }

        public int PositionX { get; set; }

        public int PositionY { get; set; }

        public int PositionZ { get; set; }

        public float PositionMapX
        {
            get
            {
                return this.PositionX / 160.0f;
            }
        }

        public float PositionMapY
        {
            get
            {
                return this.PositionY / 160.0f;
            }
        }

        public float PositionMapZ
        {
            get
            {
                return this.PositionZ / 160.0f;
            }
        }

        public byte Yaw { get; set; }

        public byte Pitch { get; set; }

        public byte Roll { get; set; }

        public double HeadingXY { get; set; }

        public double HeadingZ { get; set; }

        public int PlanetId { get; set; }

        public bool UseStartWaypoint { get; set; }
    }
}
