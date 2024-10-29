namespace RUDP.Models
{
    internal class CongestionWindow : IDisposable
    {
        internal double MinSafeBandWidth { get; set; }
        private double _safeBandWidth { get; set; }
        /// <summary>
        /// Expressed in bytes, is the maximum bandwidth that should be used to maintain the channel stability
        /// </summary>
        internal double SafeBandWidth
        {
            get => _safeBandWidth;
            set
            {
                _safeBandWidth = Math.Max(value, MinSafeBandWidth);
            }
        }
        internal double MaxRecordedSafeBandWidth { get; set; }


        internal double CurrentRTTValue { get; set; } = 0;
        internal double PreviousRTTValue { get; set; } = 0;
        internal double[] RTTValues { get; set; } = new double[20];
        internal double RTTMedian => RTTValues.Where(x => x > 0).Sum() / RTTValues.Length;
        internal int RTTValuesIndex { get; set; } = 0;
        internal bool Initialized { get; set; } = false;


        /// <summary>
        /// Given the new Round Trip Time, this method calculates all relative statistics based on hystorical data
        /// </summary>
        /// <param name="newRTTmilliseconds"></param>
        internal void SetStatistics(double newRTTmilliseconds)
        {
            // After 2 RTTs have been received we can start calculating based on history
            if (!Initialized && RTTValuesIndex > 0)
                Initialized = true;

            if (RTTValuesIndex == RTTValues.Length)
                RTTValuesIndex = 0;

            PreviousRTTValue = CurrentRTTValue;
            CurrentRTTValue = newRTTmilliseconds;
            RTTValues[RTTValuesIndex++] = newRTTmilliseconds;
            MaxRecordedSafeBandWidth = Math.Max(MaxRecordedSafeBandWidth, SafeBandWidth);
        }


        public void Dispose()
        {
            RTTValues = new double[0];
        }
    }
}
