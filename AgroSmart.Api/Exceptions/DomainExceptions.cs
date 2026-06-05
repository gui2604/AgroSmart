namespace AgroSmart.Api.Exceptions;

/// <summary>Base type for expected, business-level errors mapped to HTTP responses.</summary>
public abstract class DomainException : Exception
{
    protected DomainException(string message) : base(message) { }
}

/// <summary>A requested entity does not exist (mapped to HTTP 404).</summary>
public class NotFoundException : DomainException
{
    public NotFoundException(string message) : base(message) { }

    public static NotFoundException For(string entity, object id) =>
        new($"{entity} com identificador '{id}' não foi encontrado.");
}

/// <summary>A request violated a business rule or uniqueness constraint (mapped to HTTP 400/409).</summary>
public class ValidationException : DomainException
{
    public ValidationException(string message) : base(message) { }
}

/// <summary>The current request is not authenticated (mapped to HTTP 401).</summary>
public class UserNotAuthenticatedException : DomainException
{
    public UserNotAuthenticatedException()
        : base("Usuário não autenticado ou token inválido.") { }
}

/// <summary>The underlying database could not be reached (mapped to HTTP 503).</summary>
public class DatabaseUnavailableException : DomainException
{
    public DatabaseUnavailableException(string message) : base(message) { }
}
