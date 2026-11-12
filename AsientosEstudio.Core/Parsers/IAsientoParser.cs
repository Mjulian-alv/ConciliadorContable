// Fecha: 12/11/2026 - TAREA: 00180 - Linea: 1,2,3 - Una estrategia por tipo de planilla: sumar un tipo nuevo es una clase nueva, sin tocar las otras.
namespace AsientosEstudio.Core;

public interface IAsientoParser
{
    TipoPlanilla Tipo { get; }

    /// <summary>
    /// Arma las hojas con sus asientos. No valida ni descarta: devuelve todo lo que encontró, con el estado de cada asiento y la fila
    /// de origen de cada línea, y deja a los validadores decidir qué se alerta.
    /// </summary>
    IReadOnlyList<HojaLeida> Leer(LibroCrudo libro);
}
