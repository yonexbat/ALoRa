using System;

namespace ALoRa.Library;

public abstract class BaseObject : IDisposable
{
    private readonly object m_lock = new();

    public bool IsDisposed { get; private set; }

    public bool IsDisposing { get; private set; }

    void IDisposable.Dispose()
    {
        lock (m_lock)
        {
            if (IsDisposed || IsDisposing)
            {
                return;
            }

            IsDisposing = true;
        }

        try
        {
            Dispose(true);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Execption while disposing {0} {1}", GetType().FullName, ex);
        }
        finally
        {
            IsDisposed = true;
        }
    }

    protected abstract void Dispose(bool disposing);

    public void Dispose()
    {
        (this as IDisposable).Dispose();
    }

    protected void CheckDisposed()
    {
        lock (m_lock)
        {
            ObjectDisposedException.ThrowIf(IsDisposed || IsDisposing, this);
        }
    }
}