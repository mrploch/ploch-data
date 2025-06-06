using System;

namespace Ploch.Data.GenericRepository.Exceptions;

/// <summary>
///     Represents errors that occur due to concurrency conflicts during data update operations in the generic repository.
/// </summary>
/// <remarks>
///     This exception is typically thrown when a concurrency conflict is detected while attempting to update data
///     in the underlying data source, such as a database. Concurrency conflicts often occur when multiple processes
///     or users attempt to modify the same data simultaneously.
/// </remarks>
public class DataUpdateConcurrencyException : DataUpdateException
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="DataUpdateConcurrencyException" /> class with a specified error
    ///     message.
    /// </summary>
    /// <param name="message">
    ///     The message that describes the error.
    /// </param>
    /// <remarks>
    ///     This constructor is used to create an exception instance when a concurrency conflict occurs during
    ///     a data update operation, providing a descriptive error message.
    /// </remarks>
    public DataUpdateConcurrencyException(string? message) : base(message)
    { }

    /// <summary>
    ///     Initializes a new instance of the <see cref="DataUpdateConcurrencyException" /> class with a specified error
    ///     message
    ///     and a reference to the inner exception that is the cause of this exception.
    /// </summary>
    /// <param name="message">
    ///     The message that describes the error.
    /// </param>
    /// <param name="innerException">
    ///     The exception that is the cause of the current exception, or a null reference if no inner exception is specified.
    /// </param>
    /// <remarks>
    ///     This constructor is used to create an exception instance when a concurrency conflict occurs during
    ///     a data update operation, providing a descriptive error message and additional context about the cause of the error.
    /// </remarks>
    public DataUpdateConcurrencyException(string? message, Exception? innerException) : base(message, innerException)
    { }
}
