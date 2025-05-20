using iText.IO.Font.Constants;
using iText.Kernel.Font;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using MVC_1BIM.Models;
using System;

using ClosedXML.Excel;

namespace MVC_1BIM.Controllers
{
    public class EventoController : Controller
    {
        private static List<Evento> eventos = new List<Evento>
        {
            new Evento { Id = 1, Nome = "Show de Rock", Local = "Arena SP", Preco = 120.00m, Data = new DateTime(2024, 6, 15) },
            new Evento { Id = 2, Nome = "Feira de Tecnologia", Local = "Expo Center", Preco = 50.00m, Data = new DateTime(2024, 7, 10) }
        };

        public IActionResult Index() => View(eventos);

        public IActionResult Cadastrar() => View();

        [HttpPost]
        public IActionResult Cadastrar(Evento evento)
        {
            evento.Id = eventos.Any() ? eventos.Max(e => e.Id) + 1 : 1;
            eventos.Add(evento);
            return RedirectToAction("Index");
        }

        public IActionResult Editar(int id) => View(eventos.FirstOrDefault(e => e.Id == id));

        [HttpPost]
        public IActionResult Editar(Evento evento)
        {
            var existente = eventos.FirstOrDefault(e => e.Id == evento.Id);
            if (existente != null)
            {
                existente.Nome = evento.Nome;
                existente.Local = evento.Local;
                existente.Preco = evento.Preco;
                existente.Data = evento.Data;
            }
            return RedirectToAction("Index");
        }

        public IActionResult Excluir(int id) => View(eventos.FirstOrDefault(e => e.Id == id));

        [HttpPost]
        public IActionResult Excluir(Evento evento)
        {
            eventos.RemoveAll(e => e.Id == evento.Id);
            return RedirectToAction("Index");
        }

        public IActionResult Visualizar(int id) => View(eventos.FirstOrDefault(e => e.Id == id));

        public IActionResult GerarPdf()
        {
            var stream = new MemoryStream();
            var writer = new PdfWriter(stream);
            var pdf = new PdfDocument(writer);
            var document = new Document(pdf);

            var boldFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
            var regularFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);

            var titulo = new Paragraph("🎉 Lista de Eventos")
                .SetFont(boldFont)
                .SetFontSize(18)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetMarginBottom(20);
            document.Add(titulo);

            Table tabela = new Table(5);
            tabela.SetWidth(UnitValue.CreatePercentValue(100));

            string[] headers = { "ID", "Nome", "Local", "Preço", "Data" };
            foreach (var header in headers)
            {
                tabela.AddHeaderCell(new Cell()
                    .Add(new Paragraph(header).SetFont(boldFont))
                    .SetBackgroundColor(iText.Kernel.Colors.ColorConstants.LIGHT_GRAY)
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetPadding(5));
            }

            foreach (var evento in eventos)
            {
                tabela.AddCell(new Cell().Add(new Paragraph(evento.Id.ToString())).SetFont(regularFont).SetPadding(5));
                tabela.AddCell(new Cell().Add(new Paragraph(evento.Nome)).SetFont(regularFont).SetPadding(5));
                tabela.AddCell(new Cell().Add(new Paragraph(evento.Local)).SetFont(regularFont).SetPadding(5));
                tabela.AddCell(new Cell().Add(new Paragraph("R$ " + evento.Preco.ToString("F2"))).SetFont(regularFont).SetPadding(5));
                tabela.AddCell(new Cell().Add(new Paragraph(evento.Data.ToString("dd/MM/yyyy"))).SetFont(regularFont).SetPadding(5));
            }

            document.Add(tabela);
            document.Close();

            return File(stream.ToArray(), "application/pdf", "eventos.pdf");
        }

        public IActionResult GerarExcel()
        {
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Eventos");

                // Cabeçalho
                worksheet.Cell(1, 1).Value = "ID";
                worksheet.Cell(1, 2).Value = "Nome";
                worksheet.Cell(1, 3).Value = "Local";
                worksheet.Cell(1, 4).Value = "Preço";
                worksheet.Cell(1, 5).Value = "Data";

                // Estilo do cabeçalho
                var headerRange = worksheet.Range(1, 1, 1, 5);
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Fill.BackgroundColor = XLColor.LightBlue;
                headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                headerRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                // Preenchimento dos dados
                int linha = 2;
                foreach (var evento in eventos)
                {
                    worksheet.Cell(linha, 1).Value = evento.Id;
                    worksheet.Cell(linha, 2).Value = evento.Nome;
                    worksheet.Cell(linha, 3).Value = evento.Local;
                    worksheet.Cell(linha, 4).Value = evento.Preco;
                    worksheet.Cell(linha, 5).Value = evento.Data.ToString("dd/MM/yyyy");
                    linha++;
                }

                worksheet.Columns().AdjustToContents();

                // Alinhamento específico
                worksheet.Column(1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                worksheet.Column(4).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                worksheet.Column(5).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                // Bordas para os dados
                var dataRange = worksheet.Range(1, 1, linha - 1, 5);
                dataRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                dataRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                // Exportar
                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();
                    return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "ListaDeEventos.xlsx");
                }
            }
        }
    }
}
