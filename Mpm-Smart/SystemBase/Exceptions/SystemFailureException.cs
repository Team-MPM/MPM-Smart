namespace SystemBase.Exceptions;

public class SystemFailureException(string message = null!, Exception innerException = null!) : Exception(message, innerException);