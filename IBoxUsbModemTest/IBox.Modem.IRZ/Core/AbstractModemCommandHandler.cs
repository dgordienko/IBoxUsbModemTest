using System;
using System.Diagnostics;
using System.Threading;
using IBox.Modem.IRZ.Shell;

namespace IBox.Modem.IRZ.Core
{
    public abstract class AbstractModemCommandHandler : IModemRequestHandler
    {
        private const int Sleep = 500;
        protected volatile bool received;
        protected volatile ModemStatus modemStatus;

        protected readonly ModemManager manager = ModemManager.Instance;

        private IModemRequestHandler _nextHandler;
        protected object _locker = new object();

        protected abstract string ModemCommand {get;}

        protected virtual void OnStatusChanged(object sender, string response)
        {
            Debug.WriteLine(response);
        }

        protected abstract void OnDataReceived(object sender, string response);

        protected virtual void OnSerialPortOpened(object sender, bool response)
        {
            if (response)
            {
                try
                {
                    (sender as ModemManager).SendString(ModemCommand);
                    received = true;
                }
                catch (Exception exception)
                {
                    //request.Description.Add($"{AT} - {exception.Message}");
                    Debug.WriteLine(exception.Message);
                    received = false;
                }
            }
        }

        public IModemRequestHandler SetNext(IModemRequestHandler handler)
        {
            _nextHandler = handler;
            return handler;
        }

        public virtual ModemRequestContext Handle(ModemRequestContext request, string param)
        {
            if(param == ModemCommand)
            {
                lock (_locker)
                {
                    modemStatus = request.Response?.Clone();

                    manager.OnSerialPortOpened += OnSerialPortOpened;
                    manager.OnStatusChanged += OnStatusChanged;
                    manager.OnDataReceived += OnDataReceived;

                    if (!manager.IsOpen)
                        manager.Open(request.Connection.PortName);
                    else
                    {
                        manager.Close();
                        manager.Open(request.Connection.PortName);
                    }
                    {
                        while (received)
                        {
                            Monitor.Wait(_locker, TimeSpan.FromMilliseconds(Sleep));
                        }
                    }
                    manager.OnSerialPortOpened -= OnSerialPortOpened;
                    manager.OnStatusChanged -= OnStatusChanged;
                    manager.OnDataReceived -= OnDataReceived;
                    var result = new ModemRequestContext
                    {
                        Connection = request.Connection,
                        Response = modemStatus
                    };
                    return result;
                }
            }
            return _nextHandler?.Handle(request, param);
        }
    }
}