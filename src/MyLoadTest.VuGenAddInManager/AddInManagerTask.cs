// Copyright (c) 2014 AlphaSierraPapa for the SharpDevelop Team
// Permission is hereby granted, free of charge, to any person obtaining a copy of this
// software and associated documentation files (the "Software"), to deal in the Software
// without restriction, including without limitation the rights to use, copy, modify, merge,
// publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons
// to whom the Software is furnished to do so, subject to the following conditions:
// The above copyright notice and this permission notice shall be included in all copies or
// substantial portions of the Software.
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
// INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR
// PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE
// FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR
// OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MyLoadTest.VuGenAddInManager
{
    public sealed class AddInManagerTask<TResult>
    {
        private readonly Action<AddInManagerTask<TResult>> _continueWith;
        private Task<TResult> _task;
        private CancellationTokenSource _cancellationTokenSource;

        public AddInManagerTask(
            Func<TResult> function,
            Action<AddInManagerTask<TResult>> continueWith)
        {
            this._continueWith = continueWith;
            CreateTask(function);
        }

        public TResult Result
        {
            get
            {
                return _task.Result;
            }
        }

        public bool IsCancelled
        {
            get
            {
                return _cancellationTokenSource.IsCancellationRequested;
            }
        }

        public bool IsFaulted
        {
            get
            {
                return _task.IsFaulted;
            }
        }

        public AggregateException Exception
        {
            get
            {
                return _task.Exception;
            }
        }

        public void Start()
        {
            _task.Start();
        }

        public void Cancel()
        {
            _cancellationTokenSource.Cancel();
        }

        private void CreateTask(Func<TResult> function)
        {
            var scheduler = TaskScheduler.FromCurrentSynchronizationContext();
            _cancellationTokenSource = new CancellationTokenSource();
            _task = new Task<TResult>(function, _cancellationTokenSource.Token);
            _task.ContinueWith(OnContinueWith, scheduler);
        }

        private void OnContinueWith(Task<TResult> task)
        {
            _continueWith(this);
        }
    }
}