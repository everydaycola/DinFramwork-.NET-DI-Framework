namespace DinClassLibrary;

public class DinAmbiguousConstructorException(Type serviceType) : Exception(
    $"Ambiguous constructor selection for: {serviceType.FullName}. Multiple non-default constructors found."
    );
