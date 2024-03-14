using System;

namespace Metica.Unity
{
    public delegate void MeticaSdkDelegate<T>(ISdkResult<T> result);

    /// <summary>
    /// Represents the result of an SDK operation.
    /// </summary>
    public interface ISdkResult<T>
    {
        /// <summary>
        /// The result of the operation, if any.
        /// </summary>
        T Result { get; }
        
        /// <summary>
        /// Contains an error code, if the operation failed. Will be null if the operation was successful.
        /// </summary>
        /// <value>The error string from the result. If no error occured value is null or empty.</value>
        string Error { get; }
    }
    
    public class SdkResultImpl<T> : ISdkResult<T>
    {
        public T Result { get; internal set; }

        public string Error { get; internal set; }

        public static ISdkResult<T> WithResult(T result)
        {
            return new SdkResultImpl<T>()
            {
                Result = result
            };
        }
        
        public static ISdkResult<T> WithError(string error)  
        {
            return new SdkResultImpl<T>()
            {
                Error = error,
            };
        }

    }

}