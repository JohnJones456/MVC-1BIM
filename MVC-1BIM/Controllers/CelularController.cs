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
    public class CelularController : Controller
    {
        private static List<Celular> celulares = new List<Celular>
        {
            new Celular { Id = 1, Marca = "Samsung", Modelo = "Galaxy S21", Preco = 3500.00m, DataFabricacao = new DateTime(2021, 3, 15) },
            new Celular { Id = 2, Marca = "Apple", Modelo = "iPhone 13", Preco = 5000.00m, DataFabricacao = new DateTime(2022, 1, 10) }
        };

        public IActionResult Index() => View(celulares);

        public IActionResult Cadastrar() => View();

        [HttpPost]
        public IActionResult Cadastrar(Celular celular)
        {
            celular.Id = celulares.Any() ? celulares.Max(c => c.Id) + 1 : 1;
            celulares.Add(celular);
            return RedirectToAction("Index");
        }

        public IActionResult Editar(int id) => View(celulares.FirstOrDefault(c => c.Id == id));

        [HttpPost]
        public IActionResult Editar(Celular celular)
        {
            var existente = celulares.FirstOrDefault(c => c.Id == celular.Id);
            if (existente != null)
            {
                existente.Marca = celular.Marca;
                existente.Modelo = celular.Modelo;
                existente.Preco = celular.Preco;
                existente.DataFabricacao = celular.DataFabricacao;
            }
            return RedirectToAction("Index");
        }

        public IActionResult Excluir(int id) => View(celulares.FirstOrDefault(c => c.Id == id));

        [HttpPost]
        public IActionResult Excluir(Celular celular)
        {
            celulares.RemoveAll(c => c.Id == celular.Id);
            return RedirectToAction("Index");
        }

        public IActionResult Visualizar(int id) => View(celulares.FirstOrDefault(c => c.Id == id));

        public IActionResult GerarPdf()
        {
            var stream = new MemoryStream();
            var writer = new PdfWriter(stream);
            var pdf = new PdfDocument(writer);
            var document = new Document(pdf);

            var boldFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
            var regularFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);

            var titulo = new Paragraph("📱 Lista de Celulares")
                .SetFont(boldFont)
                .SetFontSize(18)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetMarginBottom(20);
            document.Add(titulo);

            Table tabela = new Table(5);
            tabela.SetWidth(UnitValue.CreatePercentValue(100));

            string[] headers = { "ID", "Marca", "Modelo", "Preço", "Data Fabricação" };
            foreach (var header in headers)
            {
                tabela.AddHeaderCell(new Cell()
                    .Add(new Paragraph(header).SetFont(boldFont))
                    .SetBackgroundColor(iText.Kernel.Colors.ColorConstants.LIGHT_GRAY)
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetPadding(5));
            }

            foreach (var celular in celulares)
            {
                tabela.AddCell(new Cell().Add(new Paragraph(celular.Id.ToString())).SetFont(regularFont).SetPadding(5));
                tabela.AddCell(new Cell().Add(new Paragraph(celular.Marca)).SetFont(regularFont).SetPadding(5));
                tabela.AddCell(new Cell().Add(new Paragraph(celular.Modelo)).SetFont(regularFont).SetPadding(5));
                tabela.AddCell(new Cell().Add(new Paragraph("R$ " + celular.Preco.ToString("F2"))).SetFont(regularFont).SetPadding(5));
                tabela.AddCell(new Cell().Add(new Paragraph(celular.DataFabricacao.ToString("dd/MM/yyyy"))).SetFont(regularFont).SetPadding(5));
            }

            document.Add(tabela);
            document.Close();

            return File(stream.ToArray(), "application/pdf", "celulares.pdf");
        }

        public IActionResult GerarExcel()
        {
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Celulares");

                // Cabeçalho
                worksheet.Cell(1, 1).Value = "ID";
                worksheet.Cell(1, 2).Value = "Marca";
                worksheet.Cell(1, 3).Value = "Modelo";
                worksheet.Cell(1, 4).Value = "Preço";
                worksheet.Cell(1, 5).Value = "Data de Fabricação";

                // Estilo do cabeçalho
                var headerRange = worksheet.Range(1, 1, 1, 5);
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Fill.BackgroundColor = XLColor.LightBlue;
                headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                headerRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                // Preenchimento dos dados
                int linha = 2;
                foreach (var celular in celulares)
                {
                    worksheet.Cell(linha, 1).Value = celular.Id;
                    worksheet.Cell(linha, 2).Value = celular.Marca;
                    worksheet.Cell(linha, 3).Value = celular.Modelo;
                    worksheet.Cell(linha, 4).Value = celular.Preco;
                    worksheet.Cell(linha, 5).Value = celular.DataFabricacao.ToString("dd/MM/yyyy");
                    linha++;
                }

                worksheet.Columns().AdjustToContents();

                // Alinhamento específico para colunas
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
                    return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "ListaDeCelulares.xlsx");
                }
            }
        }

    }
}
