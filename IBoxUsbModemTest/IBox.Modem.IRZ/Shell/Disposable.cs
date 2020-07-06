using System;

namespace IBox.Modem.IRZ.Shell
{
    public abstract class Disposable : IDisposable
    {
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        //public Disposable()
        //{
        //    // Constructor
        //}

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Dispose managed resources
            }

            // Dispose unmanaged resources
        }

        ~Disposable()
        {
            Dispose(false);
        }
    }
}