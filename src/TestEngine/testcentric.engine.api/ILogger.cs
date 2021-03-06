// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric Engine contributors.
// Licensed under the MIT License. See LICENSE.txt in root directory.
// ***********************************************************************

namespace TestCentric.Engine
{
    /// <summary>
    /// Interface for logging within the engine
    /// </summary>
    public interface ILogger
    {
        /// <summary>
        /// Logs the specified message at the error level.
        /// </summary>
        /// <param name="message">The message.</param>
        void Error(string message);

        /// <summary>
        /// Logs the specified message at the error level.
        /// </summary>
        /// <param name="format">The message.</param>
        /// <param name="args">The arguments.</param>
        void Error(string format, params object[] args);

        /// <summary>
        /// Logs the specified message at the warning level.
        /// </summary>
        /// <param name="message">The message.</param>
        void Warning(string message);

        /// <summary>
        /// Logs the specified message at the warning level.
        /// </summary>
        /// <param name="format">The message.</param>
        /// <param name="args">The arguments.</param>
        void Warning(string format, params object[] args);

        /// <summary>
        /// Logs the specified message at the info level.
        /// </summary>
        /// <param name="message">The message.</param>
        void Info(string message);

        /// <summary>
        /// Logs the specified message at the info level.
        /// </summary>
        /// <param name="format">The message.</param>
        /// <param name="args">The arguments.</param>
        void Info(string format, params object[] args);

        /// <summary>
        /// Logs the specified message at the debug level.
        /// </summary>
        /// <param name="message">The message.</param>
        void Debug(string message);

        /// <summary>
        /// Logs the specified message at the debug level.
        /// </summary>
        /// <param name="format">The message.</param>
        /// <param name="args">The arguments.</param>
        void Debug(string format, params object[] args);
    }
}
