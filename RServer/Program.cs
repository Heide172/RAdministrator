using System;

namespace RServer
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            var logger = NLog.LogManager.GetCurrentClassLogger();
            logger.Info("App Started {arguments} {username}", args,"pidor");

            try
            {

            }
            catch (Exception e)
            {

                logger.Error(e, "{args}", 2);
            }
            NLog.LogManager.Shutdown();
        }

    }


}