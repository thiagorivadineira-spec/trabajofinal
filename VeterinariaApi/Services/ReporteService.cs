using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using VeterinariaApi.Data;
using System.Globalization;

namespace VeterinariaApi.Services
{
    public interface IReporteService
    {
        /// <summary>Genera el PDF. tipo: "stock", "compras" o "atenciones".</summary>
        Task<byte[]> GenerarAsync(string tipo, DateTime? desde, DateTime? hasta);

        string NormalizarTipo(string? tipo);
    }

    public class ReporteService : IReporteService
    {
        public const string Stock = "stock";
        public const string Compras = "compras";
        public const string Atenciones = "atenciones";

        private static readonly CultureInfo Cultura = ObtenerCultura();

        private readonly DataContext _context;

        public ReporteService(DataContext context)
        {
            _context = context;
        }

        public string NormalizarTipo(string? tipo)
        {
            var t = (tipo ?? string.Empty).Trim().ToLowerInvariant();

            return t is Stock or Compras or Atenciones
                ? t
                : throw new ArgumentException("Tipo de reporte inválido. Use: stock, compras o atenciones.");
        }

        public async Task<byte[]> GenerarAsync(string tipo, DateTime? desde, DateTime? hasta)
        {
            switch (NormalizarTipo(tipo))
            {
                case Stock:
                    return await GenerarStockAsync();
                case Compras:
                    return await GenerarComprasAsync(desde, hasta);
                default:
                    return await GenerarAtencionesAsync(desde, hasta);
            }
        }

        // ---------------- STOCK DE MEDICAMENTOS ----------------
        private async Task<byte[]> GenerarStockAsync()
        {
            var medicamentos = await _context.Medicamentos
                .AsNoTracking()
                .Where(m => m.Activo)
                .OrderBy(m => m.Nombre)
                .Select(m => new { m.Nombre, m.Presentacion, m.StockActual, m.StockMinimo })
                .ToListAsync();

            var aReponer = medicamentos.Count(m => m.StockActual <= m.StockMinimo);

            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    ConfigurarPagina(page, "Stock de medicamentos", $"Generado el {Hora.Ahora:dd/MM/yyyy HH:mm}");

                    page.Content().PaddingVertical(10).Column(col =>
                    {
                        col.Spacing(8);

                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(c =>
                            {
                                c.RelativeColumn(4);    // Medicamento
                                c.RelativeColumn(4);    // Presentación
                                c.RelativeColumn(1.5f); // Stock
                                c.RelativeColumn(1.5f); // Mínimo
                                c.RelativeColumn(2);    // Estado
                            });

                            table.Header(h =>
                            {
                                h.Cell().Element(EncabezadoCelda).Text("Medicamento");
                                h.Cell().Element(EncabezadoCelda).Text("Presentación");
                                h.Cell().Element(EncabezadoCelda).AlignRight().Text("Stock");
                                h.Cell().Element(EncabezadoCelda).AlignRight().Text("Mínimo");
                                h.Cell().Element(EncabezadoCelda).Text("Estado");
                            });

                            foreach (var m in medicamentos)
                            {
                                var reponer = m.StockActual <= m.StockMinimo;

                                table.Cell().Element(Celda).Text(m.Nombre);
                                table.Cell().Element(Celda).Text(m.Presentacion ?? "-");
                                table.Cell().Element(Celda).AlignRight().Text(m.StockActual.ToString(Cultura));
                                table.Cell().Element(Celda).AlignRight().Text(m.StockMinimo.ToString(Cultura));
                                table.Cell().Element(Celda)
                                    .Text(reponer ? "REPONER" : "OK")
                                    .FontColor(reponer ? Colors.Red.Medium : Colors.Green.Darken2);
                            }
                        });

                        col.Item().PaddingTop(6).Text($"Medicamentos: {medicamentos.Count}   |   A reponer: {aReponer}").SemiBold();
                    });
                });
            }).GeneratePdf();
        }

        // ---------------- COMPRAS A DISTRIBUIDORES ----------------
        private async Task<byte[]> GenerarComprasAsync(DateTime? desde, DateTime? hasta)
        {
            var (desdeFecha, hastaFecha) = Periodo(desde, hasta);
            var hastaExclusiva = hastaFecha.AddDays(1);

            var lineas = await _context.Compras
                .AsNoTracking()
                .Where(c => c.Fecha >= desdeFecha && c.Fecha < hastaExclusiva)
                .OrderBy(c => c.Fecha)
                .SelectMany(c => c.Detalles.Select(d => new
                {
                    c.Fecha,
                    Distribuidor = c.Distribuidor.RazonSocial,
                    Usuario = c.Usuario.Nombre,
                    Medicamento = d.Medicamento.Nombre,
                    d.Cantidad,
                    d.PrecioUnitario
                }))
                .ToListAsync();

            var subtitulo = $"Período: {desdeFecha:dd/MM/yyyy} al {hastaFecha:dd/MM/yyyy}   |   Generado el {Hora.Ahora:dd/MM/yyyy HH:mm}";

            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    ConfigurarPagina(page, "Compras de medicamentos", subtitulo);

                    page.Content().PaddingVertical(10).Column(col =>
                    {
                        col.Spacing(8);

                        if (lineas.Count == 0)
                        {
                            col.Item().Text("No hay compras en el período seleccionado.");
                            return;
                        }

                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(c =>
                            {
                                c.RelativeColumn(2.5f);
                                c.RelativeColumn(3.5f);
                                c.RelativeColumn(2.5f);
                                c.RelativeColumn(3.5f);
                                c.RelativeColumn(1.3f);
                                c.RelativeColumn(2.3f);
                                c.RelativeColumn(2.5f);
                            });

                            table.Header(h =>
                            {
                                h.Cell().Element(EncabezadoCelda).Text("Fecha");
                                h.Cell().Element(EncabezadoCelda).Text("Distribuidor");
                                h.Cell().Element(EncabezadoCelda).Text("Registró");
                                h.Cell().Element(EncabezadoCelda).Text("Medicamento");
                                h.Cell().Element(EncabezadoCelda).AlignRight().Text("Cant.");
                                h.Cell().Element(EncabezadoCelda).AlignRight().Text("P. unitario");
                                h.Cell().Element(EncabezadoCelda).AlignRight().Text("Subtotal");
                            });

                            foreach (var l in lineas)
                            {
                                table.Cell().Element(Celda).Text(l.Fecha.ToString("dd/MM/yyyy HH:mm"));
                                table.Cell().Element(Celda).Text(l.Distribuidor);
                                table.Cell().Element(Celda).Text(l.Usuario);
                                table.Cell().Element(Celda).Text(l.Medicamento);
                                table.Cell().Element(Celda).AlignRight().Text(l.Cantidad.ToString(Cultura));
                                table.Cell().Element(Celda).AlignRight().Text(Dinero(l.PrecioUnitario));
                                table.Cell().Element(Celda).AlignRight().Text(Dinero(l.Cantidad * l.PrecioUnitario));
                            }
                        });

                        col.Item().PaddingTop(6).Text(t =>
                        {
                            t.Span($"Unidades: {lineas.Sum(l => l.Cantidad)}   |   Total: ").SemiBold();
                            t.Span(Dinero(lineas.Sum(l => l.Cantidad * l.PrecioUnitario))).SemiBold();
                        });
                    });
                });
            }).GeneratePdf();
        }

        // ---------------- ATENCIONES MÉDICAS ----------------
        private async Task<byte[]> GenerarAtencionesAsync(DateTime? desde, DateTime? hasta)
        {
            var (desdeFecha, hastaFecha) = Periodo(desde, hasta);
            var hastaExclusiva = hastaFecha.AddDays(1);

            var atenciones = await _context.Atenciones
                .AsNoTracking()
                .Where(a => a.Fecha >= desdeFecha && a.Fecha < hastaExclusiva)
                .OrderBy(a => a.Fecha)
                .Select(a => new
                {
                    a.Fecha,
                    Mascota = a.Mascota.Nombre,
                    a.Mascota.Especie,
                    Dueno = a.Mascota.Dueno.Apellido + ", " + a.Mascota.Dueno.Nombre,
                    Veterinario = a.Veterinario.Usuario.Nombre,
                    a.Diagnostico
                })
                .ToListAsync();

            var subtitulo = $"Período: {desdeFecha:dd/MM/yyyy} al {hastaFecha:dd/MM/yyyy}   |   Generado el {Hora.Ahora:dd/MM/yyyy HH:mm}";

            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    ConfigurarPagina(page, "Atenciones médicas", subtitulo);

                    page.Content().PaddingVertical(10).Column(col =>
                    {
                        col.Spacing(8);

                        if (atenciones.Count == 0)
                        {
                            col.Item().Text("No hay atenciones en el período seleccionado.");
                            return;
                        }

                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(c =>
                            {
                                c.RelativeColumn(2.3f);
                                c.RelativeColumn(2.5f);
                                c.RelativeColumn(3);
                                c.RelativeColumn(2.7f);
                                c.RelativeColumn(6);
                            });

                            table.Header(h =>
                            {
                                h.Cell().Element(EncabezadoCelda).Text("Fecha");
                                h.Cell().Element(EncabezadoCelda).Text("Mascota");
                                h.Cell().Element(EncabezadoCelda).Text("Dueño");
                                h.Cell().Element(EncabezadoCelda).Text("Veterinario");
                                h.Cell().Element(EncabezadoCelda).Text("Diagnóstico");
                            });

                            foreach (var a in atenciones)
                            {
                                table.Cell().Element(Celda).Text(a.Fecha.ToString("dd/MM/yyyy HH:mm"));
                                table.Cell().Element(Celda).Text($"{a.Mascota} ({a.Especie})");
                                table.Cell().Element(Celda).Text(a.Dueno);
                                table.Cell().Element(Celda).Text(a.Veterinario);
                                table.Cell().Element(Celda).Text(a.Diagnostico);
                            }
                        });

                        col.Item().PaddingTop(6).Text($"Atenciones: {atenciones.Count}").SemiBold();
                    });
                });
            }).GeneratePdf();
        }

        private static (DateTime desde, DateTime hasta) Periodo(DateTime? desde, DateTime? hasta)
        {
            var hastaFecha = (hasta ?? Hora.Hoy).Date;
            var desdeFecha = (desde ?? hastaFecha.AddDays(-30)).Date;

            if (desdeFecha > hastaFecha)
            {
                throw new ArgumentException("La fecha 'desde' no puede ser posterior a 'hasta'.");
            }

            return (desdeFecha, hastaFecha);
        }

        // ------------- Utilidades de formato -------------
        // Hoja A4 vertical con márgenes y filas compactas: entran el doble de filas por hoja
        // que en la versión horizontal
        private static void ConfigurarPagina(PageDescriptor page, string titulo, string subtitulo)
        {
            page.Size(PageSizes.A4);          // vertical
            page.Margin(1.2f, Unit.Centimetre);
            page.DefaultTextStyle(x => x.FontSize(9));

            page.Header().Column(col =>
            {
                col.Item().Row(row =>
                {
                    row.RelativeItem().Text(titulo).FontSize(14).SemiBold().FontColor(Colors.Teal.Darken3);
                    row.AutoItem().AlignBottom().Text("Veterinaria Volki").FontSize(9).FontColor(Colors.Grey.Darken1);
                });
                col.Item().Text(subtitulo).FontSize(8).FontColor(Colors.Grey.Darken1);
                col.Item().PaddingTop(3).LineHorizontal(0.5f).LineColor(Colors.Grey.Lighten1);
            });

            page.Footer().AlignRight().Text(t =>
            {
                t.DefaultTextStyle(x => x.FontSize(7).FontColor(Colors.Grey.Darken1));
                t.Span("Página ");
                t.CurrentPageNumber();
                t.Span(" de ");
                t.TotalPages();
            });
        }

        private static IContainer EncabezadoCelda(IContainer c) =>
            c.Background(Colors.Grey.Lighten3)
             .PaddingVertical(2)
             .PaddingHorizontal(3)
             .DefaultTextStyle(x => x.SemiBold());

        private static IContainer Celda(IContainer c) =>
            c.BorderBottom(0.5f)
             .BorderColor(Colors.Grey.Lighten2)
             .PaddingVertical(1.5f)
             .PaddingHorizontal(3);

        private static string Dinero(decimal valor) =>
            "$ " + valor.ToString("N2", Cultura);

        private static CultureInfo ObtenerCultura()
        {
            try
            {
                return CultureInfo.GetCultureInfo("es-AR");
            }
            catch (CultureNotFoundException)
            {
                // Servidores con globalización invariante (p. ej. algunos contenedores)
                return CultureInfo.InvariantCulture;
            }
        }
    }
}
