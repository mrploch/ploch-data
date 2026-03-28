using System;

namespace Ploch.Data.GenericRepository.Exceptions;

/// <summary>
///     Represents errors that occur during data update operations in the generic repository.
/// </summary>
/// <remarks>
///     This exception is typically thrown when an issue arises while attempting to update data
///     in the underlying data source, such as a database or other storage mechanism.
/// </remarks>
public class DataUpdateException : DataAccessException
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="DataUpdateException" /> class with a specified error message.
    /// </summary>
    /// <param name="message">
    ///     The message that describes the error.
    /// </param>
    public DataUpdateException(string? message) : base(message)
    { }

    /// <summary>
    ///     Initializes a new instance of the <see cref="DataUpdateException" /> class with a specified error message
    ///     and a reference to the inner exception that is the cause of this exception.
    /// </summary>
    /// <param name="message">
    ///     The message that describes the error.
    /// </param>
    /// <param name="innerException">
    ///     The exception that is the cause of the current exception, or a null reference if no inner exception is specified.
    /// </param>
    public DataUpdateException(string? message, Exception? innerException) : base(message, innerException)
    { }
}
