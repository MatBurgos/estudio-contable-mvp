using EstudioContable.Application.DTOs.Vencimientos;
using EstudioContable.Application.DTOs.Tareas;

namespace EstudioContable.Application.DTOs.Dashboard;

public record DashboardResumenDto(
    int TotalClientesActivos,
    int VencimientosMesTotal,
    int VencimientosPresentados,
    int VencimientosPendientes,
    int VencimientosVencidos,
    int TareasPendientes,
    List<VencimientoDto> ProximosVencimientos, // Próximos 7/15 días
    List<VencimientoDto> VencimientosUrgentes,   // Vencidos o por vencer hoy/mañana
    List<TareaDto> MisTareasPendientes
);
