using System;
using System.Threading;

namespace FTPHashedUpdater
{
  //Delegate for Thread
  public delegate void DelegateSafe();

  /// <summary>
  /// Simple Thread-Wrapper that can Throw events on Exception within the Thread and
  /// Can restart without beeing reinstantiated
  /// </summary>
  public class ThreadWrap
  {
    #region Events

    /// <summary>
    /// Wird aufgerufen wenn im Thread ein Fehler auftritt
    /// </summary>
    public event ThreadExceptionEventHandler ThreadExceptionOccurred;

    /// <summary>
    /// Wird aufgerufen wenn, der Thread erfolgreich beendet.
    /// ACHTUNG: Wenn eine Exception auftritt wird dieses Event nicht geworfen.
    /// </summary>
    public event EventHandler Finished;

    #endregion Events

    #region Attributes

    private DelegateSafe _delegate;
    private Thread _thread;
    private ManualResetEvent _mreEnded = new ManualResetEvent(true);
    private string _threadName;
    private volatile bool _isBackGroundWork = true;

    #endregion Attributes

    #region Properties

    public bool IsAlive { get { return _thread != null && _thread.IsAlive; } }
    public bool IsFinished { get { return _thread != null && _thread.ThreadState == ThreadState.Stopped; } }

    /// <summary>
    /// Name of the Thread as displayed in the Debugger. Changing it has no effect on any allready running Threads
    /// </summary>
    /// <value>The name of the thread.</value>
    public String ThreadName
    {
      get
      {
        return _threadName;
      }
      set
      {
        if (String.IsNullOrEmpty(value))
        {
          throw new ArgumentNullException("value", "Threadname is not allowed to be null or empty!");
        }
        _threadName = value;
      }
    }

    public bool IsBackgroundWork
    {
      get
      {
        return _isBackGroundWork;
      }
      set
      {
        _isBackGroundWork = value;
        if (_thread != null)
          _thread.IsBackground = value;
      }
    }

    #endregion Properties

    #region Constructor

    public ThreadWrap(DelegateSafe del, String name, bool isBackgroundThread = true)
    {
      if (del == null)
        throw new ArgumentException("Delegate is null");
      IsBackgroundWork = isBackgroundThread;
      ThreadName = name;
      _delegate = del;
    }

    #endregion Constructor

    #region Event-Methods

    private void OnFinished()
    {
      if (Finished != null)
        Finished(this, EventArgs.Empty);
    }

    private void OnThreadExceptionOccured(Exception e)
    {
      if (ThreadExceptionOccurred != null)
        ThreadExceptionOccurred(this, new ThreadExceptionEventArgs(e));
    }

    #endregion Event-Methods

    #region Methodes

    /// <summary>
    /// Starts the Thread
    /// </summary>
    public void Start()
    {
      if (_thread == null || IsFinished)
      {
        _thread = CreateThread();
      }
      if (!_thread.IsAlive)
        _thread.Start();
    }

    /// <summary>
    /// Blocks until thread finishes
    /// </summary>
    public void Join()
    {
      if (IsCurrentThread())
        throw new InvalidOperationException("Thread can not join itself. This would result in infinite wait-state!");

      if (_thread != null)
        _thread.Join();
    }

    /// <summary>
    /// Working method of the Thread
    /// </summary>
    private void Work()
    {
      try
      {
        _mreEnded.Reset();
        _delegate();
        OnFinished();
      }
      catch (ThreadAbortException)
      {
        //intended exception
      }
      catch (ThreadInterruptedException)
      {
        //inteded exception
      }
      catch (Exception e)
      {
        OnThreadExceptionOccured(e);
      }
      finally
      {
        _mreEnded.Set();
      }
    }

    /// <summary>
    /// Kills the Thread no matter what
    /// </summary>
    public void Terminate()
    {
      if (_thread != null)
      {
        if (_thread.ThreadState == ThreadState.WaitSleepJoin)
          _thread.Interrupt();
        else if (_thread.IsAlive)
        {
          _thread.Abort();
        }
      }
    }

    /// <summary>
    /// Waits fo rthe Thread to End for a set amount of millisec and terminates the Thread if need after that.
    /// </summary>
    /// <param name="millisecwaittime"></param>
    public void JoinOrKill(int millisecwaittime)
    {
      if (IsAlive)
        _mreEnded.WaitOne(millisecwaittime);
      if (IsAlive)
        Terminate();
    }

    /// <summary>
    /// Check if your code is currently running in this Thread
    /// </summary>
    /// <returns></returns>
    public bool IsCurrentThread()
    {
      return Thread.CurrentThread.Equals(_thread);
    }

    private Thread CreateThread()
    {
      var thread = new Thread(Work);
      if (!string.IsNullOrEmpty(ThreadName))
      {
        thread.Name = ThreadName;
      }
      else
      {
        Console.WriteLine("ERROR: Threadname con not be empty or null!");
      }
      thread.IsBackground = _isBackGroundWork;
      return thread;
    }

    #endregion Methodes
  }
}
