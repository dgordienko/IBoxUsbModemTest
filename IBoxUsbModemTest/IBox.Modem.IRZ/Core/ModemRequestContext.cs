using System.Collections.Generic;
using IBox.Modem.IRZ.Shell;

namespace IBox.Modem.IRZ.Core
{
    /// <summary>
    ///     Modem request.
    /// </summary>
    public class ModemRequestContext
    {
        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="T:IBox.Modem.IRZ.Core.ModemRequestContext"/> class.
        /// </summary>
        public ModemRequestContext()
        {
            Description = new List<string>();
        }
        /// <summary>
        /// Gets the description.
        /// </summary>
        /// <value>The description.</value>
        public List<string> Description { get; }

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