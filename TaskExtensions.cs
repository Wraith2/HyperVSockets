using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleApp6
{
    static class TaskExtensions
    {
        private static readonly Action<Task> s_logException = LogForgottenException;

        public static void AsDisconnected(this Task task, CancellationToken cancellationToken = default)
        {
            task.ContinueWith(
                s_logException,
                cancellationToken,
                TaskContinuationOptions.OnlyOnFaulted,
                TaskScheduler.Current
            ).ConfigureAwait(false);
        }

        public static void LogForgottenException(Task task)
        {
            if (task.Exception is AggregateException aggregateException && aggregateException.InnerException is Exception exception)
            {
                Debug.WriteLine(CreateExceptionMessage(exception));
            }
        }

        //public static void AsDisconnectedRequestResponse<TResult, TState>(
        //    this Func<object?, Task<TResult>> request,
        //    Func<Task<TResult>, object?, Task> response,
        //    TState state,
        //    CancellationToken cancellationToken = default
        //)
        //    where TState : class
        //{
        //    Task.Factory.StartNew(
        //        function: request,
        //        state: state,
        //        cancellationToken,
        //        TaskCreationOptions.DenyChildAttach,
        //        TaskScheduler.Default
        //    )
        //    .Unwrap()
        //    .ContinueWith(
        //        continuationFunction: response,
        //        state: state,
        //        cancellationToken,
        //        TaskContinuationOptions.DenyChildAttach,
        //        TaskScheduler.Default
        //    )
        //    .AsDisconnected(cancellationToken);
        //}

        //public static void AsDisconnectedRequest<TState>(
        //    this Func<object?, Task> request,
        //    TState state,
        //    CancellationToken cancellationToken = default
        //)
        //    where TState : class
        //{
        //    Task.Factory.StartNew(
        //        function: request,
        //        state: state,
        //        cancellationToken,
        //        TaskCreationOptions.DenyChildAttach,
        //        TaskScheduler.Default
        //    )
        //    .Unwrap()
        //    .AsDisconnected(cancellationToken);
        //}

        //public static void AsDisconnectedRequest(
        //    this Func<Task> request,
        //    CancellationToken cancellationToken = default
        //)
        //{
        //    Task.Factory.StartNew(
        //        function: request,
        //        cancellationToken,
        //        TaskCreationOptions.DenyChildAttach,
        //        TaskScheduler.Default
        //    )
        //    .Unwrap()
        //    .AsDisconnected(cancellationToken);
        //}

        public static string CreateExceptionMessage(Exception ex)
        {
            return CreateExceptionMessage(ex, Environment.NewLine);
        }
        public static string CreateExceptionMessage(Exception ex, string newline)
        {
            const string nullstr = "null";
            const int indentSize = 2;
            StringBuilder message = new StringBuilder();
            if (ex != null)
            {
                ex = ex.Demystify();
                message.Append("Exception:");
                message.Append(newline);
                message.Append(' ', indentSize);
                message.Append("name=");
                message.Append(((ex.GetType() != null) ? ex.GetType().Name : nullstr));
                message.Append(newline);
                message.Append(' ', indentSize);
                message.Append("target=");
                message.Append(ex.TargetSite?.Name ?? nullstr);
                message.Append(newline);
                message.Append(' ', indentSize);
                message.Append("message=\"");
                message.Append(ex.Message ?? nullstr);
                message.Append("\"");
                message.Append(newline);
                message.Append(' ', indentSize);
                message.Append("source=");
                message.Append(ex.Source ?? nullstr);
                message.Append(newline);
                message.Append(' ', indentSize);
                message.Append("stacktrace:");
                message.Append(newline);
                message.Append(ex.StackTrace ?? nullstr);
                message.Append(newline);
            }
            while (ex != null && ex.InnerException != null)
            {
                ex = ex.InnerException.Demystify();
                message.Append("Inner Exception:");
                message.Append(newline);
                message.Append(' ', indentSize);
                message.Append("name=");
                message.Append((ex.GetType() != null) ? ex.GetType().Name : nullstr);
                message.Append(newline);
                message.Append(' ', indentSize);
                message.Append("target=");
                message.Append((ex.TargetSite != null) ? ex.TargetSite.Name : nullstr);
                message.Append(newline);
                message.Append(' ', indentSize);
                message.Append("message=");
                message.Append(ex.Message ?? nullstr);
                message.Append(newline);
                message.Append(' ', indentSize);
                message.Append("source=");
                message.Append(ex.Source ?? nullstr);
                message.Append(newline);
                message.Append(' ', indentSize);
                message.Append("stacktrace:");
                message.Append(newline);
                message.Append(ex.StackTrace ?? nullstr);
                message.Append(newline);
            }
            return message.ToString();
        }
    }
}
