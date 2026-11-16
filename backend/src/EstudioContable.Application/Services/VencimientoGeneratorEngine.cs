using EstudioContable.Domain.Entities;
using EstudioContable.Domain.Enums;

namespace EstudioContable.Application.Services;

/// <summary>
/// Motor de cálculo de vencimientos impositivos en Argentina.
/// Aplica reglas de terminación de CUIT, traslados a días hábiles
/// y generación de listas de tareas predeterminadas.
/// </summary>
public class VencimientoGeneratorEngine
{
    /// <summary>
    /// Calcula la fecha de vencimiento precisa según impuesto, terminación de CUIT y período.
    /// </summary>
    public DateOnly CalcularFechaVencimiento(Cliente cliente, ClienteObligacion clienteObligacion, int anioPeriodo, int mesPeriodo)
    {
        // 1. Si el cliente tiene un día personalizado configurado para esta obligación, se prioriza
        if (clienteObligacion.DiaVencimientoPersonalizado.HasValue)
        {
            var (mesVenc, anioVenc) = ObtenerMesVencimiento(clienteObligacion.ObligacionId, anioPeriodo, mesPeriodo, cliente.CierreEjercicioMes);
            var dia = Math.Clamp(clienteObligacion.DiaVencimientoPersonalizado.Value, 1, DateTime.DaysInMonth(anioVenc, mesVenc));
            return AjustarDiaHabil(new DateOnly(anioVenc, mesVenc, dia));
        }

        int ultimoDigito = cliente.UltimoDigitoCuit;
        string obligacionId = clienteObligacion.ObligacionId.ToUpperInvariant();

        DateOnly fechaBase;

        switch (obligacionId)
        {
            case "IVA":
            case "IVA_F2002":
                // IVA vence el mes siguiente (M+1) entre los días 18 y 22 según terminación de CUIT
                fechaBase = CalcularVencimientoIva(anioPeriodo, mesPeriodo, ultimoDigito);
                break;

            case "F931":
            case "F931_LSD":
            case "SICOSS":
                // Cargas sociales / Libro de Sueldos Digital vence en M+1 entre los días 9 y 12
                fechaBase = CalcularVencimientoF931(anioPeriodo, mesPeriodo, ultimoDigito);
                break;

            case "MONOTRIBUTO":
                // Monotributo vence el día 20 del mes en curso (o hábil siguiente)
                fechaBase = new DateOnly(anioPeriodo, mesPeriodo, 20);
                break;

            case "IIBB_CM":
            case "CONVENIO_MULTILATERAL":
                // Convenio Multilateral CM03 vence en M+1 entre los días 15 y 19
                fechaBase = CalcularVencimientoConvenioMultilateral(anioPeriodo, mesPeriodo, ultimoDigito);
                break;

            case "IIBB_LOCAL":
            case "IIBB_ARBA":
            case "IIBB_AGIP":
                // Ingresos brutos locales: habitualmente entre los días 16 y 22
                fechaBase = CalcularVencimientoIibbLocal(anioPeriodo, mesPeriodo, ultimoDigito);
                break;

            case "GANANCIAS_PJ":
                // Sociedades: vence en el 5to mes posterior al cierre de ejercicio
                fechaBase = CalcularVencimientoGananciasPj(anioPeriodo, cliente.CierreEjercicioMes ?? 12, ultimoDigito);
                break;

            default:
                // Regla general de contingencia: día 20 del mes subsiguiente
                var mesSiguiente = mesPeriodo == 12 ? 1 : mesPeriodo + 1;
                var anioSiguiente = mesPeriodo == 12 ? anioPeriodo + 1 : anioPeriodo;
                fechaBase = new DateOnly(anioSiguiente, mesSiguiente, 20);
                break;
        }

        return AjustarDiaHabil(fechaBase);
    }

    private static (int Mes, int Anio) ObtenerMesVencimiento(string obligacionId, int anio, int mes, int? cierreEjercicio)
    {
        if (obligacionId.Equals("MONOTRIBUTO", StringComparison.OrdinalIgnoreCase))
            return (mes, anio);

        if (obligacionId.Equals("GANANCIAS_PJ", StringComparison.OrdinalIgnoreCase))
        {
            int mesCierre = cierreEjercicio ?? 12;
            int mesVenc = mesCierre + 5;
            int anioVenc = anio;
            if (mesVenc > 12)
            {
                mesVenc -= 12;
                anioVenc += 1;
            }
            return (mesVenc, anioVenc);
        }

        // Por defecto: mes siguiente
        return mes == 12 ? (1, anio + 1) : (mes + 1, anio);
    }

    private static DateOnly CalcularVencimientoIva(int anio, int mes, int ultimoDigito)
    {
        int anioVenc = mes == 12 ? anio + 1 : anio;
        int mesVenc = mes == 12 ? 1 : mes + 1;

        int dia = ultimoDigito switch
        {
            0 or 1 => 18,
            2 or 3 => 19,
            4 or 5 => 20,
            6 or 7 => 21,
            8 or 9 => 22,
            _ => 20
        };

        return new DateOnly(anioVenc, mesVenc, dia);
    }

    private static DateOnly CalcularVencimientoF931(int anio, int mes, int ultimoDigito)
    {
        int anioVenc = mes == 12 ? anio + 1 : anio;
        int mesVenc = mes == 12 ? 1 : mes + 1;

        int dia = ultimoDigito switch
        {
            0 or 1 or 2 or 3 => 9,
            4 or 5 or 6 => 10,
            7 or 8 or 9 => 11,
            _ => 10
        };

        return new DateOnly(anioVenc, mesVenc, dia);
    }

    private static DateOnly CalcularVencimientoConvenioMultilateral(int anio, int mes, int ultimoDigito)
    {
        int anioVenc = mes == 12 ? anio + 1 : anio;
        int mesVenc = mes == 12 ? 1 : mes + 1;

        int dia = ultimoDigito switch
        {
            0 or 1 or 2 => 15,
            3 or 4 or 5 => 16,
            6 or 7 => 17,
            8 or 9 => 18,
            _ => 15
        };

        return new DateOnly(anioVenc, mesVenc, dia);
    }

    private static DateOnly CalcularVencimientoIibbLocal(int anio, int mes, int ultimoDigito)
    {
        int anioVenc = mes == 12 ? anio + 1 : anio;
        int mesVenc = mes == 12 ? 1 : mes + 1;

        int dia = 17 + (ultimoDigito / 2); // 17, 18, 19, 20, 21
        return new DateOnly(anioVenc, mesVenc, dia);
    }

    private static DateOnly CalcularVencimientoGananciasPj(int anioEjercicio, int mesCierre, int ultimoDigito)
    {
        int mesVenc = mesCierre + 5;
        int anioVenc = anioEjercicio;
        if (mesVenc > 12)
        {
            mesVenc -= 12;
            anioVenc += 1;
        }

        int dia = ultimoDigito switch
        {
            0 or 1 or 2 => 13,
            3 or 4 or 5 => 14,
            6 or 7 => 15,
            8 or 9 => 16,
            _ => 15
        };

        return new DateOnly(anioVenc, mesVenc, dia);
    }

    /// <summary>
    /// Si la fecha cae sábado o domingo, se traslada al primer día hábil siguiente (lunes).
    /// </summary>
    public static DateOnly AjustarDiaHabil(DateOnly fecha)
    {
        if (fecha.DayOfWeek == DayOfWeek.Saturday)
            return fecha.AddDays(2);
        if (fecha.DayOfWeek == DayOfWeek.Sunday)
            return fecha.AddDays(1);

        return fecha;
    }

    /// <summary>
    /// Genera la lista de tareas predeterminadas que componen el checklist operativo del vencimiento.
    /// </summary>
    public List<Tarea> GenerarTareasPorDefecto(Vencimiento vencimiento, Guid? responsableId)
    {
        var tareas = new List<Tarea>();
        var fechaVenc = vencimiento.FechaVencimientoEfectiva;

        // Lista estándar de tareas para estudios contables
        var definiciones = new (string Titulo, int DiasAntes, PrioridadTarea Prioridad)[]
        {
            ("Solicitar extractos bancarios y comprobantes de compra/venta", 10, PrioridadTarea.Media),
            ("Conciliar movimientos y cargar comprobantes en sistema", 6, PrioridadTarea.Media),
            ("Realizar liquidación preliminar y validar con cliente", 3, PrioridadTarea.Alta),
            ("Presentar Declaración Jurada en organismo fiscal", 0, PrioridadTarea.Urgente),
            ("Generar VEP / boleta de pago y enviar al cliente", 0, PrioridadTarea.Urgente)
        };

        for (int i = 0; i < definiciones.Length; i++)
        {
            var def = definiciones[i];
            var fechaLimite = fechaVenc.AddDays(-def.DiasAntes);

            tareas.Add(new Tarea
            {
                EstudioId = vencimiento.EstudioId,
                ClienteId = vencimiento.ClienteId,
                VencimientoId = vencimiento.Id,
                Titulo = def.Titulo,
                AsignadoAUsuarioId = responsableId,
                Estado = EstadoTarea.Pendiente,
                Prioridad = def.Prioridad,
                FechaLimite = AjustarDiaHabil(fechaLimite),
                Orden = i + 1
            });
        }

        return tareas;
    }
}
