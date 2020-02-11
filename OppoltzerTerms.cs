using System;
using System.IO;
using System.Xml.Serialization;

namespace OppolzerTerms1986
{
    /* SUBROUTINE TO COMPUTE LUNISOLAR EFFECTS ON THE EQUATORIAL COMPONENT OF THE EARTH'S ROTATION VECTOR (DIURNAL POLAR MOTION) FOR
     * A RIGID MODEL, PURELY ELASTIC MODEL AND LIQUID-CORE MODEL. 
     * 
     * THE POLAR COORDINATE  X  AND  Y  ARE OBTAINED IN A CONVENTIONAL FRAME,
     * I.E. X-AXIS ORIENTED ALONG GREENWICH MERIDIAN AND Y-AXIS 90 DEG. WEST. */

    /// <summary>
    /// Calculates the OppltzerTerms. (Rotation Axis Orientation)
    /// </summary>
    public class OppoltzerTerms
    {
        /// <summary>
        /// Contructor for Calculation the OppoltzerTerms.
        /// </summary>
        /// <param name="parameterPath"></param>
        public OppoltzerTerms(string parameterPath)
        {
            oppolzerParameter = OppolzerParameter.deserialisieren(parameterPath);
        }

        /// <summary>
        /// Object setting the needed parameter for calculation the daily polar motion.
        /// </summary>
        public OppolzerParameter oppolzerParameter = null;

        /// <summary>
        /// Caontains the calculated pol variations by the daily polar motion in given units.
        /// </summary>
        public struct PM
        {
            /// <summary>
            /// Polar coordinate X (oriented along Greenwich meridian)
            /// </summary>
            public double dX;

            /// <summary>
            /// Polar coordinate Y (oriented along 90 degree west)
            /// </summary>
            public double dY;
        }

        /// <summary>
        /// Gets/Sets the output unit for calculation.
        /// </summary>
        public Units.Units.UnitNamesEnum OutputUnit
        {
            get
            {
                return _Unit;
            }
            set
            {
                _Unit = value;
                dConversion = Units.Units.getConversion(BaseUnit, _Unit).Factor;
            }
        }
        
        /// <summary>
        /// Internal parmater for selection and setting the output unit.
        /// </summary>
        internal Units.Units.UnitNamesEnum _Unit = BaseUnit;
        
        /// <summary>
        /// Internal parmater for setting conversion factor to the output unit.
        /// </summary>
        internal double dConversion = 1d;
        
        /// <summary>
        /// Base unit, where the vales nativly calculated. 
        /// </summary>
        public static readonly Units.Units.UnitNamesEnum BaseUnit = Units.Units.UnitNamesEnum.MilliArcSecond;

        /// <summary>
        /// Calculates the OppltzerTerms depending on the Time and EarthModel. (Rotation Axis Orientation)
        /// </summary>
        /// <param name="mjd">Time in modified Julian Day.</param>
        /// <returns> DPMx, DPMy in [mas]</returns>
        public PM calulateOppoltzerTerms(double mjd)
        {
            double REV = 1296000.0f;
            double SECCON = 206264.8062470964f;

            // compute julian century
            double JC = (mjd - 51544.5f) / 36525.0f;

            // COMPUTE FUNDAMENTAL ARGUMENTS IN ARCSECONDS
            double[] ARG = new double[5];
            ARG[0] = ((+0.064f * JC + 31.310f) * JC + 715922.633f) * JC + 485866.733f + ((1325.0f * JC) % 1.0f) * REV;
            ARG[1] = ((-0.012f * JC - 0.577f) * JC + 1292581.224f) * JC + 1287099.804f + ((99.0f * JC) % 1.0f) * REV;
            ARG[2] = ((+0.011f * JC - 13.257f) * JC + 295263.137f) * JC + 335778.877f + ((1342.0f * JC) % 1.0f) * REV;
            ARG[3] = ((+0.019f * JC - 6.891f) * JC + 1105601.328f) * JC + 1072261.307f + ((1236.0f * JC) % 1.0f) * REV;
            ARG[4] = ((0.008f * JC + 7.455f) * JC - 482890.539f) * JC + 450160.280f - ((5.0f * JC) % 1.0f) * REV;

            // CONVERT ARGUMENTS TO RADIANS
            for (int j = 0; j < 5; j++)
            {
                ARG[j] = ARG[j] % REV;
                ARG[j] = ARG[j] % REV;

                if (ARG[j] < 0f)
                    ARG[j] = ARG[j] + REV;

                ARG[j] = ARG[j] / SECCON;
            }

            double EL = ARG[0];//(1)  //EL     = MEAN ANOMALY OF THE MOON IN RADIANS AT DATE TJD (OUT)
            double ELPRIM = ARG[1];//(2) //ELPRIM = MEAN ANOMALY OF THE SUN IN RADIANS AT DATE TJD (OUT)
            double F = ARG[2];//(3) //F      = MEAN LONGITUDE OF THE MOON MINUS MEAN LONGITUDE OF THE MOON'S ASCENDING NODE IN RADIANS AT DATE TJD (OUT)
            double D = ARG[3];//(4) //D      = MEAN ELONGATION OF THE MOON FROM THE SUN IN RADIANS AT DATE TJD (OUT)
            double OMEGA = ARG[4];//(5) //OMEGA  = MEAN LONGITUDE OF THE MOON'S ASCENDING NODE IN RADIANS AT DATE TJD (OUT)

            // compute Greenwich mean sidereal time
            double GMST = (24110.54841f + 8640184.812866f * JC + 0.093104f * JC * JC - 0.0000062f * JC * JC * JC);
            // compute Greenwich mean sidereal hour angle
            double GMSHA = 2 * Math.PI * ((GMST / 86400.0f + 0.5f + JC * 36525.0f) % 1.0d);

            double DPMx = 0f;
            double DPMy = 0f;

            for (int j = 0; j < oppolzerParameter.parameter.Length; j++)
            {
                //--- formation of multiples of arguments
                double arg = oppolzerParameter.parameter[j].L * EL +
                             oppolzerParameter.parameter[j].LP * ELPRIM +
                             oppolzerParameter.parameter[j].F * F +
                             oppolzerParameter.parameter[j].D * D +
                             oppolzerParameter.parameter[j].OMEGA * OMEGA +
                             GMSHA;
                //--- evaluate pole coordinates (in mas)
                DPMx = -oppolzerParameter.parameter[j].model[oppolzerParameter.EarthModelToBeUsed] * Math.Sin(arg) + DPMx;
                DPMy = oppolzerParameter.parameter[j].model[oppolzerParameter.EarthModelToBeUsed] * Math.Cos(arg) + DPMy;
            }

            return new PM { dX = DPMx * dConversion, dY = DPMy * dConversion };
        }


        /// <summary>
        /// Calculates the North and East Component of the OppltzerTerms depending on the Time, given EarthModel, and longitude of the location. (Rotation Axis Orientation)
        /// </summary>
        /// <param name="mjd">Time in modified Julian Day.</param>
        /// <param name="stationLongitude">Longitude of the Station location.</param>
        /// <returns>PMy [rad]</returns>
        public PM calulateOppoltzerTerms(double mjd, double stationLongitude)
        {
            double stlong = Math.PI / 180d * stationLongitude;

            double sa = Math.Sin(stlong);
            double ca = Math.Cos(stlong);

            PM opz = calulateOppoltzerTerms(mjd);

            return new PM 
            {
                dX = opz.dX * ca - opz.dY * sa,
                dY = -opz.dX * sa + opz.dY * ca
            };
        }


        /// <summary>
        /// Calculates the North Component of the OppltzerTerms depending on the Time, given EarthModel, and longitude of the location. (Rotation Axis Orientation)
        /// </summary>
        /// <param name="mjd">Time in modified Julian Day.</param>
        /// <param name="stationLongitude">Longitude of the Station location.</param>
        /// <returns>PMy [rad]</returns>
        public double calulateOppoltzerTermsNorthComponent(double mjd, double stationLongitude)
        {
            double stlong = Math.PI / 180d * stationLongitude;

            PM opz = calulateOppoltzerTerms(mjd);
            return (opz.dX * Math.Cos(stlong) - opz.dY * Math.Sin(stlong));
        }


        public class OppolzerParameter
        {
            /// <summary>
            /// List of the Earthmodels, can be used 
            /// </summary>
            [XmlElement("Models")]
            public string[] Models;

            [XmlIgnore()]
            public int defaultEarthModel = 0;

            [XmlIgnore()]
            public int EarthModelToBeUsed = 0;

            public Parameter[] parameter;

            public class Parameter
            {
                [XmlAttribute("L")]
                public double L;
                [XmlAttribute("LP")]
                public double LP;
                [XmlAttribute("F")]
                public double F;
                [XmlAttribute("D")]
                public double D;
                [XmlAttribute("OMEGA")]
                public double OMEGA;

                public double[] model;

                public Parameter()
                { }
            }

            /// <summary>
            /// Serialisation of the parameter file for calculation the Opplzer terms.
            /// </summary>
            /// <param name="oppolzerParameter">Path of the file to be serialisized.</param>
            public static void serialisieren(OppolzerParameter oppolzerParameter, string path)
            {
                XmlSerializer s = new XmlSerializer(typeof(OppolzerParameter));
                TextWriter w = new StreamWriter(path);
                s.Serialize(w, oppolzerParameter);
                w.Close();
            }

            /// <summary>
            /// Deserialisation of the parameter file for calculation the Opplzer terms.
            /// </summary>
            /// <param name="path">Path of the file to be deserialisized.</param>
            /// <returns>Parameter file for calculation the Opplzer terms</returns>
            public static OppolzerParameter deserialisieren(string path)
            {
                OppolzerParameter oppolzerParameter;

                XmlSerializer s = new XmlSerializer(typeof(OppolzerParameter));
                TextReader r = new StreamReader(path);
                oppolzerParameter = (OppolzerParameter)s.Deserialize(r);
                r.Close();
                return oppolzerParameter;
            }
        }
    }
}
