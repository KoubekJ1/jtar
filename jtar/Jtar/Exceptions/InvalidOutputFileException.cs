namespace Jtar.Exceptions;

public class InvalidOutputFileException : Exception
{
    public InvalidOutputFileException()
    {

    }

    public InvalidOutputFileException(string message)
        : base(message)
    {
    }

    public InvalidOutputFileException(string message, Exception inner)
        : base(message, inner)
    {
    }
}