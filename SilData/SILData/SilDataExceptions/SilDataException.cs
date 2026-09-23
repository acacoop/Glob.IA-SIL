namespace SILData.SilDataExceptions
{
  public class SilDataException : Exception
  {
    public int StatusCode { get; }
    public bool Success { get; } = false; // Siempre false porque es una excepción

    public SilDataException(string message, int statusCode = 409) // El error es un BadRequest
        : base(message)
    {
      StatusCode = statusCode;
    }

    public SilDataException(string message, Exception innerException, int statusCode = 409)
        : base(message, innerException)
    {
      StatusCode = statusCode;
    }
  }
}
