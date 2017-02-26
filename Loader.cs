using System;

namespace Rsx
{
  /// <summary>
  /// This is a System.ComponentModel.BackgroundWorker
  /// For be used by another class?
  /// </summary>
  public class Loader : System.ComponentModel.BackgroundWorker
  {
    public Loader()
    {
    }

    /// <summary>
    /// The function that should be invoked to report progress
    /// </summary>
    /// <param name="percent">the progress percent</param>
    public delegate void Reporter(int percent);

    private void worker_ProgressChanged(object sender, System.ComponentModel.ProgressChangedEventArgs e)
    {
      if (report == null) return;

      if (e.UserState != null)
      {
        SystemException ex = e.UserState as SystemException;
        System.Windows.Forms.MessageBox.Show(ex.Message + "\n\n" + ex.StackTrace + "\n\n" + ex.TargetSite, "Problems loading a data table content");
      }
      int percentage = e.ProgressPercentage;
      report.Invoke(percentage);
    }

    private void worker_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
    {
      int length = mainMethods.Length;
      double step = 100;
      if (length != 1) step = (100.0 / (length - 1));

      for (int i = 0; i < mainMethods.Length; i++)
      {
        if (e.Cancel) continue;
        int perc = Convert.ToInt32(Math.Ceiling((step * i)));
        SystemException x = null;
        try
        {
          Action async = mainMethods[i];
          if (async == null) continue;
          async.Invoke();
        }
        catch (SystemException ex)
        {
          x = ex;
        }
        ReportProgress(perc, x);
      }
    }

    public void Set(Action[] LoadMethods, Action CallBackMethod, Reporter ReportMethod)
    {
      mainMethods = LoadMethods;
      callback = CallBackMethod;
      report = ReportMethod;
      this.WorkerReportsProgress = true;
      this.WorkerSupportsCancellation = true;
      this.DoWork += worker_DoWork;
      this.ProgressChanged += worker_ProgressChanged;
      this.RunWorkerCompleted += worker_RunWorkerCompleted;
    }

    private void worker_RunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
    {
      if (callback != null)
      {
        callback.Invoke();
      }
      this.Dispose();
    }

    private Action[] mainMethods;
    private Action callback;
    private Reporter report;
  }
}