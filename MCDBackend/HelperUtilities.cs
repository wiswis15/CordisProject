using System;
using log4net;

namespace MCDBackend
{
    public static class HelperUtilities
    {
        public static void LogException(string message, ILog log, Exception ex)
        {
            log.Error(message);
            log.Error("Reason: " + ex.Message);
            if (ex.InnerException != null && !string.IsNullOrEmpty(ex.InnerException.Message))
            {
                log.Error("Details: \n" + ex.InnerException);
            }
        }
    }
}
