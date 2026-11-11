using PedidosWeb.Core.Clientes;

namespace PedidosWeb.Core.Tests.Clientes;

public class ClientesServiceTests
{
    private readonly ClientesService _servicio = new();

    [Fact]
    public void ObtenerFicha_ClienteInexistente_DevuelveTextoVacio()
    {
        var ficha = _servicio.ObtenerFicha("C-999");

        Assert.Equal("", ficha);
    }

    [Fact]
    public void ObtenerFicha_ClienteActivo_DevuelveLaFicha()
    {
        var ficha = _servicio.ObtenerFicha("C-001");

        Assert.Equal("Ferretería El Clavo | 76123456-0 | Zona centro | Activo", ficha);
    }

    [Fact]
    public void ObtenerFicha_RutConPuntosYKMinuscula_LoNormaliza()
    {
        var ficha = _servicio.ObtenerFicha("C-004");

        Assert.Equal("Almacén La Esquina | 76990011-K | Zona centro | Activo", ficha);
    }

    [Fact]
    public void EstaActivo_ClienteActivoInactivoEInexistente_DevuelveSoloTrueSiExisteYEstaActivo()
    {
        Assert.True(_servicio.EstaActivo("C-001"));
        Assert.False(_servicio.EstaActivo("C-007"));
        Assert.False(_servicio.EstaActivo("C-999"));
    }
}
