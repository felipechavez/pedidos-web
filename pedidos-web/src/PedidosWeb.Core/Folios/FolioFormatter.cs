using System.Text.RegularExpressions;

namespace PedidosWeb.Core.Folios;

/// <summary>
/// Genera el folio de un pedido con el mismo formato del sistema legado.
/// Las reglas completas y los ejemplos están en el README, sección «La función del taller».
/// </summary>
public static class FolioFormatter
{
    /// <summary>
    /// Devuelve el folio con el formato PED-{SUCURSAL}-{NUMERO}, por ejemplo "PED-STGO-000123".
    /// </summary>
    /// <param name="numero">Número correlativo del pedido, entre 1 y 999999.</param>
    /// <param name="sucursal">Código de la sucursal: de 2 a 4 letras de la A a la Z.</param>
    public static string FormatearFolio(int numero, string sucursal)
    {
        // R5: validar numero antes que sucursal
        // R1: rango 1–999999
        if (numero < 1 || numero > 999_999)
            throw new ArgumentOutOfRangeException(nameof(numero));

        // R2: trim y mayúsculas
        // R3: null/vacía, o fuera de 2–4 letras A–Z
        if (string.IsNullOrWhiteSpace(sucursal))
            throw new ArgumentException("Código de sucursal requerido.", nameof(sucursal));

        sucursal = sucursal.Trim().ToUpperInvariant();

        if (!Regex.IsMatch(sucursal, @"^[A-Z]{2,4}$"))
            throw new ArgumentException("Sucursal debe tener entre 2 y 4 letras A–Z.", nameof(sucursal));

        // R4: padding a 6 dígitos
        return $"PED-{sucursal}-{numero:D6}";
    }
}

