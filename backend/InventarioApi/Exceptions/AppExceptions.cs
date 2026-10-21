namespace InventarioApi.Exceptions;

// Excepciones propias: el middleware las convierte en el código HTTP correcto.

/// <summary>El recurso no existe (404).</summary>
public class NotFoundException : Exception
{
    public NotFoundException(string mensaje) : base(mensaje) { }
}

/// <summary>Dato inválido o regla de negocio incumplida (400).</summary>
public class BusinessException : Exception
{
    public BusinessException(string mensaje) : base(mensaje) { }
}

/// <summary>Conflicto con datos existentes, por ejemplo un código repetido (409).</summary>
public class ConflictException : Exception
{
    public ConflictException(string mensaje) : base(mensaje) { }
}

/// <summary>Credenciales incorrectas (401).</summary>
public class UnauthorizedAppException : Exception
{
    public UnauthorizedAppException(string mensaje) : base(mensaje) { }
}
