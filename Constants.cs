

namespace BioShark_Blazor{
    public static class Constants{
        public const double DrainToleranceVal = 2;
        
        //Time in minutes before fill pump stops attempting tofill the reservoir
        public const int FillPumpCancellationTimer = 2;

        // Maximum Mass that can go into the mister
        public const int MisterMax = 1500;

        // Amount of time the sidekick runs before starting the cycle
        public const int SidekickMS = 120000;

        // PPM target for h2O2
        public const int TargetAmt = 125;

        // Sample Target Mass until we put it in the config
        public const int TestingTargetMass = 100;

        // Extra Mass factor for safety net in discharge step
        public const double ExtraMassFactor = 1.5;

        public const double OscillationConstant = 0.95;

        public const double PeakConstant = 1.0;
    }

}