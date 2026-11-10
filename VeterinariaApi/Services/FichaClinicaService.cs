using Microsoft.EntityFrameworkCore;
using VeterinariaApi.Data;
using VeterinariaApi.DTOs;

namespace VeterinariaApi.Services
{
    public interface IFichaClinicaService
    {
        /// <summary>Arma la ficha clínica completa de una mascota (o null si no existe).</summary>
        Task<FichaClinicaDto?> ObtenerAsync(int mascotaId);
    }

    /// <summary>
    /// La ficha la usan el veterinario (web) y el dueño (app móvil), por eso está en un servicio.
    /// </summary>
    public class FichaClinicaService : IFichaClinicaService
    {
        private readonly DataContext _context;

        public FichaClinicaService(DataContext context)
        {
            _context = context;
        }

        public async Task<FichaClinicaDto?> ObtenerAsync(int mascotaId)
        {
            var mascota = await _context.Mascotas
                .AsNoTracking()
                .Include(m => m.Dueno)
                .Include(m => m.Alergias)
                .Include(m => m.Vacunas)
                .Include(m => m.Cirugias)
                .Include(m => m.Atenciones).ThenInclude(a => a.Veterinario).ThenInclude(v => v.Usuario)
                .Include(m => m.Atenciones).ThenInclude(a => a.Recetas).ThenInclude(r => r.Medicamento)
                .AsSplitQuery()
                .FirstOrDefaultAsync(m => m.Id == mascotaId);

            if (mascota == null) return null;

            return new FichaClinicaDto
            {
                Mascota = new MascotaResponseDto
                {
                    Id = mascota.Id,
                    Nombre = mascota.Nombre,
                    Especie = mascota.Especie,
                    Raza = mascota.Raza,
                    Sexo = mascota.Sexo,
                    FechaNacimiento = mascota.FechaNacimiento,
                    Peso = mascota.Peso,
                    Activo = mascota.Activo,
                    DuenoId = mascota.DuenoId,
                    DuenoNombre = mascota.Dueno.Apellido + ", " + mascota.Dueno.Nombre,
                    DuenoDni = mascota.Dueno.Dni,
                    DuenoTelefono = mascota.Dueno.Telefono
                },
                Alergias = mascota.Alergias
                    .OrderBy(a => a.Descripcion)
                    .Select(a => new AlergiaResponseDto { Id = a.Id, Descripcion = a.Descripcion, FechaRegistro = a.FechaRegistro })
                    .ToList(),
                Vacunas = mascota.Vacunas
                    .OrderByDescending(v => v.FechaAplicacion)
                    .Select(v => new VacunaResponseDto
                    {
                        Id = v.Id,
                        Nombre = v.Nombre,
                        FechaAplicacion = v.FechaAplicacion,
                        ProximaDosis = v.ProximaDosis
                    })
                    .ToList(),
                Cirugias = mascota.Cirugias
                    .OrderByDescending(c => c.Fecha)
                    .Select(c => new CirugiaResponseDto { Id = c.Id, Tipo = c.Tipo, Fecha = c.Fecha, Observaciones = c.Observaciones })
                    .ToList(),
                Atenciones = mascota.Atenciones
                    .OrderByDescending(a => a.Fecha)
                    .Select(a => new AtencionResponseDto
                    {
                        Id = a.Id,
                        Fecha = a.Fecha,
                        Motivo = a.Motivo,
                        Diagnostico = a.Diagnostico,
                        Tratamiento = a.Tratamiento,
                        Observaciones = a.Observaciones,
                        Peso = a.Peso,
                        VeterinarioNombre = a.Veterinario.Usuario.Nombre,
                        Recetas = a.Recetas.Select(r => new RecetaResponseDto
                        {
                            Id = r.Id,
                            Fecha = a.Fecha,
                            MascotaNombre = mascota.Nombre,
                            MedicamentoId = r.MedicamentoId,
                            MedicamentoNombre = r.Medicamento.Nombre,
                            Presentacion = r.Medicamento.Presentacion,
                            Cantidad = r.Cantidad,
                            Dosis = r.Dosis,
                            Indicaciones = r.Indicaciones,
                            VeterinarioNombre = a.Veterinario.Usuario.Nombre
                        }).ToList()
                    })
                    .ToList()
            };
        }
    }
}
