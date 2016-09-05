using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Rsx
{
    /// <summary>
    /// A class for any worker (in principle)
    /// </summary>
    public interface IWD
    {
        object[] Arguments { get; set; }

        void Async();

        Action CallBackMethod { get; set; }
        Action ReportMethod { get; set; }
        WD.Work WorkMethod { get; set; }
        bool CancelAsync { set; }
    }

    public class WD : IWD
    {
        private object[] args;

        public object[] Arguments
        {
            get
            {
                return args;
            }
            set
            {
                args = value;
            }
        }

        public delegate void Work(ref object arrayElement, ref object toUseOnElement);

        private Action reportMethod;
        private Action callbackMethod;

        private Work workMethod;

        public bool CancelAsync
        {
            set
            {
                if (worker != null && value) worker.CancelAsync();
            }
        }

        public Work WorkMethod
        {
            get { return workMethod; }
            set { workMethod = value; }
        }

        public Action CallBackMethod
        {
            get { return callbackMethod; }
            set { callbackMethod = value; }
        }

        public Action ReportMethod
        {
            get { return reportMethod; }
            set { reportMethod = value; }
        }

        private BackgroundWorker worker;

        public void Async()
        {
            if (workMethod == null) throw new SystemException("Please specify a WorkMethod");

            if (args[2] == null) args[2] = false;
            else if (reportMethod == null) args[2] = false; //the final word is mine. If there is no report method, inform is false...

            if (worker != null) worker.CancelAsync();

            worker = new BackgroundWorker();
            worker.DoWork += new DoWorkEventHandler(worker_DoWork);
            worker.ProgressChanged += new ProgressChangedEventHandler(worker_ProgressChanged);
            worker.WorkerReportsProgress = true;
            worker.WorkerSupportsCancellation = true;
            worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(worker_RunWorkerCompleted);
            worker.RunWorkerAsync(args);
        }

        private void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;

            object[] args = e.Argument as object[];
            bool inform = (bool)args[2];
            object reader = args[1]; //might be null...

            object array = args[0];

            //if the user sent a null array, just execute the method, the user should know what does the method with these parms..
            //also the user should know hoy to try-catch errors in the method...
            if (array == null)
            {
                if (!worker.CancellationPending)
                {
                    workMethod(ref array, ref reader);
                    if (inform) worker.ReportProgress(1);
                }
            }
            else
            {
                IEnumerable<object> ls = array as IEnumerable<object>;
                foreach (object o in ls)
                {
                    if (worker.CancellationPending) break;
                    object fileinfo = o;
                    workMethod(ref fileinfo, ref reader);
                    if (inform) worker.ReportProgress(1);
                }
            }
        }

        private void worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            reportMethod();
        }

        private void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (callbackMethod != null) callbackMethod();

            IDisposable i = sender as IDisposable;
            i.Dispose();
            i = null;
        }
    }
}