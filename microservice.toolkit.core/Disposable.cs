using System;

namespace microservice.toolkit.core
{
    public abstract class Disposable : IDisposable
    {
        private bool disposedValue = false; // To detect redundant calls

        public virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    this.DisposeManage();
                }

                this.DisposeUnmanage();

                disposedValue = true;
            }
        }

        ~Disposable()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            this.Dispose(false);
        }

        // This code added to correctly implement the disposable pattern.
        void IDisposable.Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void DisposeManage()
        {
        }

        protected virtual void DisposeUnmanage()
        {
        }
    }
}
