using System.Text.RegularExpressions;

namespace BuscaVoosTeste.Domain.ValueObjects;

/// <summary>
/// Value Object que representa uma duração de tempo (intervalo de tempo).
/// Utilizado para representar duração de voos, trechos e conexões.
/// </summary>
/// <remarks>
/// Este Value Object é imutável e garante que:
/// - A duração não pode ser negativa;
/// - Zero é um valor válido.
/// A igualdade é baseada no valor interno de TimeSpan.
/// </remarks>
public readonly struct Duration : IEquatable<Duration>, IComparable<Duration>
{
    /// <summary>
    /// Valor interno que representa a duração.
    /// </summary>
    public TimeSpan Valor { get; }

    /// <summary>
    /// Cria uma nova instância de <see cref="Duration"/> a partir de um TimeSpan.
    /// </summary>
    /// <param name="valor">Valor de TimeSpan. Não pode ser negativo.</param>
    /// <exception cref="ArgumentException">
    /// Lançada quando o valor é negativo.
    /// </exception>
    private Duration(TimeSpan valor)
    {
        if (valor < TimeSpan.Zero)
        {
            throw new ArgumentException("A duração não pode ser negativa.", nameof(valor));
        }

        Valor = valor;
    }

    /// <summary>
    /// Cria uma nova instância de <see cref="Duration"/> a partir do total de minutos.
    /// </summary>
    /// <param name="totalDeMinutos">Total de minutos. Deve ser maior ou igual a zero.</param>
    /// <returns>Uma nova instância de <see cref="Duration"/>.</returns>
    /// <exception cref="ArgumentException">
    /// Lançada quando o total de minutos é negativo.
    /// </exception>
    public static Duration CriarAPartirDeMinutos(int totalDeMinutos)
    {
        if (totalDeMinutos < 0)
        {
            throw new ArgumentException("O total de minutos não pode ser negativo.", nameof(totalDeMinutos));
        }

        return new Duration(TimeSpan.FromMinutes(totalDeMinutos));
    }

    /// <summary>
    /// Cria uma nova instância de <see cref="Duration"/> a partir de horas e minutos.
    /// </summary>
    /// <param name="horas">Quantidade de horas. Deve ser maior ou igual a zero.</param>
    /// <param name="minutos">Quantidade de minutos. Deve ser maior ou igual a zero.</param>
    /// <returns>Uma nova instância de <see cref="Duration"/>.</returns>
    /// <exception cref="ArgumentException">
    /// Lançada quando horas ou minutos são negativos.
    /// </exception>
    public static Duration CriarAPartirDeHorasEMinutos(int horas, int minutos)
    {
        if (horas < 0)
        {
            throw new ArgumentException("A quantidade de horas não pode ser negativa.", nameof(horas));
        }

        if (minutos < 0)
        {
            throw new ArgumentException("A quantidade de minutos não pode ser negativa.", nameof(minutos));
        }

        return new Duration(new TimeSpan(horas, minutos, 0));
    }

    /// <summary>
    /// Cria uma nova instância de <see cref="Duration"/> a partir de uma string no formato ISO 8601.
    /// O formato esperado é PT[n]H[n]M (ex.: PT2H30M para 2 horas e 30 minutos).
    /// </summary>
    /// <param name="valorIso8601">String no formato ISO 8601 de duração (ex.: PT2H30M, PT1H, PT45M).</param>
    /// <returns>Uma nova instância de <see cref="Duration"/>.</returns>
    /// <exception cref="ArgumentException">
    /// Lançada quando o valor é nulo, vazio ou não está no formato ISO 8601 válido.
    /// </exception>
    /// <remarks>
    /// Este método suporta os formatos:
    /// - PT[n]H[n]M (ex.: PT2H30M)
    /// - PT[n]H (ex.: PT2H)
    /// - PT[n]M (ex.: PT30M)
    /// A API da Duffel utiliza este formato para representar durações de voos e segmentos.
    /// </remarks>
    public static Duration CriarAPartirDeIso8601(string valorIso8601)
    {
        if (string.IsNullOrWhiteSpace(valorIso8601))
        {
            throw new ArgumentException("O valor ISO 8601 não pode ser nulo ou vazio.", nameof(valorIso8601));
        }

        // Regex para capturar formato ISO 8601 de duração: PT[n]H[n]M
        var regex = new Regex(@"^PT(?:(\d+)H)?(?:(\d+)M)?$", RegexOptions.IgnoreCase);
        var match = regex.Match(valorIso8601.Trim());

        if (!match.Success)
        {
            throw new ArgumentException($"O valor '{valorIso8601}' não está no formato ISO 8601 válido (ex.: PT2H30M).", nameof(valorIso8601));
        }

        var horas = 0;
        var minutos = 0;

        if (match.Groups[1].Success)
        {
            horas = int.Parse(match.Groups[1].Value);
        }

        if (match.Groups[2].Success)
        {
            minutos = int.Parse(match.Groups[2].Value);
        }

        // Validar que pelo menos um valor foi capturado
        if (!match.Groups[1].Success && !match.Groups[2].Success)
        {
            throw new ArgumentException($"O valor '{valorIso8601}' não contém horas nem minutos válidos.", nameof(valorIso8601));
        }

        return new Duration(new TimeSpan(horas, minutos, 0));
    }

    /// <summary>
    /// Obtém o total de minutos desta duração.
    /// </summary>
    /// <returns>Total de minutos como um inteiro.</returns>
    public int ObterMinutosTotais()
    {
        return (int)Valor.TotalMinutes;
    }

    /// <summary>
    /// Obtém a representação desta duração no formato ISO 8601.
    /// </summary>
    /// <returns>String no formato ISO 8601 (ex.: PT2H30M).</returns>
    public string ObterIso8601()
    {
        var horas = (int)Valor.TotalHours;
        var minutos = Valor.Minutes;

        if (horas > 0 && minutos > 0)
        {
            return $"PT{horas}H{minutos}M";
        }
        else if (horas > 0)
        {
            return $"PT{horas}H";
        }
        else
        {
            return $"PT{minutos}M";
        }
    }

    /// <summary>
    /// Soma esta duração com outra duração.
    /// </summary>
    /// <param name="outra">Outra duração a ser somada.</param>
    /// <returns>Uma nova instância de <see cref="Duration"/> com a soma das durações.</returns>
    public Duration Somar(Duration outra)
    {
        return new Duration(Valor + outra.Valor);
    }

    /// <summary>
    /// Verifica se esta instância é igual a outra instância de <see cref="Duration"/>.
    /// Duas instâncias são iguais quando possuem o mesmo valor de duração.
    /// </summary>
    /// <param name="other">Instância a ser comparada.</param>
    /// <returns>True se as instâncias são iguais; caso contrário, false.</returns>
    public bool Equals(Duration other)
    {
        return Valor == other.Valor;
    }

    /// <summary>
    /// Verifica se esta instância é igual a outro objeto.
    /// </summary>
    /// <param name="obj">Objeto a ser comparado.</param>
    /// <returns>True se o objeto é uma instância de <see cref="Duration"/> igual; caso contrário, false.</returns>
    public override bool Equals(object? obj)
    {
        return obj is Duration other && Equals(other);
    }

    /// <summary>
    /// Retorna o código hash desta instância.
    /// </summary>
    /// <returns>Código hash baseado no valor da duração.</returns>
    public override int GetHashCode()
    {
        return Valor.GetHashCode();
    }

    /// <summary>
    /// Compara esta instância com outra instância de <see cref="Duration"/>.
    /// </summary>
    /// <param name="other">Instância a ser comparada.</param>
    /// <returns>
    /// Um valor negativo se esta instância é menor que a outra;
    /// zero se são iguais;
    /// um valor positivo se esta instância é maior que a outra.
    /// </returns>
    public int CompareTo(Duration other)
    {
        return Valor.CompareTo(other.Valor);
    }

    /// <summary>
    /// Retorna uma representação em string amigável desta duração.
    /// </summary>
    /// <returns>String no formato "Xh Ym" (ex.: "2h 30m").</returns>
    public override string ToString()
    {
        var horas = (int)Valor.TotalHours;
        var minutos = Valor.Minutes;

        if (horas > 0 && minutos > 0)
        {
            return $"{horas}h {minutos}m";
        }
        else if (horas > 0)
        {
            return $"{horas}h";
        }
        else
        {
            return $"{minutos}m";
        }
    }

    /// <summary>
    /// Operador de igualdade entre duas instâncias de <see cref="Duration"/>.
    /// </summary>
    public static bool operator ==(Duration left, Duration right)
    {
        return left.Equals(right);
    }

    /// <summary>
    /// Operador de desigualdade entre duas instâncias de <see cref="Duration"/>.
    /// </summary>
    public static bool operator !=(Duration left, Duration right)
    {
        return !left.Equals(right);
    }

    /// <summary>
    /// Operador de comparação menor que entre duas instâncias de <see cref="Duration"/>.
    /// </summary>
    public static bool operator <(Duration left, Duration right)
    {
        return left.CompareTo(right) < 0;
    }

    /// <summary>
    /// Operador de comparação menor ou igual que entre duas instâncias de <see cref="Duration"/>.
    /// </summary>
    public static bool operator <=(Duration left, Duration right)
    {
        return left.CompareTo(right) <= 0;
    }

    /// <summary>
    /// Operador de comparação maior que entre duas instâncias de <see cref="Duration"/>.
    /// </summary>
    public static bool operator >(Duration left, Duration right)
    {
        return left.CompareTo(right) > 0;
    }

    /// <summary>
    /// Operador de comparação maior ou igual que entre duas instâncias de <see cref="Duration"/>.
    /// </summary>
    public static bool operator >=(Duration left, Duration right)
    {
        return left.CompareTo(right) >= 0;
    }
}
