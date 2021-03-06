﻿using IBox.Modem.IRZ.Shell;

namespace IBox.Modem.IRZ.Core
{
    /// <summary>
    ///     Modem request.
    /// </summary>
    public class ModemRequestContext
    {    
        /// <summary>
        ///     Gets or sets the response.
        /// </summary>
        /// <value>The response.</value>
        public ModemStatus Response { get; set; }

        /// <summary>
        ///     Gets or sets the connection.
        /// </summary>
        /// <value>The connection.</value>
        public ConnectConfiguration Connection { get; set; }
    }
}