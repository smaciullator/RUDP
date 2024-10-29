namespace RUDP.Utilities
{
    public static class ThreadUtilities
    {
        /// <summary>
        /// Mette il pausa il thread per l'esatta durata indicata in millisecondi, senza consumare la cpu
        /// </summary>
        /// <param name="milliseconds"></param>
        public static void PauseThread(int milliseconds)
        {
            ManualResetEvent wait = new ManualResetEvent(false);
            wait.WaitOne(milliseconds);
            wait.Dispose();
        }
        /// <summary>
        /// Mette il pausa il thread per l'esatta durata indicata tramite TimeSpan, senza consumare la cpu
        /// </summary>
        /// <param name="sleepTime"></param>
        public static void PauseThread(TimeSpan sleepTime)
        {
            ManualResetEvent wait = new ManualResetEvent(false);
            wait.WaitOne(sleepTime);
            wait.Dispose();
        }
    }
}
