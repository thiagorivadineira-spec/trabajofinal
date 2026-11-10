using Microsoft.EntityFrameworkCore;
using VeterinariaApi.Models;

namespace VeterinariaApi.Data
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {
        }

        // Tablas de la base VeterinariaDb
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Especialidad> Especialidades { get; set; }
        public DbSet<Veterinario> Veterinarios { get; set; }
        public DbSet<Dueno> Duenos { get; set; }
        public DbSet<Mascota> Mascotas { get; set; }
        public DbSet<Turno> Turnos { get; set; }
        public DbSet<Atencion> Atenciones { get; set; }
        public DbSet<Vacuna> Vacunas { get; set; }
        public DbSet<Alergia> Alergias { get; set; }
        public DbSet<Cirugia> Cirugias { get; set; }
        public DbSet<Medicamento> Medicamentos { get; set; }
        public DbSet<Receta> Recetas { get; set; }
        public DbSet<Distribuidor> Distribuidores { get; set; }
        public DbSet<Compra> Compras { get; set; }
        public DbSet<DetalleCompra> DetallesCompras { get; set; }
        public DbSet<RegistroAuditoria> RegistrosAuditoria { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ---------- Usuarios ----------
            modelBuilder.Entity<Usuario>()
                .HasIndex(u => u.Username)
                .IsUnique();

            // Usuario 1 <-> 0..1 Veterinario
            modelBuilder.Entity<Veterinario>()
                .HasOne(v => v.Usuario)
                .WithOne(u => u.Veterinario)
                .HasForeignKey<Veterinario>(v => v.UsuarioId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Veterinario>()
                .HasIndex(v => v.Matricula)
                .IsUnique();

            // Usuario 0..1 <-> 0..1 Dueño (la cuenta de la app es opcional)
            modelBuilder.Entity<Dueno>()
                .HasOne(d => d.Usuario)
                .WithOne(u => u.Dueno)
                .HasForeignKey<Dueno>(d => d.UsuarioId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Dueno>()
                .HasIndex(d => d.Dni)
                .IsUnique();

            // ---------- Turnos y atenciones ----------
            modelBuilder.Entity<Turno>()
                .HasIndex(t => new { t.VeterinarioId, t.FechaHora });

            modelBuilder.Entity<Atencion>()
                .HasOne(a => a.Turno)
                .WithMany()
                .HasForeignKey(a => a.TurnoId)
                .OnDelete(DeleteBehavior.Restrict);

            // ---------- Auditoría ----------
            modelBuilder.Entity<RegistroAuditoria>()
                .HasOne(a => a.Usuario)
                .WithMany()
                .HasForeignKey(a => a.UsuarioId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<RegistroAuditoria>()
                .HasIndex(a => a.Fecha);

            // Nunca se borra en cascada: se usa baja lógica (Activo = false) y así
            // el historial clínico y las compras no se pierden. Además evita el error de
            // SQL Server "multiple cascade paths".
            foreach (var relacion in modelBuilder.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()))
            {
                relacion.DeleteBehavior = DeleteBehavior.Restrict;
            }

            // ================= DATOS INICIALES =================
            // Único usuario creado a mano: el administrador. Él da de alta a veterinarios y recepcionistas.
            // La contraseña "admin123" se guarda en texto y Program.cs la convierte a hash al iniciar.
            modelBuilder.Entity<Usuario>().HasData(new
            {
                Id = 1,
                Nombre = "Administrador",
                Username = "admin",
                PasswordHash = "admin123",
                Rol = "Admin",
                Activo = true
            });

            modelBuilder.Entity<Especialidad>().HasData(
                new { Id = 1, Nombre = "Clínica general", Descripcion = "Consultas generales y controles", Activo = true },
                new { Id = 2, Nombre = "Cirugía", Descripcion = "Cirugías programadas y castraciones", Activo = true },
                new { Id = 3, Nombre = "Dermatología", Descripcion = "Piel, pelo y alergias", Activo = true },
                new { Id = 4, Nombre = "Cardiología", Descripcion = "Corazón y sistema circulatorio", Activo = true }
            );

            modelBuilder.Entity<Distribuidor>().HasData(
                new
                {
                    Id = 1,
                    RazonSocial = "Droguería Veterinaria del Sur SA",
                    Cuit = "30-71234567-8",
                    Telefono = "2954-400100",
                    Email = "ventas@drogueriadelsur.com",
                    Direccion = "Av. Spinetto 1200",
                    Activo = true
                }
            );

            modelBuilder.Entity<Medicamento>().HasData(
                new { Id = 1, Nombre = "Amoxicilina", Presentacion = "Comprimidos 250 mg", Descripcion = "Antibiótico", StockActual = 40, StockMinimo = 10, Activo = true },
                new { Id = 2, Nombre = "Meloxicam", Presentacion = "Gotas 15 ml", Descripcion = "Antiinflamatorio", StockActual = 15, StockMinimo = 5, Activo = true },
                new { Id = 3, Nombre = "Ivermectina", Presentacion = "Frasco 50 ml", Descripcion = "Antiparasitario", StockActual = 8, StockMinimo = 4, Activo = true },
                new { Id = 4, Nombre = "Vacuna antirrábica", Presentacion = "Dosis individual", Descripcion = "Vacuna", StockActual = 25, StockMinimo = 10, Activo = true },
                new { Id = 5, Nombre = "Pipeta antipulgas", Presentacion = "Pipeta 10-20 kg", Descripcion = "Antiparasitario externo", StockActual = 3, StockMinimo = 5, Activo = true }
            );
        }
    }
}
