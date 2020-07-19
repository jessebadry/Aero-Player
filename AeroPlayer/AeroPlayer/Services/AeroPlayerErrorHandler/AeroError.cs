using AeroPlayer.Services.Notifications;
using AeroPlayer.Views.Dialogs;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace AeroPlayer.Services.AeroPlayerErrorHandler
{
    static class AeroError
    {
        public static void EmitError(string error)
        {
            Aerocations.ShowErrorNotification(error);
        }

    }
}
