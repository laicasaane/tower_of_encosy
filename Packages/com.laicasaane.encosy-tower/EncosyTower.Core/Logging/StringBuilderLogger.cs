namespace EncosyTower.Logging
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.CompilerServices;
    using System.Text;

    /// <summary>
    /// A logger that writes log messages to a <see cref="StringBuilder"/>.
    /// </summary>
    public partial class StringBuilderLogger : ILogger
    {
        private readonly StringBuilder _builder;

        public StringBuilderLogger() : this(new StringBuilder())
        {
        }

        public StringBuilderLogger([NotNull] StringBuilder builder)
        {
            _builder = builder;
        }

        public event Action OnLogEntryWritten;

        /// <summary>
        /// Gets the number of log entries written to the <see cref="StringBuilder"/>.
        /// </summary>
        public int LogEntryCount { get; private set; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override string ToString()
        {
            return _builder.ToString();
        }

        public void Clear()
        {
            LogEntryCount = 0;
            _builder.Clear();
        }

        /// <summary>
        /// Logs a message followed by a newline without any prefix.
        /// </summary>
        /// <param name="message">The message to log.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void LogLine(object message)
        {
            _builder.Append(message).AppendLine();
            InvokeOnLogEntryWritten();
        }

        /// <summary>
        /// Logs a formatted message followed by a newline without any prefix.
        /// </summary>
        /// <param name="format">The format string.</param>
        /// <param name="args">The arguments to format.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void LogLineFormat(string format, params object[] args)
        {
            _builder.AppendFormat(format, args).AppendLine();
            InvokeOnLogEntryWritten();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void LogError(object message)
        {
            _builder.Append("[Error] ").Append(message).AppendLine();
            InvokeOnLogEntryWritten();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void LogErrorFormat(string format, params object[] args)
        {
            _builder.Append("[Error] ").AppendFormat(format, args).AppendLine();
            InvokeOnLogEntryWritten();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void LogException(Exception value)
        {
            _builder.Append("[Error] ").Append(value).AppendLine();
            InvokeOnLogEntryWritten();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void LogInfo(object message)
        {
            _builder.Append("[Info] ").Append(message).AppendLine();
            InvokeOnLogEntryWritten();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void LogInfoFormat(string format, params object[] args)
        {
            _builder.Append("[Info] ").AppendFormat(format, args).AppendLine();
            InvokeOnLogEntryWritten();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void LogWarning(object message)
        {
            _builder.Append("[Warning] ").Append(message).AppendLine();
            InvokeOnLogEntryWritten();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void LogWarningFormat(string format, params object[] args)
        {
            _builder.Append("[Warning] ").AppendFormat(format, args).AppendLine();
            InvokeOnLogEntryWritten();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void InvokeOnLogEntryWritten()
        {
            LogEntryCount++;
            OnLogEntryWritten?.Invoke();
        }
    }
}

#if UNITY_COLLECTIONS

namespace EncosyTower.Logging
{
    using System.Runtime.CompilerServices;
    using EncosyTower.Common;
    using Unity.Collections;

    partial class StringBuilderLogger
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void LogFixedError<TFixedString>(in TFixedString message)
            where TFixedString : unmanaged, INativeList<byte>, IUTF8Bytes
        {
            _builder.Append("[Error] ").Append<TFixedString>(message).AppendLine();
            InvokeOnLogEntryWritten();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void LogFixedInfo<TFixedString>(in TFixedString message)
            where TFixedString : unmanaged, INativeList<byte>, IUTF8Bytes
        {
            _builder.Append("[Info] ").Append<TFixedString>(message).AppendLine();
            InvokeOnLogEntryWritten();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void LogFixedWarning<TFixedString>(in TFixedString message)
            where TFixedString : unmanaged, INativeList<byte>, IUTF8Bytes
        {
            _builder.Append("[Warning] ").Append<TFixedString>(message).AppendLine();
            InvokeOnLogEntryWritten();
        }
    }
}

#endif
