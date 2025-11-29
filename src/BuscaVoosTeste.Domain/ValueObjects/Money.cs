namespace BuscaVoosTeste.Domain.ValueObjects;

/// <summary>
/// Value Object que representa um valor monetário com quantia e moeda.
/// Utilizado para representar preços, tarifas e totais em ofertas de voo.
/// </summary>
/// <remarks>
/// Este Value Object é imutável e garante que:
/// - O valor não pode ser negativo;
/// - A moeda deve ser informada (não nula ou vazia).
/// A igualdade é baseada em quantia e moeda.
/// </remarks>
public readonly struct Money : IEquatable<Money>
{
    /// <summary>
    /// Quantia numérica do valor monetário.
    /// </summary>
    public decimal Valor { get; }

    /// <summary>
    /// Código da moeda (ex.: BRL, USD, EUR).
    /// </summary>
    public string Moeda { get; }

    /// <summary>
    /// Cria uma nova instância de <see cref="Money"/>.
    /// </summary>
    /// <param name="valor">Quantia numérica. Deve ser maior ou igual a zero.</param>
    /// <param name="moeda">Código da moeda. Não pode ser nulo ou vazio.</param>
    /// <exception cref="ArgumentException">
    /// Lançada quando o valor é negativo ou a moeda é nula/vazia.
    /// </exception>
    public Money(decimal valor, string moeda)
    {
        if (valor < 0)
        {
            throw new ArgumentException("O valor não pode ser negativo.", nameof(valor));
        }

        if (string.IsNullOrWhiteSpace(moeda))
        {
            throw new ArgumentException("A moeda deve ser informada.", nameof(moeda));
        }

        Valor = valor;
        Moeda = moeda;
    }

    /// <summary>
    /// Verifica se esta instância é igual a outra instância de <see cref="Money"/>.
    /// Duas instâncias são iguais quando possuem a mesma quantia e moeda.
    /// </summary>
    /// <param name="other">Instância a ser comparada.</param>
    /// <returns>True se as instâncias são iguais; caso contrário, false.</returns>
    public bool Equals(Money other)
    {
        return Valor == other.Valor && Moeda == other.Moeda;
    }

    /// <summary>
    /// Verifica se esta instância é igual a outro objeto.
    /// </summary>
    /// <param name="obj">Objeto a ser comparado.</param>
    /// <returns>True se o objeto é uma instância de <see cref="Money"/> igual; caso contrário, false.</returns>
    public override bool Equals(object? obj)
    {
        return obj is Money other && Equals(other);
    }

    /// <summary>
    /// Retorna o código hash desta instância.
    /// </summary>
    /// <returns>Código hash baseado em quantia e moeda.</returns>
    public override int GetHashCode()
    {
        return HashCode.Combine(Valor, Moeda);
    }

    /// <summary>
    /// Retorna uma representação em string deste valor monetário.
    /// </summary>
    /// <returns>String no formato "Valor Moeda" (ex.: "100.00 BRL").</returns>
    public override string ToString()
    {
        return $"{Valor} {Moeda}";
    }

    /// <summary>
    /// Operador de igualdade entre duas instâncias de <see cref="Money"/>.
    /// </summary>
    public static bool operator ==(Money left, Money right)
    {
        return left.Equals(right);
    }

    /// <summary>
    /// Operador de desigualdade entre duas instâncias de <see cref="Money"/>.
    /// </summary>
    public static bool operator !=(Money left, Money right)
    {
        return !left.Equals(right);
    }
}
